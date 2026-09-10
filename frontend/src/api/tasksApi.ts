import type { CreateTaskInput, Task, UpdateTaskInput } from "../types/task"

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:8090"

export class ApiError extends Error {
  readonly errors: string[]

  constructor(message: string, errors: string[] = []) {
    super(message)
    this.errors = errors
  }
}

async function parseJsonOrThrow<T>(response: Response): Promise<T> {
  if (response.status === 204) {
    return undefined as T
  }

  if (response.ok) {
    return (await response.json()) as T
  }

  const body = await response.json().catch(() => null)
  const errors: string[] = body?.errors ?? []
  throw new ApiError(errors[0] ?? `Request failed with status ${response.status}`, errors)
}

export function listTasks(): Promise<Task[]> {
  return fetch(`${API_BASE_URL}/api/tasks`).then((response) => parseJsonOrThrow<Task[]>(response))
}

export function createTask(input: CreateTaskInput): Promise<Task> {
  return fetch(`${API_BASE_URL}/api/tasks`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  }).then((response) => parseJsonOrThrow<Task>(response))
}

export function updateTask(id: number, input: UpdateTaskInput): Promise<Task> {
  return fetch(`${API_BASE_URL}/api/tasks/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  }).then((response) => parseJsonOrThrow<Task>(response))
}

export function deleteTask(id: number): Promise<void> {
  return fetch(`${API_BASE_URL}/api/tasks/${id}`, { method: "DELETE" }).then((response) =>
    parseJsonOrThrow<void>(response),
  )
}
