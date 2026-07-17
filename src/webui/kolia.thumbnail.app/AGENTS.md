# AGENTS.md

Đây là dự án **Kolia Thumbnail Engine** (React + TypeScript + Vite + Tailwind CSS v4).

## Đọc trước khi làm bất kỳ thay đổi UI/CSS nào

1. **`RULES.md`** (gốc dự án) — quy tắc bắt buộc, quan trọng nhất là quy tắc
   **Light/Dark theme** (mục 1) và bảng ánh xạ màu (mục 3–5).
2. **`.agent/skills/theming.skill.md`** — hướng dẫn kỹ thuật chi tiết + ví dụ code
   để thêm màu mới đúng cách cho cả hai theme.

Đây không phải tài liệu tham khảo tuỳ chọn — bất kỳ agent nào (Claude Code, Cursor,
GitHub Copilot, Codex, Windsurf, hoặc agent khác) chỉnh sửa component có `className`
chứa màu sắc **phải** tuân theo hai file trên trước khi commit.

## Lệnh hữu ích

```bash
npm install
npm run dev      # chạy dev server (Vite)
npm run build    # type-check (tsc -b) + build production — chạy sau mỗi thay đổi lớn
npm run lint     # oxlint
```

## Cấu trúc dự án (tóm tắt)

- `src/components/ui/` — component UI dùng chung (button, input, dialog, checkbox…)
- `src/components/layout/` — layout admin (sidebar, navbar, footer)
- `src/components/data-table/`, `src/components/filters/`, `src/components/selects/`
  — component nghiệp vụ dùng chung
- `src/features/**` — các trang tính năng theo domain (ai-providers, ai-configurations,
  social-media-providers…)
- `src/lib/theme-provider.tsx` — hệ thống theme sáng/tối (xem RULES.md)

## Quy tắc code chung khác

- Toàn bộ text hiển thị trong UI là tiếng Việt — giữ nguyên ngôn ngữ khi thêm mới.
- Dùng `cn()` từ `src/lib/utils.ts` để merge className, không nối string tay.
- Không thêm dependency mới nếu chưa cần thiết; ưu tiên các thư viện đã có trong
  `package.json` (react-hook-form, zod, @tanstack/react-query, @tanstack/react-table…).
