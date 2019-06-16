using System;

/* Licensed under the Apache License, Version 2.0 (the "License");
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
 */
namespace Sys.Workflow.engine.repository
{
    using java.util.zip;
    using Sys.Workflow.bpmn.model;
    using Sys.Workflow.engine.impl.persistence.entity;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Builder for creating new deployments.
    /// 
    /// A builder instance can be obtained through <seealso cref="Sys.Workflow.engine.IRepositoryService#createDeployment()"/>.
    /// 
    /// Multiple resources can be added to one deployment before calling the <seealso cref="#deploy()"/> operation.
    /// 
    /// After deploying, no more changes can be made to the returned deployment and the builder instance can be disposed.
    /// 
    /// 
    /// 
    /// </summary>
    public interface IDeploymentBuilder
    {
        /// <summary>
        /// 提交部署后返回的部署实体
        /// </summary>
        IDeploymentEntity Deployment { get; }

        /// <summary>
        /// 是否启用流程验证
        /// </summary>
        bool ProcessValidationEnabled { get; }

        /// <summary>
        /// 是否启用bpmn2 xsd验证，待实现
        /// </summary>
        bool Bpmn20XsdValidationEnabled { get; }

        /// <summary>
        /// 是否启用重复性校验
        /// </summary>
        bool DuplicateFilterEnabled { get; }

        /// <summary>
        /// 流程定义激活日期
        /// </summary>
        DateTime? ProcessDefinitionsActivationDate { get; }

        /// <summary>
        /// 流程部署时的附加变量
        /// </summary>
        IDictionary<string, object> DeploymentProperties { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        IDeploymentBuilder AddInputStream(string resourceName, Stream inputStream);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        IDeploymentBuilder AddClasspathResource(string resource);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        IDeploymentBuilder AddString(string resourceName, string text);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        IDeploymentBuilder AddBytes(string resourceName, byte[] bytes);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="zipInputStream"></param>
        /// <returns></returns>
        IDeploymentBuilder AddZipInputStream(ZipInputStream zipInputStream);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="bpmnModel"></param>
        /// <returns></returns>
        IDeploymentBuilder AddBpmnModel(string resourceName, BpmnModel bpmnModel);

        /// <summary>
        /// If called, no XML schema validation against the BPMN 2.0 XSD.
        /// 
        /// Not recommended in general.
        /// </summary>
        IDeploymentBuilder DisableSchemaValidation();

        /// <summary>
        /// If called, no validation that the process definition is executable on the engine will be done against the process definition.
        /// 
        /// Not recommended in general.
        /// </summary>
        IDeploymentBuilder DisableBpmnValidation();

        /// <summary>
        /// Gives the deployment the given name.
        /// </summary>
        IDeploymentBuilder Name(string name);

        /// <summary>
        /// 设置业务键值
        /// </summary>
        /// <param name="businessKey"></param>
        /// <returns></returns>
        IDeploymentBuilder BusinessKey(string businessKey);

        /// <summary>
        /// 设置业务路径
        /// </summary>
        /// <param name="businessPath"></param>
        /// <returns></returns>
        IDeploymentBuilder BusinessPath(string businessPath);

        /// <summary>
        /// 设置启动表单
        /// </summary>
        /// <param name="startForm">开始表单，如果部署是未指定开始表单，则使用bpmnXML的开始节点FormKey</param>
        /// <returns>流程定义XML</returns>
        /// <exception cref="Sys.Workflow.engine.repository.StartFormNullException">表单Null异常</exception>
        IDeploymentBuilder StartForm(string startForm, string bpmnXML);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IDeploymentBuilder DisableDuplicateStartForm();

        /// <summary>
        /// Gives the deployment the given category.
        /// </summary>
        IDeploymentBuilder Category(string category);

        /// <summary>
        /// Gives the deployment the given key.
        /// </summary>
        IDeploymentBuilder Key(string key);

        /// <summary>
        /// Gives the deployment the given tenant id.
        /// </summary>
        IDeploymentBuilder TenantId(string tenantId);

        /// <summary>
        /// If set, this deployment will be compared to any previous deployment. This means that every (non-generated) resource will be compared with the provided resources of this deployment.
        /// </summary>
        IDeploymentBuilder EnableDuplicateFiltering();

        /// <summary>
        /// Sets the date on which the process definitions contained in this deployment will be activated. This means that all process definitions will be deployed as usual, but they will be suspended from
        /// the start until the given activation date.
        /// </summary>
        IDeploymentBuilder ActivateProcessDefinitionsOn(DateTime date);

        /// <summary>
        /// Allows to add a property to this <seealso cref="IDeploymentBuilder"/> that influences the deployment.
        /// </summary>
        IDeploymentBuilder DeploymentProperty(string propertyKey, object propertyValue);

        /// <summary>
        /// Deploys all provided sources to the Activiti engine.
        /// </summary>
        IDeployment Deploy();

        /// <summary>
        /// 保存为草稿
        /// </summary>
        /// <returns></returns>
        IDeployment Save();

        /// <summary>
        /// 复制流程BpmnXML,使用选择的流程id复制一个新的流程BpmnXML.
        /// </summary>
        /// <param name="id">流程定义id</param>
        /// <param name="fullCopy">true:完全复制.false:仅复制节点,不保留节点业务属性</param>
        /// <returns></returns>
        string Copy(string id, bool fullCopy);
    }
}