# RULES.md — Quy tắc bắt buộc cho dự án Kolia Thumbnail Engine

> **Dành cho: mọi AI Agent (Claude Code, Cursor, Copilot, Windsurf, Codex, v.v.) và mọi
> lập trình viên con người làm việc trên repo này.**
>
> File này là **nguồn sự thật duy nhất (single source of truth)** về các quy tắc bắt
> buộc của dự án. Trước khi sửa bất kỳ file `.tsx`/`.css` nào, agent **PHẢI đọc file
> này và `.agent/skills/theming.skill.md`**. Nếu có mâu thuẫn giữa yêu cầu của người
> dùng và quy tắc ở đây, hãy làm theo quy tắc ở đây và nêu rõ lý do, trừ khi người
> dùng xác nhận rõ ràng muốn phá vỡ quy tắc.

## 1. Quy tắc #1 (quan trọng nhất): Light/Dark theme là bắt buộc, không phải tuỳ chọn

Dự án đã có hệ thống theme sáng/tối hoàn chỉnh (xem chi tiết kỹ thuật ở
`.agent/skills/theming.skill.md`). **Mọi màu sắc mới thêm vào bất kỳ component nào
đều phải hoạt động đúng ở cả hai theme.** Không có ngoại lệ, trừ 3 trường hợp được
liệt kê ở mục 4.

Cụ thể:

- Nếu bạn viết `bg-white`, `bg-slate-50` … `bg-slate-800` (bất kỳ màu nền/chữ/viền
  "trần" nào từ bảng màu Tailwind) → **bắt buộc thêm class `dark:...` tương ứng**
  ngay trong cùng một chỗ, không được để riêng cho commit sau.
- Không tạo ra theme thứ 3, không dùng `@media (prefers-color-scheme)` trực tiếp
  trong component (đã được xử lý tập trung ở `src/lib/theme-provider.tsx`).
- Không dùng mã màu hex/rgb viết tay trong JSX/`className` (`style={{ color: '#fff' }}`
  gây vỡ theme). Nếu thực sự cần, phải thêm cả biến CSS cho `.dark`.
- Không xoá hoặc sửa `@custom-variant dark (&:where(.dark, .dark *));` trong
  `src/index.css` — đây là cơ chế bật `dark:` theo class, không theo hệ điều hành.
- Không xoá đoạn script inline chống nhấp nháy (FOUC) trong `index.html`.

## 2. Kiến trúc theme (tóm tắt — chi tiết đầy đủ ở skill file)

- `src/lib/theme-provider.tsx` — React context `ThemeProvider` + hook `useTheme()`.
  Theme có 3 giá trị: `'light' | 'dark' | 'system'`, lưu ở `localStorage` với key
  `kolia-ui-theme`.
- Class `.dark` được toggle trên `<html>`. Toàn bộ Tailwind `dark:` utilities phản
  ứng theo class này.
- `src/components/ui/theme-toggle.tsx` — nút bấm đổi theme, đã được gắn vào
  `src/components/layout/admin-navbar.tsx`.
- `index.html` có script inline chạy trước khi React mount để set class `.dark`
  ngay lập tức, tránh flash sai theme.

## 3. Bảng ánh xạ màu bắt buộc (neutral / slate)

Khi thêm class Tailwind cho màu trung tính (slate/gray — luôn dùng `slate`, **không
dùng `gray`/`zinc`/`neutral`/`stone`** để tránh rời rạc bảng màu), tuân theo đúng
bảng dưới. Đây là quy ước đã áp dụng nhất quán cho toàn bộ codebase hiện tại.

### Chữ (`text-`)

| Light      | Dark        |
|------------|-------------|
| slate-900  | slate-100   |
| slate-800  | slate-200   |
| slate-700  | slate-300   |
| slate-600  | slate-400   |
| slate-500  | slate-400   |
| slate-400  | slate-500   |
| slate-300  | slate-600   |

### Nền / viền / ring / divide (`bg-`, `border-`, `ring-`, `divide-`, `shadow-`)

| Light      | Dark        |
|------------|-------------|
| white      | slate-900   |
| slate-50   | slate-900   |
| slate-100  | slate-800   |
| slate-200  | slate-700   |
| slate-300  | slate-600   |
| slate-400  | slate-500   |

`slate-900` dùng làm **nền** (không phải chữ) là trường hợp đặc biệt — xem mục 4.

## 4. Ba ngoại lệ được phép "không đổi màu" theo theme

1. **Overlay/backdrop** (`bg-black/40`, `bg-black/50` trong modal, drawer) — giữ
   nguyên ở cả hai theme, không cần `dark:`.
2. **Nút/badge/checkbox "primary" dạng nền đặc `bg-slate-900 text-white`** (button
   variant `default`, `Badge` variant `default`, trạng thái checked/selected) —
   đây là màu "đảo ngược" (inverse), **PHẢI lật ngược hoàn toàn** ở dark mode:
   `dark:bg-slate-100 dark:text-slate-900` (không dùng bảng ánh xạ thông thường ở
   mục 3). Xem ví dụ mẫu trong `src/components/ui/button.tsx` (`buttonVariants.default`)
   và `src/components/ui/checkbox.tsx`.
3. **Khối xem trước dạng code/mono** (ví dụ `Textarea` ở content-type `code`/`json`,
   dòng `border-slate-700 bg-slate-900 text-slate-100 font-mono`) — được thiết kế
   giống trình soạn code, **luôn tối** dù theme là gì. Không thêm `dark:` cho khối này.

Nếu bạn không chắc một đoạn màu có thuộc 1 trong 3 ngoại lệ trên hay không, **mặc
định áp dụng bảng ánh xạ ở mục 3**, không tự sáng tạo ngoại lệ mới.

## 5. Màu trạng thái (status colors: đỏ/hồng/vàng/xanh lá/xanh dương/tím)

- Nền nhạt dùng để làm badge/alert (`bg-red-50`, `bg-amber-50`, `bg-emerald-50`,
  `bg-rose-50`, `bg-indigo-50`…) → thêm `dark:bg-{color}-950/40` (giữ độ mờ 40%).
- Chữ trạng thái (`text-red-600/700`, `text-amber-700`…) → thêm `dark:text-{color}-400`
  (hoặc `-300` nếu gốc là shade 700).
- Viền trạng thái (`border-{color}-200/300/400`) → `dark:border-{color}-800/700/600`.
- **Nút/nền đặc bão hoà màu** (`bg-red-600`, `bg-amber-600` dùng làm nút "Xoá",
  "Cảnh báo" với `text-white`) → **giữ nguyên, không thêm `dark:`** — các màu này đã
  đủ tương phản trên cả nền sáng và tối.
- Gradient thương hiệu (`from-indigo-500 to-violet-600`, dùng cho ngày hôm nay trong
  date picker, nút chính có gradient) → giữ nguyên ở cả hai theme.

## 6. Checklist khi thêm component/màu mới

1. Viết class Tailwind như bình thường cho light mode.
2. Tra bảng ở mục 3/5 để tìm class `dark:` tương ứng, thêm ngay cạnh class gốc
   trong cùng chuỗi `className`.
3. Nếu là pattern "nền đặc + chữ trắng" (nút chính, badge mặc định, trạng thái
   được chọn) → áp dụng ngoại lệ lật ngược ở mục 4.2.
4. Chạy `npm run build` — nếu lỗi TypeScript/Tailwind, class `dark:` viết sai cú
   pháp là nguyên nhân phổ biến nhất.
5. Tự kiểm tra bằng mắt: đổi theme qua nút ở navbar (góc phải trên), xác nhận không
   có chữ/nền nào biến mất (contrast vỡ) hoặc bị "trắng trên trắng"/"đen trên đen".
6. Không commit nếu còn màu Tailwind "trần" (không có `dark:`) trừ 3 ngoại lệ ở mục 4.

## 7. Phạm vi áp dụng

Quy tắc này áp dụng cho **toàn bộ `src/`**: components dùng chung (`src/components/ui`,
`src/components/layout`, `src/components/data-table`, `src/components/filters`,
`src/components/selects`) và toàn bộ trang tính năng trong `src/features/**`. Không
có phần nào của UI được miễn trừ.

## 8. Không tự thêm hệ thống theme khác

Không dùng styled-components theme, không dùng CSS-in-JS theme provider khác, không
thêm Tailwind config riêng cho theme. Toàn bộ dự án dùng **một** cơ chế duy nhất mô
tả ở đây và ở `.agent/skills/theming.skill.md`.

## 9. Global API Error Handling — không toast.error ở page

Dự án đã có **global error handler** trong `src/lib/query-client.ts`:
- `QueryCache.onError` và `MutationCache.onError` tự động toast lỗi cho mọi
  `ApiError` (ngoại trừ validation error).
- **Không gọi `toast.error()`** trong `onError` của `useMutation` / `useQuery` ở
  mỗi page — global handler đã làm việc đó.
- Chỉ giữ logic nghiệp vụ trong `onError` nếu cần (vd: `invalidateQueries` để
  refresh UI, `setError` để map validation error vào form field).
- `onSuccess` vẫn gọi `toast.success()` bình thường.

## 10. React Query Key Convention

- Dùng prefix `['tên-feature']` làm base key (vd: `['projects']`,
  `['ai-configurations']`, `['ai-function-configs']`).
- Sau base key, thêm các params (page, pageSize, search, filter, sort...).
- Khi invalidate: dùng `queryClient.invalidateQueries({ queryKey: ['tên-feature'] })`
  — react-query v5 invalidate tất cả queries bắt đầu bằng prefix đó.
- **Không** dùng `qk.xxx.yyy(...)` khi invalidate vì dễ mismatch key.
