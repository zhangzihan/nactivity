[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$Version = "1.0.1.0",

    [Parameter(Mandatory = $false)]
    [string]$OutputPath = "artifacts\packages\rules",

    [Parameter(Mandatory = $false)]
    [switch]$Clean
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$nactivitiRoot = Split-Path -Parent $scriptRoot
$solutionRoot = Split-Path -Parent $nactivitiRoot
$resolvedOutputPath = [System.IO.Path]::GetFullPath((Join-Path $solutionRoot $OutputPath))
$stagingRoot = Join-Path $resolvedOutputPath "_staging"

<#
.SYNOPSIS
获取指定项目在暂存目录中实际生成的 NuGet 包文件。

.DESCRIPTION
NuGet 在生成包文件名时会对版本号做规范化处理，例如将 1.0.1.0 输出为 1.0.1。
这里不再依赖调用方传入的原始版本字符串拼接文件名，而是直接读取当前项目暂存目录里的实际产物。
#>
function Get-PackedPackageFiles {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProjectStagePath,

        [Parameter(Mandatory = $true)]
        [string]$PackageId
    )

    return Get-ChildItem -Path $ProjectStagePath -File | Where-Object {
        $_.Name -like "$PackageId.*.nupkg" -or $_.Name -like "$PackageId.*.snupkg"
    }
}

<#
.SYNOPSIS
校验指定项目的打包产物是否完整且唯一。

.DESCRIPTION
要求每个规则决策组件在当前暂存目录中必须只生成一份 .nupkg 和一份 .snupkg。
如果 dotnet pack 已执行成功但产物不完整、重复或命名异常，将抛出明确的产物校验异常，避免误报为 pack 失败。
#>
function Assert-PackedPackageFiles {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProjectPath,

        [Parameter(Mandatory = $true)]
        [string]$ProjectStagePath,

        [Parameter(Mandatory = $true)]
        [string]$PackageId
    )

    $packageFiles = @(Get-PackedPackageFiles -ProjectStagePath $ProjectStagePath -PackageId $PackageId)
    $nupkgFiles = @($packageFiles | Where-Object { $_.Extension -eq ".nupkg" })
    $snupkgFiles = @($packageFiles | Where-Object { $_.Extension -eq ".snupkg" })

    if ($nupkgFiles.Count -ne 1 -or $snupkgFiles.Count -ne 1) {
        $foundFiles = @($packageFiles | Sort-Object Name | ForEach-Object { $_.Name })
        $foundDescription = if ($foundFiles.Count -gt 0) {
            $foundFiles -join ", "
        }
        else {
            "<none>"
        }

        throw "Package artifact validation failed after successful dotnet pack: $ProjectPath. Expected exactly one .nupkg and one .snupkg for package '$PackageId'. Found: $foundDescription"
    }

    return @($nupkgFiles + $snupkgFiles)
}

$projects = @(
    @{ Project = "Sys.Bpm.Rules.API\Sys.Bpm.Rules.API.csproj"; PackageId = "Sys.Bpm.Rules.API" },
    @{ Project = "Sys.Bpm.Rules.Core\Sys.Bpm.Rules.Core.csproj"; PackageId = "Sys.Bpm.Rules.Core" },
    @{ Project = "Sys.Bpm.Rules.Hosting\Sys.Bpm.Rules.Hosting.csproj"; PackageId = "Sys.Bpm.Rules.Hosting" },
    @{ Project = "Sys.Rules.DmnEngine\src\Sys.Rules.DmnEngine\Sys.Rules.DmnEngine.csproj"; PackageId = "Sys.Rules.DmnEngine" },
    @{ Project = "Sys.Rules.DmnEngine\src\Sys.Rules.DmnEngine.Adapter\Sys.Rules.DmnEngine.Adapter.csproj"; PackageId = "Sys.Rules.DmnEngine.Adapter" }
)

Write-Host "Start packing rule decision packages..."
Write-Host "Version: $Version"
Write-Host "Output: $resolvedOutputPath"

if ($Clean -and (Test-Path $resolvedOutputPath)) {
    Remove-Item -Path $resolvedOutputPath -Recurse -Force
}

New-Item -ItemType Directory -Path $resolvedOutputPath -Force | Out-Null
New-Item -ItemType Directory -Path $stagingRoot -Force | Out-Null

foreach ($project in $projects) {
    $projectPath = Join-Path $nactivitiRoot $project.Project
    if (-not (Test-Path $projectPath)) {
        throw "Project file not found: $projectPath"
    }

    $projectStagePath = Join-Path $stagingRoot $project.PackageId
    if (Test-Path $projectStagePath) {
        Remove-Item -Path $projectStagePath -Recurse -Force
    }
    New-Item -ItemType Directory -Path $projectStagePath -Force | Out-Null

    Write-Host "Packing $($project.Project) ..."
    dotnet pack $projectPath `
        -c Release `
        -o $projectStagePath `
        /p:NactivitySkipPostBuildPack=true `
        /p:RuleDecisionPackageVersion=$Version `
        /p:Version=$Version

    $packExitCode = $LASTEXITCODE
    if ($packExitCode -ne 0) {
        throw "dotnet pack failed for $($project.Project) with exit code $packExitCode"
    }

    $packageFiles = @(Assert-PackedPackageFiles -ProjectPath $project.Project -ProjectStagePath $projectStagePath -PackageId $project.PackageId)

    foreach ($packageFile in $packageFiles) {
        Move-Item -Path $packageFile.FullName -Destination (Join-Path $resolvedOutputPath $packageFile.Name) -Force
    }
}

Remove-Item -Path $stagingRoot -Recurse -Force

$manifestPath = Join-Path $resolvedOutputPath "rule-decision-packages.manifest.txt"
$manifestLines = @(
    "version=$Version",
    "output=$resolvedOutputPath",
    "generatedAt=$([DateTimeOffset]::Now.ToString('yyyy-MM-dd HH:mm:ss zzz'))",
    "files="
)
$manifestLines += (Get-ChildItem -Path $resolvedOutputPath -File | Sort-Object Name | ForEach-Object { $_.Name })
[System.IO.File]::WriteAllLines($manifestPath, $manifestLines)

Write-Host "Rule decision package batch completed."
