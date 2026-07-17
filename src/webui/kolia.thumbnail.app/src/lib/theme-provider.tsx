import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'

/**
 * Theme system cho toàn bộ dự án.
 *
 * - 'light' | 'dark': ép cứng theme, lưu vào localStorage.
 * - 'system': theo theme của hệ điều hành (prefers-color-scheme), tự động
 *   cập nhật khi người dùng đổi theme OS trong lúc app đang mở.
 *
 * Cách hoạt động: class `.dark` được toggle trên thẻ <html>. Toàn bộ
 * Tailwind utilities dùng biến thể `dark:` (được bật qua
 * `@custom-variant dark (&:where(.dark, .dark *))` trong src/index.css)
 * sẽ tự động phản ứng theo class này.
 *
 * QUY TẮC CHO AI AGENT: xem RULES.md và .agent/skills/theming.skill.md
 * ở thư mục gốc dự án trước khi chỉnh sửa bất kỳ style nào.
 */

export type ThemeMode = 'light' | 'dark' | 'system'
export type ResolvedTheme = 'light' | 'dark'

const STORAGE_KEY = 'kolia-ui-theme'

interface ThemeContextValue {
  /** Giá trị người dùng đã chọn: 'light' | 'dark' | 'system' */
  theme: ThemeMode
  /** Theme thực tế đang áp dụng lên UI: 'light' | 'dark' (đã resolve từ 'system') */
  resolvedTheme: ResolvedTheme
  setTheme: (theme: ThemeMode) => void
  /** Chuyển nhanh giữa light <-> dark (bỏ qua system) */
  toggleTheme: () => void
}

const ThemeProviderContext = createContext<ThemeContextValue | undefined>(undefined)

function getSystemTheme(): ResolvedTheme {
  if (typeof window === 'undefined') return 'light'
  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}

function getStoredTheme(): ThemeMode {
  if (typeof window === 'undefined') return 'system'
  const stored = window.localStorage.getItem(STORAGE_KEY)
  if (stored === 'light' || stored === 'dark' || stored === 'system') return stored
  return 'system'
}

function applyThemeClass(resolved: ResolvedTheme) {
  const root = document.documentElement
  root.classList.toggle('dark', resolved === 'dark')
  root.style.colorScheme = resolved
}

export function ThemeProvider({
  children,
  defaultTheme = 'system',
}: {
  children: ReactNode
  defaultTheme?: ThemeMode
}) {
  const [theme, setThemeState] = useState<ThemeMode>(() => getStoredTheme() ?? defaultTheme)
  const [resolvedTheme, setResolvedTheme] = useState<ResolvedTheme>(() =>
    theme === 'system' ? getSystemTheme() : theme,
  )

  // Áp dụng class .dark mỗi khi theme hoặc resolvedTheme thay đổi
  useEffect(() => {
    const resolved = theme === 'system' ? getSystemTheme() : theme
    setResolvedTheme(resolved)
    applyThemeClass(resolved)
  }, [theme])

  // Lắng nghe thay đổi theme hệ điều hành khi đang ở chế độ 'system'
  useEffect(() => {
    if (theme !== 'system') return
    const mql = window.matchMedia('(prefers-color-scheme: dark)')
    const handler = () => {
      const resolved = getSystemTheme()
      setResolvedTheme(resolved)
      applyThemeClass(resolved)
    }
    mql.addEventListener('change', handler)
    return () => mql.removeEventListener('change', handler)
  }, [theme])

  const setTheme = (next: ThemeMode) => {
    window.localStorage.setItem(STORAGE_KEY, next)
    setThemeState(next)
  }

  const toggleTheme = () => {
    const current = theme === 'system' ? getSystemTheme() : theme
    setTheme(current === 'dark' ? 'light' : 'dark')
  }

  const value = useMemo(
    () => ({ theme, resolvedTheme, setTheme, toggleTheme }),
    [theme, resolvedTheme],
  )

  return <ThemeProviderContext.Provider value={value}>{children}</ThemeProviderContext.Provider>
}

export function useTheme() {
  const ctx = useContext(ThemeProviderContext)
  if (!ctx) throw new Error('useTheme phải được dùng bên trong <ThemeProvider>')
  return ctx
}
