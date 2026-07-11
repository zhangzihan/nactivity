# PostgreSQL 规则决策表升级说明 7000 -> 7001

## 目的

- 将规则决策相关四张表正式落库：
  - `decision_definition`
  - `decision_version`
  - `decision_execution_log`
  - `process_decision_reference`
- 对应流程引擎 schema 版本从 `7.0.0.0` 提升到 `7.0.0.1`。

## 适用场景

- 宿主配置 `databaseSchemaUpdate=false`，不会在启动时自动执行 schema upgrade。
- 现网 PostgreSQL 数据库已经运行在 `7.0.0.0`，需要手工补齐规则决策相关表结构。

## 推荐执行顺序

1. 备份目标 PostgreSQL 数据库。
2. 在业务低峰期执行 [PostgreSQL-规则决策表升级脚本-7000-to-7001.sql](file:///d:/Project/MDD/9.0/nactivity/docs/PostgreSQL-%E8%A7%84%E5%88%99%E5%86%B3%E7%AD%96%E8%A1%A8%E5%8D%87%E7%BA%A7%E8%84%9A%E6%9C%AC-7000-to-7001.sql)。
3. 校验四张表与索引是否存在。
4. 再部署包含本次后端改动的应用版本。

## 自动升级链路

- 若宿主环境允许自动升级，可改用引擎内置 schema 资源：
  - 创建脚本：[activiti.postgres.create.engine.sql](file:///d:/Project/MDD/9.0/nactivity/NActiviti/Sys.Bpm.Engine/resources/db/create/activiti.postgres.create.engine.sql)
  - 升级脚本：[activiti.postgres.upgradestep.7000.to.7001.engine.sql](file:///d:/Project/MDD/9.0/nactivity/NActiviti/Sys.Bpm.Engine/resources/db/upgrade/activiti.postgres.upgradestep.7000.to.7001.engine.sql)
  - 删除脚本：[activiti.postgres.drop.engine.sql](file:///d:/Project/MDD/9.0/nactivity/NActiviti/Sys.Bpm.Engine/resources/db/drop/activiti.postgres.drop.engine.sql)
  - 版本配置：[activiti.cfg.json](file:///d:/Project/MDD/9.0/nactivity/NActiviti/Sys.Bpm.Engine/resources/activiti.cfg.json#L61-L64)

## 校验建议

- 校验 `schema.version` 最终应能与应用版本链兼容。
- 校验 `process_decision_reference` 可执行读写：
  - 保存 BPMN 草稿后应产生草稿引用索引。
  - 正式部署 BPMN 后应产生正式引用索引。
  - 删除部署后索引应被清理。
- 校验 DMN 工作区 `decision-references` 查询优先命中索引，索引不完整时可回退 BPMN XML 扫描。
