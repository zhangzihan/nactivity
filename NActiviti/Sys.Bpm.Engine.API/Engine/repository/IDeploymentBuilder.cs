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
namespace org.activiti.engine.repository
{
    using java.util.zip;
    using org.activiti.bpmn.model;

    /// <summary>
    /// Builder for creating new deployments.
    /// 
    /// A builder instance can be obtained through <seealso cref="org.activiti.engine.IRepositoryService#createDeployment()"/>.
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

        IDeploymentBuilder addInputStream(string resourceName, System.IO.Stream inputStream);

        IDeploymentBuilder addClasspathResource(string resource);

        IDeploymentBuilder addString(string resourceName, string text);

        IDeploymentBuilder addBytes(string resourceName, byte[] bytes);

        IDeploymentBuilder addZipInputStream(ZipInputStream zipInputStream);

        IDeploymentBuilder addBpmnModel(string resourceName, BpmnModel bpmnModel);

        /// <summary>
        /// If called, no XML schema validation against the BPMN 2.0 XSD.
        /// 
        /// Not recommended in general.
        /// </summary>
        IDeploymentBuilder disableSchemaValidation();

        /// <summary>
        /// If called, no validation that the process definition is executable on the engine will be done against the process definition.
        /// 
        /// Not recommended in general.
        /// </summary>
        IDeploymentBuilder disableBpmnValidation();

        /// <summary>
        /// Gives the deployment the given name.
        /// </summary>
        IDeploymentBuilder name(string name);

        /// <summary>
        /// 设置业务键值
        /// </summary>
        /// <param name="businessKey"></param>
        /// <returns></returns>
        IDeploymentBuilder businessKey(string businessKey);

        /// <summary>
        /// 设置业务路径
        /// </summary>
        /// <param name="businessPath"></param>
        /// <returns></returns>
        IDeploymentBuilder businessPath(string businessPath);

        /// <summary>
        /// 设置启动表单
        /// </summary>
        /// <param name="startForm">开始表单，如果部署是未指定开始表单，则使用bpmnXML的开始节点FormKey</param>
        /// <returns>流程定义XML</returns>
        /// <exception cref="org.activiti.engine.repository.StartFormNullException">表单Null异常</exception>
        IDeploymentBuilder startForm(string startForm, string bpmnXML);

        IDeploymentBuilder disableDuplicateStartForm();

        /// <summary>
        /// Gives the deployment the given category.
        /// </summary>
        IDeploymentBuilder category(string category);

        /// <summary>
        /// Gives the deployment the given key.
        /// </summary>
        IDeploymentBuilder key(string key);

        /// <summary>
        /// Gives the deployment the given tenant id.
        /// </summary>
        IDeploymentBuilder tenantId(string tenantId);

        /// <summary>
        /// If set, this deployment will be compared to any previous deployment. This means that every (non-generated) resource will be compared with the provided resources of this deployment.
        /// </summary>
        IDeploymentBuilder enableDuplicateFiltering();

        /// <summary>
        /// Sets the date on which the process definitions contained in this deployment will be activated. This means that all process definitions will be deployed as usual, but they will be suspended from
        /// the start until the given activation date.
        /// </summary>
        IDeploymentBuilder activateProcessDefinitionsOn(DateTime date);

        /// <summary>
        /// Allows to add a property to this <seealso cref="IDeploymentBuilder"/> that influences the deployment.
        /// </summary>
        IDeploymentBuilder deploymentProperty(string propertyKey, object propertyValue);

        /// <summary>
        /// Deploys all provided sources to the Activiti engine.
        /// </summary>
        IDeployment deploy();

        /// <summary>
        /// 保存为草稿
        /// </summary>
        /// <returns></returns>
        IDeployment save();

        /// <summary>
        /// 复制流程BpmnXML,使用选择的流程id复制一个新的流程BpmnXML.
        /// </summary>
        /// <param name="id">流程定义id</param>
        /// <param name="fullCopy">true:完全复制.false:仅复制节点,不保留节点业务属性</param>
        /// <returns></returns>
        string copy(string id, bool fullCopy);
    }

}