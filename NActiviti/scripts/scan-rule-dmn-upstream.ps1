[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$OutputPath = "artifacts\reports\rule-dmn-upstream-sync"
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$nactivitiRoot = Split-Path -Parent $scriptRoot
$solutionRoot = Split-Path -Parent $nactivitiRoot
$resolvedOutputPath = [System.IO.Path]::GetFullPath((Join-Path $solutionRoot $OutputPath))

$upstreamRoot = Join-Path $solutionRoot "_upstream\ScratchyDisk.DmnEngine"
$localRoot = Join-Path $nactivitiRoot "Sys.Rules.DmnEngine"
$provenancePath = Join-Path $localRoot "SOURCE-PROVENANCE.md"
$ledgerPath = Join-Path $localRoot "UPSTREAM-SYNC-LEDGER.md"
$readmePath = Join-Path $upstreamRoot "README.md"
$changelogPath = Join-Path $upstreamRoot "changelog.md"

<#
.SYNOPSIS
Reads a required file and returns its full text content.
#>
function Get-RequiredContent {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path $Path)) {
        throw "Required file not found: $Path"
    }

    return [System.IO.File]::ReadAllText($Path)
}

<#
.SYNOPSIS
Extracts the first regex capture group from the provided text.
#>
function Get-FirstRegexGroup {
    param(
        [Parameter(Mandatory = $true)]
        [string]$InputText,

        [Parameter(Mandatory = $true)]
        [string]$Pattern
    )

    $match = [System.Text.RegularExpressions.Regex]::Match($InputText, $Pattern, [System.Text.RegularExpressions.RegexOptions]::Multiline)
    if (-not $match.Success) {
        return ""
    }

    return $match.Groups[1].Value.Trim()
}

<#
.SYNOPSIS
Finds the first line that starts with a prefix and returns its value.
#>
function Get-LineValue {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string[]]$Lines,

        [Parameter(Mandatory = $true)]
        [string]$Prefix
    )

    foreach ($line in $Lines) {
        if ($line.StartsWith($Prefix)) {
            return $line.Substring($Prefix.Length).Trim()
        }
    }

    return ""
}

<#
.SYNOPSIS
Finds the first non-empty line after a marker line.
#>
function Get-NextNonEmptyLineAfter {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string[]]$Lines,

        [Parameter(Mandatory = $true)]
        [string]$Marker
    )

    for ($index = 0; $index -lt $Lines.Length; $index++) {
        if ($Lines[$index].StartsWith($Marker)) {
            for ($innerIndex = $index + 1; $innerIndex -lt $Lines.Length; $innerIndex++) {
                $candidate = $Lines[$innerIndex].Trim()
                if (-not [string]::IsNullOrWhiteSpace($candidate)) {
                    return $candidate
                }
            }
        }
    }

    return ""
}

<#
.SYNOPSIS
Counts DMN files under a target directory.
#>
function Get-DmnFileCount {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RootPath
    )

    if (-not (Test-Path $RootPath)) {
        return 0
    }

    return @(Get-ChildItem -Path $RootPath -Recurse -File -Filter *.dmn).Count
}

<#
.SYNOPSIS
Counts C# source files under a target directory.
#>
function Get-CsFileCount {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RootPath
    )

    if (-not (Test-Path $RootPath)) {
        return 0
    }

    return @(Get-ChildItem -Path $RootPath -Recurse -File -Filter *.cs).Count
}

Write-Host "Scanning local DMN upstream cache..."
Write-Host "Output: $resolvedOutputPath"

New-Item -ItemType Directory -Path $resolvedOutputPath -Force | Out-Null

$provenanceContent = Get-RequiredContent -Path $provenancePath
$ledgerContent = Get-RequiredContent -Path $ledgerPath
$upstreamReadmeContent = Get-RequiredContent -Path $readmePath
$upstreamChangelogContent = Get-RequiredContent -Path $changelogPath
$provenanceLines = [System.IO.File]::ReadAllLines($provenancePath)
$ledgerLines = [System.IO.File]::ReadAllLines($ledgerPath)
$upstreamReadmeLines = [System.IO.File]::ReadAllLines($readmePath)

$importedCommitLine = Get-NextNonEmptyLineAfter -Lines $provenanceLines -Marker '- First-batch upstream commit:'
$importedCommit = $importedCommitLine.TrimStart('- ').Trim([char]96)
$latestObservedVersion = Get-FirstRegexGroup -InputText $upstreamChangelogContent -Pattern '^## \[([^\]]+)\] - '
$latestObservedDate = Get-FirstRegexGroup -InputText $upstreamChangelogContent -Pattern '^## \[[^\]]+\] - ([0-9]{4}-[0-9]{2}-[0-9]{2})'
$upstreamTargetFramework = Get-LineValue -Lines $upstreamReadmeLines -Prefix '**Target framework:** '
$ledgerStatus = (Get-LineValue -Lines $ledgerLines -Prefix '- Status: ').Trim([char]96)

$upstreamDmnCount = Get-DmnFileCount -RootPath $upstreamRoot
$upstreamCsCount = Get-CsFileCount -RootPath $upstreamRoot
$localVendoredCsCount = Get-CsFileCount -RootPath (Join-Path $localRoot "src\Sys.Rules.DmnEngine\Upstream")

$reportLines = @(
    "importedCommit=$importedCommit",
    "latestObservedVersion=$latestObservedVersion",
    "latestObservedDate=$latestObservedDate",
    "upstreamTargetFramework=$upstreamTargetFramework",
    "ledgerStatus=$ledgerStatus",
    "upstreamDmnFileCount=$upstreamDmnCount",
    "upstreamCsFileCount=$upstreamCsCount",
    "localVendoredCsFileCount=$localVendoredCsCount"
)

$reportPath = Join-Path $resolvedOutputPath "rule-dmn-upstream-sync.report.txt"
[System.IO.File]::WriteAllLines($reportPath, $reportLines)

Write-Host "Upstream scan completed."
Write-Host "Report: $reportPath"
