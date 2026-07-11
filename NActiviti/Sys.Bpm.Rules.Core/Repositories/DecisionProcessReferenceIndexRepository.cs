using SmartSql;
using SmartSql.Abstractions;
using Sys.Workflow.Rules;
using Sys.Workflow.Rules.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sys.Workflow.Rules.Core.Repositories
{
    /// <summary>
    /// 定义跨数据库通用的流程决策引用索引仓储实现。
    /// </summary>
    public sealed class DecisionProcessReferenceIndexRepository : IDecisionProcessReferenceIndexRepository
    {
        private const string Scope = "Sys.Workflow.Rules.Core.Repositories.DecisionProcessReferenceIndexRepository";
        private readonly ISmartSqlMapper _sqlMapper;

        /// <summary>
        /// 初始化流程决策引用索引仓储。
        /// </summary>
        /// <param name="sqlMapper">SmartSql 映射器。</param>
        public DecisionProcessReferenceIndexRepository(ISmartSqlMapper sqlMapper)
        {
            _sqlMapper = sqlMapper ?? throw new ArgumentNullException(nameof(sqlMapper));
        }

        /// <summary>
        /// 以部署为粒度重建决策引用索引快照。
        /// </summary>
        /// <param name="snapshot">部署引用快照。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        public async Task ReplaceDeploymentReferencesAsync(
            DecisionProcessReferenceIndexSnapshot snapshot,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(snapshot);

            if (string.IsNullOrWhiteSpace(snapshot.DeploymentId))
            {
                throw new ArgumentException("部署标识不能为空。", nameof(snapshot));
            }

            if (string.IsNullOrWhiteSpace(snapshot.DeploymentName))
            {
                throw new ArgumentException("部署名称不能为空。", nameof(snapshot));
            }

            var deployTime = snapshot.DeployTime == default
                ? DateTimeOffset.UtcNow
                : snapshot.DeployTime;
            var indexedTime = DateTimeOffset.UtcNow;
            var entries = BuildEntries(snapshot, deployTime, indexedTime);

            await _sqlMapper.TransactionWrapAsync(async () =>
            {
                await DeleteByDeploymentIdInternalAsync(snapshot.DeploymentId).ConfigureAwait(false);
                foreach (var entry in entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await _sqlMapper.ExecuteAsync(new RequestContext
                    {
                        Scope = Scope,
                        SqlId = "InsertEntry",
                        DataSourceChoice = DataSourceChoice.Write,
                        Request = entry
                    }).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// 删除指定部署对应的全部引用索引。
        /// </summary>
        /// <param name="deploymentId">部署标识。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        public async Task DeleteByDeploymentIdAsync(string deploymentId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(deploymentId))
            {
                return;
            }

            await DeleteByDeploymentIdInternalAsync(deploymentId).ConfigureAwait(false);
        }

        /// <summary>
        /// 查询指定部署集合中已完成索引的部署标识。
        /// </summary>
        /// <param name="deploymentIds">部署标识集合。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>已完成索引的部署标识列表。</returns>
        public async Task<IReadOnlyList<string>> GetIndexedDeploymentIdsAsync(
            IReadOnlyCollection<string> deploymentIds,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sanitizedIds = deploymentIds?
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();
            if (sanitizedIds.Count == 0)
            {
                return Array.Empty<string>();
            }

            var records = await _sqlMapper.QueryAsync<string>(new RequestContext
            {
                Scope = Scope,
                SqlId = "GetIndexedDeploymentIds",
                DataSourceChoice = DataSourceChoice.Read,
                Request = new
                {
                    items = sanitizedIds
                }
            }).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            return records
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// 按决策键查询引用索引明细行。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的索引明细行。</returns>
        public async Task<IReadOnlyList<DecisionProcessReferenceIndexEntryRecord>> QueryEntriesAsync(
            DecisionProcessReferenceIndexQuery query,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(query);

            if (string.IsNullOrWhiteSpace(query.DecisionKey))
            {
                throw new ArgumentException("决策键不能为空。", nameof(query));
            }

            var deploymentIds = query.DeploymentIds?
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();

            var records = await _sqlMapper.QueryAsync<DecisionProcessReferenceIndexEntryRecord>(new RequestContext
            {
                Scope = Scope,
                SqlId = "QueryEntries",
                DataSourceChoice = DataSourceChoice.Read,
                Request = new
                {
                    decisionKey = query.DecisionKey,
                    tenantId = query.TenantId,
                    includeDrafts = query.IncludeDrafts,
                    deploymentIds
                }
            }).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            return records.ToList();
        }

        /// <summary>
        /// 删除指定部署对应的全部引用索引。
        /// </summary>
        /// <param name="deploymentId">部署标识。</param>
        private async Task DeleteByDeploymentIdInternalAsync(string deploymentId)
        {
            await _sqlMapper.ExecuteAsync(new RequestContext
            {
                Scope = Scope,
                SqlId = "DeleteByDeploymentId",
                DataSourceChoice = DataSourceChoice.Write,
                Request = new
                {
                    deploymentId
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// 将部署快照转换为可持久化的索引行。
        /// </summary>
        /// <param name="snapshot">部署快照。</param>
        /// <param name="deployTime">规范化后的部署时间。</param>
        /// <param name="indexedTime">索引写入时间。</param>
        /// <returns>索引行集合。</returns>
        private static IReadOnlyList<DecisionProcessReferenceIndexEntryRecord> BuildEntries(
            DecisionProcessReferenceIndexSnapshot snapshot,
            DateTimeOffset deployTime,
            DateTimeOffset indexedTime)
        {
            var entries = new List<DecisionProcessReferenceIndexEntryRecord>();
            foreach (var process in snapshot.Processes ?? Array.Empty<DecisionProcessReferenceIndexProcessRecord>())
            {
                foreach (var task in process.ReferencedTasks ?? Array.Empty<DecisionProcessReferenceIndexTaskRecord>())
                {
                    entries.Add(new DecisionProcessReferenceIndexEntryRecord
                    {
                        ReferenceId = Guid.NewGuid().ToString("D"),
                        DeploymentId = snapshot.DeploymentId,
                        DeploymentName = snapshot.DeploymentName,
                        ProcessDefinitionKey = process.ProcessDefinitionKey,
                        ProcessDefinitionName = process.ProcessDefinitionName,
                        TenantId = snapshot.TenantId,
                        IsDraft = snapshot.IsDraft,
                        DeployTime = deployTime,
                        DecisionKey = task.DecisionKey,
                        TaskId = task.TaskId,
                        TaskName = task.TaskName,
                        BindingMode = task.BindingMode,
                        VersionTag = task.VersionTag,
                        IndexedTime = indexedTime
                    });
                }
            }

            if (entries.Count > 0)
            {
                return entries;
            }

            return new[]
            {
                new DecisionProcessReferenceIndexEntryRecord
                {
                    ReferenceId = Guid.NewGuid().ToString("D"),
                    DeploymentId = snapshot.DeploymentId,
                    DeploymentName = snapshot.DeploymentName,
                    ProcessDefinitionKey = null,
                    ProcessDefinitionName = null,
                    TenantId = snapshot.TenantId,
                    IsDraft = snapshot.IsDraft,
                    DeployTime = deployTime,
                    DecisionKey = null,
                    TaskId = null,
                    TaskName = null,
                    BindingMode = null,
                    VersionTag = null,
                    IndexedTime = indexedTime
                }
            };
        }
    }
}
