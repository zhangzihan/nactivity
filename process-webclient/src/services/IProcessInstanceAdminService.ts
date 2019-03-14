import { IProcessInstanceQuery } from "model/query/IProcessInstanceQuery";
import { IResources } from "model/query/IResources";
import { IProcessInstance } from "model/resource/IProcessInstance";

export interface IProcessInstanceAdminService {

  /***
  /// <summary>
  /// 获取所有当前运行的流程实例
  /// </summary>
  /// <param name="query">流程实例查询对象</param>
  /// <returns>Task<Resources<IProcessInstance>> </returns>
   */
  getAllProcessInstances(query: IProcessInstanceQuery): Promise<IResources<IProcessInstance>>;
}
