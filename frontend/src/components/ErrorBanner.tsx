interface ErrorBannerProps {
  message: string
}

export function ErrorBanner({ message }: ErrorBannerProps) {
  return (
    <div role="alert" className="rounded-lg border border-border bg-surface px-4 py-3 text-sm text-ink">
      {message}
    </div>
  )
}
