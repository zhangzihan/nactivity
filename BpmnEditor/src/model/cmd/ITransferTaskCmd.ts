

export interface ITransferTaskCmd {
  name?: string;
  description?: string;
  dueDate?: Date;
  priority?: number;
  assignees: Array<string>;
  taskId: string;
}
