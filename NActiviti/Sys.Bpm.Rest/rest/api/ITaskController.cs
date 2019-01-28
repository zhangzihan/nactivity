using Microsoft.AspNetCore.Mvc;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.rest.api.resources;
using org.springframework.data.domain;
using org.springframework.hateoas;
using System.Collections.Generic;

namespace org.activiti.cloud.services.rest.api
{
    public interface ITaskController
    {
        PagedResources<TaskResource> getTasks(Pageable pageable);

        Resource<Task> getTaskById(string taskId);

        /// <summary>
        /// 我的待办项
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <returns>待办项列表</returns>
        System.Threading.Tasks.Task<IList<TaskResource>> MyTasks(string userId);

        Resource<Task> claimTask(string taskId);

        Resource<Task> releaseTask(string taskId);

        System.Threading.Tasks.Task<IActionResult> completeTask(string taskId, CompleteTaskCmd completeTaskCmd);

        void deleteTask(string taskId);

        Resource<Task> createNewTask(CreateTaskCmd createTaskCmd);

        IActionResult updateTask(string taskId, UpdateTaskCmd updateTaskCmd);

        Resource<Task> createSubtask(string taskId, CreateTaskCmd createSubtaskCmd);

        Resources<TaskResource> getSubtasks(string taskId);
    }
}