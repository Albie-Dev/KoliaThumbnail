import { useCallback, useState, useRef } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { ChevronDown } from 'lucide-react'
import { cn } from '../../lib/utils'
import type { AdminMenuGroup, AdminMenuItem } from '../../types/admin-layout.types'
import { adminMenuGroups } from '../../lib/admin-menu'

// ── Props ──────────────────────────────────────────────
interface AdminSidebarProps {
  collapsed: boolean
  onToggle: () => void
}

// ── Icon render helper ─────────────────────────────────
function renderIcon(icon: AdminMenuItem['icon'], className?: string) {
  if (!icon) return null
  // Handle both:
  //  - function components (typeof === 'function')
  //  - forwardRef/memo components (typeof === 'object' with $$typeof, e.g. lucide-react icons)
  if (typeof icon === 'function' || (typeof icon === 'object' && icon !== null)) {
    const IconComp = icon as React.ComponentType<{ className?: string }>
    return <IconComp className={className} />
  }
  return icon
}

// ── Single menu item (recursive for N-level) ──────────
function MenuItem({
  item,
  currentPath,
  depth,
  onNavigate,
  collapsed,
}: {
  item: AdminMenuItem
  currentPath: string
  depth: number
  onNavigate: (path: string) => void
  collapsed: boolean
}) {
  const [expanded, setExpanded] = useState(() => {
    // Auto-expand if current path starts with this item's key
    return currentPath.startsWith(item.key) && item.key !== '/'
  })

  // Sync expanded when navigating from outside (e.g. clicking another item)
  const prevPathRef = useRef(currentPath)
  if (prevPathRef.current !== currentPath) {
    prevPathRef.current = currentPath
    if (currentPath.startsWith(item.key) && item.key !== '/') {
      setExpanded(true)
    }
  }
  const hasChildren = item.children && item.children.length > 0
  const isActive = hasChildren
    ? currentPath.startsWith(item.key) && item.key !== '/'
    : currentPath === item.key

  const handleClick = useCallback(() => {
    if (hasChildren) {
      setExpanded((prev) => !prev)
    } else {
      onNavigate(item.key)
    }
  }, [hasChildren, item.key, onNavigate])

  // Collapsed mode: icon-only, no label/children
  if (collapsed) {
    return (
      <li>
        <button
          type="button"
          onClick={() => onNavigate(item.key)}
          className={cn(
            'mx-auto flex h-9 w-9 items-center justify-center rounded-lg transition-colors',
            isActive
              ? 'bg-indigo-50 text-indigo-700'
              : 'text-slate-600 hover:bg-slate-100 hover:text-slate-900',
          )}
          title={item.label}
        >
          {item.icon && renderIcon(item.icon, 'h-4 w-4')}
        </button>
      </li>
    )
  }

  return (
    <li>
      <button
        type="button"
        onClick={handleClick}
        className={cn(
          'flex w-full items-center gap-3 rounded-lg px-3 py-2 text-left text-sm font-medium transition-colors',
          isActive
            ? 'bg-indigo-50 text-indigo-700'
            : 'text-slate-600 hover:bg-slate-100 hover:text-slate-900',
        )}
        style={{ paddingLeft: `${12 + depth * 16}px` }}
      >
        {/* Icon */}
        {item.icon && (
          <span className="flex-shrink-0">
            {renderIcon(item.icon, 'h-4 w-4')}
          </span>
        )}

        {/* Label */}
        <span className="flex-1 truncate">{item.label}</span>

        {/* Expand indicator */}
        {hasChildren && (
          <ChevronDown
            className={cn(
              'h-3.5 w-3.5 flex-shrink-0 text-slate-400 transition-transform duration-200',
              expanded && 'rotate-180',
            )}
          />
        )}
      </button>

      {/* Nested children — smooth expand/collapse via CSS Grid */}
      {hasChildren && (
        <div
          className={cn(
            'grid transition-all duration-300 ease-in-out',
            expanded ? 'grid-rows-[1fr]' : 'grid-rows-[0fr]',
          )}
        >
          <div className={cn('overflow-hidden transition-opacity duration-200', expanded ? 'opacity-100' : 'opacity-0')}>
            <ul className="mt-0.5 space-y-0.5">
              {item.children!.map((child) => (
                <MenuItem
                  key={child.key}
                  item={child}
                  currentPath={currentPath}
                  depth={depth + 1}
                  onNavigate={onNavigate}
                  collapsed={false}
                />
              ))}
            </ul>
          </div>
        </div>
      )}
    </li>
  )
}

// ── Menu group ─────────────────────────────────────────
function MenuGroup({
  group,
  currentPath,
  onNavigate,
  collapsed,
}: {
  group: AdminMenuGroup
  currentPath: string
  onNavigate: (path: string) => void
  collapsed: boolean
}) {
  return (
    <div>
      {group.label && (
        collapsed ? (
          <hr className="my-3 border-t border-slate-200" />
        ) : (
          <p className="mb-1.5 px-3 text-[11px] font-semibold uppercase tracking-widest text-slate-400">
            {group.label}
          </p>
        )
      )}
      <ul className={cn(collapsed ? 'space-y-1' : 'space-y-0.5')}>
        {group.items.map((item) => (
          <MenuItem
            key={item.key}
            item={item}
            currentPath={currentPath}
            depth={0}
            onNavigate={onNavigate}
            collapsed={collapsed}
          />
        ))}
      </ul>
    </div>
  )
}

// ── Brand logo / app name ─────────────────────────────
function SidebarBrand({ collapsed }: { collapsed: boolean }) {
  return (
    <div className="flex h-14 items-center gap-3 border-b border-slate-200 px-4">
      <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-indigo-600 text-xs font-bold text-white">
        K
      </div>
      {!collapsed && (
        <span className="text-base font-bold text-slate-900">KoliaEngine</span>
      )}
    </div>
  )
}

// ── Main Sidebar component ────────────────────────────
export function AdminSidebar({ collapsed, onToggle }: AdminSidebarProps) {
  const location = useLocation()
  const navigate = useNavigate()

  const handleNavigate = useCallback(
    (path: string) => {
      navigate(path)
    },
    [navigate],
  )

  return (
    <aside
      className={cn(
        'fixed left-0 top-0 z-30 flex h-screen flex-col border-r border-slate-200 bg-white transition-all duration-300 ease-in-out',
        collapsed ? 'w-16' : 'w-64',
      )}
    >
      {/* Brand */}
      <SidebarBrand collapsed={collapsed} />

      {/* Scrollable menu area */}
      <nav className={cn('flex-1 overflow-y-auto', collapsed ? 'px-1 py-3' : 'px-2 py-4')}>
        <div className={cn(collapsed ? 'space-y-1' : 'space-y-6')}>
          {adminMenuGroups.map((group, idx) => (
            <MenuGroup
              key={group.label ?? idx}
              group={group}
              currentPath={location.pathname}
              onNavigate={handleNavigate}
              collapsed={collapsed}
            />
          ))}
        </div>
      </nav>

      {/* Collapse toggle — fixed-height footer */}
      <div className="relative flex h-10 items-center border-t border-slate-200">
        <button
          type="button"
          onClick={onToggle}
          className={cn(
            'flex items-center justify-center rounded-lg text-slate-500 hover:bg-slate-100 hover:text-slate-700 transition-colors',
            collapsed
              ? 'mx-auto my-2 h-8 w-8'
              : 'absolute -right-3 top-1/2 -translate-y-1/2 h-7 w-7 rounded-full border border-slate-200 bg-white shadow-sm',
          )}
          title={collapsed ? 'Mở rộng menu' : 'Thu gọn menu'}
        >
          <svg
            className={cn('h-3.5 w-3.5 transition-transform duration-200', !collapsed && 'rotate-180')}
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            strokeWidth={2}
          >
            <path strokeLinecap="round" strokeLinejoin="round" d="M11 19l-7-7 7-7m8 14l-7-7 7-7" />
          </svg>
        </button>
      </div>
    </aside>
  )
}
