import { ITaskQuery } from "model/query/ITaskQuery";

export interface ITaskAdminService {
  /// <summary>
  /// 获取所有有任务
  /// </summary>
  /// <param cref="TaskQuery" name="query">分页</param>
  /// <returns></returns>
  getAllTasks(query: ITaskQuery): Promise<any>;

  /// <summary>
  /// 重新指派流程节点执行人，该操作由管理员操作。该节点将终止当前所有待办任务.
  /// 重新由该节点处执行流程.
  /// </summary>
  /// <param cref="ReassignTaskUserCmd" name="cmd">操作命令</param>
  /// <returns></returns>
  reassignTaskUser(cmd: any): Promise<any>;
}
