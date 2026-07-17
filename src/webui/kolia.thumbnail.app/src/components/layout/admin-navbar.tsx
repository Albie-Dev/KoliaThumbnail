import { Bell, Search, LogOut } from 'lucide-react'
import { useLocation } from 'react-router-dom'
import { Button } from '../ui/button'
import { ThemeToggle } from '../ui/theme-toggle'
import { cn } from '../../lib/utils'
import { adminMenuGroups } from '../../lib/admin-menu'
import type { AdminMenuItem } from '../../types/admin-layout.types'

// ── Props ──────────────────────────────────────────────
interface AdminNavbarProps {
  sidebarCollapsed: boolean
}

// ── Walk menu tree to build breadcrumb labels ─────────
interface BreadcrumbSegment {
  label: string
  key: string
}

function buildBreadcrumb(path: string): BreadcrumbSegment[] {
  const segments: BreadcrumbSegment[] = []

  function walk(items: AdminMenuItem[]): boolean {
    for (const item of items) {
      const fullKey = item.key
      // Check if this item's key is a prefix of the current path
      if (path.startsWith(fullKey) && fullKey !== '/') {
        segments.push({ label: item.label, key: fullKey })

        if (fullKey === path) return true // exact match → done

        // Recurse into children
        if (item.children) {
          if (walk(item.children)) return true
        }

        // Not a complete match → backtrack
        segments.pop()
      }
    }
    return false
  }

  for (const group of adminMenuGroups) {
    if (walk(group.items)) break
  }

  return segments
}

// ── Breadcrumb ────────────────────────────────────────
function Breadcrumb() {
  const location = useLocation()
  const path = location.pathname
  const segments = buildBreadcrumb(path)

  if (segments.length === 0) {
    return <span className="text-lg font-semibold text-slate-900 dark:text-slate-100">Bảng điều khiển</span>
  }

  return (
    <nav className="flex items-center gap-1.5 text-sm">
      {segments.map((seg, idx) => {
        const isLast = idx === segments.length - 1
        return (
          <span key={seg.key} className="flex items-center gap-1.5">
            {idx > 0 && <span className="text-slate-300 dark:text-slate-600">/</span>}
            <span
              className={cn(
                isLast ? 'font-semibold text-slate-900 dark:text-slate-100' : 'text-slate-500 dark:text-slate-400',
              )}
            >
              {seg.label}
            </span>
          </span>
        )
      })}
    </nav>
  )
}

// ── Navbar Component ──────────────────────────────────
export function AdminNavbar({ sidebarCollapsed }: AdminNavbarProps) {
  return (
    <header
      className={cn(
        'fixed top-0 right-0 z-20 flex h-14 items-center gap-4 border-b border-slate-200 dark:border-slate-700 bg-white/80 dark:bg-slate-900/80 px-4 backdrop-blur-md transition-all duration-300 ease-in-out',
        sidebarCollapsed ? 'left-16' : 'left-64',
      )}
    >
      {/* Left: Breadcrumb */}
      <div className="flex-1">
        <Breadcrumb />
      </div>

      {/* Right: Actions */}
      <div className="flex items-center gap-1">
        {/* Search */}
        <Button variant="ghost" size="sm" className="text-slate-500 dark:text-slate-400">
          <Search className="h-4 w-4" />
          <span className="ml-1 hidden md:inline">Tìm kiếm…</span>
          <kbd className="ml-2 hidden rounded border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-900 px-1.5 py-0.5 text-[10px] font-medium text-slate-400 dark:text-slate-500 md:inline">
            Ctrl+K
          </kbd>
        </Button>

        {/* Theme toggle: Sáng / Tối / Hệ thống */}
        <ThemeToggle />

        {/* Notifications */}
        <Button variant="ghost" size="sm" className="relative text-slate-500 dark:text-slate-400">
          <Bell className="h-4 w-4" />
          <span className="absolute -right-0.5 -top-0.5 flex h-4 w-4 items-center justify-center rounded-full bg-red-500 text-[9px] font-bold text-white">
            3
          </span>
        </Button>

        {/* User avatar */}
        <div className="ml-2 flex items-center gap-2 border-l border-slate-200 dark:border-slate-700 pl-3">
          <div className="flex h-8 w-8 items-center justify-center rounded-full bg-indigo-100 dark:bg-indigo-900/40 text-sm font-semibold text-indigo-700 dark:text-indigo-300">
            A
          </div>
          <div className="hidden text-left md:block">
            <p className="text-sm font-medium leading-tight text-slate-900 dark:text-slate-100">Admin</p>
            <p className="text-[11px] leading-tight text-slate-500 dark:text-slate-400">admin@kolia.io</p>
          </div>
          <Button variant="ghost" size="sm" className="text-slate-400 dark:text-slate-500">
            <LogOut className="h-4 w-4" />
          </Button>
        </div>
      </div>
    </header>
  )
}
