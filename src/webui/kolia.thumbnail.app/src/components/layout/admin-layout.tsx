import { useState, useEffect } from 'react'
import { Outlet } from 'react-router-dom'
import { AdminSidebar } from './admin-sidebar'
import { AdminNavbar } from './admin-navbar'
import { AdminFooter } from './admin-footer'

// ── AdminLayout ───────────────────────────────────────
// Wraps the entire enterprise UI with:
//   - Fixed left sidebar (collapsible, auto-collapse on small screens)
//   - Fixed top navbar
//   - Scrollable body (renders <Outlet /> or matched page)
//   - Footer
export function AdminLayout() {
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false)

  // Auto-collapse sidebar when viewport is below lg breakpoint (1024px)
  useEffect(() => {
    const mq = window.matchMedia('(max-width: 1023px)')
    const handler = (e: MediaQueryListEvent | MediaQueryList) => {
      setSidebarCollapsed(e.matches)
    }
    handler(mq) // run on mount
    mq.addEventListener('change', handler)
    return () => mq.removeEventListener('change', handler)
  }, [])

  return (
    <div className="h-screen bg-slate-50 dark:bg-slate-900">
      {/* Sidebar */}
      <AdminSidebar
        collapsed={sidebarCollapsed}
        onToggle={() => setSidebarCollapsed((prev) => !prev)}
      />

      {/* Navbar */}
      <AdminNavbar
        sidebarCollapsed={sidebarCollapsed}
        onToggleSidebar={() => setSidebarCollapsed((prev) => !prev)}
      />

      {/* Main Body — scrolls independently */}
      <main
        className={`fixed right-0 top-14 bottom-12 overflow-y-auto transition-all duration-300 ease-in-out ${
          sidebarCollapsed ? 'left-16' : 'left-64'
        }`}
      >
        <div className="min-h-full px-6 py-6">
          <Outlet />
        </div>
      </main>

      {/* Footer */}
      <AdminFooter sidebarCollapsed={sidebarCollapsed} />
    </div>
  )
}
