import Axios from 'axios';
import contants from 'contants';

export class ProcessDefineService implements IProcessDefineService{

    latest() {
        return new Promise((res, rej) =>{
            Axios.get(`${contants.serverUrl}/workflow/process-definitions/latest`)
                .then(data =>{
                    res(data.data.map(x => x.content));
                }).catch(err => rej(err));
        });
    }

    getProcessModel(id) : Promise<string>{
        return new Promise((res, rej) =>{
            Axios.get(`${contants.serverUrl}/workflow/process-definitions/${id}/processmodel`).then(data =>{
                return res(data.data);
            }).catch(err => rej(err));
        });
    }

}