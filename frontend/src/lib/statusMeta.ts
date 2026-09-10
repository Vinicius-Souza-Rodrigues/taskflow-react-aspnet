import type { TaskStatus } from "../types/task"

interface StatusMeta {
  label: string
  badgeClass: string
}

export const STATUS_ORDER: TaskStatus[] = ["TODO", "IN_PROGRESS", "DONE"]

export const STATUS_META: Record<TaskStatus, StatusMeta> = {
  TODO: {
    label: "To do",
    badgeClass: "bg-status-todo-bg text-status-todo-text",
  },
  IN_PROGRESS: {
    label: "In progress",
    badgeClass: "bg-status-progress-bg text-status-progress-text",
  },
  DONE: {
    label: "Done",
    badgeClass: "bg-status-done-bg text-status-done-text",
  },
}
