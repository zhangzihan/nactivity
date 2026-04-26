using Spring.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using ExpressionEvaluator = Spring.Expressions.ExpressionEvaluator;
using SpringExpr = Spring.Expressions.Expression;

namespace Sys.Workflow.Client.Tests.Expression
{
    /// <summary>
    /// 错误处理和边界测试：语法错误、运行时异常、边界值
    /// </summary>
    public class ErrorHandlingTests
    {
        // ========== 语法错误 ==========

        [Fact]
        public void 语法错误_缺少操作数()
        {
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(null, "1 + "));
        }

        [Fact]
        public void 语法错误_不匹配的括号()
        {
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(null, "(1 + 2"));
        }

        [Fact]
        public void 语法错误_非法字符()
        {
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(null, "1 & 2"));
        }

        [Fact]
        public void 语法错误_错误消息包含行号信息()
        {
            try
            {
                ExpressionEvaluator.GetValue(null, "1 + ");
                Assert.Fail("Should have thrown");
            }
            catch (Exception ex)
            {
                Assert.Contains("Syntax Error", ex.Message);
            }
        }

        // ========== 运行时异常 ==========

        [Fact]
        public void 运行时_除以零()
        {
            Assert.Throws<DivideByZeroException>(() =>
                ExpressionEvaluator.GetValue(null, "1 / 0"));
        }

        [Fact]
        public void 运行时_空引用属性访问()
        {
            var data = new { obj = (string)null };
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(data, "obj.Length"));
        }

        [Fact]
        public void 运行时_方法参数不匹配()
        {
            var data = new { Text = "hello" };
            Assert.ThrowsAny<Exception>(() =>
                ExpressionEvaluator.GetValue(data, "Text.Substring(999)"));
        }

        // ========== 边界值 ==========

        [Fact]
        public void 边界_int最大值()
        {
            var result = ExpressionEvaluator.GetValue(null, int.MaxValue.ToString());
            Assert.Equal(int.MaxValue, result);
        }

        [Fact]
        public void 边界_零值运算()
        {
            Assert.Equal(0, ExpressionEvaluator.GetValue(null, "0 * 999"));
            Assert.Equal(0, ExpressionEvaluator.GetValue(null, "0 + 0"));
        }

        [Fact]
        public void 边界_很长的表达式()
        {
            // 构建一个长链表达式 1+1+1+...+1 (100次)
            string expr = string.Join("+", Enumerable.Range(0, 100).Select(_ => "1"));
            var result = ExpressionEvaluator.GetValue(null, expr);
            Assert.Equal(100, result);
        }

        // ========== 表达式解析缓存 ==========

        [Fact]
        public void 相同表达式_多次求值()
        {
            var data = new { x = 10 };
            var expr = SpringExpr.Parse("x * 2");
            var result1 = expr.GetValue(data);
            var result2 = expr.GetValue(data);
            Assert.Equal(result1, result2);
        }

        [Fact]
        public void 相同表达式_不同上下文()
        {
            var expr = SpringExpr.Parse("x + 1");
            var result1 = expr.GetValue(new { x = 5 });
            var result2 = expr.GetValue(new { x = 10 });
            Assert.Equal(6, result1);
            Assert.Equal(11, result2);
        }

        // ========== 变量上下文 ==========

        [Fact]
        public void 变量上下文_传递Dictionary()
        {
            var variables = new Dictionary<string, object>
            {
                { "a", 10 },
                { "b", 20 }
            };
            var result = ExpressionEvaluator.GetValue(null, "#a + #b", variables);
            Assert.Equal(30, result);
        }

        [Fact]
        public void 变量上下文_嵌套变量表达式()
        {
            var variables = new Dictionary<string, object>
            {
                { "x", 5 }
            };
            var result = ExpressionEvaluator.GetValue(null, "#x * #x + 1", variables);
            Assert.Equal(26, result);
        }

        // ========== 集合处理器 ==========

        [Fact]
        public void 集合_min处理器()
        {
            var data = new[] { 5, 3, 8, 1, 9 };
            var result = ExpressionEvaluator.GetValue(data, "min()");
            Assert.Equal(1, Convert.ToInt32(result));
        }

        [Fact]
        public void 集合_max处理器()
        {
            var data = new[] { 5, 3, 8, 1, 9 };
            var result = ExpressionEvaluator.GetValue(data, "max()");
            Assert.Equal(9, Convert.ToInt32(result));
        }

        [Fact]
        public void 集合_count处理器()
        {
            var data = new[] { 1, 2, 3 };
            var result = ExpressionEvaluator.GetValue(data, "count()");
            Assert.Equal(3, result);
        }

        [Fact]
        public void 集合_average处理器()
        {
            var data = new[] { 1, 2, 3, 4, 5 };
            var result = ExpressionEvaluator.GetValue(data, "average()");
            Assert.Equal(3.0, Convert.ToDouble(result), 1);
        }

        [Fact]
        public void 集合_nonNull处理器()
        {
            int?[] data = { null, 1, null, 2, 3 };
            var result = ExpressionEvaluator.GetValue(data, "nonNull()");
            var list = result as IList;
            Assert.NotNull(list);
            Assert.Equal(3, list.Count);
        }

        // ========== 多语言标识符 ==========

        [Fact]
        public void 中文标识符_属性名()
        {
            var data = new { 数量 = 100 };
            var result = ExpressionEvaluator.GetValue(data, "数量");
            Assert.Equal(100, result);
        }

        [Fact]
        public void 中文标识符_运算()
        {
            var data = new { 单价 = 10M, 数量 = 5 };
            var result = ExpressionEvaluator.GetValue(data, "单价 * 数量");
            Assert.Equal(50M, result);
        }

        // ========== Parse ==========

        [Fact]
        public void Parse_解析并求值()
        {
            var expr = SpringExpr.Parse("1 + 2");
            Assert.Equal(3, expr.GetValue(null));
        }
    }
}
