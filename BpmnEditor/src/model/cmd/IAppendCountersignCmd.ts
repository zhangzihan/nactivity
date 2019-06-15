

export interface IAppendCountersignCmd {
  name?: string;
  description?: string;
  dueDate?: Date;
  priority?: number;
  assignees: Array<string>;
  taskId: string;
}
