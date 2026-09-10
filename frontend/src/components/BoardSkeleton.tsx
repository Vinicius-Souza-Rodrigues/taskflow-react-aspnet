const PLACEHOLDER_COLUMNS = [0, 1, 2]
const PLACEHOLDER_CARDS = [0, 1]

export function BoardSkeleton() {
  return (
    <div className="flex gap-4 overflow-hidden" aria-hidden="true">
      {PLACEHOLDER_COLUMNS.map((column) => (
        <div
          key={column}
          className="flex min-w-[85vw] flex-col gap-2 rounded-xl bg-surface p-3 sm:min-w-[320px] lg:min-w-0 lg:flex-1"
        >
          <div className="mb-2 h-4 w-20 animate-pulse rounded bg-border" />
          {PLACEHOLDER_CARDS.map((card) => (
            <div key={card} className="h-20 animate-pulse rounded-xl bg-surface-2" />
          ))}
        </div>
      ))}
    </div>
  )
}
