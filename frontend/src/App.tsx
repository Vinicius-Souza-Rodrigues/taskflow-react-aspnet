import { TaskBoard } from "./components/TaskBoard"
import { ThemeToggle } from "./components/ThemeToggle"

function App() {
  return (
    <div className="min-h-screen bg-bg">
      <header className="flex items-center justify-between border-b border-border px-4 py-4 sm:px-6">
        <h1 className="text-xl font-semibold text-ink">TaskFlow</h1>
        <ThemeToggle />
      </header>
      <main className="px-4 py-6 sm:px-6">
        <TaskBoard />
      </main>
    </div>
  )
}

export default App
