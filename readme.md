* 翻译自java activiti https://github.com/Activiti/Activiti

## ANTLR 4 表达式语法代码生成

项目使用 ANTLR 4.9.3 解析 Spring.NET 表达式语言，语法文件和生成工具位于：

| 文件 | 路径 |
|------|------|
| 语法文件 | `Sys.Expression/Expressions/Parser/Expression.g4` |
| ANTLR 工具 | `tools/antlr-4.9.3-complete.jar` |
| 生成代码输出 | `Sys.Expression/Expressions/Parser/Antlr4/` |

### 修改语法后重新生成代码

1. 编辑 `Sys.Expression/Expressions/Parser/Expression.g4`
2. 在 `Sys.Expression/Expressions/Parser/Antlr4/` 目录下执行以下命令：

```bash
java -jar "../../../../tools/antlr-4.9.3-complete.jar" -Dlanguage=CSharp -package Spring.Expressions.Parser.Antlr4Gen -no-listener -visitor "../Expression.g4"
```

> 也可使用绝对路径：
> ```bash
> java -jar "d:/Project/MDD/9.0/nactivity/tools/antlr-4.9.3-complete.jar" -Dlanguage=CSharp -package Spring.Expressions.Parser.Antlr4Gen -no-listener -visitor "d:/Project/MDD/9.0/nactivity/Sys.Expression/Expressions/Parser/Expression.g4"
> ```

3. 重新编译 `Sys.Expression` 项目验证无错误

### 命令参数说明

| 参数 | 说明 |
|------|------|
| `-Dlanguage=CSharp` | 生成 C# 代码 |
| `-package Spring.Expressions.Parser.Antlr4Gen` | 生成代码的命名空间 |
| `-no-listener` | 不生成 ParseTreeListener（项目使用 Visitor 模式） |
| `-visitor` | 生成 ParseTreeVisitor（`SpringExpressionVisitor` 继承自 `ExpressionBaseVisitor`） |

### 生成的文件

执行后会在输出目录生成以下文件：

| 文件 | 说明 |
|------|------|
| `ExpressionLexer.cs` | 词法分析器 |
| `ExpressionParser.cs` | 语法分析器 |
| `ExpressionBaseVisitor.cs` | Visitor 基类（`SpringExpressionVisitor` 继承此类） |
| `ExpressionVisitor.cs` | Visitor 接口 |
| `Expression.tokens` | Token 定义 |
| `ExpressionLexer.tokens` | Lexer Token 定义 |
| `Expression.interp` | 解析器解释数据 |
| `ExpressionLexer.interp` | 词法器解释数据 |

> **注意**：生成代码中引用了 `RuleVersionAttribute`，该类型不在 `Antlr4.Runtime.Standard` 包中，由 `Sys.Expression/Expressions/Parser/RuleVersionAttributeCompat.cs` 提供。

### 前置条件

- 安装 JDK 8+（`java` 命令可用）

## 工作流系统数据库配置
* MySql创建数据库schema activiti
* 修改数据库配置 appsettings.json
```
"WorkflowDataSource"：{
    "providerName": "MySql",
    "database": "activiti",　//数据库名称
    "connectionString": "server=localhost;database=数据库名称;uid=用户名;pwd=密码;Character Set=utf8"
}
```
* 使用MySql DbProvider resources\db\mapping\mappings.xml
```
<DbProvider Name="MySqlClientFactory" ParameterPrefix="?" Type="MySql.Data.MySqlClient.MySqlClientFactory,MySqlConnector"/>
```

* SQLServer 创建数据库 activiti
* 修改数据库配置 appsettings.json
```
"WorkflowDataSource"：{
    "providerName": "Microsoft.Data.SqlClient",
    "database": "activiti",　//数据库名称
    "connectionString": "server=localhost;database=数据库名称;uid=用户名;pwd=密码;"
}
```
* 使用SQLServer DbProvider resources\db\mapping\mappings.xml
```
<DbProvider Name="SqlClientFactory" ParameterPrefix="@" Type="System.Data.SqlClient.SqlClientFactory,System.Data.SqlClient"/>
```

## 决策规则仓储配置
 * `AddProcessEngine(configuration)` 现已内建 DMN 决策运行时装配。
 * 宿主优先通过依赖注入接入 `IDecisionExecutionService`、`IDecisionGovernanceService` 与 `IDecisionCatalogService`。
* 若配置了 `DecisionRulesStore`，则自动按数据库类型启用规则定义仓储、版本仓储与执行日志仓储。
* 若未配置 `DecisionRulesStore`，但 `WorkflowDataSource` 已明确为 PostgreSQL 或 SQL Server，则自动复用工作流数据源连接串启用规则仓储。
* 若以上条件都不满足，则回退为文件系统版 DMN 定义解析模式。

### 显式配置 DecisionRulesStore

```json
{
  "WorkflowDataSource": {
    "providerName": "Npgsql",
    "database": "activiti",
    "connectionString": "Host=localhost;Database=activiti;Username=postgres;Password=postgres"
  },
  "DecisionRulesStore": {
    "connectionString": "Host=localhost;Database=rulesdb;Username=postgres;Password=postgres",
    "configPath": "resources/db/mapping/postgres/rules.mappings.xml",
    "isWatchConfigFile": false
  }
}
```

### 复用 WorkflowDataSource 的 PostgreSQL 连接串

```json
{
  "WorkflowDataSource": {
    "providerName": "Npgsql",
    "database": "activiti",
    "connectionString": "Host=localhost;Database=activiti;Username=postgres;Password=postgres"
  }
}
```

### 显式配置 SQL Server 版 DecisionRulesStore

```json
{
  "WorkflowDataSource": {
    "providerName": "Microsoft.Data.SqlClient",
    "database": "activiti",
    "connectionString": "Server=localhost;Database=activiti;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;"
  },
  "DecisionRulesStore": {
    "databaseType": "mssql",
    "connectionString": "Server=localhost;Database=rulesdb;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;",
    "configPath": "resources/db/mapping/mssql/rules.mappings.xml",
    "isWatchConfigFile": false
  }
}
```

### 复用 WorkflowDataSource 的 SQL Server 连接串

```json
{
  "WorkflowDataSource": {
    "providerName": "Microsoft.Data.SqlClient",
    "database": "activiti",
    "connectionString": "Server=localhost;Database=activiti;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;"
  }
}
```

* `DecisionRulesStore:configPath` 为空时会按数据库类型自动选择默认值：PostgreSQL 使用 `resources/db/mapping/postgres/rules.mappings.xml`，SQL Server 使用 `resources/db/mapping/mssql/rules.mappings.xml`。
* `DecisionRulesStore:databaseType` 建议显式填写为 `postgres` 或 `mssql`；若为空，宿主会根据 `WorkflowDataSource` 自动推断。
* 若 `configPath` 使用相对路径，将按宿主程序 `AppDomain.CurrentDomain.BaseDirectory` 解析。
* `rules.mappings.xml` 默认会装载：
  * `decision_definition`
  * `decision_version`
  * `decision_execution_log`
  三类 SmartSql 映射。
* 规则治理与审计接口示例见：
  * [规则决策治理与审计接口示例-20260707](./docs/analysis/规则决策治理与审计接口示例-20260707.md)
* 当前规则治理写接口支持 `operator` 自动回填：
  * 当 `drafts/publish/rollback` 请求体中的 `operator` 为空时，控制器会优先使用 `Authentication.AuthenticatedUser.Id`
  * 宿主已接入 `SecurityPoliciesApplicationMiddle` 时，通常无需前端重复传入 `operator`
* `execution-logs/query` 当前返回分页结构：
  * `recordCount`
  * `pageNo`
  * `pageSize`
  * `totalCount`
  * `list`

## 初始化数据库表结构
* 首次启动修改配置文件：resources\activiti.cfg.json 配置
  * databaseSchemaUpdate配置说明: 
    *  update：true 当数据库表结构发生变化时设置为true，系统会自动更新到新的表结构
    *  drop-create：系统会先删除表再创建表，该配置仅在开发时可用，nuget包引用该功能不可用
    *  create: 该配置仅在数据库为空时创建表结构，否则系统会无法启动，首次启动后请修改该配置为false，否则再次启动服务将无法启动。
  * 当前流程引擎 schema 版本链已提升到 `7.0.0.1`；若宿主仍使用 `databaseSchemaUpdate=false`，需按数据库类型手工执行对应升级脚本：PostgreSQL 使用 `docs\PostgreSQL-规则决策表升级脚本-7000-to-7001.sql`，SQL Server 使用 `docs\SqlServer-规则决策表升级脚本-7000-to-7001.sql`
  * 当宿主运行在 PostgreSQL 或 SQL Server 且仍未完成 `7000 -> 7001` 升级时，引擎会在启动日志中输出显式告警；该告警仅用于暴露缺失升级，不会替代手工执行脚本
*  
  ```
  {
    "id": "processEngineConfiguration",
    "type": "Sys.Workflow.Engine.Impl.Cfg.StandaloneProcessEngineConfiguration",
    //web主机应用程序名称
    "applicationName": "workflow",
    //数据库更新策略，true=update drop-create create
    "databaseSchemaUpdate": "数据库更新策略",
    ...
  ```

* 手工创建数据库表：
  * 创建表：
    * 流程运行时表结构： resources\db\create\activiti.mysql.create.engine.sql  
    * 流程历史记录表结构：resources\db\create\activiti.mysql.create.history.sql
  * 删除表
    * 流程运行时表结构： resources\db\drop\activiti.mysql.drop.engine.sql  
    * 流程历史记录表结构：resources\db\drop\activiti.mysql.drop.history.sql
* PostgreSQL 规则决策扩展表升级：
  * 手工脚本： `docs\PostgreSQL-规则决策表升级脚本-7000-to-7001.sql`
  * 升级说明： `docs\PostgreSQL-规则决策表升级说明-7000-to-7001.md`
  * 引擎内置 upgrade step： `NActiviti\Sys.Bpm.Engine\resources\db\upgrade\activiti.postgres.upgradestep.7000.to.7001.engine.sql`
* SQL Server 规则决策扩展表升级：
  * 手工脚本： `docs\SqlServer-规则决策表升级脚本-7000-to-7001.sql`
  * 引擎内置 upgrade step： `NActiviti\Sys.Bpm.Engine\resources\db\upgrade\activiti.mssql.upgradestep.7000.to.7001.engine.sql`
  * 创建脚本已同步包含四张规则扩展表： `NActiviti\Sys.Bpm.Engine\resources\db\create\activiti.mssql.create.engine.sql`
  * 启动期告警覆盖：
    * `schema.version` 落后于 `7.0.0.1`
    * `decision_definition`
    * `decision_version`
    * `decision_execution_log`
    * `process_decision_reference`
  * 日志检索建议：
    * 前缀： `[decision-schema-warning]`
    * EventId： `700001` = schema 版本落后，`700002` = 扩展表缺失
  * 运行期查询接口：
    * `GET Api/v1/workflow/process-deployer/schema-status`
    * 返回核心字段：
      * `status`
      * `databaseType`
      * `expectedSchemaVersion`
      * `actualSchemaVersion`
      * `missingTables`
      * `recommendedAction`
* 规则决策主链验证脚本：
  * 路径： `NActiviti\scripts\verify-rule-decision-stack.ps1`
  * 默认行为：清理规则决策主链相关 `bin/obj/artifacts` 后，依次构建 `Sys.Bpm.Rules.Core`、`Sys.Bpm.Engine` 并执行 `Sys.Bpm.Engine.Tests`
  * 示例：
    * `powershell -ExecutionPolicy Bypass -File .\NActiviti\scripts\verify-rule-decision-stack.ps1`
    * `powershell -ExecutionPolicy Bypass -File .\NActiviti\scripts\verify-rule-decision-stack.ps1 -SkipTests`
    * `powershell -ExecutionPolicy Bypass -File .\NActiviti\scripts\verify-rule-decision-stack.ps1 -CleanOnly`
* schema-status smoke 验证脚本：
  * 路径： `NActiviti\scripts\test-decision-schema-status.ps1`
  * 用途：对运行中的宿主服务执行 `GET Api/v1/workflow/process-deployer/schema-status` 并校验数据库类型与状态
  * 示例：
    * `powershell -ExecutionPolicy Bypass -File .\NActiviti\scripts\test-decision-schema-status.ps1 -BaseUrl http://localhost:5000 -ExpectedDatabaseType postgres -ExpectedStatus Healthy`
    * `powershell -ExecutionPolicy Bypass -File .\NActiviti\scripts\test-decision-schema-status.ps1 -BaseUrl http://localhost:5000 -ExpectedDatabaseType mssql -ExpectedStatus Healthy`
* 宿主配置样例：
  * PostgreSQL： `docs\samples\appsettings.rule-decision.postgres.sample.json`
  * SQL Server： `docs\samples\appsettings.rule-decision.mssql.sample.json`
* 数据库校验 SQL：
  * PostgreSQL： `docs\samples\sql\postgres-decision-schema-check.sql`
  * SQL Server： `docs\samples\sql\mssql-decision-schema-check.sql`
* 前后端联调回归清单：
  * `docs\analysis\前后端多数据库联调回归清单-20260708.md`
* 双数据库联调总清单：
  * `docs\analysis\规则决策双数据库联调与验证清单-20260708.md`

## 运行BpmnWebTest项目,打开流程编辑器访问地址 http://localhost:11015/index.html
* 打开编辑器地址，点击下方CODE按钮，复制测试项目 resources\samples\ 目录下的bpmn文件内容，可以查看一些示例流程.
* 点击加号可以创建新的流程，复制samples\workflow.client\bpmn\请假单.bpmn文件内容，点部署，运行客户端示例。
* 编辑器代码 BpmnEditor
