import { IResources } from "model/query/IResources";
import { IHistoricInstance } from "model/resource/IHistoricInstance";
import { IHistoricInstanceQuery } from "model/query/IHistoricInstanceQuery";

export interface IProcessInstanceHistoriceService {

  /***  
    /// <summary>
    /// 获取流程历史实例
    /// </summary>
    /// <param name="query">查询对象</param>
    /// <returns>实例列表</returns>
   */
  processInstances(query: IHistoricInstanceQuery): Promise<IResources<IHistoricInstance>>;

  /**
    /// <summary>
    /// 查找一个流程历史实例
    /// </summary>
    /// <param name="processInstanceId">流程实例id</param>
    /// <returns>历史实例</returns>
   */
  getProcessInstanceById(processInstanceId: string): Promise<IHistoricInstance>;
}
