
export interface IStartProcessInstanceCmd {
  /// <summary>
  /// 
  /// </summary>
  processDefinitionKey?: string;

  /// <summary>
  /// 流程实例业务名称,方便查询显示.
  /// </summary>
  processInstanceName?: string;

  tenantId?: string;

  /// <summary>
  /// 流程定义id
  /// </summary>
  processDefinitionId?: string;

  /// <summary>
  /// 流程启动变量
  /// </summary>
  variables?: any;

  /// <summary>
  /// 业务键值,主要用来保存启动流程时的业务主键,可以是主键id，
  /// 也可以是表单编号,同一流程只要保持唯一性就可以了
  /// </summary>
  businessKey?: string;

  /// <summary>
  /// 启动表单,根据表单判断
  /// </summary>
  startForm?: string;
}
