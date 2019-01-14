
interface IProcessDefineService {
    latest() : Promise<any>;

    getProcessModel(id) : Promise<string>
}