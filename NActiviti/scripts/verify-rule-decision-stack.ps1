param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    [switch]$SkipTests,

    [switch]$CleanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Write-Step {
    <#
    .SYNOPSIS
    输出统一的阶段日志，便于定位当前执行到的步骤。
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    Write-Host ""
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Invoke-DotNetCommand {
    <#
    .SYNOPSIS
    统一执行 dotnet 命令，并在退出码非 0 时立即失败。
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    $stableArguments = @("-m:1", "-nologo", "-v:q")
    & dotnet @Arguments @stableArguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments -join ' ') 执行失败，退出码: $LASTEXITCODE"
    }
}

function Remove-BuildOutputs {
    <#
    .SYNOPSIS
    清理规则决策主链相关项目的 bin、obj 与根级 artifacts，避免脏输出污染构建。
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot
    )

    $paths = @(
        (Join-Path $RepositoryRoot "artifacts"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Bpm.Engine\bin"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Bpm.Engine\obj"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Bpm.Engine.API\bin"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Bpm.Engine.API\obj"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Bpm.Features\bin"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Bpm.Features\obj"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Bpm.Model\bin"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Bpm.Model\obj"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Bpm.ProcessValidation\bin"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Bpm.ProcessValidation\obj"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Bpm.Rest\bin"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Bpm.Rest\obj"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Bpm.Rest.API\bin"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Bpm.Rest.API\obj"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Bpm.Rest.Client\bin"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Bpm.Rest.Client\obj"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Bpm.Rules.API\bin"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Bpm.Rules.API\obj"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Bpm.Rules.Core\bin"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Bpm.Rules.Core\obj"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Bpm.Share\bin"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Bpm.Share\obj"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Rules.DmnEngine\src\Sys.Rules.DmnEngine\bin"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Rules.DmnEngine\src\Sys.Rules.DmnEngine\obj"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Rules.DmnEngine\src\Sys.Rules.DmnEngine.Adapter\bin"),
        (Join-Path $RepositoryRoot "NActiviti\Sys.Rules.DmnEngine\src\Sys.Rules.DmnEngine.Adapter\obj"),
        (Join-Path $RepositoryRoot "Smart.Sql\SmartSql\bin"),
        (Join-Path $RepositoryRoot "Smart.Sql\SmartSql\obj"),
        (Join-Path $RepositoryRoot "Smart.Sql\SmartSql.DapperDeserializer\bin"),
        (Join-Path $RepositoryRoot "Smart.Sql\SmartSql.DapperDeserializer\obj"),
        (Join-Path $RepositoryRoot "Smart.Sql\SmartSql.DIExtension\bin"),
        (Join-Path $RepositoryRoot "Smart.Sql\SmartSql.DIExtension\obj"),
        (Join-Path $RepositoryRoot "Smart.Sql\SmartSql.DyRepository\bin"),
        (Join-Path $RepositoryRoot "Smart.Sql\SmartSql.DyRepository\obj"),
        (Join-Path $RepositoryRoot "Smart.Sql\SmartSql.TypeHandler\bin"),
        (Join-Path $RepositoryRoot "Smart.Sql\SmartSql.TypeHandler\obj"),
        (Join-Path $RepositoryRoot "Spring.Core\bin"),
        (Join-Path $RepositoryRoot "Spring.Core\obj"),
        (Join-Path $RepositoryRoot "Sys.Expression\bin"),
        (Join-Path $RepositoryRoot "Sys.Expression\obj"),
        (Join-Path $RepositoryRoot "Tests\Sys.Bpm.Engine.Tests\bin"),
        (Join-Path $RepositoryRoot "Tests\Sys.Bpm.Engine.Tests\obj")
    )

    foreach ($path in $paths) {
        if (Test-Path $path) {
            Remove-Item -Path $path -Recurse -Force
        }
    }
}

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$nactivitiRoot = Split-Path -Parent $scriptRoot
$repositoryRoot = Split-Path -Parent $nactivitiRoot

Write-Step "清理规则决策主链构建输出"
Remove-BuildOutputs -RepositoryRoot $repositoryRoot

if ($CleanOnly) {
    Write-Step "仅清理模式已完成"
    exit 0
}

$rulesCoreProject = Join-Path $nactivitiRoot "Sys.Bpm.Rules.Core\Sys.Bpm.Rules.Core.csproj"
$engineProject = Join-Path $nactivitiRoot "Sys.Bpm.Engine\Sys.Bpm.Engine.csproj"
$testsProject = Join-Path $repositoryRoot "Tests\Sys.Bpm.Engine.Tests\Sys.Bpm.Engine.Tests.csproj"

Write-Step "构建 Sys.Bpm.Rules.Core"
Invoke-DotNetCommand -Arguments @("build", $rulesCoreProject, "-c", $Configuration)

Write-Step "构建 Sys.Bpm.Engine"
Invoke-DotNetCommand -Arguments @("build", $engineProject, "-c", $Configuration)

if (-not $SkipTests) {
    Write-Step "执行 Sys.Bpm.Engine.Tests"
    Invoke-DotNetCommand -Arguments @("test", $testsProject, "-c", $Configuration)
}

Write-Step "规则决策主链验证完成"
