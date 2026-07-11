import { QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { Toaster } from 'sonner'
import './App.css'
import { queryClient } from './lib/query-client'
import { SidebarProvider } from './lib/sidebar-context'
import { AppSidebar } from './components/app-sidebar'
import { AdminLayout } from './components/layout/admin-layout'
import { flattenMenuItems, adminMenuGroups } from './lib/admin-menu'

// ── Generate routes from menu configuration ───────────
const flatMenuItems = flattenMenuItems(adminMenuGroups)
const pageRoutes = flatMenuItems.map((item) => ({
  path: item.key,
  element: item.component ? <item.component /> : null,
}))

function App() {
  return (
    <BrowserRouter>
      <SidebarProvider>
        <QueryClientProvider client={queryClient}>
          <Routes>
            {/* Admin Layout */}
            <Route path="/" element={<AdminLayout />}>
              {/* Redirect root to dashboard */}
              <Route index element={<Navigate to="/dashboard" replace />} />

              {/* Dynamic routes from menu config */}
              {pageRoutes.map(
                (route) =>
                  route.element && (
                    <Route
                      key={route.path}
                      path={route.path.replace(/^\//, '')}
                      element={route.element}
                    />
                  ),
              )}

              {/* Catch-all */}
              <Route
                path="*"
                element={
                  <div className="flex h-full items-center justify-center text-slate-400">
                    <p className="text-lg">404 — Trang không tồn tại</p>
                  </div>
                }
              />
            </Route>
          </Routes>

          <Toaster position="top-right" richColors />
          <AppSidebar />
        </QueryClientProvider>
      </SidebarProvider>
    </BrowserRouter>
  )
}

export default App
