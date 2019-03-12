import { IResources } from "model/query/IResources";
import { ITaskQuery } from "model/query/ITaskQuery";
import { ITaskModel } from "model/resource/ITaskModel";
import { ICompleteTaskCmd } from "model/cmd/ICompleteTaskCmd";
import { ICreateTaskCmd } from "model/cmd/ICreateTaskCmd";
import { IUpdateTaskCmd } from "model/cmd/IUpdateTaskCmd";

export interface ITaskService {
  /// <summary>
  /// 获取所有任务
  /// </summary>
  /// <param name="query">TaskQuery</param>
  /// <returns>Resources<TaskModel> </returns>
  getTasks(query: ITaskQuery): Promise<IResources<ITaskModel>>;

  /// <summary>
  /// 读取任务
  /// </summary>
  /// <param name="taskId">任务id</param>
  /// <returns>TaskModel</returns>
  getTaskById(taskId: string): Promise<ITaskModel>;

  /// <summary>
  /// 我的待办项
  /// </summary>
  /// <param name="userId">用户id</param>
  /// <returns>待办项列表Task<Resources<TaskModel>></returns>
  myTasks(userId: string): Promise<IResources<ITaskModel>>;

  /// <summary>
  /// 领取任务
  /// </summary>
  /// <param name="taskId">任务id</param>
  /// <returns>Task<TaskModel></returns>
  claimTask(taskId: string): Promise<ITaskModel>;

  /// <summary>
  /// 释放任务,当前处理人如果不想解决该任务,可以将此任务释放掉,后续其他人员可以领取该任务
  /// </summary>
  /// <param name="taskId">任务id</param>
  /// <returns>Task<TaskModel> </returns>
  releaseTask(taskId: string): Promise<ITaskModel>;

  /// <summary>
  /// 处理人已经完该该任务
  /// </summary>
  /// <param name="taskId">任务id</param>
  /// <param name="completeTaskCmd">任务完成命令</param>
  /// <returns></returns>
  completeTask(taskId: string, completeTaskCmd: ICompleteTaskCmd): Promise<any>;

  /// <summary>
  /// 删除任务
  /// </summary>
  /// <param name="taskId">任务id</param>
  deleteTask(taskId: string): Promise<any>;

  /// <summary>
  /// 创建新的任务
  /// </summary>
  /// <param name="createTaskCmd">创建任务命令</param>
  /// <returns>Task<TaskModel> </returns>
  createNewTask(createTaskCmd: ICreateTaskCmd): Promise<ITaskModel>;

  /// <summary>
  /// 更新任务状态
  /// </summary>
  /// <param name="taskId">任务id</param>
  /// <param name="updateTaskCmd">更新任务状态</param>
  /// <returns></returns>
  updateTask(taskId: string, updateTaskCmd: IUpdateTaskCmd): Promise<any>;

  /// <summary>
  /// 创建子任务
  /// </summary>
  /// <param name="taskId">任务id</param>
  /// <param name="createSubtaskCmd">创建子任务命令</param>
  /// <returns>Task<TaskModel> </returns>
  createSubtask(taskId: string, createSubtaskCmd: ICreateTaskCmd): Promise<ITaskModel>;

  /// <summary>
  /// 读取某个任务下的所有子任务
  /// </summary>
  /// <param name="taskId">任务id</param>
  /// <returns>Task<Resources <TaskModel>></returns>
  getSubtasks(taskId: string): Promise<ITaskModel>;
}
