/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Sys.Workflow.Hateoas;
using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Core.Pageables;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Cloud.Services.Rest.Assemblers;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System;
using Sys.Workflow.Expressions;
using System.Reflection;
using System.Linq;
using Sys.Expressions;

namespace Sys.Workflow.Cloud.Services.Rest.Controllers
{

    /// <inheritdoc />
    [Route(WorkflowConstants.PROC_ADMIN_DEF_ROUTER_V1)]
    [ApiController]
    public class ProcessDefinitionAdminControllerImpl : ControllerBase, IProcessDefinitionAdminController
    {
        private readonly ProcessDefinitionResourceAssembler resourceAssembler;

        private readonly PageableProcessDefinitionRepositoryService pageableRepositoryService;

        private readonly ExpressionTypeRegistry expressionTypeRegistry;

        //public virtual string handleAppException(ActivitiObjectNotFoundException ex)
        //{
        //    return ex.Message;
        //}

        /// <inheritdoc />

        public ProcessDefinitionAdminControllerImpl(ProcessDefinitionResourceAssembler resourceAssembler, PageableProcessDefinitionRepositoryService pageableRepositoryService, ExpressionTypeRegistry expressionTypeRegistry)
        {
            this.resourceAssembler = resourceAssembler;
            this.pageableRepositoryService = pageableRepositoryService;
            this.expressionTypeRegistry = expressionTypeRegistry;
        }

        /// <inheritdoc />

        [HttpGet]
        public virtual Resources<ProcessDefinition> GetAllProcessDefinitions(
            [FromQuery][BindingBehavior(BindingBehavior.Optional)]Pageable pageable)
        {
            IPage<ProcessDefinition> page = pageableRepositoryService.GetAllProcessDefinitions(pageable);
            //return pagedResourcesAssembler.toResource(pageable, page, resourceAssembler);
            //return new PagedResources<PagedResources<ProcessDefinitionResource>>();

            return null;
        }

        [HttpPost]
        public async Task<bool> UploadFormulaAssembly(ICollection<IFormFile> files)
        {
            string binDir = Directory.GetCurrentDirectory();

            foreach (var file in files)
            {
                string fileName = Path.Combine(binDir, file.Name);
                string oldFile = Path.Combine(binDir, Guid.NewGuid().ToString());
                try
                {
                    if (System.IO.File.Exists(fileName))
                    {
                        System.IO.File.Move(fileName, oldFile);
                    }

                    using (FileStream stream = System.IO.File.Open(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        await file.CopyToAsync(stream);
                        await stream.FlushAsync();
                    }

                    Assembly assembly = Assembly.LoadFrom(fileName);
                    IEnumerable<Type> types = assembly.GetTypes().Where(x => x.GetCustomAttribute(typeof(FormulaTypeAttribute)) != null);
                    foreach (var type in types)
                    {
                        expressionTypeRegistry.Register(type);
                    }
                }
                catch (Exception ex)
                {
                    if (System.IO.File.Exists(oldFile))
                    {
                        System.IO.File.Move(oldFile, fileName);
                    }
                }
                finally
                {
                    if (System.IO.File.Exists(oldFile))
                    {
                        System.IO.File.Delete(oldFile);
                    }
                }
            }

            return true;
        }
    }
}