using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Sys.Workflow.Rules;
using Sys.Workflow.Rules.Models;

namespace Sys.Rules.DmnEngine
{
    /// <summary>
    /// 定义基于文件系统的决策版本仓储实现。
    /// </summary>
    public sealed class FileSystemDecisionVersionRepository : IDecisionVersionRepository
    {
        private readonly IReadOnlyList<string> _definitionSearchDirectories;

        /// <summary>
        /// 初始化文件系统版本仓储。
        /// </summary>
        /// <param name="definitionSearchDirectories">DMN 定义搜索目录。</param>
        public FileSystemDecisionVersionRepository(IEnumerable<string>? definitionSearchDirectories = null)
        {
            _definitionSearchDirectories = NormalizeSearchDirectories(definitionSearchDirectories);
        }

        /// <summary>
        /// 按决策键与版本标签加载版本记录。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="versionTag">版本标签。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>命中的版本记录。</returns>
        public async Task<DecisionVersionRecord?> GetByVersionTagAsync(string decisionKey, string versionTag, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(decisionKey) || string.IsNullOrWhiteSpace(versionTag))
            {
                return null;
            }

            var definitionFile = ResolveTaggedDefinitionFile(decisionKey, versionTag);
            if (definitionFile == null)
            {
                return null;
            }

            return await BuildVersionRecordAsync(decisionKey, definitionFile, isPublished: true, isDraft: false, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// 加载指定决策的最新已发布版本。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>最新已发布版本记录。</returns>
        public async Task<DecisionVersionRecord?> GetLatestPublishedAsync(string decisionKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(decisionKey))
            {
                return null;
            }

            var normalizedDecisionKey = GetNormalizedDecisionKey(decisionKey);
            var latestPublished = EnumeratePublishedDefinitionFiles(normalizedDecisionKey).FirstOrDefault();
            if (latestPublished == null)
            {
                return null;
            }

            return await BuildVersionRecordAsync(decisionKey, latestPublished, isPublished: true, isDraft: false, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// 查询指定决策的全部版本记录。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>版本记录集合。</returns>
        public async Task<IReadOnlyList<DecisionVersionRecord>> QueryByDecisionKeyAsync(string decisionKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(decisionKey))
            {
                return Array.Empty<DecisionVersionRecord>();
            }

            var normalizedDecisionKey = GetNormalizedDecisionKey(decisionKey);
            var versionRecords = new List<DecisionVersionRecord>();

            foreach (var definitionFile in EnumeratePublishedDefinitionFiles(normalizedDecisionKey))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var record = await BuildVersionRecordAsync(decisionKey, definitionFile, isPublished: true, isDraft: false, cancellationToken).ConfigureAwait(false);
                versionRecords.Add(record);
            }

            return versionRecords;
        }

        /// <summary>
        /// 文件系统版本仓储当前仅支持读取，不支持保存。
        /// </summary>
        /// <param name="record">待保存的版本记录。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>不会返回结果，始终抛出异常。</returns>
        public Task<DecisionVersionRecord> SaveAsync(DecisionVersionRecord record, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("FileSystemDecisionVersionRepository 当前仅支持读取，不支持写入版本记录。");
        }

        /// <summary>
        /// 按绑定模式解析运行时应命中的决策版本。
        /// </summary>
        /// <param name="request">版本解析请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>解析得到的版本结果。</returns>
        public async Task<DecisionVersionResolutionResult> ResolveAsync(DecisionVersionResolutionRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.DecisionKey))
            {
                throw new ArgumentException("决策主键不能为空。", nameof(request));
            }

            if (string.Equals(request.BindingMode, "LatestPublished", StringComparison.OrdinalIgnoreCase))
            {
                var latestPublished = await GetLatestPublishedAsync(request.DecisionKey, cancellationToken).ConfigureAwait(false);
                if (latestPublished == null)
                {
                    throw new DmnEngineException($"bindingMode 为 LatestPublished 时，未找到决策主键 `{request.DecisionKey}` 对应的已发布 DMN 定义文件。");
                }

                return CreateResolutionResult(latestPublished, request.DecisionKey);
            }

            if (!string.IsNullOrWhiteSpace(request.VersionTag))
            {
                var taggedVersion = await GetByVersionTagAsync(request.DecisionKey, request.VersionTag, cancellationToken).ConfigureAwait(false);
                if (taggedVersion == null)
                {
                    var modeName = string.IsNullOrWhiteSpace(request.BindingMode) ? "指定版本" : request.BindingMode;
                    throw new DmnEngineException($"bindingMode 为 {modeName} 时，未找到 decisionKey `{request.DecisionKey}` 对应 versionTag `{request.VersionTag}` 的 DMN 定义文件。");
                }

                return CreateResolutionResult(taggedVersion, request.DecisionKey);
            }

            var draftFilePath = ResolveDraftDefinitionFilePath(request.DecisionKey);
            var definitionContent = await File.ReadAllTextAsync(draftFilePath, cancellationToken).ConfigureAwait(false);

            return new DecisionVersionResolutionResult
            {
                DecisionId = request.DecisionKey,
                VersionId = request.DecisionKey,
                DecisionKey = request.DecisionKey,
                VersionTag = null,
                VersionNumber = 0,
                DefinitionContent = definitionContent,
                Checksum = null,
                DefinitionSource = draftFilePath
            };
        }

        /// <summary>
        /// 将版本记录转换为运行时解析结果。
        /// </summary>
        /// <param name="record">版本记录。</param>
        /// <param name="decisionKey">决策键。</param>
        /// <returns>运行时解析结果。</returns>
        private static DecisionVersionResolutionResult CreateResolutionResult(DecisionVersionRecord record, string decisionKey)
        {
            return new DecisionVersionResolutionResult
            {
                DecisionId = record.DecisionId,
                VersionId = record.VersionId,
                DecisionKey = decisionKey,
                VersionTag = record.VersionTag,
                VersionNumber = record.VersionNumber,
                DefinitionContent = record.DefinitionContent,
                Checksum = record.Checksum,
                DefinitionSource = BuildDefinitionSource(decisionKey, record.VersionTag)
            };
        }

        /// <summary>
        /// 构建版本记录。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="definitionFile">定义文件信息。</param>
        /// <param name="isPublished">是否已发布。</param>
        /// <param name="isDraft">是否为草稿。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>版本记录。</returns>
        private static async Task<DecisionVersionRecord> BuildVersionRecordAsync(
            string decisionKey,
            DefinitionFileInfo definitionFile,
            bool isPublished,
            bool isDraft,
            CancellationToken cancellationToken)
        {
            var versionTag = definitionFile.VersionTag
                ?? throw new InvalidOperationException("构建版本记录时要求 VersionTag 不为空。");
            var definitionContent = await File.ReadAllTextAsync(definitionFile.FilePath, cancellationToken).ConfigureAwait(false);
            var normalizedDecisionKey = GetNormalizedDecisionKey(decisionKey);
            var versionNumber = ConvertVersionTagToNumber(versionTag);

            return new DecisionVersionRecord
            {
                VersionId = $"{normalizedDecisionKey}:{versionTag}",
                DecisionId = normalizedDecisionKey,
                DecisionKey = normalizedDecisionKey,
                VersionNumber = versionNumber,
                VersionTag = versionTag,
                IsPublished = isPublished,
                IsDraft = isDraft,
                DefinitionContent = definitionContent,
                Checksum = null,
                CreatedTime = File.GetCreationTimeUtc(definitionFile.FilePath),
                PublishedTime = isPublished ? File.GetLastWriteTimeUtc(definitionFile.FilePath) : null
            };
        }

        /// <summary>
        /// 解析指定版本标签的定义文件。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="versionTag">版本标签。</param>
        /// <returns>定义文件信息。</returns>
        private DefinitionFileInfo? ResolveTaggedDefinitionFile(string decisionKey, string versionTag)
        {
            foreach (var candidatePath in EnumerateVersionTaggedCandidateFilePaths(decisionKey, versionTag))
            {
                if (File.Exists(candidatePath))
                {
                    return new DefinitionFileInfo(Path.GetFullPath(candidatePath), versionTag);
                }
            }

            var normalizedDecisionKey = GetNormalizedDecisionKey(decisionKey);
            return EnumeratePublishedDefinitionFiles(normalizedDecisionKey)
                .FirstOrDefault(file => string.Equals(file.VersionTag, versionTag, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 解析草稿定义文件路径。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <returns>草稿定义文件路径。</returns>
        private string ResolveDraftDefinitionFilePath(string decisionKey)
        {
            foreach (var candidatePath in EnumerateCandidateFilePaths(decisionKey))
            {
                if (File.Exists(candidatePath))
                {
                    return Path.GetFullPath(candidatePath);
                }
            }

            var fileName = Path.GetFileName(decisionKey);
            if (!fileName.EndsWith(".dmn", StringComparison.OrdinalIgnoreCase))
            {
                fileName = $"{fileName}.dmn";
            }

            foreach (var searchDirectory in _definitionSearchDirectories)
            {
                if (!Directory.Exists(searchDirectory))
                {
                    continue;
                }

                var matchedPath = Directory
                    .EnumerateFiles(searchDirectory, fileName, SearchOption.AllDirectories)
                    .FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(matchedPath))
                {
                    return Path.GetFullPath(matchedPath);
                }
            }

            throw new DmnEngineException($"未找到决策主键 `{decisionKey}` 对应的 DMN 定义文件。");
        }

        /// <summary>
        /// 枚举已发布版本定义文件。
        /// </summary>
        /// <param name="normalizedDecisionKey">规范化后的决策键。</param>
        /// <returns>定义文件集合。</returns>
        private IEnumerable<DefinitionFileInfo> EnumeratePublishedDefinitionFiles(string normalizedDecisionKey)
        {
            return EnumerateDefinitionFiles(normalizedDecisionKey)
                .Select(file => new DefinitionFileInfo(
                    Path.GetFullPath(file),
                    TryExtractVersionTagFromFileName(normalizedDecisionKey, file)))
                .Where(candidate => !string.IsNullOrWhiteSpace(candidate.VersionTag))
                .OrderByDescending(candidate => candidate.VersionTag, PublishedVersionTagComparer.Instance)
                .ThenByDescending(candidate => candidate.FilePath, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 枚举可能的定义文件路径。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <returns>候选路径集合。</returns>
        private IEnumerable<string> EnumerateCandidateFilePaths(string decisionKey)
        {
            var candidateNames = new List<string> { decisionKey };
            if (!decisionKey.EndsWith(".dmn", StringComparison.OrdinalIgnoreCase))
            {
                candidateNames.Add($"{decisionKey}.dmn");
            }

            if (Path.IsPathRooted(decisionKey))
            {
                foreach (var candidateName in candidateNames)
                {
                    yield return candidateName;
                }

                yield break;
            }

            foreach (var searchDirectory in _definitionSearchDirectories)
            {
                foreach (var candidateName in candidateNames)
                {
                    yield return Path.Combine(searchDirectory, candidateName);
                }
            }
        }

        /// <summary>
        /// 枚举带版本标签的候选文件路径。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="versionTag">版本标签。</param>
        /// <returns>候选路径集合。</returns>
        private IEnumerable<string> EnumerateVersionTaggedCandidateFilePaths(string decisionKey, string versionTag)
        {
            var normalizedDecisionKey = decisionKey.EndsWith(".dmn", StringComparison.OrdinalIgnoreCase)
                ? decisionKey.Substring(0, decisionKey.Length - 4)
                : decisionKey;
            var candidateNames = new[]
            {
                $"{normalizedDecisionKey}.{versionTag}.dmn",
                $"{normalizedDecisionKey}-{versionTag}.dmn"
            };

            if (Path.IsPathRooted(normalizedDecisionKey))
            {
                foreach (var candidateName in candidateNames)
                {
                    yield return candidateName;
                }

                yield break;
            }

            foreach (var searchDirectory in _definitionSearchDirectories)
            {
                foreach (var candidateName in candidateNames)
                {
                    yield return Path.Combine(searchDirectory, candidateName);
                }
            }
        }

        /// <summary>
        /// 枚举同一决策键下的定义文件。
        /// </summary>
        /// <param name="normalizedDecisionKey">规范化后的决策键。</param>
        /// <returns>定义文件集合。</returns>
        private IEnumerable<string> EnumerateDefinitionFiles(string normalizedDecisionKey)
        {
            var fileName = $"{normalizedDecisionKey}.dmn";
            var baseFileName = Path.GetFileName(normalizedDecisionKey);

            foreach (var searchDirectory in _definitionSearchDirectories)
            {
                if (!Directory.Exists(searchDirectory))
                {
                    continue;
                }

                foreach (var filePath in Directory.EnumerateFiles(searchDirectory, "*.dmn", SearchOption.AllDirectories))
                {
                    var normalizedFilePath = Path.GetFullPath(filePath);
                    if (string.Equals(Path.GetFileName(normalizedFilePath), fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return normalizedFilePath;
                        continue;
                    }

                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(normalizedFilePath);
                    if (fileNameWithoutExtension.StartsWith($"{baseFileName}.", StringComparison.OrdinalIgnoreCase) ||
                        fileNameWithoutExtension.StartsWith($"{baseFileName}-", StringComparison.OrdinalIgnoreCase))
                    {
                        yield return normalizedFilePath;
                    }
                }
            }
        }

        /// <summary>
        /// 从文件名中提取版本标签。
        /// </summary>
        /// <param name="normalizedDecisionKey">规范化后的决策键。</param>
        /// <param name="filePath">定义文件路径。</param>
        /// <returns>提取到的版本标签。</returns>
        private static string? TryExtractVersionTagFromFileName(string normalizedDecisionKey, string filePath)
        {
            var baseFileName = Path.GetFileName(normalizedDecisionKey);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

            if (fileNameWithoutExtension.Length <= baseFileName.Length)
            {
                return null;
            }

            if (fileNameWithoutExtension.StartsWith($"{baseFileName}.", StringComparison.OrdinalIgnoreCase) ||
                fileNameWithoutExtension.StartsWith($"{baseFileName}-", StringComparison.OrdinalIgnoreCase))
            {
                return fileNameWithoutExtension.Substring(baseFileName.Length + 1);
            }

            return null;
        }

        /// <summary>
        /// 规范化决策键。
        /// </summary>
        /// <param name="decisionKey">原始决策键。</param>
        /// <returns>规范化后的决策键。</returns>
        private static string GetNormalizedDecisionKey(string decisionKey)
        {
            var normalizedDecisionKey = decisionKey.Trim();
            return normalizedDecisionKey.EndsWith(".dmn", StringComparison.OrdinalIgnoreCase)
                ? normalizedDecisionKey.Substring(0, normalizedDecisionKey.Length - 4)
                : normalizedDecisionKey;
        }

        /// <summary>
        /// 构建定义来源描述。
        /// </summary>
        /// <param name="decisionKey">决策键。</param>
        /// <param name="versionTag">版本标签。</param>
        /// <returns>定义来源标识。</returns>
        private static string BuildDefinitionSource(string decisionKey, string? versionTag)
        {
            return string.IsNullOrWhiteSpace(versionTag)
                ? $"{GetNormalizedDecisionKey(decisionKey)}.dmn"
                : $"{GetNormalizedDecisionKey(decisionKey)}.{versionTag}.dmn";
        }

        /// <summary>
        /// 将版本标签转换为可排序的数值版本号。
        /// </summary>
        /// <param name="versionTag">版本标签。</param>
        /// <returns>数值版本号。</returns>
        private static long ConvertVersionTagToNumber(string versionTag)
        {
            var normalizedVersion = Regex.Replace(versionTag.Trim(), "^[vV]", string.Empty);
            if (!Version.TryParse(normalizedVersion, out var parsedVersion))
            {
                return 0;
            }

            var major = parsedVersion.Major < 0 ? 0 : parsedVersion.Major;
            var minor = parsedVersion.Minor < 0 ? 0 : parsedVersion.Minor;
            var build = parsedVersion.Build < 0 ? 0 : parsedVersion.Build;
            var revision = parsedVersion.Revision < 0 ? 0 : parsedVersion.Revision;
            return ((long)major * 1_000_000_000L) +
                   ((long)minor * 1_000_000L) +
                   ((long)build * 1_000L) +
                   revision;
        }

        /// <summary>
        /// 规范化搜索目录列表。
        /// </summary>
        /// <param name="definitionSearchDirectories">用户提供的搜索目录。</param>
        /// <returns>规范化后的目录集合。</returns>
        private static IReadOnlyList<string> NormalizeSearchDirectories(IEnumerable<string>? definitionSearchDirectories)
        {
            var directories = new List<string>();

            if (definitionSearchDirectories != null)
            {
                foreach (var directory in definitionSearchDirectories)
                {
                    if (!string.IsNullOrWhiteSpace(directory))
                    {
                        directories.Add(Path.GetFullPath(directory));
                    }
                }
            }

            directories.Add(Path.GetFullPath(AppContext.BaseDirectory));
            directories.Add(Path.GetFullPath(Environment.CurrentDirectory));

            return directories
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// 表示定义文件信息。
        /// </summary>
        private sealed class DefinitionFileInfo
        {
            /// <summary>
            /// 初始化定义文件信息。
            /// </summary>
            /// <param name="filePath">文件路径。</param>
            /// <param name="versionTag">版本标签。</param>
            public DefinitionFileInfo(string filePath, string? versionTag)
            {
                FilePath = filePath;
                VersionTag = versionTag;
            }

            /// <summary>
            /// 文件路径。
            /// </summary>
            public string FilePath { get; }

            /// <summary>
            /// 版本标签。
            /// </summary>
            public string? VersionTag { get; }
        }

        /// <summary>
        /// 提供版本标签比较逻辑。
        /// </summary>
        private sealed class PublishedVersionTagComparer : IComparer<string>
        {
            /// <summary>
            /// 单例实例。
            /// </summary>
            public static PublishedVersionTagComparer Instance { get; } = new PublishedVersionTagComparer();

            /// <summary>
            /// 比较两个版本标签。
            /// </summary>
            /// <param name="x">左值。</param>
            /// <param name="y">右值。</param>
            /// <returns>比较结果。</returns>
            public int Compare(string? x, string? y)
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }

                if (x == null)
                {
                    return -1;
                }

                if (y == null)
                {
                    return 1;
                }

                if (TryParseVersion(x, out var parsedX) &&
                    TryParseVersion(y, out var parsedY))
                {
                    return parsedX.CompareTo(parsedY);
                }

                return StringComparer.OrdinalIgnoreCase.Compare(x, y);
            }

            /// <summary>
            /// 尝试将版本标签转换为 Version 对象。
            /// </summary>
            /// <param name="versionTag">版本标签。</param>
            /// <param name="version">解析得到的版本对象。</param>
            /// <returns>是否解析成功。</returns>
            private static bool TryParseVersion(string versionTag, out Version version)
            {
                var normalizedVersion = Regex.Replace(versionTag.Trim(), "^[vV]", string.Empty);
                return Version.TryParse(normalizedVersion, out version!);
            }
        }
    }
}
