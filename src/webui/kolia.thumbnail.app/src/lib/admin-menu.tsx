// import {
//   LayoutDashboard,
//   BarChart3,
//   Video,
//   Eye,
//   LineChart,
//   DollarSign,
//   GitCompare,
//   TrendingUp,
//   Flame,
//   Sparkles,
//   Trophy,
//   Image,
//   Search,
//   Lightbulb,
//   FlaskConical,
//   FolderOpen,
//   Bell,
//   Puzzle,
//   Settings,
//   Users,
//   Shield,
//   Brain,
//   type LucideIcon,
//   MessageCircle,
//   Share2,
//   Globe,
//   KeyRound,
// } from 'lucide-react'
// import type { AdminMenuGroup, AdminMenuItem } from '../types/admin-layout.types'

// // ── Pages ─────────────────────────────────────────────
// // Dùng React.lazy() để mỗi trang được tách thành 1 chunk riêng (code-splitting
// // theo route). Trình duyệt chỉ tải code của trang khi người dùng thực sự
// // điều hướng tới route đó, thay vì tải toàn bộ mọi trang ngay từ đầu.
// // Khi thêm trang mới vào menu, hãy khai báo theo đúng mẫu lazy() bên dưới
// // thay vì import tĩnh trực tiếp.
// import { useLocation } from 'react-router-dom'
// import { lazy, type ComponentType } from 'react'

// const AiProvidersPage = lazy(() =>
//   import('../features/ai-providers/ai-providers-page').then((m) => ({ default: m.AiProvidersPage })),
// )
// const AiConfigurationsPage = lazy(() =>
//   import('../features/ai-configurations/ai-configuration-page').then((m) => ({
//     default: m.AiConfigurationsPage,
//   })),
// )

// const SocialMediaProvidersPage = lazy(() =>
//   import('../features/social-media-providers/social-media-providers-page').then((m) => ({ default: m.SocialMediaProvidersPage })),
// )

// const PlaceholderPage: ComponentType = () => {
//   const path = useLocation().pathname
//   return (
//     <div className="flex h-full flex-col items-center justify-center gap-2 text-slate-400 dark:text-slate-500">
//       <p className="text-lg">🚧 Trang đang được phát triển</p>
//       <p className="text-xs text-slate-300 dark:text-slate-600">{path}</p>
//     </div>
//   )
// }

// // ── Menu configuration ─────────────────────────────────
// // Supports N-level nesting. Each leaf item can have a `component`.
// // Groups are separated visually in the sidebar.
// // Cấu trúc menu được thiết kế theo các nhóm chức năng của Viewstats Pro:
// // tổng quan, phân tích kênh/video, công cụ sáng tạo nội dung, cảnh báo,
// // cấu hình AI và quản trị hệ thống.
// //
// // LƯU Ý: mỗi item (kể cả item con) giờ có thêm `iconColor` (mã hex) để
// // icon được tô màu riêng thay vì dùng chung 1 màu mặc định. Nếu component
// // render sidebar hiện tại chưa đọc field này, cần cập nhật:
// //   - `AdminMenuItem` type: thêm `iconColor?: string`
// //   - Nơi render icon: <item.icon style={{ color: item.iconColor }} .../>

// export const adminMenuGroups: AdminMenuGroup[] = [
//   // ── Group: Tổng quan ───────────────────────────────
//   {
//     label: 'Tổng quan',
//     items: [
//       {
//         key: '/dashboard',
//         label: 'Bảng điều khiển',
//         icon: LayoutDashboard,
//         iconColor: '#3b82f6', // blue-500
//         component: PlaceholderPage,
//       },
//       {
//         key: '/analytics',
//         label: 'Phân tích tổng quan',
//         icon: BarChart3,
//         iconColor: '#6366f1', // indigo-500
//         component: PlaceholderPage,
//       },
//     ],
//   },

//   // ── Group: Kênh & Video ─────────────────────────────
//   {
//     label: 'Kênh & Video',
//     items: [
//       {
//         key: '/channels',
//         label: 'Channelytics',
//         icon: Video,
//         iconColor: '#ef4444', // red-500
//         children: [
//           {
//             key: '/channels/overview',
//             label: 'Thống kê kênh',
//             icon: Eye,
//             iconColor: '#f87171', // red-400
//             component: PlaceholderPage,
//           },
//           {
//             key: '/channels/growth',
//             label: 'Tăng trưởng & Dự báo',
//             icon: LineChart,
//             iconColor: '#fb923c', // orange-400
//             component: PlaceholderPage,
//           },
//           {
//             key: '/channels/revenue',
//             label: 'Ước tính doanh thu',
//             icon: DollarSign,
//             iconColor: '#22c55e', // green-500
//             component: PlaceholderPage,
//           },
//         ],
//       },
//       {
//         key: '/comparison',
//         label: 'So sánh kênh',
//         icon: GitCompare,
//         iconColor: '#a855f7', // purple-500
//         component: PlaceholderPage,
//       },
//       {
//         key: '/outliers',
//         label: 'Video nổi bật (Outliers)',
//         icon: TrendingUp,
//         iconColor: '#f59e0b', // amber-500
//         children: [
//           {
//             key: '/outliers/videos',
//             label: 'Video vượt trội',
//             icon: Flame,
//             iconColor: '#f97316', // orange-500
//             component: PlaceholderPage,
//           },
//           {
//             key: '/outliers/trends',
//             label: 'Xu hướng theo ngách',
//             icon: Sparkles,
//             iconColor: '#eab308', // yellow-500
//             component: PlaceholderPage,
//           },
//         ],
//       },
//       {
//         key: '/top-channels',
//         label: 'Bảng xếp hạng kênh',
//         icon: Trophy,
//         iconColor: '#facc15', // yellow-400
//         component: PlaceholderPage,
//       },
//     ],
//   },

//   // ── Group: Công cụ sáng tạo ─────────────────────────
//   {
//     label: 'Công cụ sáng tạo',
//     items: [
//       {
//         key: '/thumbnails',
//         label: 'Thumbnail',
//         icon: Image,
//         iconColor: '#ec4899', // pink-500
//         children: [
//           {
//             key: '/thumbnails/search',
//             label: 'Tìm kiếm Thumbnail',
//             icon: Search,
//             iconColor: '#f472b6', // pink-400
//             component: PlaceholderPage,
//           },
//           {
//             key: '/thumbnails/inspiration',
//             label: 'Gợi ý ý tưởng',
//             icon: Lightbulb,
//             iconColor: '#fbbf24', // amber-400
//             component: PlaceholderPage,
//           },
//         ],
//       },
//       {
//         key: '/ab-testing',
//         label: 'A/B Testing',
//         icon: FlaskConical,
//         iconColor: '#14b8a6', // teal-500
//         component: PlaceholderPage,
//       },
//       {
//         key: '/collections',
//         label: 'Bộ sưu tập ý tưởng',
//         icon: FolderOpen,
//         iconColor: '#8b5cf6', // violet-500
//         component: PlaceholderPage,
//       },
//       {
//         key: '/ai-chat',
//         label: 'Hỏi đáp AI',
//         icon: MessageCircle,
//         iconColor: '#3b82f6', // blue-500
//         component: PlaceholderPage,
//       },
//     ],
//   },

//   // ── Group: Cảnh báo & Tích hợp ───────────────────────
//   {
//     label: 'Cảnh báo & Tích hợp',
//     items: [
//       {
//         key: '/alerts',
//         label: 'Cảnh báo',
//         icon: Bell,
//         iconColor: '#f43f5e', // rose-500
//         component: PlaceholderPage,
//       },
//       {
//         key: '/extension',
//         label: 'Chrome Extension',
//         icon: Puzzle,
//         iconColor: '#0ea5e9', // sky-500
//         component: PlaceholderPage,
//       },
//     ],
//   },

//   // ── Group: Cấu hình ─────────────────────────────────
//   {
//     label: 'Cấu hình',
//     items: [
//       {
//         key: '/configuration/ai',
//         label: 'Cấu hình AI',
//         icon: Brain,
//         iconColor: '#06b6d4', // cyan-500
//         children: [
//           {
//             key: '/configuration/ai/providers',
//             label: 'Nhà cung cấp',
//             icon: Globe,
//             iconColor: '#0891b2', // cyan-600
//             component: AiProvidersPage,
//           },
//           {
//             key: '/configuration/ai/configurations',
//             label: 'Cấu hình Key',
//             icon: KeyRound,
//             iconColor: '#0284c7', // sky-600
//             component: AiConfigurationsPage,
//           },
//         ],
//       },
//       {
//         key: '/configuration/social-media',
//         label: 'Cấu hình Mạng xã hội',
//         icon: Share2,
//         iconColor: '#3b82f6', // blue-500
//         children: [
//           {
//             key: '/configuration/social-media/providers',
//             label: 'Nhà cung cấp',
//             icon: Globe,
//             iconColor: '#2563eb', // blue-600
//             component: SocialMediaProvidersPage,
//           },
//           {
//             key: '/configuration/social-media/configurations',
//             label: 'Cấu hình Key',
//             icon: KeyRound,
//             iconColor: '#f59e0b', // amber-500
//             component: AiConfigurationsPage,
//           },
//         ],
//       }
//     ],
//   },

//   // ── Group: Quản trị ─────────────────────────────────
//   {
//     label: 'Quản trị',
//     items: [
//       {
//         key: '/users',
//         label: 'Người dùng',
//         icon: Users,
//         iconColor: '#3b82f6', // blue-500
//         component: PlaceholderPage,
//       },
//       {
//         key: '/roles',
//         label: 'Phân quyền',
//         icon: Shield,
//         iconColor: '#64748b', // slate-500
//         component: PlaceholderPage,
//       },
//       {
//         key: '/settings',
//         label: 'Cài đặt',
//         icon: Settings,
//         iconColor: '#71717a', // zinc-500
//         component: PlaceholderPage,
//       },
//     ],
//   },
// ]

// // ── Helper: flatten all leaf items with their full path ──
// export interface FlatMenuItem {
//   key: string
//   label: string
//   icon?: LucideIcon
//   iconColor?: string
//   component?: ComponentType
//   parentKey?: string
//   depth: number
// }

// export function flattenMenuItems(groups: AdminMenuGroup[]): FlatMenuItem[] {
//   const result: FlatMenuItem[] = []

//   function walk(items: AdminMenuItem[], parentKey: string | undefined, depth: number): void {
//     for (const item of items) {
//       if (item.children && item.children.length > 0) {
//         walk(item.children, item.key, depth + 1)
//       } else if (item.component) {
//         result.push({
//           key: item.key,
//           label: item.label,
//           icon: typeof item.icon === 'function' ? (item.icon as LucideIcon) : undefined,
//           iconColor: item.iconColor,
//           component: item.component,
//           parentKey,
//           depth,
//         })
//       }
//     }
//   }

//   for (const group of groups) {
//     walk(group.items, undefined, 0)
//   }

//   return result
// }


import {
  LayoutDashboard,
  Archive,
  Video,
  Newspaper,
  Images,
  Library,
  Image,
  Type,
  Wand2,
  Heading1,
  PackageCheck,
  Bell,
  Puzzle,
  Settings,
  Users,
  Shield,
  Brain,
  Share2,
  Globe,
  KeyRound,
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

const SocialMediaProvidersPage = lazy(() =>
  import('../features/social-media-providers/social-media-providers-page').then((m) => ({ default: m.SocialMediaProvidersPage })),
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
// Supports N-level nesting. Each leaf item can have a `component`.
// Groups are separated visually in the sidebar.
//
// Cấu trúc menu rút gọn gồm:
//   1. Tổng quan            – trang tổng quan hệ thống
//   2. Kho lưu trữ          – nơi lưu trữ dữ liệu/tài nguyên
//   3. Generate Thumbnail & Title – PIPELINE 6 bước tạo nội dung, đi từ
//      thu thập dữ liệu (video, tin tức) → tham khảo/tạo thumbnail →
//      tạo title → đóng gói bộ hoàn chỉnh. Icon + iconColor của từng bước
//      được chọn theo 1 dải màu chuyển dần (đỏ → cam → vàng → xanh lá →
//      xanh dương → chàm) để người dùng cảm nhận được thứ tự luồng xử lý
//      (workflow) ngay trên sidebar.
//   4. Cảnh báo & Tích hợp, Cấu hình, Quản trị – giữ nguyên như cũ.
//
// LƯU Ý: mỗi item (kể cả item con) có thêm `iconColor` (mã hex) để icon
// được tô màu riêng thay vì dùng chung 1 màu mặc định.

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
        component: PlaceholderPage,
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
        component: PlaceholderPage,
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
        component: PlaceholderPage,
      },
      {
        key: '/pipeline/news',
        label: '2. Tin tức',
        icon: Newspaper,
        iconColor: '#f97316', // orange-500
        component: PlaceholderPage,
      },
      {
        key: '/pipeline/reference',
        label: '3. Thumbnail tham khảo',
        icon: Images,
        iconColor: '#eab308', // yellow-500
        children: [
          {
            key: '/pipeline/reference/library',
            label: '3.1 Thumbnail library',
            icon: Library,
            iconColor: '#facc15', // yellow-400
            component: PlaceholderPage,
          },
        ],
      },
      {
        key: '/pipeline/thumbnail',
        label: '4. Thumbnail',
        icon: Image,
        iconColor: '#22c55e', // green-500
        children: [
          {
            key: '/pipeline/thumbnail/display-text',
            label: '4.1 Tạo display text',
            icon: Type,
            iconColor: '#10b981', // emerald-500
            component: PlaceholderPage,
          },
          {
            key: '/pipeline/thumbnail/generate',
            label: '4.2 Tạo thumbnail',
            icon: Wand2,
            iconColor: '#14b8a6', // teal-500
            component: PlaceholderPage,
          },
        ],
      },
      {
        key: '/pipeline/video-title',
        label: '5. Tạo video title',
        icon: Heading1,
        iconColor: '#0ea5e9', // sky-500
        component: PlaceholderPage,
      },
      {
        key: '/pipeline/complete-set',
        label: '6. Bộ hoàn chỉnh',
        icon: PackageCheck,
        iconColor: '#6366f1', // indigo-500
        component: PlaceholderPage,
      },
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
      },
      {
        key: '/extension',
        label: 'Chrome Extension',
        icon: Puzzle,
        iconColor: '#0ea5e9', // sky-500
        component: PlaceholderPage,
      },
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
      if (item.children && item.children.length > 0) {
        walk(item.children, item.key, depth + 1)
      } else if (item.component) {
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
    }
  }

  for (const group of groups) {
    walk(group.items, undefined, 0)
  }

  return result
}