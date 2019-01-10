using Microsoft.AspNetCore.Mvc;
using Sys.Bpm.api.model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Bpm.rest.api
{
    public interface IProcessDefinitionDeployerController
    {
        /// <summary>
        /// 部署流程定义
        /// </summary>
        /// <param name="deployer">流程定义部署Model</param>
        /// <returns>Ok()</returns>
        Task<IActionResult> Deploy(ProcessDefinitionDeployer deployer);
    }
}
