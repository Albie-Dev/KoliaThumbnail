# Kolia Thumbnail App — FE Implementation Plan

**Base:** Vite + React + TypeScript (default template)
**Mục tiêu:** Component `DataTable` enterprise, xử lý paging/error đồng bộ với BE (`ErrorResponse`, `GlobalExceptionMiddleware`), toast xịn, responsive, clean code.

> Giả định paging DTO (BE chưa cung cấp): `PagedResult<T> { items, pageNumber, pageSize, totalCount, totalPages }`. Nếu shape thật khác, chỉ cần sửa 1 adapter duy nhất (`toPagedResult`), không ảnh hưởng phần còn lại.

---

## 1. Tech Stack

| Layer | Lựa chọn | Version | Lý do |
|---|---|---|---|
| Build tool | Vite | 6.x | sẵn có trong default template |
| Framework | React | 19.x | `useTransition`, `useOptimistic` cho UX mượt khi paginate/sort |
| Language | TypeScript | 5.6+ strict mode | type-safe toàn bộ contract API |
| Table engine | TanStack Table | v8 | headless, full quyền control render, server-side sort/filter/paging |
| Data fetching/cache | TanStack Query | v5 | cache theo `queryKey`, retry policy riêng theo status code, auto cancel request cũ |
| Virtualization | TanStack Virtual | v3 | bật khi > 200 rows/trang |
| UI kit | shadcn/ui (Radix primitives) | latest | không lock vendor, copy code vào repo, dễ theme enterprise |
| CSS | Tailwind CSS | v4 | token hoá màu/spacing, dark mode |
| Toast | Sonner | latest | animation mượt, stack toast, `toast.promise()` khớp mutation |
| Form | React Hook Form + Zod | latest | map thẳng `ValidationError[]` từ BE vào field lỗi |
| URL state | nuqs | latest | đồng bộ page/sort/filter lên query string → bookmark/share được |
| HTTP | native `fetch` + wrapper | — | không cần axios, giảm bundle |
| Testing | Vitest + Testing Library + MSW | latest | mock cả case 400/500 để test toast/error state |

---

## 2. Folder Structure

```
src/
├── lib/
│   ├── api/
│   │   ├── http-client.ts        # fetch wrapper, parse ErrorResponse -> ApiError
│   │   └── api-error.ts          # class ApiError
│   ├── query-client.ts           # QueryClient + global onError -> toast
│   └── utils.ts
├── components/
│   ├── ui/                       # shadcn primitives (button, dialog, skeleton...)
│   └── data-table/
│       ├── data-table.tsx
│       ├── data-table-pagination.tsx
│       ├── data-table-toolbar.tsx
│       ├── data-table-error-state.tsx
│       ├── data-table-empty-state.tsx
│       ├── data-table-skeleton.tsx
│       └── use-data-table-state.ts   # sync table state <-> nuqs
├── features/
│   └── thumbnails/
│       ├── api.ts                # queryFn/mutationFn riêng feature
│       ├── columns.tsx
│       ├── schema.ts             # zod schema
│       └── thumbnail-table.tsx
└── types/
    ├── api-error.types.ts        # khớp ErrorResponse/ValidationError
    └── paging.types.ts
```

---

## 3. API Layer — khớp đúng `ErrorResponse` từ Middleware

### 3.1 Type contract (khớp BE)

```ts
// types/api-error.types.ts
export interface ValidationErrorDto {
  property: string;
  message: string;
  errorCode: string;
}

export interface ErrorResponseDto {
  code: string;
  message: string;
  traceId?: string;
  errors?: ValidationErrorDto[];
}
```

### 3.2 `ApiError`

```ts
// lib/api/api-error.ts
export class ApiError extends Error {
  constructor(
    public code: string,
    message: string,
    public status: number,
    public traceId?: string,
    public errors?: ValidationErrorDto[],
  ) {
    super(message);
    this.name = "ApiError";
  }

  get isValidationError() {
    return this.code === "VALIDATION_ERROR";
  }
}
```

### 3.3 HTTP client

```ts
// lib/api/http-client.ts
async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${import.meta.env.VITE_API_BASE_URL}${path}`, {
    ...init,
    headers: { "Content-Type": "application/json", ...init?.headers },
  });

  if (!res.ok) {
    const body: ErrorResponseDto | null = await res.json().catch(() => null);
    throw new ApiError(
      body?.code ?? "UNKNOWN_ERROR",
      body?.message ?? "Đã có lỗi xảy ra, vui lòng thử lại.",
      res.status,
      body?.traceId,
      body?.errors,
    );
  }

  return res.status === 204 ? (undefined as T) : res.json();
}

export const httpClient = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, body: unknown) =>
    request<T>(path, { method: "POST", body: JSON.stringify(body) }),
  put: <T>(path: string, body: unknown) =>
    request<T>(path, { method: "PUT", body: JSON.stringify(body) }),
  delete: <T>(path: string) => request<T>(path, { method: "DELETE" }),
};
```

Không retry ở tầng này — nhường quyền quyết định retry cho TanStack Query (chỗ biết ngữ cảnh query/mutation).

---

## 4. Error Handling Strategy — 3 nhánh đúng theo Middleware

Middleware BE có 3 catch block khác nhau → FE phải phân biệt, không dùng chung 1 toast generic:

| BE case | HTTP | `code` | FE xử lý |
|---|---|---|---|
| `AppException` (business rule) | tuỳ `StatusCode` | tuỳ nghiệp vụ | Toast `warning`, hiện thẳng `message` cho user |
| `FluentValidation.ValidationException` | 400 | `VALIDATION_ERROR` | **Không toast** — map `errors[]` vào field qua `form.setError(property, { message })` |
| `Exception` (unhandled) | 500 | `INTERNAL_SERVER_ERROR` | Toast `error` + nút "Sao chép mã lỗi" dùng `traceId` để gửi support |

### 4.1 Global handler tại `QueryClient`

```ts
// lib/query-client.ts
export const queryClient = new QueryClient({
  queryCache: new QueryCache({
    onError: (error) => {
      if (!(error instanceof ApiError) || error.isValidationError) return;
      toast.error(error.message, {
        description: error.traceId ? `Mã theo dõi: ${error.traceId}` : undefined,
        action: error.traceId
          ? { label: "Sao chép mã lỗi", onClick: () => navigator.clipboard.writeText(error.traceId!) }
          : undefined,
      });
    },
  }),
  mutationCache: new MutationCache({
    onError: (error) => {
      if (!(error instanceof ApiError) || error.isValidationError) return;
      toast.error(error.message);
    },
  }),
  defaultOptions: {
    queries: {
      retry: (count, error) =>
        error instanceof ApiError ? error.status >= 500 && count < 2 : count < 2,
      staleTime: 30_000,
    },
  },
});
```

`VALIDATION_ERROR` bị chặn ở `onError` global vì nó **chỉ** xảy ra trong mutation submit form — xử lý riêng ngay tại component đang mở form, không toast (toast biến mất còn form thì user cần thấy lỗi ngay tại field).

### 4.2 Mapping validation error vào form

```ts
onError: (error) => {
  if (error instanceof ApiError && error.isValidationError) {
    error.errors?.forEach((e) =>
      form.setError(e.property as keyof FormValues, { message: e.message }),
    );
  }
},
```

---

## 5. Component `DataTable<T>` — Generic, tái dùng toàn hệ thống

### 5.1 Props contract

```ts
interface PageParams {
  pageIndex: number;
  pageSize: number;
  sorting?: SortingState;
  filters?: ColumnFiltersState;
  search?: string;
}

interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

interface DataTableProps<T> {
  columns: ColumnDef<T>[];
  queryKey: unknown[];
  queryFn: (params: PageParams) => Promise<PagedResult<T>>;
  toolbar?: (table: Table<T>) => ReactNode;
  emptyMessage?: string;
}
```

### 5.2 Các trạng thái bắt buộc cover

- **Loading lần đầu** → Skeleton rows (không spinner che toàn màn hình)
- **`isFetching` khi đổi trang/sort** → giữ data cũ hiển thị bằng `placeholderData: keepPreviousData`, overlay mờ nhẹ, tránh layout jump
- **Empty result** → illustration + message rõ ràng, không phải bảng trắng trơn
- **Error** (`ApiError` hoặc network) → inline error banner ngay trong khung bảng + nút "Thử lại" gọi `refetch()`. Không chỉ dựa vào toast vì toast tự biến mất, còn banner thì user quay lại tab vẫn thấy được lý do bảng trống.
- **Responsive**: ≥ 768px → table thật; < 768px → chuyển sang list dạng card, ẩn cột phụ qua `column.getIsVisible()`

### 5.3 Đồng bộ state bảng lên URL (`nuqs`)

```ts
const [pageIndex, setPageIndex] = useQueryState("page", parseAsInteger.withDefault(0));
const [pageSize, setPageSize] = useQueryState("size", parseAsInteger.withDefault(20));
const [sorting, setSorting] = useQueryState("sort", parseAsJson<SortingState>().withDefault([]));
```

→ Người dùng share link kèm filter/sort/page, F5 không mất state.

---

## 6. Toast System (Sonner)

- Đặt `<Toaster />` 1 lần ở root, `position="top-right"`, `richColors`
- 4 variant chuẩn hoá: `success` (mutation OK), `warning` (`AppException`), `error` (500 + traceId), `info` (thông báo hệ thống)
- Mutation dùng `toast.promise()`:

```ts
toast.promise(mutateAsync(payload), {
  loading: "Đang xử lý...",
  success: "Cập nhật thành công",
  error: (err) => (err instanceof ApiError ? err.message : "Có lỗi xảy ra"),
});
```

- Toast lỗi 500 luôn kèm action "Sao chép mã lỗi" (traceId) — hỗ trợ debug production mà không cần log FE riêng.

---

## 7. Styling / Design System

- Tailwind v4 token hoá: `--color-primary`, `--radius`, spacing scale nhất quán 4/8/12/16/24
- shadcn/ui components cần: `Table`, `Skeleton`, `Dialog`, `DropdownMenu`, `Popover`, `Badge` (hiển thị status/code lỗi), `Sonner`
- Dark mode qua `class` strategy, test cả 2 theme cho bảng (border/hover state dễ vỡ ở dark mode)
- Sticky header + sticky action column khi bảng scroll ngang trên mobile

---

## 8. Performance

- Virtualize khi > 200 rows/trang (`@tanstack/react-virtual`)
- Debounce ô search 300ms trước khi bắn query
- `keepPreviousData` tránh nháy trắng khi đổi trang
- Memo hoá `columns` definition (tránh re-render toàn bảng mỗi lần parent render)
- Request cancellation tự động qua TanStack Query khi `queryKey` đổi trước khi response cũ về

---

## 9. Testing Strategy

- **MSW**: mock 3 case chính — 200 success, 400 `VALIDATION_ERROR`, 500 `INTERNAL_SERVER_ERROR` — verify đúng nhánh xử lý (form error / toast / banner)
- **Vitest + Testing Library**: test `DataTable` với data giả — loading → data → empty → error → retry
- Test riêng `http-client`: đảm bảo parse đúng field `code/message/traceId/errors` kể cả khi BE trả body rỗng hoặc không phải JSON

---

## 10. Roadmap triển khai

1. `http-client` + `ApiError` + `QueryClient` error handler + Sonner setup
2. `DataTable` core: render headless, skeleton/empty/error state
3. Server-side pagination + sorting, đồng bộ `PageParams` ↔ URL (`nuqs`)
4. Toolbar: search debounce, column visibility, filter theo cột
5. Mutation flow (create/update/delete) + mapping `ValidationError[]` vào form
6. Responsive card view (mobile) + virtualization (dataset lớn)
7. Viết MSW handlers mock đủ 3 nhánh lỗi, test toàn bộ luồng

---

## 11. Việc cần xác nhận từ BE

- [ ] Shape thật của paging response : đọc src\services\thumbnails\Kolia.Thumbnail.API\Models\Commons\APIModel.cs 
- [ ] Danh sách `code` nghiệp vụ cụ thể (`AppException.Code`) để FE map message/icon phù hợp thay vì generic đọc ở đây: src\services\crawlers\Kolia.Crawler.API\Exceptions\
- [ ] Format `TraceId` có phải chuẩn `HttpContext.TraceIdentifier` dạng OpenTelemetry `traceId` (32 hex)