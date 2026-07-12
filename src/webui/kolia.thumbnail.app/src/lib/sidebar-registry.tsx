import type { ReactNode, RefObject } from 'react'
import type { SidebarContent } from './sidebar-context'

/**
 * Hợp đồng chung mà MỌI form hiển thị trong AppSidebar phải tuân theo.
 * Toàn bộ các form hiện có (create/edit provider, create/edit configuration...)
 * vốn đã implement đúng shape này thông qua forwardRef + useImperativeHandle.
 */
export interface SidebarFormHandle {
  submit: () => void | Promise<void>
  isSubmitting: boolean
}

export interface SidebarRenderProps<TContent extends SidebarContent = SidebarContent> {
  content: NonNullable<TContent>
  onClose: () => void
  formRef: RefObject<SidebarFormHandle | null>
}

export interface SidebarEntryConfig<TContent extends SidebarContent = SidebarContent> {
  /** Tiêu đề hiển thị trên header của sidebar */
  title: (content: NonNullable<TContent>) => string
  /** Nhãn nút submit ở trạng thái bình thường, ví dụ "Tạo" / "Lưu" */
  submitLabel: string
  /** Nhãn nút submit khi đang xử lý, mặc định "Đang xử lý…" */
  submittingLabel?: string
  /** Render nội dung form bên trong SidebarBody */
  render: (props: SidebarRenderProps<TContent>) => ReactNode
}

const registry = new Map<string, SidebarEntryConfig<any>>()

/**
 * Mỗi feature (ai-providers, ai-configurations, ...) tự gọi hàm này trong
 * file "<feature>.sidebar.tsx" của chính nó để đăng ký loại sidebar mà nó cung cấp.
 *
 * KHÔNG cần và KHÔNG được sửa app-sidebar.tsx khi thêm feature mới.
 */
export function registerSidebarEntry<TContent extends SidebarContent = SidebarContent>(
  type: string,
  config: SidebarEntryConfig<TContent>,
) {
  if (import.meta.env?.DEV && registry.has(type)) {
    console.warn(
      `[sidebar-registry] Loại sidebar "${type}" đã được đăng ký trước đó — cấu hình cũ sẽ bị ghi đè. ` +
        `Kiểm tra lại xem có 2 feature nào cùng dùng chung "type" hay không.`,
    )
  }
  registry.set(type, config)
}

export function getSidebarEntry(type: string | undefined): SidebarEntryConfig | undefined {
  if (!type) return undefined
  return registry.get(type)
}
