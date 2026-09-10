interface EmptyStateProps {
  message: string
}

export function EmptyState({ message }: EmptyStateProps) {
  return (
    <p className="rounded-lg border border-dashed border-border p-4 text-center text-xs text-muted">{message}</p>
  )
}
