
export interface IProcessDefinitionDeployer {
  /**
   * @description 是否启用BPMN2.0模式验证
   */
  disableSchemaValidation?: boolean;

  /**
   * @description 是否验证流程
   */
  disableBpmnValidation?: boolean;

  /**
   * @description 模型名称
   */
  name: string;

  /**
   * @description 模型XML schema 默认使用 "http://camunda.org/schema/1.0/bpmn20"
   */
  category?: string;

  /**
   * @description 模型唯一标识
   */
  key?: string;

  /**
   * @description 模型所属租户id
   */
  tenantId?: string;

  /**
   * @description 如果启用，此部署将与以前的任何部署进行比较。
  * 这意味着每个(非生成的)资源都将与此部署提供的资源进行比较
  * 如果与已有部署相同,则不会产生新的部署，默认启用
   */
  enableDuplicateFiltering?: boolean;

  /**
   * @description BPMN2.0模型定义XML字符串
   */
  bpmnXML: string;

  /**
   * @description 业务键值
   */
  businessKey: string;

  /**
   * @description 业务路径
   */
  businessPath?: string;

  /**
   * @description 启动表单
   */
  startForm?: string
}
