import { STATUS_META } from "../lib/statusMeta"
import type { CreateTaskInput, Task, TaskStatus } from "../types/task"
import { CreateTaskForm } from "./CreateTaskForm"
import { EmptyState } from "./EmptyState"
import { TaskCard } from "./TaskCard"

interface TaskColumnProps {
  status: TaskStatus
  tasks: Task[]
  onStatusChange: (task: Task, status: TaskStatus) => void
  onDelete: (task: Task) => void
  onCreate?: (input: CreateTaskInput) => void
  isCreating: boolean
  updatingTaskId: number | null
  deletingTaskId: number | null
}

export function TaskColumn({
  status,
  tasks,
  onStatusChange,
  onDelete,
  onCreate,
  isCreating,
  updatingTaskId,
  deletingTaskId,
}: TaskColumnProps) {
  return (
    <section className="flex min-w-[85vw] snap-start flex-col rounded-xl bg-surface p-3 sm:min-w-[320px] lg:min-w-0 lg:flex-1">
      <header className="mb-3 flex items-center justify-between px-1">
        <h2 className="text-xs font-semibold uppercase tracking-wide text-muted">{STATUS_META[status].label}</h2>
        <span className="text-xs font-medium text-muted">{tasks.length}</span>
      </header>

      <ul className="flex flex-col gap-2">
        {tasks.map((task) => (
          <TaskCard
            key={task.id}
            task={task}
            onStatusChange={(newStatus) => onStatusChange(task, newStatus)}
            onDelete={() => onDelete(task)}
            isUpdating={updatingTaskId === task.id}
            isDeleting={deletingTaskId === task.id}
          />
        ))}
      </ul>

      {tasks.length === 0 ? (
        <EmptyState message={`Nenhuma task em "${STATUS_META[status].label}" ainda.`} />
      ) : null}

      {onCreate ? (
        <div className="mt-2">
          <CreateTaskForm onSubmit={onCreate} isSubmitting={isCreating} />
        </div>
      ) : null}
    </section>
  )
}
