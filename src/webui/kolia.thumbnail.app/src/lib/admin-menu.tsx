import {
  LayoutDashboard,
  Archive,
  Video,
  Newspaper,
  Images,
  Library,
  Type,
  Wand2,
  Heading1,
  PackageCheck,
  Bell,
  Settings,
  Users,
  Shield,
  Brain,
  Share2,
  Globe,
  KeyRound,
  Cloud,
  CalendarClock,
  type LucideIcon,
} from 'lucide-react'
import type { AdminMenuGroup, AdminMenuItem } from '../types/admin-layout.types'

// ── Pages ─────────────────────────────────────────────
// Dùng React.lazy() để mỗi trang được tách thành 1 chunk riêng (code-splitting
// theo route). Trình duyệt chỉ tải code của trang khi người dùng thực sự
// điều hướng tới route đó, thay vì tải toàn bộ mọi trang ngay từ đầu.
// Khi thêm trang mới vào menu, hãy khai báo theo đúng mẫu lazy() bên dưới
// thay vì import tĩnh trực tiếp.
import { useLocation } from 'react-router-dom'
import { lazy, type ComponentType } from 'react'

const AiProvidersPage = lazy(() =>
  import('../features/ai-providers/ai-providers-page').then((m) => ({ default: m.AiProvidersPage })),
)
const AiConfigurationsPage = lazy(() =>
  import('../features/ai-configurations/ai-configuration-page').then((m) => ({
    default: m.AiConfigurationsPage,
  })),
)

const AiFunctionConfigsPage = lazy(() =>
  import('../features/ai-function-configs/ai-function-configs-page').then((m) => ({
    default: m.AiFunctionConfigsPage,
  })),
)

const SocialMediaProvidersPage = lazy(() =>
  import('../features/social-media-providers/social-media-providers-page').then((m) => ({ default: m.SocialMediaProvidersPage })),
)

const ProjectsPage = lazy(() =>
  import('../features/projects/projects-page').then((m) => ({ default: m.ProjectsPage })),
)

const DashboardPage = lazy(() =>
  import('../features/dashboard/dashboard-page').then((m) => ({ default: m.DashboardPage })),
)

const ContentBriefPage = lazy(() =>
  import('../features/content-brief/content-brief-page').then((m) => ({ default: m.ContentBriefPage })),
)

const NewsPage = lazy(() =>
  import('../features/news/news-page').then((m) => ({ default: m.NewsPage })),
)

const ReferenceSearchPage = lazy(() =>
  import('../features/thumbnail-library/reference-search-page').then((m) => ({ default: m.ReferenceSearchPage })),
)

const ThumbnailLibraryPage = lazy(() =>
  import('../features/thumbnail-library/thumbnail-library-page').then((m) => ({ default: m.ThumbnailLibraryPage })),
)

const DisplayTextPage = lazy(() =>
  import('../features/display-text/display-text-page').then((m) => ({ default: m.DisplayTextPage })),
)

const VideoTitlePage = lazy(() =>
  import('../features/video-title/video-title-page').then((m) => ({ default: m.VideoTitlePage })),
)

const CompletePackagePage = lazy(() =>
  import('../features/complete-package/complete-package-page').then((m) => ({ default: m.CompletePackagePage })),
)

const ThumbnailGenerationPage = lazy(() =>
  import('../features/thumbnail-generation/thumbnail-generation-page').then((m) => ({ default: m.ThumbnailGenerationPage })),
)

const GoogleServicesPage = lazy(() =>
  import('../features/google-services/google-service-page').then((m) => ({ default: m.GoogleServicesPage })),
)

const ScheduledJobsPage = lazy(() =>
  import('../features/scheduled-jobs/scheduled-jobs-page').then((m) => ({ default: m.ScheduledJobsPage })),
)

const PlaceholderPage: ComponentType = () => {
  const path = useLocation().pathname
  return (
    <div className="flex h-full flex-col items-center justify-center gap-2 text-slate-400 dark:text-slate-500">
      <p className="text-lg">🚧 Trang đang được phát triển</p>
      <p className="text-xs text-slate-300 dark:text-slate-600">{path}</p>
    </div>
  )
}

// ── Menu configuration ─────────────────────────────────
export const adminMenuGroups: AdminMenuGroup[] = [
  // ── Group: Tổng quan ───────────────────────────────
  {
    label: 'Tổng quan',
    items: [
      {
        key: '/dashboard',
        label: 'Tổng quan',
        icon: LayoutDashboard,
        iconColor: '#3b82f6', // blue-500
        component: DashboardPage,
      },
    ],
  },

  // ── Group: Kho lưu trữ ──────────────────────────────
  {
    label: 'Kho lưu trữ',
    items: [
      {
        key: '/archive',
        label: 'Kho lưu trữ',
        icon: Archive,
        iconColor: '#8b5cf6', // violet-500
        component: ProjectsPage,
      },
    ],
  },

  // ── Group: Generate Thumbnail & Title (pipeline) ────
  // Thứ tự khai báo = thứ tự bước trong quy trình (1 → 6).
  // Dải màu đi từ ấm (đỏ/cam) sang lạnh (xanh/chàm) để gợi ý chiều đi
  // của luồng xử lý từ đầu vào đến sản phẩm hoàn chỉnh.
  {
    label: 'Generate Thumbnail & Title',
    items: [
      {
        key: '/pipeline/video-content',
        label: '1. Nội dung video',
        icon: Video,
        iconColor: '#ef4444', // red-500
        component: ContentBriefPage,
      },
      {
        key: '/pipeline/news',
        label: '2. Tin tức',
        icon: Newspaper,
        iconColor: '#f97316', // orange-500
        component: NewsPage,
      },
      {
        key: '/pipeline/reference',
        label: '3. Thumbnail tham khảo',
        icon: Images,
        iconColor: '#eab308', // yellow-500
        component: ReferenceSearchPage,
        children: [
          {
            key: '/pipeline/reference/library',
            label: '3.1 Thumbnail library',
            icon: Library,
            iconColor: '#facc15', // yellow-400
            component: ThumbnailLibraryPage,
          },
        ],
      },
      {
        key: '/pipeline/thumbnail',
        label: '4. Thumbnail',
        icon: Type,
        iconColor: '#22c55e', // green-500
        children: [
          {
            key: '/pipeline/thumbnail/display-text',
            label: '4.1 Tạo display text',
            icon: Type,
            iconColor: '#10b981', // emerald-500
            component: DisplayTextPage,
          },
          {
            key: '/pipeline/thumbnail/generate',
            label: '4.2 Tạo thumbnail',
            icon: Wand2,
            iconColor: '#14b8a6', // teal-500
            component: ThumbnailGenerationPage,
          },
        ],
      },
      {
        key: '/pipeline/video-title',
        label: '5. Tạo video title',
        icon: Heading1,
        iconColor: '#0ea5e9', // sky-500
        component: VideoTitlePage,
      },
      {
        key: '/pipeline/complete-set',
        label: '6. Bộ hoàn chỉnh',
        icon: PackageCheck,
        iconColor: '#6366f1', // indigo-500
        component: CompletePackagePage,
      },
    ],
  },

  // ── Group: Tác vụ tự động ──────────────────────────────
  {
    label: 'Tác vụ tự động',
    items: [
      {
        key: '/scheduled-jobs',
        label: 'Import lịch trình',
        icon: CalendarClock,
        iconColor: '#f43f5e', // rose-500
        component: ScheduledJobsPage,
      }
    ],
  },

  // ── Group: Cảnh báo & Tích hợp (giữ nguyên) ─────────
  {
    label: 'Cảnh báo & Tích hợp',
    items: [
      {
        key: '/alerts',
        label: 'Cảnh báo',
        icon: Bell,
        iconColor: '#f43f5e', // rose-500
        component: PlaceholderPage,
      }
    ],
  },

  // ── Group: Cấu hình (giữ nguyên) ─────────────────────
  {
    label: 'Cấu hình',
    items: [
      {
        key: '/configuration/ai',
        label: 'Cấu hình AI',
        icon: Brain,
        iconColor: '#06b6d4', // cyan-500
        children: [
          {
            key: '/configuration/ai/providers',
            label: 'Nhà cung cấp',
            icon: Globe,
            iconColor: '#0891b2', // cyan-600
            component: AiProvidersPage,
          },
          {
            key: '/configuration/ai/configurations',
            label: 'Cấu hình Key',
            icon: KeyRound,
            iconColor: '#0284c7', // sky-600
            component: AiConfigurationsPage,
          },
          {
            key: '/configuration/ai/function-configs',
            label: 'Cấu hình chức năng',
            icon: Brain,
            iconColor: '#8b5cf6', // violet-500
            component: AiFunctionConfigsPage,
          },
        ],
      },
      {
        key: '/configuration/social-media',
        label: 'Cấu hình Mạng xã hội',
        icon: Share2,
        iconColor: '#3b82f6', // blue-500
        children: [
          {
            key: '/configuration/social-media/providers',
            label: 'Nhà cung cấp',
            icon: Globe,
            iconColor: '#2563eb', // blue-600
            component: SocialMediaProvidersPage,
          },
          {
            key: '/configuration/social-media/configurations',
            label: 'Cấu hình Key',
            icon: KeyRound,
            iconColor: '#f59e0b', // amber-500
            component: AiConfigurationsPage,
          },
        ],
      },
      {
        key: '/configuration/google-services',
        label: 'Google Service Accounts',
        icon: Cloud,
        iconColor: '#4285F4', // Google Blue
        component: GoogleServicesPage,
      },
    ],
  },

  // ── Group: Quản trị (giữ nguyên) ─────────────────────
  {
    label: 'Quản trị',
    items: [
      {
        key: '/users',
        label: 'Người dùng',
        icon: Users,
        iconColor: '#3b82f6', // blue-500
        component: PlaceholderPage,
      },
      {
        key: '/roles',
        label: 'Phân quyền',
        icon: Shield,
        iconColor: '#64748b', // slate-500
        component: PlaceholderPage,
      },
      {
        key: '/settings',
        label: 'Cài đặt',
        icon: Settings,
        iconColor: '#71717a', // zinc-500
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
  iconColor?: string
  component?: ComponentType
  parentKey?: string
  depth: number
}

export function flattenMenuItems(groups: AdminMenuGroup[]): FlatMenuItem[] {
  const result: FlatMenuItem[] = []

  function walk(items: AdminMenuItem[], parentKey: string | undefined, depth: number): void {
    for (const item of items) {
      if (item.component) {
        result.push({
          key: item.key,
          label: item.label,
          icon: typeof item.icon === 'function' ? (item.icon as LucideIcon) : undefined,
          iconColor: item.iconColor,
          component: item.component,
          parentKey,
          depth,
        })
      }
      if (item.children && item.children.length > 0) {
        walk(item.children, item.key, depth + 1)
      }
    }
  }

  for (const group of groups) {
    walk(group.items, undefined, 0)
  }

  return result
}