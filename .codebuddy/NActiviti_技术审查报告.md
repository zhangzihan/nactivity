# NActiviti 技术审查报告

**项目：** `d:/Project/MDD/9.0/nactivity`  
**基准：** Java Activiti 6.x / Flowable 移植到 .NET 6  
**方法：** 源码静态审计（只读，无构建/运行）  
**证据标注：** ✅ 源码已确认 | ⚠️ 推断风险 | 🔍 需运行时验证

---

## 一、项目分层与架构概览

### 1.1 模块划分

| 模块 | 命名空间前缀 | 职责 |
|------|-------------|------|
| `Sys.Bpm.Model` | `Sys.Workflow.Bpmn.*` | BPMN 领域对象、XML解析转换 |
| `Sys.Bpm.ProcessValidation` | `Sys.Workflow.Validation.*` | 流程定义规则校验 |
| `Sys.Bpm.Engine.API` | `Sys.Workflow.Engine.*` | 公共接口、异常、变量、事件契约 |
| `Sys.Bpm.Engine` | `Sys.Workflow.Engine.Impl.*` | 核心运行时：命令链、会话、持久化、部署、异步作业、事件 |
| `Sys.Bpm.Rest.API` | `Sys.Workflow.Rest.*` | REST DTO、事件契约、分页/转换模型 |
| `Sys.Bpm.Rest` | `Sys.Workflow.Cloud.*` | DI装配、控制器、中间件、包装服务 |
| `Sys.Bpm.Rest.Client` | `Sys.Workflow.*Client` | HTTP 客户端封装 |
| `Sys.Bpm.Features` | `Sys.Workflow.Features.*` | 业务扩展监听器 |

> ✅ 来源：`NActiviti.sln` 项目引用关系 + 各 `*.csproj` 命名空间

### 1.2 主执行链路（已确认）

```
HTTP 请求
  └─ Controller (WorkflowController 派生)
       └─ ProcessEngineWrapper
            └─ RuntimeServiceImpl / TaskServiceImpl / RepositoryServiceImpl
                 └─ CommandExecutorImpl（拦截器链首端）
                      ├─ LogInterceptor
                      ├─ CommandContextInterceptor   ← 创建/复用 CommandContext
                      │    └─ TransactionContextInterceptor
                      └─ CommandInvoker（链末端）
                           ├─ command.Execute(commandContext) → Agenda
                           └─ ExecuteOperations() 循环消费 Agenda
                                └─ EntityManager → DataManager → DbSqlSession → SmartSql XML
                                                               └─ Flush() → ADO.NET → DB
```

> ✅ 来源：`CommandExecutorImpl.cs`、`CommandContextInterceptor.cs`、`CommandInvoker.cs`、`DbSqlSession.cs`

---

## 二、与原版 Activiti 的兼容性差异

### 2.1 服务层覆盖

| 服务接口 | 原版 Activiti | 本项目状态 |
|---------|--------------|-----------|
| `RuntimeService` | ✓ | ✅ `RuntimeServiceImpl` 完整 |
| `TaskService` | ✓ | ✅ `TaskServiceImpl` + 大量扩展方法 |
| `RepositoryService` | ✓ | ✅ `RepositoryServiceImpl` 完整 |
| `HistoryService` | ✓ | ✅ `HistoryServiceImpl` 完整 |
| `ManagementService` | ✓ | ✅ `ManagementServiceImpl` 完整 |
| `IdentityService` | ✓ | ❌ **缺失**，用户查询通过外部 HTTP 接口完成 |
| `FormService` | ✓ | ❌ **缺失** |
| `DynamicBpmnService` | ✓ | 🔍 需确认 |

### 2.2 裁剪模块（`<Compile Remove>` 证据）

以下模块文件存在但被从编译中排除：

| 裁剪项 | 原因推断 |
|--------|---------|
| `JtaProcessEngineConfiguration.cs` | .NET 无 JTA，已裁剪 |
| `MultiSchemaMultiTenantProcessEngineConfiguration.cs` | 多租户功能不启用 |
| `SpringBeanFactoryProxyMap.cs` | Spring 依赖注入替换为 ASP.NET Core DI |
| `JtaRetryInterceptor.cs` / `JtaTransactionInterceptor.cs` | 同上 JTA 裁剪 |
| `JuelScriptEngine.cs` / `JuelScriptEngineFactory.cs` | JUEL 表达式引擎，.NET 无等价物 |
| `DefaultProcessDiagramCanvas.cs` / `DefaultProcessDiagramGenerator.cs` | 流程图生成图形依赖未移植 |
| 全部 `Image\impl\icon\*.cs` | 同上 |
| `SecurityPoliciesApplicationService.cs` 等 4 个安全策略文件（REST层） | 安全策略简化 |

> ✅ 来源：`Sys.Bpm.Engine.csproj` + `Sys.Bpm.Rest.csproj`

### 2.3 空实现 / 已知 TODO

| 文件 | 问题 |
|------|------|
| `ProcessEngineFactory.InitProcessEngineFromResource()` | **空方法体** `{ }`，Java 原版通过此方法从 XML 文件初始化引擎，.NET 版未实现 |
| `CommandInvoker.cs:60` | `// TODO: commandcontext和command不在一个线程导致异常`，捕获 `InvalidCastException` 后静默返回 `default`，**吞掉异常** |
| `ProcessEngineWrapper.CheckWritePermissionsOnTask()` | 注释 `//TODO: to check the user write permissions on task`，权限检查为空 |
| `ProcessDiagramGeneratorWrapper.cs` | 图形生成功能不可用（底层已裁剪） |

> ✅ 来源：直接读取对应文件

### 2.4 业务扩展（超越原版功能）

相比原版 Activiti，本项目增加了大量中国业务场景定制：

| 扩展能力 | 体现位置 |
|---------|---------|
| **BusinessKey / BusinessPath** | `IProcessInstance`、`IProcessDefinition`、所有查询对象 |
| **StartForm 唯一性约束** | `StartFormUniqueException`、`StartFormNullException`，部署时强约束 |
| **草稿部署** | `ProcessDefinitionDeployerController` draft 相关 API |
| **会签/加签** | `AddCountersignCmd`、`AppendCountersignCmd`、`NoneCountersignUsersException`、`AllPassParallelMultiInstanceBehavior` |
| **任务转办** | `TransferTaskCmd`、`taskService.Transfer()` |
| **RunTask（退回指定节点）** | `RunTaskCmd`、`RunableOperation`、`taskService.RunTask()` |
| **任务释放（AssigneeRelease）** | `AssigneeReleaseTaskCmd` |
| **BookmarkRule** | `NotExistsRuleTypeException`、`ExistsRuleTypeException` |
| **子流程多业务键监听** | `SubProcessActivityMultipleBusinessKeyListener`、`CallActivityMultipleBusinessKeyListener` |
| **批量异步启动** | `RuntimeServiceImpl.StartProcessInstanceByCmdsAsync()` 使用 `Task.Factory.StartNew` 并发 |

---

## 三、性能瓶颈诊断

### 3.1 内存分页（高危，已确认）

**受影响查询：**

| DataManager | 方法 | 注释原文 |
|------------|------|---------|
| `MybatisTaskDataManager` | `FindTasksAndVariablesByQueryCriteria` | `paging doesn't work for combining task instances and variables due to an outer join, so doing it in-memory` |
| `MybatisHistoricTaskInstanceDataManager` | `FindHistoricTaskInstancesAndVariablesByQueryCriteria` | 同上 |
| `MybatisHistoricProcessInstanceDataManager` | `FindHistoricProcessInstancesAndVariablesByQueryCriteria` | 同上 |
| `MybatisExecutionDataManager` | `FindProcessInstanceAndVariablesByQueryCriteria` | 同上 |

**工作机制：**
1. 强制将 `FirstResult` 重置为 0，`MaxResults` 设为查询限制（默认 **20000**）
2. 从数据库拉取最多 20000 条记录到内存
3. 再用 `.Skip().Take()` 做内存分页

**风险量化：**
- 极端情况：单次查询拉取 20000 条任务记录到内存，每条 Task + Variable 约 1–5 KB，峰值单次内存占用 **20 MB – 100 MB**
- 并发 10 个请求同时触发 → **200 MB – 1 GB** 额外堆内存压力
- 触发条件：任何带变量的分页任务/流程实例查询

> ✅ 来源：`MybatisTaskDataManager.cs:74-115`

### 3.2 反射桥接热点（中危，已确认）

`AbstractDataManager` 的三个核心委托（`selectById`、`selectList`、`selectOne`）全部通过运行时反射调用 `DbSqlSession` 的泛型方法：

```csharp
// AbstractDataManager.cs:35-39
MethodInfo m = db.GetType().GetMethod("selectbyid", ...);
var invoker = m.MakeGenericMethod(new Type[] { managedType, outType });
var res = invoker.Invoke(db, new object[] { id, useCache });
```

**风险：**
- `GetMethod` + `MakeGenericMethod` + `Invoke` 无缓存，每次查询执行 3 次反射操作
- .NET 反射调用比直接调用慢 10–100x；高并发下 CPU 热点明显
- 会产生额外 GC 压力（MethodInfo 装箱、参数 `object[]`）

**建议：** 将 `selectById`/`selectList`/`selectOne` 改为缓存 `MethodInfo`，或用 `Expression.Lambda` 编译为强类型委托。

> ✅ 来源：`AbstractDataManager.cs:33-86`

### 3.3 流程定义缓存无界（中危，已确认）

```json
// activiti.cfg.json:26
"processDefinitionCacheLimit": -1
```

```csharp
// ProcessEngineConfigurationImpl.cs:617
protected internal int processDefinitionCacheLimit = -1; // By default, no limit
```

`DefaultDeploymentCache` 底层是 `IMemoryCache`，当 `limit <= 0` 时以**无过期策略**创建缓存。三个缓存均默认无界：
- `processDefinitionCache`（流程定义）
- `processDefinitionInfoCache`（定义Info）
- `knowledgeBaseCache`（规则引擎知识库）

**风险：** 部署大量流程版本后，缓存持续增长，存活的 `BpmnModel` 对象不会被 GC，最终导致内存耗尽。

### 3.4 异步作业执行器（中危，风险已确认）

关键配置（`activiti.cfg.json` + `DefaultAsyncJobExecutor.cs`）：

| 参数 | 默认值 | 风险 |
|------|--------|------|
| `asyncExecutorDefaultAsyncJobAcquireWaitTime` | **15000 ms** | 作业获取间隔最大 15 秒，定时器精度低 |
| `maxTimerJobsPerAcquisition` | **1** | 每轮只获取 1 个计时器作业，高负载严重积压 |
| `corePoolSize` | **2** | 线程池最小线程数过小 |
| `maxPoolSize` | **10** | 并发上限较低 |
| `queueSize` | **100** | 队列满时新作业丢弃 |

`AcquireAsyncJobsDueRunnable` 竞争场景：集群部署时，多节点同时尝试获取同一批作业，触发 `ActivitiOptimisticLockingException`（代码中有专门捕获处理）。

> ✅ 来源：`AcquireAsyncJobsDueRunnable.cs:116-126`、`DefaultAsyncJobExecutor.cs:35-50`、`activiti.cfg.json:9-11`

### 3.5 ThreadLocal 与异步上下文冲突（高危，已确认）

**证据链：**

```csharp
// Context.cs:30-37 —— 引擎上下文使用 ThreadLocal
protected internal static ThreadLocal<Stack<ICommandContext>> commandContextThreadLocal = new(true);
protected internal static ThreadLocal<Stack<ProcessEngineConfigurationImpl>> processEngineConfigurationStackThreadLocal = new(true);
```

```csharp
// RuntimeServiceImpl.cs:716 —— 批量启动使用 Task.Factory.StartNew
Task.Factory.StartNew((command) => {
    var instance = StartProcessInstanceByCmd(command as IStartProcessInstanceCmd);
    tsc.TrySetResult(instance);
}, cmd);
```

```csharp
// CommandInvoker.cs:58-66 —— 已暴露的跨线程问题
catch (InvalidCastException ex)
{
    // TODO: commandcontext和command不在一个线程导致异常
    log.LogDebug($"【线程异常】:{ex.Message}");
    return default; // 静默吞掉异常，调用方无感知
}
```

**风险：** `Task.Factory.StartNew` 在 .NET 线程池调度新线程时，不继承父线程的 `ThreadLocal`，导致 `Context.CommandContext` 在子线程中为 `null`，触发 `InvalidCastException`，且**被静默忽略返回 null**，调用方收到空结果但不知道哪里出错。

**同时存在：** SmartSql 会话层使用 `AsyncLocal`（`DbConnectionSessionStore`），与引擎层 `ThreadLocal` 混用，在异步/跨线程场景下行为不一致。

### 3.6 同步事件链尾延迟（低危，已确认）

`ActivitiEventSupport.DispatchEvent()` 同步遍历所有注册监听器：

```csharp
// ActivitiEventSupport.cs:119-125
if (eventListeners.Count > 0) {
    foreach (IActivitiEventListener listener in eventListeners) {
        DispatchEvent(@event, listener);  // 同步调用
    }
}
```

REST 层还额外增加了 `MessageProducerCommandContextCloseListener`，在命令上下文关闭时同步发送消息总线事件。监听器数量越多，每次命令提交的尾延迟越高。

---

## 四、架构问题与可维护性评估

### 4.1 稳定性隐患（高优先级）

| 编号 | 问题 | 位置 | 影响 |
|------|------|------|------|
| S-01 | **异常被吞掉** | `CommandInvoker.cs:58-66` | 批量启动时部分实例启动失败，调用方收到 `null` 无感知，难以排查 |
| S-02 | **ThreadLocal+Task.Factory.StartNew** | `RuntimeServiceImpl.cs:716` | 跨线程上下文丢失，批量启动结果不可靠 |
| S-03 | **权限检查空实现** | `ProcessEngineWrapper:413-416` | 任何用户可修改任意任务，无访问控制 |
| S-04 | **安全策略文件被裁剪** | `Sys.Bpm.Rest.csproj:25-29` | 4个安全策略文件不编译，细粒度授权无效 |

### 4.2 性能优化优先级

| 优先级 | 问题 | 改进方向 | 预期收益 |
|--------|------|---------|---------|
| P0 | 内存分页（3.1节） | 重写 SQL 支持数据库侧分页，避免联表或拆分查询 | 单次峰值内存下降 90%+ |
| P1 | 反射桥接（3.2节） | 将 `MakeGenericMethod` 替换为预编译强类型委托或直接泛型调用 | CPU 减少 10–100x 反射开销 |
| P1 | 无界缓存（3.3节） | 设置合理 `processDefinitionCacheLimit`（如 500–1000），添加过期策略 | 防止内存泄漏 |
| P2 | 异步作业配置（3.4节） | 调整 `maxTimerJobsPerAcquisition` 为 10+，`corePoolSize` 为 4+ | 提升作业吞吐量 |
| P2 | 事件链同步（3.6节） | 高频事件类型考虑异步派发（生产者消费者队列） | 降低命令提交尾延迟 |

### 4.3 模式与分层评估

**良好实践：**
- ✅ 命令模式拦截器链职责清晰，新拦截器可零侵入插入
- ✅ 领域对象与持久化分离（Entity / DataManager / EntityManager 三层）
- ✅ SmartSql XML Mapping 将 SQL 从代码剥离，便于 DBA 优化
- ✅ `PerformanceSettings` 将性能开关集中管理，可配置化

**改进空间：**
- ⚠️ `AbstractDataManager` 反射桥接破坏了类型安全和性能，应由代码生成或泛型约束替代
- ⚠️ `ProcessEngineWrapper` 承担了过多职责（权限、DTO转换、服务调用、日志），是典型"上帝类"，建议按领域拆分
- ⚠️ `DefaultDeploymentCache` 的 `Clear()` 方法先 `Dispose()` 再重建 `IMemoryCache`，在并发访问时存在短暂 NPE 窗口
- ⚠️ 命名空间与目录名不一致（目录 `Sys.Bpm.*`，命名空间 `Sys.Workflow.*`），增加新人理解成本

### 4.4 测试与可观测性

- ✅ 日志框架到位：引擎层 `ILogger<T>`，REST层 `Serilog`
- ❌ `Test/ProcessTestController.cs` 被 `<Compile Remove>` 排除，测试控制器不编译
- ❌ 无关键路径性能埋点（命令执行时间、查询耗时、作业队列深度均无 Metrics 输出）
- ❌ 无分布式追踪（无 `ActivitySource` 或 OpenTelemetry 集成）

---

## 五、结构化改进建议

### 5.1 立即可做（不需要架构变更）

1. **修复 S-01（CommandInvoker 异常吞掉）**：将 `InvalidCastException` 的静默处理替换为重新抛出 + 记录完整堆栈；或确认跨线程根因后在正确层级处理。

2. **设置缓存上限**：在 `activiti.cfg.json` 中将 `processDefinitionCacheLimit` 设为合理值（建议 500），避免内存无限增长。

3. **调整异步作业参数**：
   - `maxTimerJobsPerAcquisition` → 10
   - `maxAsyncJobsDuePerAcquisition` → 20
   - `asyncExecutorCorePoolSize` → 4

4. **开启性能开关（评估后）**：
   - `EnableExecutionRelationshipCounts = true`：减少删除时的关联查询往返
   - 根据流程深度决定是否开启 `EnableEagerExecutionTreeFetching`

### 5.2 需验证后做

5. **内存分页改造**：对带变量的分页查询，评估是否能拆为两步查询（先分页查 ID，再查变量），避免全量加载。需先采样生产环境典型查询的返回行数。

6. **ThreadLocal → AsyncLocal 迁移**：将 `Context.cs` 中所有 `ThreadLocal` 改为 `AsyncLocal`，与 SmartSql 层保持一致，从根本上解决异步场景上下文丢失。迁移前需充分回归测试，因为会影响所有命令执行路径。

7. **反射缓存**：在 `AbstractDataManager` 中用 `ConcurrentDictionary<(Type,Type), MethodInfo>` 缓存已构建的 `MethodInfo`，或用 `Expression.Compile` 生成强类型委托，消除运行时反射开销。

### 5.3 长期重构项

8. **拆分 ProcessEngineWrapper**：按 `Process`、`Task`、`Variable`、`History` 等领域拆为独立服务类，降低单类复杂度。

9. **补全 IdentityService**：当前用户信息通过外部 HTTP 接口获取，建议抽象为本地 `IIdentityService` 接口，解耦引擎与外部用户服务。

10. **Metrics 埋点**：在 `CommandInvoker`、`DbSqlSession.FlushInserts/Updates/Deletes`、`AcquireAsyncJobsDueRunnable` 添加 `System.Diagnostics.Metrics` 计数器和直方图，暴露关键性能指标。

---

## 六、运行时验证清单（需上线后采样）

| 验证项 | 采样点 | 指标 |
|--------|--------|------|
| 内存分页实际触发频率 | `MybatisTaskDataManager.FindTasksAndVariablesByQueryCriteria` 入口 | 方法调用次数 × 实际返回行数 |
| 反射调用耗时 | `AbstractDataManager.selectList` | 每次执行 ns 级耗时 vs 直接调用基准 |
| 缓存命中率 | `DefaultDeploymentCache.Get` | hit/miss 比值 |
| 异步作业积压 | `AcquireAsyncJobsDueRunnable` 轮询 | 队列深度、`millisToWait=0` 频率 |
| 跨线程异常频率 | `CommandInvoker.catch(InvalidCastException)` | 每分钟触发次数（当前被静默） |
| 事件链耗时 | `ActivitiEventSupport.DispatchEvent` | 监听器总数 × 平均单个监听器耗时 |
| 命令提交 P99 延迟 | `CommandContextInterceptor.Execute` | 整体耗时分布 |

---

## 七、总结

NActiviti 是一个结构清晰、功能完整的 .NET Activiti 移植实现，核心命令链、持久化模型、部署管理和异步作业体系均忠实还原了 Java 原版设计，并在业务层面做了大量中国场景扩展（会签、转办、退回、草稿部署等）。

**主要风险按优先级：**

1. 🔴 **异常静默**（`CommandInvoker` 跨线程 InvalidCastException 被吞掉）— 影响正确性
2. 🔴 **ThreadLocal + 异步并发**（批量启动上下文丢失）— 影响可靠性
3. 🟠 **内存分页**（任务/变量联表查询全量加载）— 高负载内存危机
4. 🟠 **无界缓存**（流程定义缓存无上限）— 慢速内存泄漏
5. 🟡 **反射热点**（DataManager 每次查询三次反射）— CPU 开销
6. 🟡 **异步作业配置保守** — 作业积压与尾延迟
