---
name: nactiviti-architecture-and-performance-analysis
overview: 基于现有只读调研，为该 .NET 6 Activiti 仿制项目制定一份可执行的架构评估与性能诊断方案，覆盖兼容性、可维护性、性能指标、瓶颈定位及数据支撑路径。
todos:
  - id: trace-core-chain
    content: 使用[subagent:code-explorer]补全启动到持久化主链证据
    status: completed
  - id: compare-activiti
    content: 对照原版梳理服务覆盖、空实现、裁剪与扩展
    status: completed
    dependencies:
      - trace-core-chain
  - id: profile-hotspots
    content: 量化查询、缓存、异步作业与事件链性能风险
    status: completed
    dependencies:
      - trace-core-chain
  - id: assess-architecture
    content: 汇总分层、模式、边界与可维护性改进建议
    status: completed
    dependencies:
      - trace-core-chain
      - compare-activiti
      - profile-hotspots
  - id: draft-report
    content: 生成结构化分析报告与后续验证清单
    status: completed
    dependencies:
      - assess-architecture
---

## User Requirements

- 对 `@NActiviti` 指向的现有流程引擎项目进行只读技术审查，最终输出一份结构化分析报告。
- 报告需同时覆盖“架构与实现评估”以及“性能瓶颈诊断”两大主题，并给出具体、可执行的改进建议。
- 结论必须有明确证据支撑，不能只给泛泛观点。

## Product Overview

- 输出物是一份面向现有代码库的审查报告，按模块、执行链路、问题级别、证据来源组织内容。
- 报告呈现应层级清晰，适合用章节、表格、对比项和趋势说明来展示；阅读效果上要能直观看出核心链路、主要差异、热点瓶颈和优化优先级。
- 报告中需明确区分：源码已确认事实、基于代码推断的风险、以及仍需运行期验证的项。

## Core Features

- 复盘项目分层、启动流程、命令执行、持久化、部署、异步作业、REST 与事件链路。
- 评估代码组织、设计模式使用、模块边界和维护成本，并提出优化方案。
- 对照 Java Activiti 原版，梳理兼容项、缺失项、裁剪项和定制扩展。
- 识别吞吐量、响应时间、资源占用等关键指标及其采集口径。
- 定位数据库查询、内存分页、缓存、反射、异步轮询、同步事件等性能热点。
- 输出按优先级排序的优化建议，并附对应证据与验证方案。

## Tech Stack Selection

- 解决方案入口：`d:/Project/MDD/9.0/nactivity/NActiviti.sln`
- 核心运行时：`.NET 6`，已在多个 `*.csproj` 中确认
- Web 宿主：`ASP.NET Core`，`Sys.Bpm.Rest` 与 `Sys.Bpm.Rest.API` 基于 `Microsoft.AspNetCore.App`
- 持久化：`SmartSql` + XML Mapping + ADO.NET Provider，核心落点在 `DbSqlSession` 与各 `Mybatis*DataManager`
- 缓存：`Microsoft.Extensions.Caching.Memory`
- 日志：`Microsoft.Extensions.Logging`，REST 层补充 `Serilog`
- HTTP/容错：`HttpClient` + `Polly`
- 命名空间现实：目录名多为 `Sys.Bpm.*`，核心命名空间实际为 `Sys.Workflow.*`

## Implementation Approach

### 方法与总体策略

本方案采用“源码证据优先的分层审计”方法：先沿真实执行链复盘架构，再按兼容性、模式合理性、可维护性、性能热点四个维度交叉分析，最后汇总成结构化报告。
由于当前处于只读计划阶段，不做构建、压测和运行期采样，因此报告会把“已从代码确认的事实”与“需运行时验证的风险”严格分层，避免把推断写成既成结论。

### 关键技术判断

- 该项目核心实现集中在 `Sys.Bpm.Engine`，装配中心是 `Engine/impl/cfg/ProcessEngineConfigurationImpl.cs`，应以它为主轴串起 DI、命令执行、会话、缓存、部署、异步作业和事件。
- 项目明显沿用 Java Activiti/Flowable 的命令链、上下文、部署器、缓存、异步执行器等思路，但在持久化、依赖注入、HTTP 与缓存实现上进行了 .NET 化替换。
- 兼容性分析不能只看 API 名称，还要核对生命周期、空实现、被裁剪模块和业务扩展，例如 `ProcessEngineFactory.InitProcessEngineFromResource()` 为空、`IdentityService/FormService` 缺位、以及 `BusinessKey/BusinessPath/StartForm/草稿部署/BookmarkRule/会签` 等定制能力。

### Performance &amp; Reliability

- 启动/部署链路：流程定义冷加载和部署解析主要受 BPMN 资源规模影响，热点在 `DeploymentManager`、`BpmnDeployer`、缓存填充与本地化处理。
- 查询链路：`MybatisTaskDataManager` 与 `MybatisExecutionDataManager` 存在“先查大集合再内存分页”路径，复杂度近似随结果集线性增长，是高负载衰减的关键来源。
- 执行链路：`AbstractDataManager` 通过反射桥接 `DbSqlSession`，高频调用下会增加 CPU 与 GC 压力。
- 执行树链路：`EnableEagerExecutionTreeFetching` 用更少数据库往返换更大单次加载内存，适合与 execution tree 大小一起评估。
- 删除/维护链路：`EnableExecutionRelationshipCounts` 可减少关系探测查询，但会增加 execution 更新与乐观锁竞争概率。
- 异步作业链路：`AcquireAsyncJobsDueRunnable`、`ExecuteAsyncRunnable`、过期重置与独占作业锁会形成数据库竞争与尾延迟风险。
- 可靠性风险：`CommandInvoker.cs` 中对线程/上下文问题的注释与 `InvalidCastException` 兜底返回默认值，是必须重点审查的稳定性信号。

### 避免技术债的原则

- 仅沿现有项目边界和既有模式做评估，不引入新架构假设。
- 建议优先落在配置、查询路径、缓存边界、事件链和管理器层，避免先做大范围重构。
- 结论按“立即可做 / 需验证后做 / 长期重构项”分层，控制建议的可实施性和影响面。

## Implementation Notes

- 报告中所有结论都要回指到已确认文件，避免凭经验泛化。
- 对性能部分，优先引用现有静态参数与查询模式，例如 `ExecutionQueryLimit`、`TaskQueryLimit`、`MaxNrOfStatementsInBulkInsert`、多个 cache limit 默认 `-1`。
- 对未实测项，明确给出推荐采样点，而不是虚构数值；优先建议的观测点为 `CommandInvoker`、`DbSqlSession`、`MybatisTaskDataManager`、`MybatisExecutionDataManager`、`AcquireAsyncJobsDueRunnable`、REST 控制器入口。
- 若后续获准做运行期验证，应复用现有 `ILogger`/`Serilog` 体系，避免在热点路径输出大对象、变量全量或高频噪声日志。
- 本阶段不做代码修改，建议报告应保持对现有行为的向后兼容导向。

## Architecture Design

### 已确认的系统结构

- **模型层**：`Sys.Bpm.Model`，承载 BPMN 领域对象
- **校验层**：`Sys.Bpm.ProcessValidation`，负责定义校验规则集
- **契约层**：`Sys.Bpm.Engine.API`，提供共享接口、异常、变量与事件契约
- **引擎层**：`Sys.Bpm.Engine`，实现配置装配、命令链、会话、持久化、部署、异步作业和事件
- **REST 契约层**：`Sys.Bpm.Rest.API`，承载 REST DTO、事件契约和分页/转换模型
- **REST 宿主层**：`Sys.Bpm.Rest`，承载依赖注入、控制器、中间件、包装服务
- **客户端层**：`Sys.Bpm.Rest.Client`，封装 HTTP 客户端
- **特性扩展层**：`Sys.Bpm.Features`，放置业务增强监听器

### 关键执行关系

- 启动装配：`AddProcessEngine` → `AddProcessEngineConfiguration` → `UseProcessEngine`
- 服务调用：`RuntimeServiceImpl/RepositoryServiceImpl/TaskServiceImpl` → `CommandExecutor`
- 命令执行：`CommandContextInterceptor` → `CommandContext` → `CommandInvoker` → `Agenda`
- 持久化：`EntityManager` → `DataManager` → `DbSqlSession` → SmartSql XML Mapping
- 部署解析：`DeploymentManager` → `BpmnDeployer` → `ProcessDefinitionCache`
- 异步作业：`DefaultAsyncJobExecutor` → 获取任务 → 执行任务 → 重置过期任务
- REST 与事件：Controller/Wrapper → Engine Service；命令关闭时由 CloseListener 聚合并发送事件

## Directory Structure

### Directory Structure Summary

本任务不新增或修改代码文件，重点审查下列已确认的关键文件，并基于这些文件输出结构化分析报告。

```text
d:/Project/MDD/9.0/nactivity/
├── NActiviti.sln  # [ANALYZE] 解决方案入口。用于确认项目划分与整体装配边界。
└── NActiviti/
    ├── Sys.Bpm.Engine/DependencyInjection/ProcessEngineServiceCollectionExtensions.cs  # [ANALYZE] DI 注册入口。核对引擎、缓存、HTTP、异步执行器注入方式。
    ├── Sys.Bpm.Engine/DependencyInjection/BuilderExtensions/ProcessEngine.cs  # [ANALYZE] 引擎配置与服务装配。确认默认 ProcessEngineConfiguration 构建方式。
    ├── Sys.Bpm.Engine/DependencyInjection/WorkflowApplicationBuilderExtensions.cs  # [ANALYZE] 应用启动入口。定位 BuildProcessEngine 和生命周期销毁逻辑。
    ├── Sys.Bpm.Engine/Engine/impl/cfg/ProcessEngineConfigurationImpl.cs  # [ANALYZE] 核心装配中心。审查服务、会话、缓存、部署器、事件与性能开关。
    ├── Sys.Bpm.Engine/Engine/impl/cfg/PerformanceSettings.cs  # [ANALYZE] 性能策略定义。核对 execution tree、relationship count、本地化等开关。
    ├── Sys.Bpm.Engine/Engine/ProcessEngineFactory.cs  # [ANALYZE] 引擎工厂与生命周期。核对默认引擎、注册逻辑、兼容性空实现。
    ├── Sys.Bpm.Engine/Engine/impl/ProcessEngineImpl.cs  # [ANALYZE] IProcessEngine 实现。确认服务聚合与工厂注册关系。
    ├── Sys.Bpm.Engine/Engine/impl/interceptor/CommandContext.cs  # [ANALYZE] 命令上下文。审查 flush、close、session 管理与异常处理。
    ├── Sys.Bpm.Engine/Engine/impl/interceptor/CommandContextInterceptor.cs  # [ANALYZE] 上下文拦截器。核对上下文创建、复用与异常分支。
    ├── Sys.Bpm.Engine/Engine/impl/cfg/CommandExecutorImpl.cs  # [ANALYZE] 命令执行器装配。确认拦截器链次序与调用入口。
    ├── Sys.Bpm.Engine/Engine/impl/interceptor/CommandInvoker.cs  # [ANALYZE] 命令链末端。重点审查 Agenda 调度与线程/上下文风险。
    ├── Sys.Bpm.Engine/Engine/impl/interceptor/TransactionCommandContextCloseListener.cs  # [ANALYZE] 提交/回滚触发点。确认事务收口位置。
    ├── Sys.Bpm.Engine/Engine/impl/db/DbSqlSession.cs  # [ANALYZE] 核心持久化会话。核对查询、缓存装载、批量插入、事务提交与热点路径。
    ├── Sys.Bpm.Engine/Engine/impl/persistence/entity/data/AbstractDataManager.cs  # [ANALYZE] DataManager 通用抽象。重点评估反射调用成本。
    ├── Sys.Bpm.Engine/Engine/impl/persistence/entity/data/impl/MybatisExecutionDataManager.cs  # [ANALYZE] 执行树与流程实例查询。重点审查 eager fetch 与内存分页。
    ├── Sys.Bpm.Engine/Engine/impl/persistence/entity/data/impl/MybatisTaskDataManager.cs  # [ANALYZE] 任务查询热点。重点审查变量联表与结果集分页策略。
    ├── Sys.Bpm.Engine/Engine/impl/util/ProcessInstanceHelper.cs  # [ANALYZE] 流程实例创建与启动主链路。核对变量、标题、历史、事件和首个 execution 调度。
    ├── Sys.Bpm.Engine/Engine/impl/runtime/ProcessInstanceBuilderImpl.cs  # [ANALYZE] 流程启动构建器实现。核对扩展输入面与兼容性。
    ├── Sys.Bpm.Engine/Engine/runtime/IProcessInstanceBuilder.cs  # [ANALYZE] 启动构建接口。对照原版能力范围与定制扩展。
    ├── Sys.Bpm.Engine/Engine/impl/cmd/StartProcessInstanceCmd.cs  # [ANALYZE] 启动流程命令。审查定义查找、启动分支与异常路径。
    ├── Sys.Bpm.Engine/Engine/impl/RuntimeServiceImpl.cs  # [ANALYZE] RuntimeService 实现。重点核对同步/异步启动与线程风险。
    ├── Sys.Bpm.Engine/Engine/impl/persistence/deploy/DeploymentManager.cs  # [ANALYZE] 部署与定义缓存入口。核对冷加载、缓存命中与删除逻辑。
    ├── Sys.Bpm.Engine/Engine/impl/bpmn/deployer/BpmnDeployer.cs  # [ANALYZE] BPMN 部署器。审查解析、版本、缓存更新与本地化逻辑。
    ├── Sys.Bpm.Engine/Engine/impl/asyncexecutor/DefaultAsyncJobExecutor.cs  # [ANALYZE] 异步作业总执行器。核对线程、队列、获取与停止策略。
    ├── Sys.Bpm.Engine/Engine/impl/asyncexecutor/AcquireAsyncJobsDueRunnable.cs  # [ANALYZE] 异步任务轮询获取逻辑。评估等待曲线与数据库竞争。
    ├── Sys.Bpm.Engine/Engine/impl/asyncexecutor/ExecuteAsyncRunnable.cs  # [ANALYZE] 单任务执行逻辑。评估独占锁、失败处理与尾延迟。
    ├── Sys.Bpm.Engine/Engine/delegate/event/impl/ActivitiEventDispatcherImpl.cs  # [ANALYZE] 事件分发器。核对同步派发成本与监听器扇出。
    ├── Sys.Bpm.Engine/Engine/ProcessEngineConfiguration.cs  # [ANALYZE] 公共配置面。核对对外暴露的参数与兼容性表述。
    ├── Sys.Bpm.Rest/ProcessEngineRestExtention.cs  # [ANALYZE] REST 装配入口。确认包装服务、控制器发现、总线与中间件集成。
    ├── Sys.Bpm.Rest/core/ProcessEngineWrapper.cs  # [ANALYZE] REST 包装层。评估对象转换、权限、事件发布与附加开销。
    ├── Sys.Bpm.Rest/rest/controllers/ProcessDefinitionDeployerController.cs  # [ANALYZE] 部署控制器。核对草稿、部署、保存与查询路径。
    ├── Sys.Bpm.Rest/rest/controllers/WorkflowUsersController.cs  # [ANALYZE] 用户相关控制器。用于补齐边界能力与兼容性差异审查。
    ├── Sys.Bpm.Rest/events/listeners/MessageProducerCommandContextCloseListener.cs  # [ANALYZE] 命令关闭后的事件发送。评估同步尾延迟风险。
    ├── Sys.Bpm.Rest/events/listeners/ProcessEngineEventsAggregator.cs  # [ANALYZE] 事件聚合器。确认命令上下文内的事件收集方式。
    ├── Sys.Bpm.ProcessValidation/ProcessValidation/ProcessValidatorFactory.cs  # [ANALYZE] 校验器工厂。确认默认规则集装配。
    ├── Sys.Bpm.ProcessValidation/ProcessValidation/ProcessValidatorImpl.cs  # [ANALYZE] 校验执行器。评估校验扩展点与复杂度。
    ├── Sys.Bpm.Model/Model/Process.cs  # [ANALYZE] BPMN 领域核心对象。作为模型层对照基准。
    └── Sys.Bpm.Features/Engine/impl/bpmn/listener/SubProcessActivityMultipleBusinessKeyListener.cs  # [ANALYZE] 业务扩展样例。用于评估与原版差异和可维护性影响。
```

## Agent Extensions

### SubAgent

- **code-explorer**
- Purpose: 继续做跨项目只读检索，补全启动链、命令链、持久化链、REST 链与兼容性差异证据。
- Expected outcome: 形成覆盖关键模块的文件证据清单，降低报告遗漏和误判风险。