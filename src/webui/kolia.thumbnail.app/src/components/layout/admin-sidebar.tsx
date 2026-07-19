import { useCallback, useState, useRef, useMemo } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { ChevronDown, CircleCheck, CircleX, CircleAlert, CircleMinus } from 'lucide-react'
import { cn } from '../../lib/utils'
import type { AdminMenuGroup, AdminMenuItem } from '../../types/admin-layout.types'
import { adminMenuGroups } from '../../lib/admin-menu'
import { useTheme } from '../../lib/theme-provider'
import { useActiveProjectId } from '../../lib/project-context'
import { getProjectById } from '../../features/projects/api'
import { qk } from '../../lib/query-keys'
import { CProjectStepStatus, CProjectStepNumber } from '../../types/enums/pipeline.enums'
import koliaIconLight from '../../assets/logo/kolia-icon-only.svg'
import koliaIconDark from '../../assets/logo/kolia-icon-only-dark-theme.svg'
import koliaLogoLight from '../../assets/logo/kolia-primary-logo.svg'
import koliaLogoDark from '../../assets/logo/kolia-primary-logo-dark-theme.svg'

// ── Props ──────────────────────────────────────────────
interface AdminSidebarProps {
  collapsed: boolean
  onToggle: () => void
}

// ── Icon render helper ─────────────────────────────────
function renderIcon(icon: AdminMenuItem['icon'], className?: string, color?: string) {
  if (!icon) return null
  // Handle both:
  //  - function components (typeof === 'function')
  //  - forwardRef/memo components (typeof === 'object' with $$typeof, e.g. lucide-react icons)
  if (typeof icon === 'function' || (typeof icon === 'object' && icon !== null)) {
    const IconComp = icon as React.ComponentType<{ className?: string; style?: React.CSSProperties }>
    return <IconComp className={className} style={color ? { color } : undefined} />
  }
  return icon
}

// ── Step status icon ──────────────────────────────────
function StepStatusIcon({ status }: { status: number | undefined }) {
  if (status === undefined) return null
  switch (status) {
    case CProjectStepStatus.Completed:
      return <CircleCheck className="h-3 w-3 text-emerald-500 shrink-0" />
    case CProjectStepStatus.Failed:
      return <CircleX className="h-3 w-3 text-rose-500 shrink-0" />
    case CProjectStepStatus.InProgress:
      return <CircleAlert className="h-3 w-3 text-blue-500 shrink-0" />
    case CProjectStepStatus.Skipped:
      return <CircleMinus className="h-3 w-3 text-slate-400 shrink-0" />
    default:
      return null
  }
}

// ── Single menu item (recursive for N-level) ──────────
function MenuItem({
  item,
  currentPath,
  depth,
  onNavigate,
  collapsed,
  stepStatus,
  isDisabled,
}: {
  item: AdminMenuItem
  currentPath: string
  depth: number
  onNavigate: (path: string) => void
  collapsed: boolean
  stepStatus?: number
  isDisabled?: boolean
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
    if (isDisabled && !hasChildren) return
    if (hasChildren) {
      setExpanded((prev) => !prev)
    }
    if (item.component && !isDisabled) {
      onNavigate(item.key)
    }
  }, [hasChildren, item.component, item.key, onNavigate, isDisabled])

  // Collapsed mode: icon-only, no label/children
  if (collapsed) {
    return (
      <li>
        <button
          type="button"
          disabled={isDisabled}
          onClick={() => !isDisabled && onNavigate(item.key)}
          className={cn(
            'mx-auto flex h-9 w-9 items-center justify-center rounded-lg transition-colors',
            isActive
              ? 'bg-indigo-50 dark:bg-indigo-950/40 text-indigo-700 dark:text-indigo-300'
              : 'text-slate-600 dark:text-slate-300 hover:bg-slate-100 hover:dark:bg-slate-800 hover:text-slate-900 hover:dark:text-slate-100',
          )}
          title={item.label}
        >
          {item.icon && renderIcon(item.icon, 'h-4 w-4', item.iconColor)}
        </button>
      </li>
    )
  }

  return (
    <li>
      <button
        type="button"
        onClick={handleClick}
        disabled={isDisabled && !hasChildren}
        className={cn(
          'flex w-full items-center gap-3 rounded-lg px-3 py-2 text-left text-sm font-medium transition-colors',
          isActive
            ? 'bg-indigo-50 dark:bg-indigo-950/40 text-indigo-700 dark:text-indigo-300'
            : isDisabled && !hasChildren
              ? 'text-slate-400 dark:text-slate-600 cursor-not-allowed'
              : 'text-slate-600 dark:text-slate-300 hover:bg-slate-100 hover:dark:bg-slate-800 hover:text-slate-900 hover:dark:text-slate-100',
        )}
        style={{ paddingLeft: `${12 + depth * 16}px` }}
      >
        {/* Icon */}
        {item.icon && (
          <span className="flex-shrink-0">
            {renderIcon(item.icon, 'h-4 w-4', item.iconColor)}
          </span>
        )}

        {/* Label */}
        <span className="flex-1 truncate">{item.label}</span>

        {/* Step status icon */}
        {stepStatus !== undefined && <StepStatusIcon status={stepStatus} />}

        {/* Expand indicator */}
        {hasChildren && (
          <ChevronDown
            className={cn(
              'h-3.5 w-3.5 flex-shrink-0 text-slate-400 dark:text-slate-500 transition-transform duration-200',
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
          <div className={cn('overflow-hidden transition-opacity duration-200', expanded ? 'opacity-100' : 'opacity-0 pointer-events-none')}>
            <ul className="mt-0.5 space-y-0.5">
              {item.children!.map((child) => (
                <MenuItem
                  key={child.key}
                  item={child}
                  currentPath={currentPath}
                  depth={depth + 1}
                  onNavigate={onNavigate}
                  collapsed={false}
                  stepStatus={stepStatus}
                  isDisabled={isDisabled}
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
  stepStatusMap,
  isPipelineDisabled,
}: {
  group: AdminMenuGroup
  currentPath: string
  onNavigate: (path: string) => void
  collapsed: boolean
  stepStatusMap?: Record<string, number>
  isPipelineDisabled?: boolean
}) {
  return (
    <div>
      {group.label && (
        collapsed ? (
          <hr className="my-3 border-t border-slate-200 dark:border-slate-700" />
        ) : (
          <p className="mb-1.5 px-3 text-[11px] font-semibold uppercase tracking-widest text-slate-400 dark:text-slate-500">
            {group.label}
          </p>
        )
      )}
      <ul className={cn(collapsed ? 'space-y-1' : 'space-y-0.5')}>
        {group.items.map((item) => {
          const stepKey = item.key
          const status = stepStatusMap?.[stepKey]
          // Nhận biết pipeline item bằng key pattern /pipeline/*
          const isPipelineItem = stepKey.startsWith('/pipeline/')
          // Pipeline items bị disable khi: không có project
          // hoặc step chưa tới (nếu đã có dữ liệu steps)
          const childDisabled = isPipelineItem && (
            isPipelineDisabled ||
            // Nếu step không có trong map (undefined) → cũng disabled (chưa tới)
            status === undefined ||
            (status !== CProjectStepStatus.Completed &&
              status !== CProjectStepStatus.InProgress)
          )
          return (
            <MenuItem
              key={item.key}
              item={item}
              currentPath={currentPath}
              depth={0}
              onNavigate={onNavigate}
              collapsed={collapsed}
              stepStatus={status}
              isDisabled={childDisabled}
            />
          )
        })}
      </ul>
    </div>
  )
}

// ── Brand logo / app name ─────────────────────────────
function SidebarBrand({ collapsed }: { collapsed: boolean }) {
  const { resolvedTheme } = useTheme()
  const isDark = resolvedTheme === 'dark'

  return (
    <div className="flex h-14 items-center gap-3 border-b border-slate-200 dark:border-slate-700 px-4">
      {collapsed ? (
        <img src={isDark ? koliaIconDark : koliaIconLight} alt="Kolia" className="h-8 w-8" />
      ) : (
        <img src={isDark ? koliaLogoDark : koliaLogoLight} alt="Kolia Thumbnail Engine" className="h-8" />
      )}
    </div>
  )
}

// ── Main Sidebar component ────────────────────────────
export function AdminSidebar({ collapsed, onToggle }: AdminSidebarProps) {
  const location = useLocation()
  const navigate = useNavigate()
  const [activeProjectId] = useActiveProjectId()

  // Fetch project steps để hiển thị trạng thái trên sidebar menu
  const { data: projectDetail } = useQuery({
    queryKey: activeProjectId ? qk.projects.detail(activeProjectId) : ['projects', 'empty'],
    queryFn: () => getProjectById(activeProjectId!),
    enabled: !!activeProjectId,
    staleTime: 30_000,
  })

  // Map stepNumber → stepStatus cho từng menu key
  const stepStatusMap = useMemo<Record<string, number>>(() => {
    if (!projectDetail?.steps) return {}
    const map: Record<string, number> = {}
    for (const step of projectDetail.steps) {
      // Map stepNumber → menu key
      switch (step.stepNumber) {
        case CProjectStepNumber.ContentBrief:
          map['/pipeline/video-content'] = step.stepStatus
          break
        case CProjectStepNumber.News:
          map['/pipeline/news'] = step.stepStatus
          break
        case CProjectStepNumber.ThumbnailReference:
          map['/pipeline/reference'] = step.stepStatus
          map['/pipeline/reference/library'] = step.stepStatus
          break
        case CProjectStepNumber.GenerateThumbnail:
          map['/pipeline/thumbnail/display-text'] = step.stepStatus
          map['/pipeline/thumbnail/generate'] = step.stepStatus
          break
        case CProjectStepNumber.VideoTitle:
          map['/pipeline/video-title'] = step.stepStatus
          break
      }
    }
    return map
  }, [projectDetail])

  const hasProject = !!activeProjectId

  const handleNavigate = useCallback(
    (path: string) => {
      const params = new URLSearchParams(location.search)
      const projectId = params.get('projectId')
      if (projectId) {
        navigate(path + '?projectId=' + encodeURIComponent(projectId))
      } else {
        navigate(path)
      }
    },
    [navigate, location.search],
  )

  return (
    <aside
      className={cn(
        'fixed left-0 top-0 z-30 flex h-screen flex-col border-r border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 transition-all duration-300 ease-in-out',
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
              stepStatusMap={stepStatusMap}
              isPipelineDisabled={!hasProject}
            />
          ))}
        </div>
      </nav>

      {/* Collapse toggle — fixed-height footer */}
      <div className="relative flex h-10 items-center border-t border-slate-200 dark:border-slate-700">
        <button
          type="button"
          onClick={onToggle}
          className={cn(
            'flex items-center justify-center rounded-lg text-slate-500 dark:text-slate-400 hover:bg-slate-100 hover:dark:bg-slate-800 hover:text-slate-700 hover:dark:text-slate-200 transition-colors',
            collapsed
              ? 'mx-auto my-2 h-8 w-8'
              : 'absolute -right-3 top-1/2 -translate-y-1/2 h-7 w-7 rounded-full border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 shadow-sm',
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
