
export interface ICompleteTaskCmd {

  /// <summary>
  /// 任务关联的业务主键,需要和Assignee同时使用
  /// </summary>
  businessKey?: string;

  /// <summary>
  /// 任务执行者,需要和BusinessKey同时使用
  /// </summary>
  assignee?: string;

  /// <summary>
  /// 任务附加备注
  /// </summary>
  comment?: string;

  /// <summary>
  /// 提交的数据
  /// </summary>
  outputVariables?: any;

  /// <summary>
  /// 任务id
  /// </summary>
  taskId?: string;

  /// <summary>
  /// 任务名称
  /// </summary>
  taskName?: string;

  /// <summary>
  /// 变量是否仅任务可见
  /// </summary>
  localScope?: string;

  /// <summary>
  /// 未找到任务是否抛出异常
  /// </summary>
  notFoundThrowError?: boolean;

  /// <summary>
  /// 如果下一步需要从当前任务中指定人员处理,则使用这个参数
  /// </summary>
  runtimeAssigneeUser?: {
    users?: [],

    /// <summary>
    /// 流程变量字段
    /// </summary>
    field?: string
  };
}
