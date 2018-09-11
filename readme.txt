
项目说明：vs2017 15.7及以上,使用.net core sdk 2.1.
Sys.Bpm.Model:遵循BPMN2.0规范xml模型文件解析.
Sys.Bpm.Engine:activiti流程引擎,转换了java的主要代码.
Sys.Bpm.Rest:流程建模及流程管理rest服务,没有完成.

SmartSql是第三方开源库,主要提供java mybaits的.net实现，activiti的orm使用的是mybaits

Spring.Core github 官方已经不维护了，主要使用表达式解析那块的相关api做SmartSQL动态条件解析用.

数据库创建脚本：Sys.Bpm.Engine\db\activiti.sql
BpmnTest是个控制台项目,仅只是写了一些简单的测试.