[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$BaseUrl,

    [Parameter(Mandatory = $false)]
    [string]$ExpectedDatabaseType,

    [Parameter(Mandatory = $false)]
    [string]$ExpectedStatus,

    [Parameter(Mandatory = $false)]
    [switch]$AllowSkipped
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Write-Step {
    <#
    .SYNOPSIS
    输出统一的联调阶段日志。
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    Write-Host ""
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Normalize-ExpectedValue {
    <#
    .SYNOPSIS
    统一归一化需要比较的字符串值。
    #>
    param(
        [string]$Value
    )

    return ($Value ?? "").Trim().ToLowerInvariant()
}

$normalizedBaseUrl = $BaseUrl.TrimEnd("/")
$requestUrl = "$normalizedBaseUrl/Api/v1/workflow/process-deployer/schema-status"

Write-Step "请求 schema-status 接口"
$response = Invoke-RestMethod -Uri $requestUrl -Method Get

if ($null -eq $response) {
    throw "schema-status 接口未返回任何内容。"
}

$databaseType = [string]$response.databaseType
$status = [string]$response.status

Write-Step "接口返回摘要"
Write-Host ("databaseType       : {0}" -f $databaseType)
Write-Host ("status             : {0}" -f $status)
Write-Host ("expectedVersion    : {0}" -f ([string]$response.expectedSchemaVersion))
Write-Host ("actualVersion      : {0}" -f ([string]$response.actualSchemaVersion))
Write-Host ("decisionChecked    : {0}" -f ([string]$response.decisionSchemaChecked))
Write-Host ("healthy            : {0}" -f ([string]$response.decisionSchemaHealthy))
Write-Host ("upgradeRequired    : {0}" -f ([string]$response.upgradeRequired))
Write-Host ("missingTables      : {0}" -f ((@($response.missingTables) -join ", ")))
Write-Host ("recommendedAction  : {0}" -f ([string]$response.recommendedAction))

if (-not $AllowSkipped -and (Normalize-ExpectedValue $status) -eq "skipped") {
    throw "当前 schema-status 为 Skipped，但本次验证未允许跳过。请检查宿主数据库类型与规则仓储配置。"
}

if (-not [string]::IsNullOrWhiteSpace($ExpectedDatabaseType)) {
    $expectedDatabaseTypeNormalized = Normalize-ExpectedValue $ExpectedDatabaseType
    $actualDatabaseTypeNormalized = Normalize-ExpectedValue $databaseType
    if ($expectedDatabaseTypeNormalized -ne $actualDatabaseTypeNormalized) {
        throw "数据库类型不匹配。期望: $ExpectedDatabaseType，实际: $databaseType"
    }
}

if (-not [string]::IsNullOrWhiteSpace($ExpectedStatus)) {
    $expectedStatusNormalized = Normalize-ExpectedValue $ExpectedStatus
    $actualStatusNormalized = Normalize-ExpectedValue $status
    if ($expectedStatusNormalized -ne $actualStatusNormalized) {
        throw "schema-status 不匹配。期望: $ExpectedStatus，实际: $status"
    }
}

Write-Step "schema-status smoke 验证通过"
