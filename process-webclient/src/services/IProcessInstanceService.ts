import { IProcessInstanceQuery } from "model/query/IProcessInstanceQuery";
import { IStartProcessInstanceCmd } from "model/cmd/IStartProcessInstanceCmd";
import { ISignalCmd } from "model/cmd/ISignalCmd";
import { IResources } from "model/query/IResources";
import { IProcessInstance } from "model/resource/IProcessInstance";
import { IProcessInstanceTaskQuery } from "model/query/IProcessInstanceTaskQuery";
import { ITaskModel } from "model/resource/ITaskModel";

export interface IProcessInstanceService {

  /***
   * @description 获取流程实例
    /// <summary>
    /// 获取流程实例
    /// </summary>
    /// <param name="query">查询对象 ProcessInstanceQuery</param>
    /// <returns>Resources<ProcessInstance></returns>
   */
  processInstances(query: IProcessInstanceQuery): Promise<IResources<IProcessInstance>>;

  /***
   * @description 启动一个流程
    /// <summary>
    /// 启动一个流程
    /// </summary>
    /// <param name="cmd">流程启动命令 StartProcessInstanceCmd</param>
    /// <returns>ProcessInstance</returns>
   */
  start(cmd: IStartProcessInstanceCmd): Promise<IProcessInstance>;

  /***
   * @description 查找一个流程实例
    /// <summary>
    /// 
    /// </summary>
    /// <param name="processInstanceId">流程实例id</param>
    /// <returns>Resource<ProcessInstance></returns>
   */
  getProcessInstanceById(processInstanceId: string): Promise<IProcessInstance>;

  /***
   * @description 获取流程图,未实现
    /// <summary>
    /// 
    /// </summary>
    /// <param name="processInstanceId">流程实例id</param>
    /// <returns></returns>
   */
  getProcessDiagram(processInstanceId: string): Promise<string>;

  /***
   * @description 发送流程信号
    /// <summary>
    /// 发送流程信号
    /// </summary>
    /// <param name="cmd">信号命令 SignalCmd</param>
    /// <returns></returns>
   */
  sendSignal(cmd: ISignalCmd): Promise<any>;

  /***
   * @description 挂起流程实例
    /// <summary>
    /// 挂起流程实例
    /// </summary>
    /// <param name="processInstanceId">流程实例id</param>
    /// <returns></returns>
   */
  suspend(processInstanceId: string): Promise<any>;

  /***
   * @description 激活挂起的流程实例
    /// <summary>
    /// 激活挂起的流程实例
    /// </summary>
    /// <param name="processInstanceId">流程实例id</param>
    /// <returns></returns>
   */
  activate(processInstanceId: string): Promise<any>;

  /***
   * @description 终止流程实例
    /// <summary>
    /// 
    /// </summary>
    /// <param name="processInstanceId">流程实例id</param>
    /// <param name="reason">终止原因</param>
    /// <returns></returns>
   */
  terminate(processInstanceId: string, reason: string): Promise<any>;

  /// <summary>
  /// 获取某个流程所有的任务
  /// </summary>
  /// <param name="processInstanceId">流程实例id</param>
  /// <param name="ProcessInstanceTaskQuery">流程任务查询对象</param>
  /// <returns>Resources<TaskModel></returns>
  getTasks(processInstanceId: string, query: IProcessInstanceTaskQuery): Promise<IResources<ITaskModel>>;
}
