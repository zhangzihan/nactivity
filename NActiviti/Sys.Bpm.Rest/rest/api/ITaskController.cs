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

        Resource<Task> claimTask(string taskId);

        Resource<Task> releaseTask(string taskId);

        IActionResult completeTask(string taskId, CompleteTaskCmd completeTaskCmd);

        void deleteTask(string taskId);

        Resource<Task> createNewTask(CreateTaskCmd createTaskCmd);

        IActionResult updateTask(string taskId, UpdateTaskCmd updateTaskCmd);

        Resource<Task> createSubtask(string taskId, CreateTaskCmd createSubtaskCmd);

        Resources<TaskResource> getSubtasks(string taskId);
    }
}