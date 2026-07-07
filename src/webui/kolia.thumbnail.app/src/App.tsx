import { QueryClientProvider } from '@tanstack/react-query'
import { Toaster } from 'sonner'
import './App.css'
import { queryClient } from './lib/query-client'
import { ThumbnailTable } from './features/thumbnails/thumbnail-table'

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <main className="mx-auto flex min-h-screen w-full max-w-7xl flex-col px-4 py-8 sm:px-6 lg:px-8">
        <section className="rounded-3xl border border-slate-200/80 bg-white/80 p-6 shadow-sm backdrop-blur sm:p-8">
          <ThumbnailTable />
        </section>
      </main>
      <Toaster position="top-right" richColors />
    </QueryClientProvider>
  )
}

export default App
