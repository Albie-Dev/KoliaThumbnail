import { useMemo } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Plus, Search } from 'lucide-react'
import { DataTable } from '../../components/data-table/data-table'
import { useDataTableState } from '../../components/data-table/use-data-table-state'
import { Button } from '../../components/ui/button'
import { Input } from '../../components/ui/input'
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card'
import { useSidebarContext } from '../../lib/sidebar-context'
import { formatDateTime } from '../../lib/date-formatter'
import { fetchThumbnails, type ThumbnailItem } from './api'

export function ThumbnailTable() {
  const { open } = useSidebarContext()
  const { page, setPage, pageSize, setPageSize, search, setSearch, sortBy, sortOrder, handleSort } = useDataTableState(1, 10)

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['thumbnails', page, pageSize, search, sortBy, sortOrder],
    queryFn: () => fetchThumbnails(page, pageSize),
  })

  const columns = useMemo(
    () => [
      { key: 'name', header: 'Tên', sortable: true, render: (item: ThumbnailItem) => item.name },
      { key: 'shortName', header: 'Mã', sortable: true, render: (item: ThumbnailItem) => item.shortName },
      {
        key: 'image',
        header: 'Logo',
        render: (item: ThumbnailItem) =>
          item.imageUrl ? (
            <img src={item.imageUrl} alt={item.name} className="h-8 w-8 rounded object-cover" />
          ) : (
            <span className="text-sm text-slate-400">—</span>
          ),
      },
      {
        key: 'created',
        header: 'Tạo lúc',
        render: (item: ThumbnailItem) => formatDateTime(item.creationTime),
      },
    ],
    [],
  )

  const tableError = error instanceof Error ? error.message : null
  const resolvedData = data?.items ?? []

  return (
    <div className="space-y-4">
      <Card>
          <CardHeader>
            <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
              <div>
                <CardTitle>AI Providers</CardTitle>
                <p className="mt-1 text-sm text-slate-500">Danh sách nhà cung cấp được lấy từ API thumbnail.</p>
              </div>
              <Button
                onClick={() => open('create-ai-provider')}
                className="w-full gap-2 sm:w-auto"
              >
                <Plus className="h-4 w-4" />
                Thêm mới
              </Button>
            </div>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex flex-col gap-2 sm:flex-row sm:items-center">
              <div className="relative flex-1">
                <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
                <Input
                  value={search}
                  onChange={(event) => {
                    setSearch(event.target.value)
                    setPage(1)
                  }}
                  placeholder="Nhập tên hoặc mã"
                  className="w-full pl-9"
                />
              </div>
              <Button variant="outline" onClick={() => { setSearch(''); setPage(1) }} className="sm:w-auto">
                Xóa
              </Button>
            </div>

            <DataTable
              title="AI Providers"
              columns={columns}
              data={resolvedData}
              isLoading={isLoading}
              error={tableError}
              onRetry={() => void refetch()}
              emptyMessage="Không có nhà cung cấp nào phù hợp."
              page={data?.pageNumber ?? 1}
              pageSize={data?.pageSize ?? 10}
              totalPages={data?.totalPages ?? 1}
              totalCount={data?.totalCount ?? 0}
              onPageChange={(nextPage) => setPage(nextPage)}
              onPageSizeChange={(nextSize) => {
                setPageSize(nextSize)
                setPage(1)
              }}
              sortBy={sortBy}
              sortOrder={sortOrder}
              onSort={handleSort}
            />
          </CardContent>
        </Card>
      </div>
    )
}
