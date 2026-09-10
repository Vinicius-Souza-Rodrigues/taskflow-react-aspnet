import { Plus, X } from "lucide-react"
import { useState, type FormEvent } from "react"
import type { CreateTaskInput } from "../types/task"

interface CreateTaskFormProps {
  onSubmit: (input: CreateTaskInput) => void
  isSubmitting: boolean
}

export function CreateTaskForm({ onSubmit, isSubmitting }: CreateTaskFormProps) {
  const [isOpen, setIsOpen] = useState(false)
  const [title, setTitle] = useState("")
  const [description, setDescription] = useState("")
  const [error, setError] = useState<string | null>(null)

  function handleSubmit(event: FormEvent) {
    event.preventDefault()

    if (!title.trim()) {
      setError("Título é obrigatório.")
      return
    }

    onSubmit({ title: title.trim(), description: description.trim() || null })
    setTitle("")
    setDescription("")
    setError(null)
    setIsOpen(false)
  }

  if (!isOpen) {
    return (
      <button
        type="button"
        onClick={() => setIsOpen(true)}
        className="flex w-full items-center gap-1.5 rounded-lg px-2 py-2 text-sm font-medium text-muted transition-colors duration-150 hover:bg-surface-2 hover:text-ink"
      >
        <Plus size={16} />
        Adicionar task
      </button>
    )
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-2 rounded-lg border border-border bg-bg p-2.5">
      <input
        autoFocus
        type="text"
        value={title}
        onChange={(event) => setTitle(event.target.value)}
        placeholder="Título da task"
        maxLength={200}
        aria-invalid={error !== null}
        className={`rounded-md border bg-bg px-2.5 py-1.5 text-sm text-ink placeholder:text-muted ${error ? "border-danger" : "border-border"}`}
      />
      <textarea
        value={description}
        onChange={(event) => setDescription(event.target.value)}
        placeholder="Descrição (opcional)"
        rows={2}
        className="resize-none rounded-md border border-border bg-bg px-2.5 py-1.5 text-sm text-ink placeholder:text-muted"
      />

      {error ? <p className="text-xs text-danger">{error}</p> : null}

      <div className="flex items-center gap-2">
        <button
          type="submit"
          disabled={isSubmitting}
          className="rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-white transition-colors duration-150 hover:bg-primary-hover disabled:opacity-50"
        >
          {isSubmitting ? "Criando..." : "Criar"}
        </button>
        <button
          type="button"
          onClick={() => setIsOpen(false)}
          aria-label="Cancelar"
          className="rounded-md p-1.5 text-muted transition-colors duration-150 hover:bg-surface-2"
        >
          <X size={16} />
        </button>
      </div>
    </form>
  )
}
