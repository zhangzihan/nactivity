# AGENTS.md

本文件面向 AI 编码代理，介绍本仓库的结构、构建方式与开发约定。阅读本文件前不需要任何项目背景知识。

## 项目概述

本项目（**nactivity**）是 Java 工作流引擎 [Activiti](https://github.com/Activiti/Activiti) 的 C# / .NET 移植版，并在此基础上扩展了 DMN 规则决策引擎。主要能力：

- BPMN 2.0 流程引擎（运行时、历史、仓储、任务、管理服务）
- ASP.NET Core REST API 层与 REST 客户端
- DMN 决策规则引擎（规则定义仓储、版本管理、执行日志、治理接口）
- Spring.NET 表达式语言移植（基于 ANTLR 4.9.3 生成解析器）

- 解决方案文件：`NActiviti.sln`
- Git 远端：`https://github.com/zhangzihan/nactivity.git`
- 项目文档与代码注释以**中文**为主，提交说明、文档请沿用中文

## 技术栈

| 类别 | 技术 |
|------|------|
| 语言 / 运行时 | C#（LangVersion 13.0），目标框架主要为 **.NET 6**（`net6.0`） |
| Web | ASP.NET Core 6（`Microsoft.AspNetCore.App` 框架引用） |
| 数据访问 | SmartSql（仓库内置源码，见 `Smart.Sql/`），SQL 映射为 XML |
| 数据库 | SQL Server（`Microsoft.Data.SqlClient`）、PostgreSQL（`Npgsql`）、MySQL（`MySqlConnector`） |
| 表达式语言 | ANTLR 4.9.3（语法文件 `Sys.Expression/Expressions/Parser/Expression.g4`） |
| 脚本 | CS-Script / Roslyn（`Microsoft.CodeAnalysis.CSharp.Scripting`） |
| 配置 | `appsettings.json` + `resources/activiti.cfg.json`，支持携程 Apollo |
| 日志 | Serilog |
| 测试 | xunit 2.9.x、Xunit.Extensions.Ordering、Moq、Microsoft.AspNetCore.TestHost |

注意：

- `Sys.Expression` 多目标 `netstandard2.1;net4.8`，是唯一需要 .NET Framework 4.8 参考程序集的项目。
- `Samples/Workflow.Client` 仍是 `netcoreapp2.2`，属于历史示例代码。
- 根目录 `gloable.json`（注意拼写）未锁定 SDK 版本，安装 .NET 6 SDK 即可构建。
- 根目录 `Directory.build.props` 配置了**强名称签名**，密钥文件指向仓库外部路径 `D:\Project\MDD\9.0\src\QhSoft.snk`；在其他机器上构建时若缺少该文件需调整 `SignAssembly` 或提供密钥。

## 仓库结构与模块划分

```
NActiviti.sln                  解决方案（包含全部项目）
├── NActiviti/                 工作流与规则决策主体代码
│   ├── Sys.Bpm.Engine         流程引擎核心（RootNamespace: Sys.Workflow）
│   ├── Sys.Bpm.Engine.API     引擎对外 API 接口（Sys.Workflow）
│   ├── Sys.Bpm.Model          BPMN 2.0 模型（XML 解析/序列化）
│   ├── Sys.Bpm.Rest           ASP.NET Core REST 层（控制器、DI 扩展）
│   ├── Sys.Bpm.Rest.API       REST 契约/DTO
│   ├── Sys.Bpm.Rest.Client    REST 客户端
│   ├── Sys.Bpm.Features       特性扩展
│   ├── Sys.Bpm.Share          共享组件
│   ├── Sys.Bpm.ProcessValidation 流程校验
│   ├── Sys.Bpm.Rules.API      规则决策运行时契约与 DTO（Sys.Workflow.Rules）
│   ├── Sys.Bpm.Rules.Core     规则定义/版本/执行日志仓储实现
│   ├── Sys.Bpm.Rules.Hosting  规则宿主装配（默认 DI 入口、部署期绑定校验接线）
│   ├── Sys.Rules.DmnEngine    DMN 执行内核（Fork 自 ScratchyDisk.DmnEngine）
│   │   └── src/Sys.Rules.DmnEngine.Adapter  DMN 内核与运行时契约的适配层
│   ├── BpmnWebTest            Web 测试宿主（当前仅余 bin/obj 构建产物，
│   │                          源码不在仓库中；Dockerfile 发布的 BpmnWebApiTest.dll 对应它）
│   ├── scripts/               规则决策相关 PowerShell 脚本（见下文）
│   └── 规则决策组件包说明.md   规则决策五个包的职责与发布约束
├── Sys.Expression/            Spring.NET 表达式语言移植（含 ANTLR 生成代码）
├── Smart.Sql/                 SmartSql ORM 内置源码（SmartSql、DapperDeserializer、
│                              DIExtension、DyRepository、TypeHandler）
├── Tests/                     测试项目（见「测试」一节）
├── Samples/Workflow.Client    请假单示例客户端（netcoreapp2.2，历史代码）
├── BuildScripts/              NuGet 打包脚本与 nuspec
├── docs/                      设计分析、数据库升级脚本、配置样例
├── _upstream/                 上游参考代码（ScratchyDisk.DmnEngine 原版，只读参考，勿改）
└── tools/                     antlr-4.9.3-complete.jar
```

引擎内部目录延续 Activiti（Java）的包结构：`Engine/impl/`（持久化 `persistence`、配置 `cfg`、命令 `cmd`、EL、脚本、作业 `job` 等）、`resources/`（SQL 与 SmartSql 映射）。熟悉 Activiti 源码即可按 Java 包名定位对应 C# 代码。

## 构建与测试

### 构建

```bash
dotnet build NActiviti.sln            # 全量构建（默认 Debug）
dotnet build -c Release NActiviti.sln # Release 构建
```

### 测试

测试项目全部位于 `Tests/`，均为 `net6.0` + xunit：

- `Sys.Bpm.Engine.Tests` —— 引擎集成测试（**依赖数据库**：`appsettings.json` 默认连接本机 SQL Server 的 `DEVDB` 库，含 `resources/activiti.cfg.json` 与 BPMN 样例）
- `Sys.Bpm.Model.Tests` —— BPMN 模型测试
- `Sys.Bpm.Client.Tests` —— REST 客户端测试
- `Sys.Expression.Test` —— 表达式语言测试
- `Sys.Rules.DmnEngine.Tests` / `Sys.Rules.DmnEngine.BusinessRuleTaskTests` —— DMN 内核与业务规则任务测试

```bash
dotnet test Tests/Sys.Bpm.Engine.Tests/Sys.Bpm.Engine.Tests.csproj   # 单个测试项目
```

注意：`Sys.Bpm.Engine.Tests` 是集成测试，运行前需按 readme 初始化数据库（创建库、配置 `databaseSchemaUpdate` 建表）。没有数据库环境时优先运行不依赖数据库的测试项目。

### 规则决策主链一键验证

```powershell
powershell -ExecutionPolicy Bypass -File .\NActiviti\scripts\verify-rule-decision-stack.ps1
# 可选参数：-SkipTests 只构建；-CleanOnly 只清理 bin/obj/artifacts
```

该脚本依次清理并构建 `Sys.Bpm.Rules.Core`、`Sys.Bpm.Engine`，然后执行 `Sys.Bpm.Engine.Tests`，是改动规则决策相关代码后的标准验证手段。

### schema-status 冒烟脚本

```powershell
powershell -ExecutionPolicy Bypass -File .\NActiviti\scripts\test-decision-schema-status.ps1 `
  -BaseUrl http://localhost:5000 -ExpectedDatabaseType postgres -ExpectedStatus Healthy
```

对运行中的宿主调用 `GET Api/v1/workflow/process-deployer/schema-status` 校验规则决策表结构状态。

## 代码风格与约定

- 命名空间与 Java 原版对应：引擎核心为 `Sys.Workflow.*`，REST 层为 `Sys.Bpm.*`，规则决策为 `Sys.Workflow.Rules.*`。
- 大量 `.cs` 文件是从 Java 逐行移植的，命名、结构尽量保持与上游 Activiti 一致，便于对照排查；新增代码请跟随所在文件的既有风格（注释多为中文）。
- `Sys.Bpm.Engine.csproj` 中存在大量 `<Compile Remove>` + `<None Include>` 对：这些文件（JTA、多租户、流程图绘制等 Java 专属能力）**保留在目录中但不参与编译**，新增文件时注意不要被这些排除规则误伤，也不要随意把它们加回编译。
- 引擎的 SQL 与 SmartSql 映射在 `NActiviti/Sys.Bpm.Engine/resources/` 下（`db/create`、`db/drop`、`db/upgrade`、`db/mapping`），修改表结构必须同步更新 create/drop/upgrade 脚本及 `docs/` 下的手工升级脚本。
- 引擎代码中的 `resources/**` 以 `CopyToOutputDirectory=PreserveNewest` 复制到输出目录，宿主通过 `ConfigFile: activiti.cfg.json` 加载引擎配置。
- 规则决策五个包（`Sys.Bpm.Rules.API`、`Sys.Bpm.Rules.Core`、`Sys.Bpm.Rules.Hosting`、`Sys.Rules.DmnEngine`、`Sys.Rules.DmnEngine.Adapter`）必须**同版本批次发布**，详见 `NActiviti/规则决策组件包说明.md`。
- `_upstream/` 是上游原版参考代码，不要修改；对照 DMN 上游可用 `NActiviti/scripts/scan-rule-dmn-upstream.ps1`。

## ANTLR 表达式语法代码生成

修改 `Sys.Expression/Expressions/Parser/Expression.g4` 后需重新生成解析器（需 JDK 8+）：

```bash
cd Sys.Expression/Expressions/Parser/Antlr4
java -jar "../../../../tools/antlr-4.9.3-complete.jar" -Dlanguage=CSharp \
  -package Spring.Expressions.Parser.Antlr4Gen -no-listener -visitor "../Expression.g4"
```

生成代码引用的 `RuleVersionAttribute` 由 `Sys.Expression/Expressions/Parser/RuleVersionAttributeCompat.cs` 提供（`Antlr4.Runtime.Standard` 包中无此类型），不要删除。

## 数据库与配置

- 支持 SQL Server、PostgreSQL、MySQL；通过 `appsettings.json` 的 `WorkflowDataSource.providerName` 与 `resources/db/mapping/mappings.xml` 的 `DbProvider` 切换。
- 当前流程引擎 schema 版本为 **7.0.0.1**。`databaseSchemaUpdate=false` 的宿主需手工执行升级脚本：PostgreSQL 用 `docs/PostgreSQL-规则决策表升级脚本-7000-to-7001.sql`，SQL Server 用 `docs/SqlServer-规则决策表升级脚本-7000-to-7001.sql`。
- 规则决策仓储由 `DecisionRulesStore` 配置启用；未配置但 `WorkflowDataSource` 为 PostgreSQL/SQL Server 时会自动复用其连接串；否则回退为文件系统 DMN 解析模式。详见 `readme.md`。
- `activiti.cfg.json` 的 `databaseSchemaUpdate`：`true`=自动升级；`drop-create` 仅限开发；`create` 仅用于空库首次初始化。

## 打包与发布

- 规则决策批次打包（统一版本入口 `NActiviti/Directory.Build.props` 的 `RuleDecisionPackageVersion`）：

```powershell
.\NActiviti\scripts\pack-rule-decision-packages.ps1 -Version 1.0.1 -OutputPath artifacts\packages\rules -Clean
.\NActiviti\scripts\publish-rule-decision-packages.ps1 -InputPath artifacts\packages\rules -OutputPath artifacts\packages\publish-ready -Clean
```

产物默认输出到 `artifacts/packages/rules`，并生成 `rule-decision-packages.manifest.txt`。

- 旧版 NuGet 打包：`BuildScripts/NuGetPack.bat`（依赖 `.nuget/NuGet.exe` 与两个 nuspec，输出到上级目录 `..\packages`）。
- Docker：`publish_docker.bat` 执行 `dotnet publish -c Release -r linux-x64 ./BpmnWebTest` 后构建镜像 `apiworkflow`（端口 5005）。注意根目录 `Dockerfile` 基础镜像仍是 `microsoft/dotnet:3.1-aspnetcore-runtime` 且入口为 `BpmnWebApiTest.dll`，与当前 .NET 6 代码已不完全同步，使用前需核实。
- CI：`azure-pipelines.yml` 仅执行一次 `dotnet build`（`continueOnError: true`），不能作为质量门禁，改动后请本地跑测试验证。

## 安全注意事项

- 强名称密钥 `QhSoft.snk` 位于仓库外（`D:\Project\MDD\9.0\src\`），不要把密钥复制进仓库或提交。
- `appsettings.json` / `activiti.cfg.json` 含数据库连接串，提交前确认不包含真实生产凭据。
- REST 层使用 JWT（`System.IdentityModel.Tokens.Jwt`）做认证；规则治理写接口的 `operator` 字段会由 `Authentication.AuthenticatedUser.Id` 自动回填，不要绕过该机制。
- `databaseSchemaUpdate=drop-create` 会删除并重建全部表，严禁在生产配置中启用。

## 文档导航

- `readme.md` —— 数据库配置、规则决策仓储配置、表结构初始化、ANTLR 生成说明
- `docs/文档索引.md` —— 设计分析文档入口
- `docs/analysis/` —— 功能差异分析、规则决策治理接口示例、联调回归清单等
- `docs/samples/` —— PostgreSQL / SQL Server 宿主配置样例与校验 SQL
- `NActiviti/规则决策组件包说明.md` —— 规则决策包的职责与发布约束
