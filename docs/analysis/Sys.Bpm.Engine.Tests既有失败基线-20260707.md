# Sys.Bpm.Engine.Tests既有失败基线

- 日期：2026-07-07
- 适用范围：`Tests\Sys.Bpm.Engine.Tests`
- 记录目的：固化当前已确认、与本轮规则治理/DMN 开发无直接因果关系的既有失败基线，避免后续回归时误判为新增缺陷。

## 1. 复核命令

```powershell
dotnet test .\Tests\Sys.Bpm.Engine.Tests\Sys.Bpm.Engine.Tests.csproj --filter "FullyQualifiedName~DeleteTaskCmd_删除单个任务_任务不再存在|FullyQualifiedName~DeleteTaskCmd_批量删除任务_任务全部删除|FullyQualifiedName~StartProcessInstanceCmd_定义ID不存在_抛出ActivitiObjectNotFoundException" -p:WarningLevel=0 -clp:ErrorsOnly
```

## 2. 当前失败清单

| 用例 | 文件 | 当前现象 | 基线判断 |
| --- | --- | --- | --- |
| `DeleteTaskCmd_删除单个任务_任务不再存在` | `Tests\CmdTests.cs` | 断言期望 `Assert.Null(ex)`，实际抛出 `ActivitiException` | 既有失败 |
| `DeleteTaskCmd_批量删除任务_任务全部删除` | `Tests\CmdTests.cs` | 断言期望 `Assert.Null(ex)`，实际抛出 `ActivitiException` | 既有失败 |
| `StartProcessInstanceCmd_定义ID不存在_抛出ActivitiObjectNotFoundException` | `Tests\CmdTests.cs` | 断言期望包含 `"No process definition found"`，实际消息为小写风格 | 既有失败 |

## 3. 失败明细

### 3.1 DeleteTaskCmd_删除单个任务_任务不再存在

- 用例位置：
  - [CmdTests.cs:L246-L262](file:///d:/Project/MDD/9.0/nactivity/Tests/Sys.Bpm.Engine.Tests/Tests/CmdTests.cs#L246-L262)
- 当前实际异常：

```text
Sys.Workflow.Engine.ActivitiException: The task cannot be deleted because is part of a running process
```

- 直接结论：
  - 当前引擎语义不允许直接删除仍属于运行中流程实例的任务。
  - 测试名称与断言表达的是“应允许删除”，与当前实现语义不一致。

### 3.2 DeleteTaskCmd_批量删除任务_任务全部删除

- 用例位置：
  - [CmdTests.cs:L265-L281](file:///d:/Project/MDD/9.0/nactivity/Tests/Sys.Bpm.Engine.Tests/Tests/CmdTests.cs#L265-L281)
- 当前实际异常：

```text
Sys.Workflow.Engine.ActivitiException: The task cannot be deleted because is part of a running process
```

- 直接结论：
  - 批量删除路径与单条删除路径的实际语义一致。
  - 当前实现更偏向“运行中任务不可直接删”，测试断言仍停留在旧预期。

### 3.3 StartProcessInstanceCmd_定义ID不存在_抛出ActivitiObjectNotFoundException

- 用例位置：
  - [CmdTests.cs:L598-L607](file:///d:/Project/MDD/9.0/nactivity/Tests/Sys.Bpm.Engine.Tests/Tests/CmdTests.cs#L598-L607)
- 当前实际消息片段：

```text
no deployed process definition found with ...
```

- 失败原因：
  - 测试断言使用：

```csharp
Assert.Contains("No process definition found", ex.Message);
```

  - 实际异常消息改为小写风格，导致字符串断言失败。

## 4. 对本轮工作的影响判断

- 本轮已完成的规则治理、事务边界、分页审计、`operator` 自动回填、发布前 DMN 校验等改动，均通过了对应隔离测试。
- 上述 3 条失败发生在 `CmdTests` 既有命令测试中，与本轮新增的决策治理链无直接耦合。
- 因此后续若执行 `Sys.Bpm.Engine.Tests` 全量回归，应将这 3 条视为“已知失败基线”，而非默认认定为本轮引入回归。

## 5. 后续处理建议

- 建议后续单独开一轮“命令测试基线修复”工作，避免与规则治理链改动混在同一批提交中。
- 建议修复策略按以下顺序评审：
  1. 先确认 `DeleteTaskCmd` 的期望语义到底应是“允许删除运行中任务”还是“禁止删除并抛异常”
  2. 若当前实现语义正确，则修测试断言与命名
  3. 若测试语义正确，则回溯 `DeleteTaskCmd` 当前限制逻辑
  4. 对异常消息断言统一改为更稳健的大小写无关或关键片段断言
