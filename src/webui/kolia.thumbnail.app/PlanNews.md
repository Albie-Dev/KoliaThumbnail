# PLAN FE: Trang quản trị "Nguồn tin tức" (News Source)
### Dự án: kolia-thumbnail-app (React + TS + Vite + Tailwind v4)
### Đối tượng đọc: AI Coding Agent (Claude Code) — thực thi tuần tự, không bỏ bước

---

## 0. ĐÃ ĐỌC GÌ TRƯỚC KHI VIẾT PLAN NÀY

Đã đọc trực tiếp trong repo FE để đảm bảo plan **đúng 100% convention hiện có**, không bịa pattern mới:
- `CLAUDE.md`, `AGENTS.md`, `RULES.md`, `.agent/skills/theming.skill.md` — quy tắc theme sáng/tối bắt buộc.
- `src/features/ai-providers/**` và `src/features/social-media-providers/**` — 2 feature admin CRUD
  gần giống nhất (có `type`/badge màu, paging, filter, sidebar form) — dùng làm **khuôn mẫu bắt buộc**.
- `src/lib/sidebar-registry.tsx`, `src/lib/admin-menu.tsx` — cơ chế đăng ký route + sidebar form,
  không được sửa `app-sidebar.tsx`/router thủ công, phải đăng ký qua registry đúng cách đang dùng.
- `src/components/data-table/*`, `src/components/filters/status-filter.tsx`,
  `src/components/selects/select-dropdown.tsx` — component dùng chung, tái sử dụng nguyên bản.
- Backend `AdminNewsSourceController.cs` (bạn vừa gửi) — để khớp đúng route, method, DTO field.

**Nguyên tắc bắt buộc xuyên suốt:** feature mới đặt tại `src/features/news-sources/`, đặt tên file
**theo đúng số nhiều/số ít y hệt** `ai-providers`/`social-media-providers` (không tự sáng tạo cách đặt
tên khác), tái dùng `DataTable`, `SelectDropdown`, `StatusFilterGroup`, `ConfirmDialog`, `FormSection`,
`FormGroup`, `FormLabel`, `FormField` — **không viết lại các component này**.

---

## 1. VỊ TRÍ TRONG MENU

Backend route là `admin/news-sources` (config nguồn — không phải nghiệp vụ hàng ngày), nên đặt cùng
nhóm với "Nhà cung cấp AI" / "Nhà cung cấp Mạng xã hội" trong `adminMenuGroups` (nhóm **"Cấu hình"**),
tạo submenu con mới **"Cấu hình nguồn tin tức"**:

```
Cấu hình
 ├─ Cấu hình AI            (đã có)
 ├─ Cấu hình Mạng xã hội   (đã có)
 ├─ Google Service Accounts (đã có)
 └─ Nguồn tin tức           (MỚI)
     └─ Nguồn tin            → NewsSourcesPage
```

Không cần submenu con lồng thêm cấp nữa (không có "Cấu hình Key" riêng cho nguồn tin — khác AI/Social
vì nguồn tin không có khái niệm API key xoay vòng).

Icon gợi ý: `Rss` (từ `lucide-react`, đã có sẵn trong dependency vì `lucide-react` dùng chung cho cả
project) — màu `#f97316` (orange-500, cùng tông với "2. Tin tức" ở nhóm pipeline để gợi liên kết).

---

## 2. HỢP ĐỒNG DỮ LIỆU FE ↔ BE (dựa trên `AdminNewsSourceController.cs` đã đọc)

> ⚠️ **Lưu ý bắt buộc cho AI Agent:** controller đã cho thấy đúng route/method/param, nhưng **chưa
> thấy nội dung file DTO thật** (`NewsSourceListItemDto`, `NewsSourceDetailDto`,
> `NewsSourceCreateDto`, `NewsSourceUpdateDto`, `NewsSourceTestFetchResultDto`). Trước khi code FE,
> **phải mở các file DTO đó trong repo BE để lấy đúng tên field** (PascalCase → camelCase khi map
> sang TS). Danh sách field bên dưới là suy ra hợp lý từ `NewsSourceEntity` đã thiết kế ở plan crawl
> trước đó — dùng làm khung sườn, KHÔNG bịa thêm field ngoài BE thực có, và KHÔNG bỏ sót field nào
> BE thực có mà FE cần hiển thị.

### Route (khớp `[Route("admin/news-sources")]` đã đọc)
| FE action | Method + Route | Ghi chú |
|---|---|---|
| List phân trang | `GET /admin/news-sources/paging` | Có `Name = "ListNewsSourcesPaging"` — param: `PagedRequestDto` + `group`, `region`, `isTrusted`, `includeDeleted`, `deletedOnly` |
| Chi tiết | `GET /admin/news-sources/{id}` | |
| Tạo mới | `POST /admin/news-sources` | 201 kèm `Location` header (dùng `CreatedAtRoute`) |
| Cập nhật | `PUT /admin/news-sources/{id}` | |
| Toggle IsTrusted | `PATCH /admin/news-sources/{id}/toggle` | Không cần body |
| Test fetch | `POST /admin/news-sources/{id}/test` | Body: `string[]` keywords (optional) |
| Xoá mềm | `DELETE /admin/news-sources/{id}` | 204 No Content |

> Khác với `ai-providers`/`social-media-providers` (dùng path `/paging` nhưng không thấy tách riêng
> route name), ở đây BE **đặt tên route rõ ràng từng cái** (`ListNewsSourcesPaging`,
> `GetNewsSourceById`...) — FE không cần quan tâm route name (chỉ BE dùng để `CreatedAtRoute`), nhưng
> cần đúng path.

### `NewsSourceListItemDto` (suy ra) — dùng cho bảng danh sách
```ts
export interface NewsSourceListItemDto {
  id: string
  name: string
  domain: string
  sourceGroup: number          // CNewsSourceGroup
  region: number                // CMarketScope: Domestic=1, International=2, Both=3
  fetchMode: number              // CSourceFetchMode
  rssOrFeedUrl: string
  priority: number
  isTrusted: boolean
  consecutiveFailureCount: number
  lastFetchedAt: string | null
  lastFailedAt: string | null
  isDeleted: boolean
  creationTime?: string | null
  lastModificationTime?: string | null
}
```

### `NewsSourceDetailDto extends NewsSourceListItemDto` — không thêm field khác biệt trừ khi BE có
(theo đúng convention `AIProviderDetailDto extends AIProviderBaseDto` đã thấy).

### `NewsSourceCreateDto` / `NewsSourceUpdateDto`
```ts
export interface NewsSourceCreateDto {
  name: string
  domain: string
  sourceGroup: number
  region: number
  fetchMode: number
  rssOrFeedUrl: string
  priority: number
  isTrusted: boolean
}
export interface NewsSourceUpdateDto extends NewsSourceCreateDto {}
```

### `NewsSourceTestFetchResultDto` (suy ra — dùng cho dialog preview)
```ts
export interface NewsSourceTestFetchResultDto {
  success: boolean
  tierUsed: number | string        // tier nào đã fetch thành công (1=RSS,2=GoogleNews,3=Sitemap,4=Cache)
  itemCount: number
  sampleItems: {
    title: string
    url: string
    publishedAt: string | null
  }[]
  errorMessage?: string | null
}
```

---

## 3. ENUM FILE MỚI

### `src/features/news-sources/news-source-group-type.ts`
Theo đúng pattern `ai-provider-type.ts`/`social-media-provider-type.ts` (object `as const` + mảng
OPTIONS + hàm `getXLabel` + hàm `getXBadgeClass`):

```ts
export const CNewsSourceGroup = {
  InternationalFinance: 1,
  OfficialData: 2,
  VietnamFinance: 3,
  ChartMarket: 4,
  YoutubeSearchTrend: 5,
} as const

export type CNewsSourceGroup = (typeof CNewsSourceGroup)[keyof typeof CNewsSourceGroup]

export interface CNewsSourceGroupOption { id: CNewsSourceGroup; label: string }

export const NEWS_SOURCE_GROUP_OPTIONS: CNewsSourceGroupOption[] = [
  { id: CNewsSourceGroup.InternationalFinance, label: 'Tin tài chính quốc tế' },
  { id: CNewsSourceGroup.OfficialData, label: 'Dữ liệu/chính thống' },
  { id: CNewsSourceGroup.VietnamFinance, label: 'Tin tài chính Việt Nam' },
  { id: CNewsSourceGroup.ChartMarket, label: 'Biểu đồ/thị trường' },
  { id: CNewsSourceGroup.YoutubeSearchTrend, label: 'YouTube/Search trend' },
]

export function getNewsSourceGroupLabel(v: CNewsSourceGroup): string | undefined {
  return NEWS_SOURCE_GROUP_OPTIONS.find((o) => o.id === v)?.label
}

const BADGE_COLORS = [
  'bg-blue-100 text-blue-700 dark:bg-blue-950/40 dark:text-blue-400',
  'bg-emerald-100 text-emerald-700 dark:bg-emerald-950/40 dark:text-emerald-400',
  'bg-amber-100 text-amber-700 dark:bg-amber-950/40 dark:text-amber-400',
  'bg-violet-100 text-violet-700 dark:bg-violet-950/40 dark:text-violet-400',
  'bg-rose-100 text-rose-700 dark:bg-rose-950/40 dark:text-rose-400',
] as const

export function getNewsSourceGroupBadgeClass(v: number): string {
  return BADGE_COLORS[Math.abs(v) % BADGE_COLORS.length]
}
```

> **Khác biệt quan trọng so với `ai-provider-type.ts` gốc:** file gốc dùng
> `'bg-blue-100 text-blue-700'` **KHÔNG có `dark:`** — đây thực ra là 1 điểm chưa tuân thủ RULES.md
> mục 5 trong code cũ (badge màu trạng thái phải có `dark:bg-{color}-950/40` + `dark:text-{color}-400`).
> **Không copy y nguyên thiếu sót đó.** Feature mới `news-sources` phải viết đúng RULES.md ngay từ đầu
> (đã thể hiện ở ví dụ trên). Nếu muốn, có thể tách việc thêm `dark:` cho `ai-provider-type.ts`/
> `social-media-provider-type.ts` cũ thành 1 task dọn dẹp riêng — không bắt buộc trong scope plan này.

### `src/features/news-sources/news-source-fetch-mode-type.ts`
```ts
export const CSourceFetchMode = {
  RssDirect: 1,
  GoogleNewsFallback: 2,
  SitemapFallback: 3,
  Custom: 4,
} as const
export type CSourceFetchMode = (typeof CSourceFetchMode)[keyof typeof CSourceFetchMode]
export const SOURCE_FETCH_MODE_OPTIONS = [
  { id: CSourceFetchMode.RssDirect, label: 'RSS trực tiếp' },
  { id: CSourceFetchMode.GoogleNewsFallback, label: 'RSS + Google News fallback' },
  { id: CSourceFetchMode.SitemapFallback, label: 'RSS + Sitemap fallback' },
  { id: CSourceFetchMode.Custom, label: 'Tùy chỉnh (Custom fetcher)' },
]
export function getSourceFetchModeLabel(v: number): string | undefined {
  return SOURCE_FETCH_MODE_OPTIONS.find((o) => o.id === v)?.label
}
```

### Region (CMarketScope) — kiểm tra xem đã có enum dùng chung chưa
`grep -r "CMarketScope" src/` trước khi tạo mới — nếu `features/news/schema.ts` hoặc
`types/enums/pipeline.enums.ts` đã định nghĩa `CMarketScope` (Domestic/International/Both) thì
**import lại, không định nghĩa trùng enum ở 2 nơi**. Chỉ tạo enum riêng nếu xác nhận chưa tồn tại.

---

## 4. DANH SÁCH FILE CẦN TẠO (`src/features/news-sources/`)

| File | Vai trò | Mẫu tham chiếu |
|---|---|---|
| `news-source-group-type.ts` | Enum nhóm nguồn + label + badge màu | `ai-provider-type.ts` |
| `news-source-fetch-mode-type.ts` | Enum fetch mode + label | `ai-provider-type.ts` |
| `api.ts` | Hàm gọi API: list/get/create/update/toggle/testFetch/delete | `ai-providers/api.ts` |
| `schema.ts` | Zod schema create/update | `ai-providers/schema.ts` |
| `news-sources-page.tsx` | Trang chính: DataTable + filter + actions | `ai-providers-page.tsx` |
| `news-source.sidebar.tsx` | Đăng ký sidebar create/edit | `ai-provider.sidebar.tsx` |
| `create-news-source-form.tsx` | Form tạo mới | `create-ai-provider-form.tsx` |
| `edit-news-source-form.tsx` | Form chỉnh sửa | `edit-ai-provider-form.tsx` |
| `news-source-health-badge.tsx` | Component nhỏ hiển thị trạng thái vận hành (mới, không có mẫu sẵn — xem mục 6) | — |
| `news-source-test-fetch-dialog.tsx` | Dialog hiển thị kết quả test fetch (mới — xem mục 7) | dùng `Dialog` từ `components/ui/dialog.tsx` |

**Sửa file có sẵn:**
- `src/lib/admin-menu.tsx` — thêm `lazy import` + menu item (mục 1).

---

## 5. `api.ts` CHI TIẾT

```ts
import { httpClient } from '../../lib/api/http-client'
import { buildPagedQuery } from '../../lib/api/build-paged-query'
import type { BackendPagedResponse, PagedResult, PagedRequestParams } from '../../types/paging.types'
import type { CreateNewsSourceInput } from './schema'

export interface NewsSourceListItemDto {
  id: string
  name: string
  domain: string
  sourceGroup: number
  region: number
  fetchMode: number
  rssOrFeedUrl: string
  priority: number
  isTrusted: boolean
  consecutiveFailureCount: number
  lastFetchedAt: string | null
  lastFailedAt: string | null
  isDeleted: boolean
  creationTime?: string | null
  lastModificationTime?: string | null
}

export interface NewsSourceDetailDto extends NewsSourceListItemDto {
  deletionTime?: string | null
}

export interface NewsSourceTestFetchResultDto {
  success: boolean
  tierUsed: number | string
  itemCount: number
  sampleItems: { title: string; url: string; publishedAt: string | null }[]
  errorMessage?: string | null
}

function toPagedResult(
  payload: BackendPagedResponse<NewsSourceListItemDto>,
): PagedResult<NewsSourceListItemDto> {
  return {
    items: payload.items,
    pageNumber: payload.pageInfo.pageNumber,
    pageSize: payload.pageInfo.pageSize,
    totalCount: payload.pageInfo.totalRecords,
    totalPages: payload.pageInfo.totalPages,
  }
}

export interface NewsSourceListParams extends PagedRequestParams {
  group?: number
  region?: number
  isTrusted?: boolean
  includeDeleted?: boolean
  deletedOnly?: boolean
}

export async function getNewsSourcesWithPaging(params: NewsSourceListParams) {
  const query = buildPagedQuery({ includeTotalCount: true, includeItems: true, ...params })
  if (params.group !== undefined) query.set('group', String(params.group))
  if (params.region !== undefined) query.set('region', String(params.region))
  if (params.isTrusted !== undefined) query.set('isTrusted', String(params.isTrusted))
  if (params.includeDeleted !== undefined) query.set('includeDeleted', String(params.includeDeleted))
  if (params.deletedOnly !== undefined) query.set('deletedOnly', String(params.deletedOnly))

  const response = await httpClient.get<BackendPagedResponse<NewsSourceListItemDto>>(
    `/admin/news-sources/paging?${query.toString()}`,
  )
  return toPagedResult(response)
}

export async function getNewsSourceById(id: string): Promise<NewsSourceDetailDto> {
  return httpClient.get<NewsSourceDetailDto>(`/admin/news-sources/${id}`)
}

export async function createNewsSource(data: CreateNewsSourceInput): Promise<NewsSourceDetailDto> {
  return httpClient.post<NewsSourceDetailDto>('/admin/news-sources', data)
}

export interface UpdateNewsSourceInput extends CreateNewsSourceInput { id: string }

export async function updateNewsSource(data: UpdateNewsSourceInput): Promise<NewsSourceDetailDto> {
  const { id, ...body } = data
  return httpClient.put<NewsSourceDetailDto>(`/admin/news-sources/${id}`, body)
}

export async function toggleNewsSource(id: string): Promise<NewsSourceDetailDto> {
  return httpClient.patch<NewsSourceDetailDto>(`/admin/news-sources/${id}/toggle`, {})
}

export async function testFetchNewsSource(
  id: string,
  keywords: string[],
): Promise<NewsSourceTestFetchResultDto> {
  return httpClient.post<NewsSourceTestFetchResultDto>(`/admin/news-sources/${id}/test`, keywords)
}

export async function deleteNewsSource(id: string): Promise<void> {
  await httpClient.delete(`/admin/news-sources/${id}`)
}
```

> **Kiểm tra trước khi code:** mở `src/lib/api/http-client.ts` để xác nhận có sẵn method `patch` hay
> chưa (`ai-providers`/`social-media-providers` chỉ dùng get/post/put/delete, chưa dùng `patch` ở đâu
> trong repo). Nếu `httpClient` chưa có hàm `patch`, phải thêm vào `http-client.ts` theo đúng pattern
> các hàm khác (không tạo `fetch` thủ công riêng trong `api.ts` của feature).

---

## 6. `news-sources-page.tsx` — ĐIỂM KHÁC BIỆT SO VỚI MẪU `ai-providers-page.tsx`

Copy cấu trúc tổng thể y hệt `ai-providers-page.tsx` (DataTable + `useDataTableState` +
`useQueryState` cho filter qua URL + `ConfirmDialog` cho xoá), nhưng có **thêm 3 điểm khác biệt**:

**a. Cột "Trạng thái vận hành"** — đây là lý do chính khiến feature này khác các feature admin CRUD
thuần túy khác: nguồn tin có thể "đang khoẻ" hay "đang lỗi liên tục". Thêm component mới
`news-source-health-badge.tsx`:

```tsx
interface Props { consecutiveFailureCount: number; isTrusted: boolean; lastFailedAt: string | null }

export function NewsSourceHealthBadge({ consecutiveFailureCount, isTrusted, lastFailedAt }: Props) {
  if (!isTrusted) {
    return (
      <span className="inline-flex items-center gap-1 rounded-full bg-slate-100 dark:bg-slate-800 px-2.5 py-0.5 text-xs font-medium text-slate-500 dark:text-slate-400">
        Đã tắt
      </span>
    )
  }
  if (consecutiveFailureCount >= 3) {
    return (
      <span className="inline-flex items-center gap-1 rounded-full bg-red-50 dark:bg-red-950/40 px-2.5 py-0.5 text-xs font-medium text-red-600 dark:text-red-400">
        Lỗi liên tục ({consecutiveFailureCount})
      </span>
    )
  }
  if (consecutiveFailureCount >= 1) {
    return (
      <span className="inline-flex items-center gap-1 rounded-full bg-amber-50 dark:bg-amber-950/40 px-2.5 py-0.5 text-xs font-medium text-amber-700 dark:text-amber-400">
        Cảnh báo ({consecutiveFailureCount})
      </span>
    )
  }
  return (
    <span className="inline-flex items-center gap-1 rounded-full bg-emerald-50 dark:bg-emerald-950/40 px-2.5 py-0.5 text-xs font-medium text-emerald-700 dark:text-emerald-400">
      Hoạt động tốt
    </span>
  )
}
```
Ngưỡng màu (0 lỗi = xanh, 1-2 = vàng, ≥3 = đỏ, `isTrusted=false` = xám "Đã tắt") khớp đúng với ngưỡng
circuit breaker/health-check đã thiết kế ở plan BE trước (mục 3.10.f — health check tự set
`IsTrusted=false` sau 3 lỗi liên tiếp).

**b. Nút "Test fetch"** cạnh nút Sửa/Xoá trong cột actions — mở `news-source-test-fetch-dialog.tsx`
(mục 7), **không dùng sidebar** (vì đây là hành động xem preview, không phải form nhập liệu).

**c. Nút "Bật/Tắt"** (toggle nhanh `isTrusted`) — dùng `Button variant="ghost"` với icon `Power`/`PowerOff`
từ `lucide-react`, gọi `useMutation({ mutationFn: () => toggleNewsSource(item.id) })`, `onSuccess`
invalidate `['news-sources']` — **không cần** `ConfirmDialog` cho toggle (đây là thao tác nhanh, dễ bật
lại, khác với xoá là không thể hoàn tác).

**Filter sidebar** (`filterContent`) thêm: `SelectDropdown` cho `SourceGroup` (dùng
`NEWS_SOURCE_GROUP_OPTIONS`), `SelectDropdown` cho `Region`, và 1 checkbox riêng "Chỉ hiện nguồn đang
lỗi" (map sang `isTrusted=false` khi check — **không nhầm với `StatusFilterGroup` đã có sẵn cho
active/deleted/all**, đây là filter nghiệp vụ khác, độc lập).

**Cột bảng đề xuất (theo đúng field DTO mục 2):**
`Tên` · `Domain` · `Nhóm` (badge màu từ `getNewsSourceGroupBadgeClass`) · `Khu vực` (Nội địa/Quốc tế/Cả hai)
· `Fetch mode` · `Priority` · `Trạng thái vận hành` (badge mục a) · `Fetch gần nhất` (`formatDateTime`)
· `Tạo lúc` · actions (Test fetch, Bật/Tắt, Sửa, Xoá).

---

## 7. `news-source-test-fetch-dialog.tsx` — COMPONENT MỚI (không có mẫu sẵn trong repo)

Vì đây là action "xem preview kết quả thật ngay lập tức" — khác hẳn create/edit form (không sửa dữ
liệu), nên **không đăng ký qua `sidebar-registry`** (registry hiện chỉ định nghĩa cho form nhập liệu
có `submit`/`isSubmitting`). Dùng thẳng `Dialog` (`components/ui/dialog.tsx`) quản lý bằng state cục
bộ trong `news-sources-page.tsx`:

```tsx
interface Props {
  open: boolean
  onClose: () => void
  sourceId: string | null
  sourceName: string
}

export function NewsSourceTestFetchDialog({ open, onClose, sourceId, sourceName }: Props) {
  const [keywords, setKeywords] = useState<string[]>([])
  const { mutate, data, isPending, reset } = useMutation({
    mutationFn: () => testFetchNewsSource(sourceId!, keywords),
  })

  useEffect(() => { if (open) reset() }, [open, reset])

  return (
    <Dialog open={open} onClose={onClose} title={`Test fetch: ${sourceName}`}>
      <div className="space-y-4">
        <TagInput
          value={keywords}
          onChange={setKeywords}
          placeholder="Nhập keyword để test (bỏ trống = mặc định)"
        />
        <Button onClick={() => mutate()} disabled={isPending}>
          {isPending ? 'Đang test...' : 'Chạy test fetch'}
        </Button>

        {data && (
          <div className="rounded-lg border border-slate-200 dark:border-slate-700 p-3 space-y-2">
            <p className="text-sm">
              Kết quả: <strong>{data.success ? 'Thành công' : 'Thất bại'}</strong>
              {' — '}Tier dùng: {data.tierUsed} — {data.itemCount} tin
            </p>
            {data.errorMessage && (
              <p className="text-sm text-red-600 dark:text-red-400">{data.errorMessage}</p>
            )}
            <ul className="space-y-1 text-xs text-slate-600 dark:text-slate-300">
              {data.sampleItems.map((it) => (
                <li key={it.url} className="truncate">
                  <a href={it.url} target="_blank" rel="noreferrer" className="underline">{it.title}</a>
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>
    </Dialog>
  )
}
```
Dùng lại `TagInput` (`components/ui/tag-input.tsx`) đã có sẵn trong repo cho phần nhập keyword —
**không viết input keyword tự chế**.

---

## 8. WIRING VÀO `admin-menu.tsx`

Thêm theo đúng convention `lazy()` đã thấy trong file, chèn vào nhóm `'Cấu hình'`, ngay sau
`'/configuration/google-services'` (hoặc tạo children mới `'Nguồn tin tức'` nếu muốn để dư địa mở
rộng sau này — vd sau này thêm "Lịch sử fetch"):

```tsx
const NewsSourcesPage = lazy(() =>
  import('../features/news-sources/news-sources-page').then((m) => ({ default: m.NewsSourcesPage })),
)
```

```tsx
{
  key: '/configuration/news-sources',
  label: 'Nguồn tin tức',
  icon: Rss,
  iconColor: '#f97316', // orange-500
  component: NewsSourcesPage,
},
```
(thêm `Rss` vào import `lucide-react` ở đầu file `admin-menu.tsx`).

**KHÔNG sửa `app-sidebar.tsx`/router thủ công** — hệ thống đã tự render route dựa trên
`adminMenuGroups` qua `flattenMenuItems` (đã đọc, xác nhận cơ chế này).

---

## 9. THỨ TỰ THỰC HIỆN

1. Mở BE DTO thật (`NewsSourceListItemDto`, `...DetailDto`, `...CreateDto`, `...UpdateDto`,
   `...TestFetchResultDto`) — đối chiếu, sửa lại mục 2/5 của plan này nếu field khác.
2. Kiểm tra `http-client.ts` có `patch` chưa — thêm nếu thiếu.
3. Kiểm tra `types/enums/pipeline.enums.ts` / `features/news/schema.ts` đã có `CMarketScope` chưa —
   tái dùng nếu có.
4. Tạo `news-source-group-type.ts`, `news-source-fetch-mode-type.ts`.
5. Tạo `api.ts`.
6. Tạo `schema.ts` (zod, theo đúng field create/update mục 2 — `name`, `domain`, `sourceGroup`,
   `region`, `fetchMode`, `rssOrFeedUrl` (`.url()`), `priority` (`z.number().min(0)`), `isTrusted`).
7. Tạo `news-source-health-badge.tsx`.
8. Tạo `create-news-source-form.tsx` + `edit-news-source-form.tsx` (copy cấu trúc
   `create-ai-provider-form.tsx`/`edit-ai-provider-form.tsx`, đổi field).
9. Tạo `news-source.sidebar.tsx` (đăng ký `create-news-source` / `edit-news-source`).
10. Tạo `news-source-test-fetch-dialog.tsx`.
11. Tạo `news-sources-page.tsx` (copy `ai-providers-page.tsx`, thêm 3 điểm khác biệt mục 6).
12. Wiring `admin-menu.tsx` (mục 8).
13. Chạy `npm run build` — bắt buộc theo `CLAUDE.md`, sửa hết lỗi TypeScript/Tailwind trước khi báo xong.
14. Tự kiểm tra bằng mắt: đổi theme sáng/tối ở navbar, xác nhận badge trạng thái + badge nhóm nguồn
    không vỡ contrast ở cả 2 theme (đúng RULES.md mục 6 bước 5).

---

## 10. TIÊU CHÍ NGHIỆM THU (checklist)

- [ ] `npm run build` pass, không lỗi TS/Tailwind.
- [ ] `npm run lint` (oxlint) sạch cho toàn bộ file mới.
- [ ] Trang `/configuration/news-sources` hiển thị đúng danh sách, phân trang, search, sort hoạt động
      giống hệt UX của `AiProvidersPage`.
- [ ] Filter theo Nhóm nguồn / Khu vực / trạng thái hoạt động đúng, phản ánh lên URL query (giữ khi
      reload trang — đúng pattern `useQueryState` đã dùng).
- [ ] Badge "Trạng thái vận hành" đổi màu đúng theo `consecutiveFailureCount`/`isTrusted` (test bằng
      cách sửa tạm giá trị mock/dữ liệu test ở BE).
- [ ] Tạo mới 1 nguồn → validate URL sai hiển thị lỗi đúng field (test cơ chế BE validate URL khi
      Create trả về `InvalidOperationException` → FE hiển thị đúng qua `ApiError`/`setError`, theo
      đúng pattern `onError` ở `create-ai-provider-form.tsx`).
- [ ] Sửa `RssOrFeedUrl` của 1 nguồn → gọi `Test fetch` ngay → xác nhận preview phản ánh URL MỚI
      (không phải cache cũ) — khớp với yêu cầu cache-invalidation đã thiết kế ở BE.
- [ ] Toggle On/Off hoạt động tức thì, cập nhật badge mà không cần reload trang (nhờ
      `invalidateQueries`).
- [ ] Xoá mềm → `ConfirmDialog` xác nhận đúng như các feature khác, item chuyển sang trạng thái
      "Đã xoá" giống cột `actions` của `AiProvidersPage`.
- [ ] Toàn bộ màu sắc mới thêm (badge nhóm nguồn, badge trạng thái vận hành) đều có `dark:` đầy đủ theo
      RULES.md mục 5 — không copy thiếu sót `dark:` từ `ai-provider-type.ts`/
      `social-media-provider-type.ts` cũ.
- [ ] Không sửa `app-sidebar.tsx` hay router thủ công — chỉ đăng ký qua `admin-menu.tsx` +
      `sidebar-registry.tsx` đúng convention.

---

## 11. GHI CHÚ PHẠM VI (KHÔNG LÀM TRONG PLAN NÀY)

- Không đụng vào `src/features/news/` (trang "2. Tin tức" thuộc pipeline Bước 1 — nơi team tick chọn
  tin, xem bảng "Đề xuất chọn"). Đây là trang **khác** — dùng dữ liệu `NewsItemEntity` đã crawl, còn
  `news-sources` là trang **cấu hình nguồn** để crawl. Có thể tích hợp sau (vd hiển thị badge Nhóm
  nguồn ở `news-page.tsx` dựa theo `sourceGroup` trả về trong `NewsItemDto`) nhưng đó là **task riêng,
  ngoài phạm vi plan này** — chỉ nêu ở đây để AI Agent không nhầm 2 trang là một.
- `BackgroundJobs/NewsSourceHealthCheckJob` (BE, Phase 2) không có phần FE riêng bắt buộc — badge
  trạng thái vận hành (mục 6a) đã đủ để admin thấy nguồn nào cần chú ý, không cần thêm trang riêng
  cho lịch sử health-check trong bản đầu.