import { Trash2 } from "lucide-react"
import { formatShortDate } from "../lib/formatDate"
import type { Task, TaskStatus } from "../types/task"
import { StatusSelect } from "./StatusSelect"

interface TaskCardProps {
  task: Task
  onStatusChange: (status: TaskStatus) => void
  onDelete: () => void
  isUpdating: boolean
  isDeleting: boolean
}

export function TaskCard({ task, onStatusChange, onDelete, isUpdating, isDeleting }: TaskCardProps) {
  return (
    <li
      className={`group rounded-xl border border-border bg-surface-2 p-3 transition-[opacity,border-color,box-shadow] duration-200 hover:border-primary/30 hover:shadow-sm ${isDeleting ? "opacity-0" : "opacity-100"}`}
    >
      <div className="flex items-start justify-between gap-2">
        <h3 className="text-sm font-medium text-ink">{task.title}</h3>
        <button
          type="button"
          onClick={onDelete}
          aria-label={`Excluir "${task.title}"`}
          className="shrink-0 rounded-md p-1 text-muted opacity-0 transition-colors duration-150 group-hover:opacity-100 hover:bg-danger/10 hover:text-danger focus-visible:opacity-100"
        >
          <Trash2 size={14} />
        </button>
      </div>

      {task.description ? <p className="mt-1 text-sm text-muted">{task.description}</p> : null}

      <div className="mt-3 flex items-center justify-between gap-2">
        <StatusSelect value={task.status} onChange={onStatusChange} disabled={isUpdating} />
        <span className="text-xs text-muted">{formatShortDate(task.updatedAt)}</span>
      </div>
    </li>
  )
}
