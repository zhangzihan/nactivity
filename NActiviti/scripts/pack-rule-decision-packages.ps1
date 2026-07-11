[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$Version = "1.0.1-local",

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

$projects = @(
    @{ Project = "Sys.Bpm.Rules.API\Sys.Bpm.Rules.API.csproj"; PackageId = "Sys.Bpm.Rules.API" },
    @{ Project = "Sys.Bpm.Rules.Core\Sys.Bpm.Rules.Core.csproj"; PackageId = "Sys.Bpm.Rules.Core" },
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
        /p:RuleDecisionPackageVersion=$Version `
        /p:Version=$Version

    if ($LASTEXITCODE -ne 0) {
        throw "Pack failed: $($project.Project)"
    }

    $packageFiles = @(
        "$($project.PackageId).$Version.nupkg",
        "$($project.PackageId).$Version.snupkg"
    )

    foreach ($packageFile in $packageFiles) {
        $sourceFile = Join-Path $projectStagePath $packageFile
        if (Test-Path $sourceFile) {
            Move-Item -Path $sourceFile -Destination (Join-Path $resolvedOutputPath $packageFile) -Force
        }
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
