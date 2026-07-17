import { useState } from 'react'
import { Outlet } from 'react-router-dom'
import { AdminSidebar } from './admin-sidebar'
import { AdminNavbar } from './admin-navbar'
import { AdminFooter } from './admin-footer'

// ── AdminLayout ───────────────────────────────────────
// Wraps the entire enterprise UI with:
//   - Fixed left sidebar (collapsible)
//   - Fixed top navbar
//   - Scrollable body (renders <Outlet /> or matched page)
//   - Footer
export function AdminLayout() {
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false)

  return (
    <div className="h-screen bg-slate-50 dark:bg-slate-900">
      {/* Sidebar */}
      <AdminSidebar
        collapsed={sidebarCollapsed}
        onToggle={() => setSidebarCollapsed((prev) => !prev)}
      />

      {/* Navbar */}
      <AdminNavbar sidebarCollapsed={sidebarCollapsed} />

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
