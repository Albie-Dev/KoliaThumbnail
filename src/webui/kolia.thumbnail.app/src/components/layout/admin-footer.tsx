import { cn } from '../../lib/utils'
import koliaIcon from '../../assets/logo/kolia-icon-only.svg'

// ── Props ──────────────────────────────────────────────
interface AdminFooterProps {
  sidebarCollapsed: boolean
}

// ── Footer Component ──────────────────────────────────
export function AdminFooter({ sidebarCollapsed }: AdminFooterProps) {
  const year = new Date().getFullYear()

  return (
    <footer
      className={cn(
        'fixed bottom-0 border-t border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 px-6 py-3 transition-all duration-300 ease-in-out z-10',
        sidebarCollapsed ? 'left-16 right-0' : 'left-64 right-0',
      )}
    >
      <div className="flex items-center justify-between text-xs text-slate-400 dark:text-slate-500">
        <div className="flex items-center gap-2">
          <img src={koliaIcon} alt="Kolia" className="h-4 w-4 opacity-60" />
          <p>© {year} KoliaEngine. All rights reserved.</p>
        </div>
        <div className="flex items-center gap-4">
          <a href="#" className="hover:text-slate-600 hover:dark:text-slate-300 transition-colors">
            Điều khoản
          </a>
          <a href="#" className="hover:text-slate-600 hover:dark:text-slate-300 transition-colors">
            Bảo mật
          </a>
          <span>v1.0.0</span>
        </div>
      </div>
    </footer>
  )
}
