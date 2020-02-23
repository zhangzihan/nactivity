
interface IProcessDefineService {
  latest(query: any): Promise<any>;

  getProcessModel(id): Promise<string>;

  processDefinitions(query: any): Promise<any>;
}
