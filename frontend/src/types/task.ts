export type TaskStatus = "TODO" | "IN_PROGRESS" | "DONE"

export interface Task {
  id: number
  title: string
  description: string | null
  status: TaskStatus
  createdAt: string
  updatedAt: string
}

export interface CreateTaskInput {
  title: string
  description: string | null
}

export interface UpdateTaskInput {
  title: string
  description: string | null
  status: TaskStatus
}
