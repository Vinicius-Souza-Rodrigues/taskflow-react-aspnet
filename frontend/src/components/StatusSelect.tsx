import { ChevronDown } from "lucide-react"
import { STATUS_META, STATUS_ORDER } from "../lib/statusMeta"
import type { TaskStatus } from "../types/task"

interface StatusSelectProps {
  value: TaskStatus
  onChange: (status: TaskStatus) => void
  disabled?: boolean
}

export function StatusSelect({ value, onChange, disabled }: StatusSelectProps) {
  return (
    <div className="relative inline-flex">
      <select
        value={value}
        disabled={disabled}
        onChange={(event) => onChange(event.target.value as TaskStatus)}
        aria-label="Status da task"
        className={`appearance-none rounded-full border-0 py-1 pl-2.5 pr-6 text-xs font-medium transition-opacity duration-150 disabled:opacity-50 ${STATUS_META[value].badgeClass}`}
      >
        {STATUS_ORDER.map((status) => (
          <option key={status} value={status}>
            {STATUS_META[status].label}
          </option>
        ))}
      </select>
      <ChevronDown
        size={12}
        className="pointer-events-none absolute top-1/2 right-2 -translate-y-1/2 opacity-70"
      />
    </div>
  )
}
