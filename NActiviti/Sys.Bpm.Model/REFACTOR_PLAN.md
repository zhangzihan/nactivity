# Sys.Bpm.Model 重构实施计划

> 版本：v1.1  
> 日期：2026-04-26  
> 状态：P0-P2 已执行  
> 项目：仿 Java Activiti 7.0 → C# .NET 6.0 工作流引擎

---

## 一、项目现状概述

### 1.1 基本信息

| 项目 | 详情 |
|------|------|
| 目标框架 | .NET 6.0 |
| 根命名空间 | `Sys.Bpm`（csproj 配置） |
| 实际命名空间 | `Sys.Workflow.Bpmn.Models` / `Sys.Workflow.Bpmn.Converters` / `Sys` 等 |
| 源文件数 | ~222 个 .cs 文件 |
| 核心模块 | Model (100+), Converter (96+), Extensions (1), DI (1), 工具类 (12) |
| NuGet 依赖 | Newtonsoft.Json, Microsoft.Extensions.Hosting |
| 转换工具 | Tangible Software Solutions（Java → C# 自动转换） |

### 1.2 问题汇总表

| # | 严重度 | 类别 | 问题 | 影响范围 | 工作量 |
|---|--------|------|------|----------|--------|
| 1 | 🔴 P0 | 类型安全 | `Values` write-only 属性模式，`as` 转换无 null 检查 | 69 个 Model 文件 | 大 |
| 2 | 🔴 P0 | 线程安全 | `ConcurrentDictionary` 与 `Dictionary` 混用 | 3 个核心文件 | 中 |
| 4 | 🟡 P1 | DI 隐患 | `BuildServiceProvider()` 创建临时容器 | 1 个文件 | 小 |
| 5 | 🟡 P1 | XML 解析 | Converter 层 if-else 链过长，过度模拟 Java StAX | Converter 层 | 大 |
| 6 | 🟢 P2 | Java 残留 | DateTimeHelper、HashMapHelper 等 Java 兼容层 | 6+ 个文件 + 引用处 | 中 |
| 7 | 🟢 P2 | 异常处理 | 吞异常、多余 catch、serialVersionUID 残留 | 5+ 处 | 小 |
| 8 | 🟢 P3 | 测试覆盖 | Model 层缺少单元测试 | 全部 | 大 |

---

## 二、分阶段任务清单

### 阶段一：P0 — 类型安全与线程安全（预计 2-3 周）

> **目标**：消除运行时隐患，确保核心 Model 层的类型安全和一致性

---

#### 任务 1.1：修复 `Values` 属性模式的类型安全问题

**问题描述**：

`BaseElement` 及所有子类使用 write-only `Values` 属性实现深拷贝，存在以下风险：
- Write-only 属性违反 C# 设计准则（CA1044）
- `as` 类型转换无 null 检查，类型不匹配时 `NullReferenceException`
- 每个类重复大量样板代码，易遗漏字段
- `Clone()` 间接调用 `Values`，语义不直观

**修改范围**：

| 文件 | 修改内容 |
|------|----------|
| `Model/BaseElement.cs` | 重构 `Values` setter，添加类型安全检查 |
| `Model/FlowElement.cs` | 同上 |
| `Model/FlowNode.cs` | 同上 |
| `Model/Activity.cs` | 同上 |
| `Model/UserTask.cs` | 同上 |
| 其余 64 个 Model 文件 | 同上 |

**具体步骤**：

1. **在 `BaseElement.cs` 中添加类型安全的 `Values` setter 基础实现**

   ```csharp
   // 修改前
   public override BaseElement Values
   {
       set
       {
           var val = value as BaseElement; // 不安全！
           // ...
       }
   }

   // 修改后 — 方案 A（最小改动）
   public override BaseElement Values
   {
       set
       {
           if (value == null) throw new ArgumentNullException(nameof(value));
           if (value.GetType() != GetType()) 
               throw new ArgumentException($"Expected {GetType().Name}, got {value.GetType().Name}");
           var val = (BaseElement)value; // 安全的强制转换
           // ...
       }
   }
   ```

2. **逐文件修复所有子类的 `Values` setter**，按继承层级自底向上：
   - 第一批：叶子节点类（`UserTask`, `ServiceTask`, `ScriptTask`, `StartEvent`, `EndEvent` 等）
   - 第二批：中间抽象类（`Activity`, `FlowNode`, `FlowElement`, `Event`, `Gateway`）
   - 第三批：`BaseElement` 根类

3. **为 `Clone()` 方法添加 XML 文档注释**，明确说明深拷贝语义

**验收标准**：

- [ ] 所有 `Values` setter 中 `as` 转换替换为带类型检查的强制转换
- [ ] 类型不匹配时抛出 `ArgumentException` 而非 `NullReferenceException`
- [ ] 无编译错误，现有测试全部通过
- [ ] 代码分析无 CA1044 警告（或标记为抑制并注明原因）

**影响分析**：

- 仅影响 Model 层内部，不改变公共 API 签名
- `Clone()` 方法行为不变，只是增加了类型安全保护
- 需要确保下游 Engine 层的测试全部通过

---

#### 任务 1.2：统一 Dictionary / ConcurrentDictionary 使用

**问题描述**：

`BaseElement` 中 `extensionElements` 使用 `ConcurrentDictionary`，但 `attributes` 使用 `Dictionary`；`Values` setter 中重建集合时类型不一致，导致线程安全语义混乱。

**修改范围**：

| 文件 | 当前状态 | 修改内容 |
|------|----------|----------|
| `Model/BaseElement.cs` | `extensionElements` = ConcurrentDictionary, `attributes` = Dictionary | 统一为 Dictionary（推荐） |
| `Model/ExtensionElement.cs` | 同上 | 统一为 Dictionary |
| `DictionaryHelper.cs` | 使用 ConcurrentDictionary | 移除 ConcurrentDictionary 依赖 |

**具体步骤**：

1. **评估线程安全需求**：BPMN Model 对象通常是单线程使用（解析/构建时），无需 `ConcurrentDictionary`
2. **将 `extensionElements` 从 `ConcurrentDictionary` 改为 `Dictionary`**
3. **更新 `DictionaryHelper.PutAll()` 方法**，移除 ConcurrentDictionary 相关代码
4. **检查 `Values` setter 中的集合重建逻辑**，确保类型一致

**验收标准**：

- [ ] `BaseElement` 和 `ExtensionElement` 中所有字典类型统一为 `Dictionary`
- [ ] 无 `ConcurrentDictionary` 残留（除非有明确的多线程场景）
- [ ] 编译通过，测试通过

**影响分析**：

- Model 对象在运行时通常不跨线程共享，改为 Dictionary 不影响功能
- 如果未来需要线程安全，应在使用层面加锁而非在 Model 层

---

### 阶段二：P1 — 代码清理与基础设施修复（预计 1-2 周）

> **目标**：修复 DI 隐患，降低维护负担

---

#### 任务 2.2：修复 DI 注册隐患

**问题描述**：

```csharp
// BpmModelServiceProvider.cs
BpmnModelLoggerFactory.factory = services.BuildServiceProvider().GetService<ILoggerFactory>();
```

`BuildServiceProvider()` 创建临时 ServiceProvider，可能导致单例不一致和性能问题。

**修改范围**：

| 文件 | 修改内容 |
|------|----------|
| `DependencyInjection/BpmModelServiceProvider.cs` | 改用延迟初始化模式 |

**具体步骤**：

1. **改为延迟解析 `ILoggerFactory`**：

   ```csharp
   // 修改后
   public static IServiceCollection AddBpmModelServiceProvider(this IServiceCollection services)
   {
       services.AddSingleton<BpmnModelLoggerFactory>(sp => 
           new BpmnModelLoggerFactory(sp.GetService<ILoggerFactory>()));
       return services;
   }
   ```

2. **重构 `BpmnModelLoggerFactory`**，使其从 DI 容器获取 `ILoggerFactory` 而非静态赋值

3. **确保向后兼容**：如果 Engine 层直接使用 `BpmnModelLoggerFactory.factory`，需同步修改

**验收标准**：

- [ ] 不再调用 `BuildServiceProvider()`
- [ ] `ILoggerFactory` 通过 DI 容器正确解析
- [ ] 日志功能正常工作

---

#### 任务 2.4：简化 Converter 层 if-else 链

**问题描述**：

`BpmnXMLConverter.ConvertToBpmnModel()` 中有 200+ 行 if-else 链匹配 XML 元素类型，应改为字典查找。

**修改范围**：

| 文件 | 修改内容 |
|------|----------|
| `Converter/BpmnXMLConverter.cs` | 将 if-else 链改为 `Dictionary<string, Func<...>>` 查找 |

**具体步骤**：

1. 提取已有的 `convertersToBpmnMap` 字典，补全缺失的映射
2. 将主入口的 if-else 链替换为字典查找 + 策略模式
3. 确保 XML 解析结果与重构前一致

**验收标准**：

- [ ] if-else 链替换为字典查找
- [ ] 新增 BPMN 元素类型只需添加字典条目
- [ ] 所有 BPMN XML 解析测试通过

---

### 阶段三：P2 — C# 地道化改造（预计 3-4 周）

> **目标**：将 Java 惯用法逐步替换为 C# 惯用法，提升代码可读性和可维护性

---

#### 任务 3.2：替换 Java 兼容工具类

**修改范围**：

| 文件 | 当前用途 | 替换方案 |
|------|----------|----------|
| `DateTimeHelper.cs` | Java Calendar 枚举体系（ERA, YEAR, MONTH...） | 使用 `DateTime`/`DateTimeOffset` 原生方法 |
| `HashMapHelper.cs` | Java HashMap 兼容层 | 完全删除，使用 `Dictionary<TKey, TValue>` |
| `StringHelper.cs` | Java String 方法移植 | 逐步替换为 C# `string` 方法和 LINQ |
| `INamespaceContext.cs` | Java javax.xml.namespace.NamespaceContext 翻译 | 使用 `XmlNamespaceManager` |
| `DictionaryHelper.cs` | Java Map.putAll() 等 | 使用 C# `foreach` 或 `CollectionExtensions` |

**具体步骤**：

1. **全局搜索每个工具类的引用处**
2. **逐个替换为 C# 原生方案**：
   - `DateTimeHelper.Get(field)` → `DateTime.Year`, `DateTime.Month` 等
   - `HashMapHelper` → `Dictionary<TKey, TValue>`
   - `StringHelper.IsEmpty(str)` → `string.IsNullOrEmpty(str)`
   - `DictionaryHelper.PutAll(dict1, dict2)` → `foreach (var kv in dict2) dict1[kv.Key] = kv.Value;`
3. **替换完成后删除工具类文件**

**验收标准**：

- [ ] Java 兼容工具类全部移除或标记 `[Obsolete]`
- [ ] 所有引用改为 C# 原生方案
- [ ] 编译通过，测试通过

---

#### 任务 3.3：修复异常处理问题

**修改范围**：

| 位置 | 问题 | 修复方案 |
|------|------|----------|
| `UriHelper.OpenStream()` | `catch (Exception) { throw; }` 多余 | 删除 try-catch |
| `XMLException` | 保留 Java `serialVersionUID` | 删除该常量 |
| `BpmnXMLConverter.ConvertToXML()` | 吞掉 `BPMNDIExport` 异常 | 至少记录日志 |
| 各处 `catch (Exception e)` | 过于宽泛 | 捕获更具体的异常类型 |

**验收标准**：

- [ ] 无多余的 try-catch-rethrow
- [ ] 无 Java 遗留常量
- [ ] 异常不被静默吞掉

---

### 阶段四：P3 — 架构优化（长期规划）

> **目标**：考虑更深层次的架构改进，可作为后续版本的目标

---

#### 任务 4.1：考虑用 XmlSerializer 重构 Converter 层

**现状**：Converter 层完全模拟 Java StAX pull-parser 模式，有 4 层嵌套结构（base/child/parser/export），总计 96+ 文件。

**可选方案**：

| 方案 | 描述 | 优点 | 缺点 |
|------|------|------|------|
| A. 维持现状 | 保持 StAX 模式，仅做局部优化 | 改动最小 | 仍有 Java 风格 |
| B. XmlSerializer | 使用 `System.Xml.Serialization` 重写 | .NET 原生，代码量减少 60%+ | 需大量属性标注，BPMN XML 结构复杂 |
| C. 混合方案 | 顶层用 XmlSerializer，扩展元素仍手动解析 | 兼顾便利与灵活性 | 两套模式并存 |

**建议**：先执行阶段一至三，待 Model 层稳定后再评估此任务。

---

#### 任务 4.2：补充单元测试

**范围**：

| 测试类别 | 覆盖目标 | 优先级 |
|----------|----------|--------|
| Model 构造测试 | 每个 BPMN 元素的构造和属性赋值 | 高 |
| Clone 测试 | 验证深拷贝的正确性和类型安全 | 高 |
| XML 序列化/反序列化测试 | 核心 BPMN XML 文件的往返测试 | 高 |
| 边界条件测试 | 空值、特殊字符、循环引用等 | 中 |

**建议测试框架**：xUnit + FluentAssertions

---

## 三、风险评估与回退策略

| 风险 | 可能性 | 影响 | 缓解措施 |
|------|--------|------|----------|
| 重构导致 Engine 层编译失败 | 中 | 高 | 每个任务完成后立即运行全解决方案编译 |
| `Values` 模式修改改变深拷贝行为 | 低 | 高 | 修改前为所有 Model 类编写 Clone 测试用例 |
| 命名空间重命名引入遗漏 | 高 | 中 | 使用 IDE 的全局重命名功能，逐步替换 |
| 删除工具类后遗漏引用 | 中 | 中 | 全局搜索后再删除，编译验证 |
| DI 修改影响日志功能 | 低 | 中 | 修改后验证日志输出正常 |

### 回退策略

- 每个阶段基于独立分支开发，合并前通过 Code Review
- 如果某任务导致严重回归，可通过 Git revert 回退到上一稳定状态
- 建议在 CI 中增加 BPMN XML 解析的回归测试

---

## 四、执行时间线

```
Week 1-3:  阶段一（P0）— 类型安全与线程安全
Week 4-5:  阶段二（P1）— 代码清理与基础设施修复
Week 6-9:  阶段三（P2）— C# 地道化改造
Week 10+:  阶段四（P3）— 架构优化（长期规划）
```

---

## 五、关键依赖关系

```
任务 1.1（Values 类型安全）
  └── 任务 3.1（自动属性）依赖 1.1 完成后再统一处理

任务 1.2（统一 Dictionary）
  └── 任务 3.2（替换工具类）会进一步清理 DictionaryHelper

任务 2.3（统一命名空间）
  └── 需全解决方案协同，建议在阶段二最后执行

任务 4.1（XmlSerializer 重构）
  └── 依赖阶段一至三全部完成后再评估
```

---

## 六、参考资源

- [Activiti 7.0 源码](https://github.com/Activiti/Activiti)
- [BPMN 2.0 规范](https://www.omg.org/spec/BPMN/2.0/)
- [C# 编码约定](https://learn.microsoft.com/zh-cn/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Framework Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/)
