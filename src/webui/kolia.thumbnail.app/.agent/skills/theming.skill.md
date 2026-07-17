# Skill: theming (Light/Dark theme cho Kolia Thumbnail Engine)

**Kích hoạt skill này khi:** bạn tạo file `.tsx` mới trong `src/`, sửa `className`
của bất kỳ component nào, thêm màu sắc mới (Tailwind class hoặc CSS), hoặc người
dùng nhắc tới "theme", "dark mode", "light mode", "giao diện sáng/tối", "màu sắc".

**Bắt buộc đọc trước:** `RULES.md` ở gốc dự án (quy tắc tổng, bảng ánh xạ màu).
File này bổ sung chi tiết kỹ thuật + ví dụ code cụ thể.

---

## 1. Stack kỹ thuật

- Tailwind CSS v4 (config kiểu CSS-first, không có `tailwind.config.js`).
- Dark mode kiểu **class-based**, kích hoạt bằng dòng sau trong `src/index.css`:

  ```css
  @custom-variant dark (&:where(.dark, .dark *));
  ```

  → Điều này khiến `dark:` áp dụng khi có class `.dark` trên `<html>` (hoặc tổ
  tiên), **không** áp dụng theo `prefers-color-scheme` như Tailwind mặc định.

- `src/lib/theme-provider.tsx`: context React quản lý theme.
  - `ThemeMode = 'light' | 'dark' | 'system'`
  - `useTheme()` trả về `{ theme, resolvedTheme, setTheme, toggleTheme }`
  - Lưu lựa chọn vào `localStorage['kolia-ui-theme']`
  - Khi `theme === 'system'`, lắng nghe `matchMedia('(prefers-color-scheme: dark)')`
    để tự cập nhật khi OS đổi theme lúc app đang mở.
  - Áp dụng bằng `document.documentElement.classList.toggle('dark', ...)`.
- `src/components/ui/theme-toggle.tsx`: UI dropdown 3 lựa chọn (Sáng/Tối/Hệ thống),
  đã gắn vào `AdminNavbar` (`src/components/layout/admin-navbar.tsx`).
- `index.html`: script inline (chạy trước khi React mount) đọc `localStorage` và
  set class `.dark` ngay, tránh FOUC (flash of wrong theme).

## 2. Cách wrap component gốc (đã setup — không cần lặp lại)

```tsx
// src/main.tsx
<ThemeProvider defaultTheme="system">
  <NuqsAdapter>
    <App />
  </NuqsAdapter>
</ThemeProvider>
```

Nếu bạn viết component mới cần đọc/đổi theme theo hành động của người dùng (ví dụ:
1 nút "Xem trước dark mode" trong 1 form), dùng hook có sẵn:

```tsx
import { useTheme } from '../../lib/theme-provider'

function MyComponent() {
  const { resolvedTheme, setTheme } = useTheme()
  // resolvedTheme là 'light' | 'dark' đã resolve, dùng để hiển thị icon/logic JS
  // (KHÔNG dùng resolvedTheme để tự viết class có điều kiện — luôn ưu tiên dùng
  // Tailwind `dark:` trong className, chỉ dùng resolvedTheme khi cần rẽ nhánh JS
  // thực sự, ví dụ chọn theme cho 1 thư viện chart bên thứ 3).
}
```

## 3. Quy trình thêm màu cho 1 component mới — ví dụ từng bước

Giả sử bạn tạo 1 thẻ cảnh báo (alert) mới:

**Bước 1 — viết class cho light mode như bình thường:**

```tsx
<div className="rounded-lg border border-amber-200 bg-amber-50 p-3 text-amber-700">
  ...
</div>
```

**Bước 2 — tra bảng ánh xạ (RULES.md mục 5) và thêm `dark:` ngay:**

```tsx
<div className="rounded-lg border border-amber-200 dark:border-amber-800 bg-amber-50 dark:bg-amber-950/40 p-3 text-amber-700 dark:text-amber-300">
  ...
</div>
```

**Bước 3 — nếu là màu trung tính (slate), tra bảng ánh xạ ở RULES.md mục 3:**

```tsx
// Light-only (SAI — thiếu dark:)
<p className="text-slate-500">Ghi chú phụ</p>

// Đúng
<p className="text-slate-500 dark:text-slate-400">Ghi chú phụ</p>
```

**Bước 4 — nếu là pattern "nền đặc + chữ trắng" (nút chính/badge/trạng thái chọn),
lật ngược hoàn toàn thay vì dùng bảng ánh xạ thông thường:**

```tsx
// Đúng — xem thêm src/components/ui/button.tsx (variant "default")
className="bg-slate-900 text-white hover:bg-slate-800 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-300"
```

## 4. Các lỗi thường gặp cần tự kiểm tra trước khi hoàn thành task

- [ ] Còn `bg-white`/`bg-slate-50..800` mà **không có** `dark:` cạnh nó? → thêm.
- [ ] Có `text-slate-*` không có `dark:text-slate-*`? → thêm theo bảng.
- [ ] Copy-paste 1 pattern "nền đặc `bg-slate-900` + `text-white`" mà **quên** lật
  ngược? Đây là lỗi phổ biến nhất — hậu quả là nút/badge biến mất (đen trên đen)
  khi ở dark mode. Luôn tự hỏi: "nếu nền đảo thành `slate-100`, chữ trắng còn đọc
  được không?" — nếu không, phải thêm `dark:text-slate-900`.
- [ ] Dùng `gray-`/`zinc-`/`neutral-`/`stone-` thay vì `slate-`? → đổi thành `slate`
  để đồng bộ (dự án dùng đúng 1 họ màu trung tính).
- [ ] Thêm `style={{ color: '#...' }}` hoặc hex trực tiếp? → tránh, dùng Tailwind
  class hoặc biến CSS có định nghĩa `.dark` riêng trong `src/index.css`.
- [ ] Đã chạy `npm run build` để chắc chắn không lỗi cú pháp Tailwind/TS?

## 5. Tệp liên quan (đọc khi cần sửa sâu hơn)

| File | Vai trò |
|---|---|
| `src/index.css` | `@custom-variant dark`, biến scrollbar, nền trang cho `.dark` |
| `src/App.css` | Biến `:root`/`.dark` dự phòng (đồng bộ với `index.css`) |
| `index.html` | Script chống FOUC, set `.dark` trước khi React mount |
| `src/lib/theme-provider.tsx` | Context + hook `useTheme` |
| `src/components/ui/theme-toggle.tsx` | UI đổi theme |
| `src/components/layout/admin-navbar.tsx` | Nơi `ThemeToggle` được gắn vào |
| `src/components/ui/button.tsx`, `badge.tsx`, `checkbox.tsx` | Ví dụ mẫu cho pattern "lật ngược" ở mục 4 |
| `RULES.md` (gốc dự án) | Quy tắc tổng + bảng ánh xạ đầy đủ |

## 6. Khi người dùng yêu cầu điều gì đó mâu thuẫn với skill này

Nếu người dùng yêu cầu (ví dụ) "chỉ làm dark mode cho trang X" hoặc "dùng màu hex
trực tiếp cho nhanh" — **hãy nhắc rằng điều này phá vỡ tính nhất quán toàn dự án**,
nhưng vẫn thực hiện nếu người dùng xác nhận rõ ràng muốn vậy. Mặc định luôn ưu tiên
áp dụng đầy đủ theo `RULES.md`.
