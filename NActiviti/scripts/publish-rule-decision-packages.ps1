[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$InputPath = "artifacts\packages\rules-clean",

    [Parameter(Mandatory = $false)]
    [string]$OutputPath = "artifacts\packages\publish-ready",

    [Parameter(Mandatory = $false)]
    [string]$Source = "",

    [Parameter(Mandatory = $false)]
    [string]$ApiKey = "",

    [Parameter(Mandatory = $false)]
    [switch]$Push,

    [Parameter(Mandatory = $false)]
    [switch]$Clean
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$nactivitiRoot = Split-Path -Parent $scriptRoot
$solutionRoot = Split-Path -Parent $nactivitiRoot
$resolvedInputPath = [System.IO.Path]::GetFullPath((Join-Path $solutionRoot $InputPath))
$resolvedOutputPath = [System.IO.Path]::GetFullPath((Join-Path $solutionRoot $OutputPath))
$manifestPath = Join-Path $resolvedInputPath "rule-decision-packages.manifest.txt"

$packageIds = @(
    "Sys.Bpm.Rules.API",
    "Sys.Bpm.Rules.Core",
    "Sys.Rules.DmnEngine",
    "Sys.Rules.DmnEngine.Adapter"
)

<#
.SYNOPSIS
Reads a required key from the batch package manifest.
#>
function Get-RequiredManifestValue {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Lines,

        [Parameter(Mandatory = $true)]
        [string]$Key
    )

    $matchedLine = $Lines | Where-Object { $_ -like "$Key=*" } | Select-Object -First 1
    if (-not $matchedLine) {
        throw "Manifest key not found: $Key"
    }

    return $matchedLine.Substring($Key.Length + 1)
}

<#
.SYNOPSIS
Validates that a package file exists before publish preparation continues.
#>
function Assert-PackageExists {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path $Path)) {
        throw "Package file not found: $Path"
    }
}

Write-Host "Preparing publish-ready rule decision packages..."
Write-Host "Input: $resolvedInputPath"
Write-Host "Output: $resolvedOutputPath"

if (-not (Test-Path $resolvedInputPath)) {
    throw "Input directory not found: $resolvedInputPath"
}

if (-not (Test-Path $manifestPath)) {
    throw "Manifest file not found: $manifestPath"
}

if ($Clean -and (Test-Path $resolvedOutputPath)) {
    Remove-Item -Path $resolvedOutputPath -Recurse -Force
}

New-Item -ItemType Directory -Path $resolvedOutputPath -Force | Out-Null

$manifestLines = [System.IO.File]::ReadAllLines($manifestPath)
$version = Get-RequiredManifestValue -Lines $manifestLines -Key "version"

$publishManifestLines = @(
    "version=$version",
    "source=$Source",
    "pushEnabled=$Push",
    "preparedAt=$([DateTimeOffset]::Now.ToString('yyyy-MM-dd HH:mm:ss zzz'))",
    "files="
)

foreach ($packageId in $packageIds) {
    foreach ($extension in @("nupkg", "snupkg")) {
        $fileName = "$packageId.$version.$extension"
        $sourceFile = Join-Path $resolvedInputPath $fileName
        Assert-PackageExists -Path $sourceFile
        Copy-Item -Path $sourceFile -Destination (Join-Path $resolvedOutputPath $fileName) -Force
        $publishManifestLines += $fileName
    }
}

$publishManifestPath = Join-Path $resolvedOutputPath "rule-decision-publish.manifest.txt"
[System.IO.File]::WriteAllLines($publishManifestPath, $publishManifestLines)

if ($Push) {
    if ([string]::IsNullOrWhiteSpace($Source)) {
        throw "Package source is required when -Push is specified."
    }

    if ([string]::IsNullOrWhiteSpace($ApiKey)) {
        throw "ApiKey is required when -Push is specified."
    }

    foreach ($packageId in $packageIds) {
        $packageFile = Join-Path $resolvedOutputPath "$packageId.$version.nupkg"
        dotnet nuget push $packageFile --source $Source --api-key $ApiKey --skip-duplicate
        if ($LASTEXITCODE -ne 0) {
            throw "NuGet push failed: $packageFile"
        }
    }
}

Write-Host "Publish-ready package preparation completed."
Write-Host "Manifest: $publishManifestPath"
