import { STATUS_ORDER } from "../lib/statusMeta"
import {
  useChangeTaskStatusMutation,
  useCreateTaskMutation,
  useDeleteTaskMutation,
  useTasksQuery,
} from "../hooks/useTasks"
import type { Task, TaskStatus } from "../types/task"
import { BoardSkeleton } from "./BoardSkeleton"
import { ErrorBanner } from "./ErrorBanner"
import { TaskColumn } from "./TaskColumn"

export function TaskBoard() {
  const tasksQuery = useTasksQuery()
  const createTaskMutation = useCreateTaskMutation()
  const changeStatusMutation = useChangeTaskStatusMutation()
  const deleteTaskMutation = useDeleteTaskMutation()

  if (tasksQuery.isPending) {
    return <BoardSkeleton />
  }

  if (tasksQuery.isError) {
    return <ErrorBanner message="Não foi possível carregar as tasks. Verifique se a API está no ar." />
  }

  const tasks = tasksQuery.data
  const updatingTaskId = changeStatusMutation.isPending ? (changeStatusMutation.variables?.task.id ?? null) : null
  const deletingTaskId = deleteTaskMutation.isPending ? (deleteTaskMutation.variables ?? null) : null

  function tasksByStatus(status: TaskStatus): Task[] {
    return tasks.filter((task) => task.status === status)
  }

  return (
    <div className="flex snap-x snap-mandatory gap-4 overflow-x-auto pb-2 lg:snap-none lg:overflow-visible">
      {STATUS_ORDER.map((status) => (
        <TaskColumn
          key={status}
          status={status}
          tasks={tasksByStatus(status)}
          onStatusChange={(task, newStatus) => changeStatusMutation.mutate({ task, status: newStatus })}
          onDelete={(task) => deleteTaskMutation.mutate(task.id)}
          onCreate={status === "TODO" ? (input) => createTaskMutation.mutate(input) : undefined}
          isCreating={createTaskMutation.isPending}
          updatingTaskId={updatingTaskId}
          deletingTaskId={deletingTaskId}
        />
      ))}
    </div>
  )
}
