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

## 初始化数据库表结构
* 首次启动修改配置文件：resources\activiti.cfg.json 配置
  * databaseSchemaUpdate配置说明: 
    *  update：true 当数据库表结构发生变化时设置为true，系统会自动更新到新的表结构
    *  drop-create：系统会先删除表再创建表，该配置仅在开发时可用，nuget包引用该功能不可用
    *  create: 该配置仅在数据库为空时创建表结构，否则系统会无法启动，首次启动后请修改该配置为false，否则再次启动服务将无法启动。
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

## 运行BpmnWebTest项目,打开流程编辑器访问地址 http://localhost:11015/index.html
* 打开编辑器地址，点击下方CODE按钮，复制测试项目 resources\samples\ 目录下的bpmn文件内容，可以查看一些示例流程.
* 点击加号可以创建新的流程，复制samples\workflow.client\bpmn\请假单.bpmn文件内容，点部署，运行客户端示例。
* 编辑器代码 BpmnEditor
