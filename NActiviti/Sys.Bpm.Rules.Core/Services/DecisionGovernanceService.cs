using SmartSql;
using SmartSql.Abstractions;
using Sys.Rules.DmnEngine.Engine.Definition;
using Sys.Rules.DmnEngine.Parser;
using Sys.Workflow.Rules.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Sys.Workflow.Rules.Core.Services
{
    /// <summary>
    /// 定义基于仓储的决策治理服务。
    /// </summary>
    public sealed class DecisionGovernanceService : IDecisionGovernanceService
    {
        private readonly IDecisionDefinitionRepository _decisionDefinitionRepository;
        private readonly IDecisionVersionRepository _decisionVersionRepository;
        private readonly ISmartSqlMapper? _sqlMapper;

        /// <summary>
        /// 初始化决策治理服务。
        /// </summary>
        /// <param name="decisionDefinitionRepository">决策定义仓储。</param>
        /// <param name="decisionVersionRepository">决策版本仓储。</param>
        /// <param name="sqlMapper">SmartSql 映射器；存在时用于治理事务控制。</param>
        public DecisionGovernanceService(
            IDecisionDefinitionRepository decisionDefinitionRepository,
            IDecisionVersionRepository decisionVersionRepository,
            ISmartSqlMapper? sqlMapper = null)
        {
            _decisionDefinitionRepository = decisionDefinitionRepository ?? throw new ArgumentNullException(nameof(decisionDefinitionRepository));
            _decisionVersionRepository = decisionVersionRepository ?? throw new ArgumentNullException(nameof(decisionVersionRepository));
            _sqlMapper = sqlMapper;
        }

        /// <summary>
        /// 保存决策草稿定义与草稿版本。
        /// </summary>
        /// <param name="request">草稿保存请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>草稿保存结果。</returns>
        public async Task<DecisionDraftSaveResult> SaveDraftAsync(DecisionDraftSaveRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            ValidateDraftRequest(request);

            return await ExecuteInTransactionAsync(async () =>
            {
                var existingDefinition = await ResolveDefinitionAsync(request.DecisionId, request.DecisionKey, cancellationToken).ConfigureAwait(false);
                var draftVersionTag = ResolveDraftVersionTag(request.VersionTag, existingDefinition?.DraftVersionTag);

                var definitionRecord = await _decisionDefinitionRepository.SaveAsync(new DecisionDefinitionRecord
                {
                    DecisionId = existingDefinition?.DecisionId ?? request.DecisionId ?? string.Empty,
                    DecisionKey = request.DecisionKey,
                    Name = request.Name,
                    Category = request.Category,
                    Description = request.Description,
                    DecisionType = request.DecisionType,
                    SourceFormat = string.IsNullOrWhiteSpace(request.SourceFormat) ? existingDefinition?.SourceFormat ?? "DMN" : request.SourceFormat,
                    Status = "Draft",
                    DraftVersionTag = draftVersionTag,
                    PublishedVersionTag = existingDefinition?.PublishedVersionTag,
                    Tags = request.Tags,
                    CreatedBy = existingDefinition?.CreatedBy ?? request.Operator,
                    CreatedTime = existingDefinition?.CreatedTime ?? default,
                    ModifiedBy = request.Operator
                }, cancellationToken).ConfigureAwait(false);

                var draftVersion = await _decisionVersionRepository.SaveAsync(new DecisionVersionRecord
                {
                    DecisionId = definitionRecord.DecisionId,
                    DecisionKey = definitionRecord.DecisionKey,
                    VersionTag = draftVersionTag,
                    IsDraft = true,
                    IsPublished = false,
                    DefinitionContent = request.DefinitionContent,
                    Checksum = request.Checksum,
                    CreatedBy = request.Operator
                }, cancellationToken).ConfigureAwait(false);

                if (!string.Equals(definitionRecord.DraftVersionTag, draftVersion.VersionTag, StringComparison.OrdinalIgnoreCase))
                {
                    definitionRecord.DraftVersionTag = draftVersion.VersionTag;
                    definitionRecord = await _decisionDefinitionRepository.SaveAsync(definitionRecord, cancellationToken).ConfigureAwait(false);
                }

                return new DecisionDraftSaveResult
                {
                    Definition = definitionRecord,
                    DraftVersion = draftVersion
                };
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// 发布指定决策版本。
        /// </summary>
        /// <param name="request">版本发布请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>发布结果。</returns>
        public async Task<DecisionVersionPublishResult> PublishVersionAsync(PublishDecisionVersionRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            ValidatePublishRequest(request);

            return await ExecuteInTransactionAsync(async () =>
            {
                var definitionRecord = await RequireDefinitionAsync(request.DecisionKey, cancellationToken).ConfigureAwait(false);
                var sourceVersionTag = ResolvePublishSourceVersionTag(request.VersionTag, definitionRecord.DraftVersionTag);
                var sourceVersion = await RequireVersionAsync(request.DecisionKey, sourceVersionTag, cancellationToken).ConfigureAwait(false);
                ValidateDefinitionContentBeforeActivation(definitionRecord.SourceFormat, definitionRecord.DecisionKey, sourceVersion.VersionTag, sourceVersion.DefinitionContent);
                var publishedVersion = await PublishVersionInternalAsync(sourceVersion, request.PublishComment, request.Operator, cancellationToken).ConfigureAwait(false);

                definitionRecord.Status = "Published";
                definitionRecord.PublishedVersionTag = publishedVersion.VersionTag;
                if (string.Equals(definitionRecord.DraftVersionTag, publishedVersion.VersionTag, StringComparison.OrdinalIgnoreCase))
                {
                    definitionRecord.DraftVersionTag = null;
                }

                definitionRecord.ModifiedBy = request.Operator;
                definitionRecord = await _decisionDefinitionRepository.SaveAsync(definitionRecord, cancellationToken).ConfigureAwait(false);

                return new DecisionVersionPublishResult
                {
                    Definition = definitionRecord,
                    PublishedVersion = publishedVersion
                };
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// 将指定版本重新发布为当前生效版本。
        /// </summary>
        /// <param name="request">版本回滚请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>回滚结果。</returns>
        public async Task<DecisionVersionPublishResult> RollbackVersionAsync(RollbackDecisionVersionRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            ValidateRollbackRequest(request);

            return await ExecuteInTransactionAsync(async () =>
            {
                var definitionRecord = await RequireDefinitionAsync(request.DecisionKey, cancellationToken).ConfigureAwait(false);
                var targetVersion = await RequireVersionAsync(request.DecisionKey, request.TargetVersionTag, cancellationToken).ConfigureAwait(false);
                var publishComment = string.IsNullOrWhiteSpace(request.RollbackComment)
                    ? $"Rollback to {request.TargetVersionTag}"
                    : request.RollbackComment;
                ValidateDefinitionContentBeforeActivation(definitionRecord.SourceFormat, definitionRecord.DecisionKey, targetVersion.VersionTag, targetVersion.DefinitionContent);
                var publishedVersion = await PublishVersionInternalAsync(targetVersion, publishComment, request.Operator, cancellationToken).ConfigureAwait(false);

                definitionRecord.Status = "Published";
                definitionRecord.PublishedVersionTag = publishedVersion.VersionTag;
                definitionRecord.ModifiedBy = request.Operator;
                definitionRecord = await _decisionDefinitionRepository.SaveAsync(definitionRecord, cancellationToken).ConfigureAwait(false);

                return new DecisionVersionPublishResult
                {
                    Definition = definitionRecord,
                    PublishedVersion = publishedVersion
                };
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// 在可用时使用 SmartSql 事务包裹治理写操作。
        /// </summary>
        /// <typeparam name="TResult">返回结果类型。</typeparam>
        /// <param name="handler">实际执行业务委托。</param>
        /// <returns>业务执行结果。</returns>
        private async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> handler)
        {
            ArgumentNullException.ThrowIfNull(handler);

            if (_sqlMapper == null)
            {
                return await handler().ConfigureAwait(false);
            }

            TResult? result = default;
            await _sqlMapper.TransactionWrapAsync(async () =>
            {
                result = await handler().ConfigureAwait(false);
            }).ConfigureAwait(false);

            return result!;
        }

        /// <summary>
        /// 校验草稿保存请求。
        /// </summary>
        /// <param name="request">草稿保存请求。</param>
        private static void ValidateDraftRequest(DecisionDraftSaveRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.DecisionKey))
            {
                throw new ArgumentException("决策主键不能为空。", nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("决策名称不能为空。", nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.DefinitionContent))
            {
                throw new ArgumentException("决策定义内容不能为空。", nameof(request));
            }
        }

        /// <summary>
        /// 校验版本发布请求。
        /// </summary>
        /// <param name="request">版本发布请求。</param>
        private static void ValidatePublishRequest(PublishDecisionVersionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.DecisionKey))
            {
                throw new ArgumentException("决策主键不能为空。", nameof(request));
            }
        }

        /// <summary>
        /// 校验版本回滚请求。
        /// </summary>
        /// <param name="request">版本回滚请求。</param>
        private static void ValidateRollbackRequest(RollbackDecisionVersionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.DecisionKey))
            {
                throw new ArgumentException("决策主键不能为空。", nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.TargetVersionTag))
            {
                throw new ArgumentException("目标回滚版本不能为空。", nameof(request));
            }
        }

        /// <summary>
        /// 解析现有决策定义。
        /// </summary>
        /// <param name="decisionId">决策定义标识。</param>
        /// <param name="decisionKey">决策唯一键。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的定义记录；若不存在则返回空。</returns>
        private async Task<DecisionDefinitionRecord?> ResolveDefinitionAsync(string? decisionId, string decisionKey, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(decisionId))
            {
                var definitionById = await _decisionDefinitionRepository.GetByDecisionIdAsync(decisionId, cancellationToken).ConfigureAwait(false);
                if (definitionById != null)
                {
                    return definitionById;
                }
            }

            return await _decisionDefinitionRepository.GetByDecisionKeyAsync(decisionKey, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// 要求存在指定决策定义。
        /// </summary>
        /// <param name="decisionKey">决策唯一键。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的决策定义。</returns>
        private async Task<DecisionDefinitionRecord> RequireDefinitionAsync(string decisionKey, CancellationToken cancellationToken)
        {
            var definitionRecord = await _decisionDefinitionRepository.GetByDecisionKeyAsync(decisionKey, cancellationToken).ConfigureAwait(false);
            if (definitionRecord == null)
            {
                throw new InvalidOperationException($"未找到决策 `{decisionKey}` 的定义主记录。");
            }

            return definitionRecord;
        }

        /// <summary>
        /// 要求存在指定决策版本。
        /// </summary>
        /// <param name="decisionKey">决策唯一键。</param>
        /// <param name="versionTag">版本标签。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的决策版本。</returns>
        private async Task<DecisionVersionRecord> RequireVersionAsync(string decisionKey, string versionTag, CancellationToken cancellationToken)
        {
            var versionRecord = await _decisionVersionRepository.GetByVersionTagAsync(decisionKey, versionTag, cancellationToken).ConfigureAwait(false);
            if (versionRecord == null)
            {
                throw new InvalidOperationException($"未找到决策 `{decisionKey}` 的版本 `{versionTag}`。");
            }

            return versionRecord;
        }

        /// <summary>
        /// 解析草稿版本标签。
        /// </summary>
        /// <param name="requestedVersionTag">请求版本标签。</param>
        /// <param name="existingDraftVersionTag">已有草稿版本标签。</param>
        /// <returns>最终草稿版本标签。</returns>
        private static string ResolveDraftVersionTag(string? requestedVersionTag, string? existingDraftVersionTag)
        {
            if (!string.IsNullOrWhiteSpace(requestedVersionTag))
            {
                return requestedVersionTag;
            }

            if (!string.IsNullOrWhiteSpace(existingDraftVersionTag))
            {
                return existingDraftVersionTag;
            }

            return "draft";
        }

        /// <summary>
        /// 解析发布来源版本标签。
        /// </summary>
        /// <param name="requestedVersionTag">请求版本标签。</param>
        /// <param name="draftVersionTag">当前草稿版本标签。</param>
        /// <returns>用于发布的版本标签。</returns>
        private static string ResolvePublishSourceVersionTag(string? requestedVersionTag, string? draftVersionTag)
        {
            if (!string.IsNullOrWhiteSpace(requestedVersionTag))
            {
                return requestedVersionTag;
            }

            if (!string.IsNullOrWhiteSpace(draftVersionTag))
            {
                return draftVersionTag;
            }

            throw new InvalidOperationException("未指定发布版本，且当前不存在可发布的草稿版本。");
        }

        /// <summary>
        /// 在版本进入生效态前校验其定义内容是否合法。
        /// </summary>
        /// <param name="sourceFormat">定义来源格式。</param>
        /// <param name="decisionKey">决策主键。</param>
        /// <param name="versionTag">版本标签。</param>
        /// <param name="definitionContent">定义内容。</param>
        private static void ValidateDefinitionContentBeforeActivation(
            string? sourceFormat,
            string decisionKey,
            string? versionTag,
            string definitionContent)
        {
            if (!ShouldValidateDmnDefinition(sourceFormat))
            {
                return;
            }

            try
            {
                var detectedVersion = DetectDmnVersion(definitionContent);
                var model = DmnParser.ParseString(definitionContent, detectedVersion);
                _ = DmnDefinitionFactory.CreateDmnDefinition(model);
            }
            catch (Exception ex) when (ex is DmnParserException or InvalidOperationException or ArgumentException or XmlException)
            {
                var targetVersion = string.IsNullOrWhiteSpace(versionTag) ? "未命名版本" : versionTag;
                throw new InvalidOperationException(
                    $"决策 `{decisionKey}` 的版本 `{targetVersion}` 在发布前 DMN 校验失败：{ex.Message}",
                    ex);
            }
        }

        /// <summary>
        /// 判断当前定义是否应按 DMN 执行发布前校验。
        /// </summary>
        /// <param name="sourceFormat">定义来源格式。</param>
        /// <returns>若应执行 DMN 校验则返回真。</returns>
        private static bool ShouldValidateDmnDefinition(string? sourceFormat)
        {
            return string.IsNullOrWhiteSpace(sourceFormat) ||
                string.Equals(sourceFormat, "DMN", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 识别 DMN 定义版本，并兼容 Camunda 1.3 导出扩展。
        /// </summary>
        /// <param name="definitionContent">DMN XML 内容。</param>
        /// <returns>识别出的 DMN 版本。</returns>
        private static DmnParser.DmnVersionEnum DetectDmnVersion(string definitionContent)
        {
            var detectedVersion = DmnParser.DetectVersion(definitionContent);
            if (detectedVersion == DmnParser.DmnVersionEnum.V1_3 &&
                definitionContent.Contains("exporter", StringComparison.OrdinalIgnoreCase) &&
                definitionContent.Contains("amunda", StringComparison.OrdinalIgnoreCase))
            {
                return DmnParser.DmnVersionEnum.V1_3ext;
            }

            return detectedVersion;
        }

        /// <summary>
        /// 将目标版本保存为已发布状态。
        /// </summary>
        /// <param name="sourceVersion">目标版本。</param>
        /// <param name="publishComment">发布说明。</param>
        /// <param name="operatorName">操作人。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>已发布版本记录。</returns>
        private async Task<DecisionVersionRecord> PublishVersionInternalAsync(
            DecisionVersionRecord sourceVersion,
            string? publishComment,
            string? operatorName,
            CancellationToken cancellationToken)
        {
            return await _decisionVersionRepository.SaveAsync(new DecisionVersionRecord
            {
                VersionId = sourceVersion.VersionId,
                DecisionId = sourceVersion.DecisionId,
                DecisionKey = sourceVersion.DecisionKey,
                VersionNumber = sourceVersion.VersionNumber,
                VersionTag = sourceVersion.VersionTag,
                IsDraft = false,
                IsPublished = true,
                DefinitionContent = sourceVersion.DefinitionContent,
                Checksum = sourceVersion.Checksum,
                PublishComment = publishComment,
                PublishedBy = operatorName,
                CreatedBy = sourceVersion.CreatedBy,
                CreatedTime = sourceVersion.CreatedTime
            }, cancellationToken).ConfigureAwait(false);
        }
    }
}
