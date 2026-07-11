import type { ReactNode } from 'react'
import { SlidersHorizontal } from 'lucide-react'
import {
  Sidebar,
  SidebarHeader,
  SidebarBody,
  SidebarFooter,
} from '../ui/sidebar'
import { Button } from '../ui/button'

interface DataTableFilterSidebarProps {
  isOpen: boolean
  onClose: () => void
  onApply?: () => void
  onReset?: () => void
  children?: ReactNode
}

export function DataTableFilterSidebar({
  isOpen,
  onClose,
  onApply,
  onReset,
  children,
}: DataTableFilterSidebarProps) {
  const handleApply = () => {
    onApply?.()
    onClose()
  }

  const handleReset = () => {
    onReset?.()
  }

  return (
    <Sidebar isOpen={isOpen} onClose={onClose} side="right">
      <SidebarHeader title="Bộ lọc" onClose={onClose} />

      <SidebarBody>
        {children ? (
          children
        ) : (
          <div className="flex flex-col items-center justify-center gap-3 py-12 text-center">
            <SlidersHorizontal className="h-10 w-10 text-slate-300" />
            <p className="text-sm text-slate-400">Không có bộ lọc nào.</p>
          </div>
        )}
      </SidebarBody>

      <SidebarFooter>
        <Button variant="ghost" size="sm" onClick={handleReset}>
          Reset
        </Button>
        <Button variant="outline" size="sm" onClick={onClose}>
          Đóng
        </Button>
        <Button size="sm" onClick={handleApply}>
          Áp dụng
        </Button>
      </SidebarFooter>
    </Sidebar>
  )
}
