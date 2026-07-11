import {
  LayoutDashboard,
  Image,
  Smartphone,
  Cpu,
  Settings,
  Users,
  BarChart3,
  Shield,
  Database,
  Server,
  Brain,
  type LucideIcon,
} from 'lucide-react'
import type { AdminMenuGroup, AdminMenuItem } from '../types/admin-layout.types'

// ── Pages ─────────────────────────────────────────────
import { useLocation } from 'react-router-dom'
import type { ComponentType } from 'react'
import { AiProvidersPage } from '../features/ai-providers/ai-providers-page'

const PlaceholderPage: ComponentType = () => {
  const path = useLocation().pathname
  return (
    <div className="flex h-full flex-col items-center justify-center gap-2 text-slate-400">
      <p className="text-lg">🚧 Trang đang được phát triển</p>
      <p className="text-xs text-slate-300">{path}</p>
    </div>
  )
}

// ── Menu configuration ─────────────────────────────────
// Supports N-level nesting. Each leaf item can have a `component`.
// Groups are separated visually in the sidebar.

export const adminMenuGroups: AdminMenuGroup[] = [
  // ── Group: Tổng quan ───────────────────────────────
  {
    label: 'Tổng quan',
    items: [
      {
        key: '/dashboard',
        label: 'Bảng điều khiển',
        icon: LayoutDashboard,
        component: PlaceholderPage,
      },
      {
        key: '/analytics',
        label: 'Phân tích',
        icon: BarChart3,
        component: PlaceholderPage,
      },
    ],
  },

  // ── Group: Quản lý nội dung ─────────────────────────
  {
    label: 'Quản lý nội dung',
    items: [
      {
        key: '/thumbnails',
        label: 'Thumbnail',
        icon: Image,
        children: [
          {
            key: '/thumbnails/all',
            label: 'Tất cả Thumbnail',
            component: PlaceholderPage,
          },
          {
            key: '/thumbnails/templates',
            label: 'Mẫu Thumbnail',
            icon: Image,
            children: [
              {
                key: '/thumbnails/templates/list',
                label: 'Danh sách mẫu',
                component: PlaceholderPage,
              },
              {
                key: '/thumbnails/templates/categories',
                label: 'Danh mục mẫu',
                component: PlaceholderPage,
              },
            ],
          },
        ],
      },
      {
        key: '/media',
        label: 'Thư viện Media',
        icon: Database,
        component: PlaceholderPage,
      },
    ],
  },

  // ── Group: Cấu hình ─────────────────────────────────
  {
    label: 'Cấu hình',
    items: [
      {
        key: '/configuration/ai',
        label: 'AI',
        icon: Brain,
        children: [
          {
            key: '/configuration/ai/providers',
            label: 'Providers',
            component: AiProvidersPage,
          },
        ],
      },
      {
        key: '/ai-config',
        label: 'Cấu hình AI',
        icon: Cpu,
        component: PlaceholderPage,
      },
    ],
  },

  // ── Group: Hệ thống ─────────────────────────────────
  {
    label: 'Hệ thống',
    items: [
      {
        key: '/devices',
        label: 'Thiết bị',
        icon: Smartphone,
        component: PlaceholderPage,
      },
      {
        key: '/servers',
        label: 'Máy chủ',
        icon: Server,
        children: [
          {
            key: '/servers/list',
            label: 'Danh sách máy chủ',
            component: PlaceholderPage,
          },
          {
            key: '/servers/status',
            label: 'Trạng thái',
            component: PlaceholderPage,
          },
        ],
      },
    ],
  },

  // ── Group: Quản trị ─────────────────────────────────
  {
    label: 'Quản trị',
    items: [
      {
        key: '/users',
        label: 'Người dùng',
        icon: Users,
        component: PlaceholderPage,
      },
      {
        key: '/roles',
        label: 'Phân quyền',
        icon: Shield,
        component: PlaceholderPage,
      },
      {
        key: '/settings',
        label: 'Cài đặt',
        icon: Settings,
        component: PlaceholderPage,
      },
    ],
  },
]

// ── Helper: flatten all leaf items with their full path ──
export interface FlatMenuItem {
  key: string
  label: string
  icon?: LucideIcon
  component?: ComponentType
  parentKey?: string
  depth: number
}

export function flattenMenuItems(groups: AdminMenuGroup[]): FlatMenuItem[] {
  const result: FlatMenuItem[] = []

  function walk(items: AdminMenuItem[], parentKey: string | undefined, depth: number): void {
    for (const item of items) {
      if (item.children && item.children.length > 0) {
        walk(item.children, item.key, depth + 1)
      } else if (item.component) {
        result.push({
          key: item.key,
          label: item.label,
          icon: typeof item.icon === 'function' ? (item.icon as LucideIcon) : undefined,
          component: item.component,
          parentKey,
          depth,
        })
      }
    }
  }

  for (const group of groups) {
    walk(group.items, undefined, 0)
  }

  return result
}
