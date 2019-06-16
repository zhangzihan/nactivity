using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Api.Utils;
using Sys.Workflow.Cloud.Services.Core.Pageables;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Impl;

namespace Sys.Workflow.Cloud.Services.Api.Commands
{
    /// <summary>
    /// 流程部署命令
    /// </summary>
    public class QueryDeploymentCmd : ICommand
    {
        /// <inheritdoc />
        public string Id => "queryDeploymentCmd";

        /// <summary>
        /// 读取分页记录
        /// </summary>
        /// <param name="repositoryService">仓储服务</param>
        /// <param name="pageableRepositoryService">分页仓储服务</param>
        /// <param name="qo">查询对象</param>
        /// <returns></returns>
        public IPage<Deployment> LoadPage(IRepositoryService repositoryService,
            PageableDeploymentRespositoryService pageableRepositoryService,
            DeploymentQuery qo)
        {
            DeploymentQueryImpl query = repositoryService.CreateDeploymentQuery() as DeploymentQueryImpl;

            FastCopy.Copy<DeploymentQuery, DeploymentQueryImpl>(qo, query);

            pageableRepositoryService.SortApplier.ApplySort(query, qo.Pageable);

            IPage<Deployment> defs = pageableRepositoryService.PageRetriever.LoadPage(query, qo.Pageable, pageableRepositoryService.DeploymentConverter);

            return defs;
        }
    }
}
