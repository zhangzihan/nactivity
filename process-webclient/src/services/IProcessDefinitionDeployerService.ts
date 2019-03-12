import { IProcessDefinitionDeployer } from "./IProcessDefinitionDeployer";
import { IDeploymentQuery } from "model/query/IDeploymentQuery";
import { IResources } from "model/query/IResources";


export interface IProcessDefinitionDeployerService {

  /** 
   * @description 获取最终部署的流程定义
   * @param query 查询对象
  */
  latest(query: IDeploymentQuery): Promise<IResources<any>>;

  /**
   * @description 部署流程
   * @param deployer 流程部署
   */
  deploy(deployer: IProcessDefinitionDeployer): Promise<any>;

  /**
   * @description 保存为草稿
   * @param deployer 流程部署
   */
  save(deployer: IProcessDefinitionDeployer): Promise<any>;

  /**
   * @description 获取所有部署的流程包含草稿流程
   * @param query 查询对象
   */
  allDeployments(query: IDeploymentQuery): Promise<IResources<any>>;

  /**
   * @description 删除部署流程,如果有已存在的实例,后台会抛出异常
   * @param id 部署id
   */
  remove(id: string): Promise<any>;

  /**
   * @description 读取模型BMPNXML
   * @param id 部署id
   */
  getProcessModel(id: string): Promise<string>;
}
