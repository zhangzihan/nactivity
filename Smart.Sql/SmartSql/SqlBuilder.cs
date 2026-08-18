using Microsoft.Extensions.Logging;
using SmartSql.Abstractions;
using SmartSql.Configuration.Statements;
using SmartSql.Configuration.Tags;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace SmartSql
{
    public class SqlBuilder : ISqlBuilder
    {
        /// <summary>
        /// 语句级 SQL 模板缓存项
        /// </summary>
        private class CachedStatementSql
        {
            public bool IsStatic;
            public string Sql;
        }

        private readonly ILogger _logger;
        private readonly SmartSqlContext _smartSqlContext;

        /// <summary>
        /// 静态语句的 SQL 模板缓存。
        /// 键与 Statement 对象绑定（ConditionalWeakTable）：SQL 映射热更新会重建 Statement 实例，
        /// 旧缓存随之自动失效并被 GC 回收，不会产生脏缓存或内存泄漏。
        /// </summary>
        private readonly ConditionalWeakTable<Statement, CachedStatementSql> _cachedStatementSql = new ConditionalWeakTable<Statement, CachedStatementSql>();

        public SqlBuilder(ILogger<SqlBuilder> logger,
            SmartSqlContext smartSqlContext)
        {
            _logger = logger;
            _smartSqlContext = smartSqlContext;
        }

        public string BuildSql(RequestContext context)
        {
            if (!String.IsNullOrEmpty(context.RealSql))
            {
                context.IsStatementSql = false;
            }
            else
            {
                var statement = _smartSqlContext.GetStatement(context.FullSqlId);
                context.Statement = statement;
                var cached = _cachedStatementSql.GetValue(statement,
                    s => new CachedStatementSql { IsStatic = IsStaticStatement(s) });
                if (cached.IsStatic && cached.Sql is object)
                {
                    // 命中缓存：直接复用 SQL 模板，跳过标签树遍历。
                    // #{} 参数占位符的值绑定发生在 PreparedCommand 阶段、${} 变量替换发生在
                    // RequestContext.SetupSql 阶段，均在本方法之后，缓存模板对二者没有影响；
                    // context.Sql 仍需填充，PreparedCommand 会读取它做通配符转义检测。
                    context.Sql = new StringBuilder(cached.Sql);
                    context.RealSql = cached.Sql;
                }
                else
                {
                    context.Sql = new StringBuilder();
                    statement.BuildSql(context);
                    context.RealSql = context.Sql.ToString().Trim().Replace("\r", " ");
                    if (cached.IsStatic)
                    {
                        // 静态语句的标签树输出不依赖请求参数，SQL 模板可安全缓存；
                        // 并发下多线程写入的是同一字符串，结果一致，无需加锁。
                        cached.Sql = context.RealSql;
                    }
                }
            }

            return context.RealSql;
        }

        /// <summary>
        /// 判断语句的标签树是否“静态”，即 BuildSql 的输出不依赖请求参数的运行时取值。
        /// 动态 SQL（条件标签求值、For 循环展开、Placeholder 内联参数值）无法按语句缓存，
        /// 仍保持每次请求重建。
        /// </summary>
        private static bool IsStaticStatement(Statement statement)
        {
            return IsStaticTags(statement.SqlTags, new HashSet<Statement>());
        }

        private static bool IsStaticTags(IList<ITag> tags, ISet<Statement> visited)
        {
            if (tags is null)
            {
                return true;
            }
            foreach (var tag in tags)
            {
                switch (tag.Type)
                {
                    case TagType.SqlText:
                        // 纯文本，输出恒定
                        break;
                    case TagType.Env:
                        // Env 仅取决于 SmartSqlContext.DbProvider，同一 SqlBuilder 实例内恒定
                        break;
                    case TagType.Include:
                    {
                        // Include 展开的是被引用的语句，递归判断；visited 防止循环引用
                        var include = tag as Include;
                        if (include?.Ref is null)
                        {
                            return false;
                        }
                        if (visited.Add(include.Ref) && !IsStaticTags(include.Ref.SqlTags, visited))
                        {
                            return false;
                        }
                        break;
                    }
                    case TagType.Trim:
                    case TagType.Dynamic:
                    case TagType.Where:
                    case TagType.Set:
                        // 容器标签自身输出固定，子标签全部静态则整体静态
                        if (tag is Tag container && !IsStaticTags(container.ChildTags, visited))
                        {
                            return false;
                        }
                        break;
                    default:
                        // 其余标签（If / IsEqual 等条件标签、Switch、For、Placeholder 等）的输出
                        // 依赖请求参数的运行时取值，无法按语句缓存
                        return false;
                }
            }
            return true;
        }
    }
}
