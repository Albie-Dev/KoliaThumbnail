import { useEffect, useRef, useState, useCallback } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { FolderOpen } from 'lucide-react'
import { SelectDropdown } from '../selects/select-dropdown'
import { useActiveProjectId } from '../../lib/project-context'
import { getProjectsWithPaging, type ProjectSummaryDto } from '../../features/projects/api'
import { SortDirection, type BackendPagedResponse, type PagedRequestParams } from '../../types/paging.types'
import { qk } from '../../lib/query-keys'

/**
 * Project selector hiển thị ở topbar.
 * - Fetch projects với sort CreationTime DESC (mới nhất lên đầu)
 * - Auto-select project có ngày tạo gần nhất nếu chưa có project nào được chọn
 * - Đồng bộ với URL: khi chọn project → set projectId, khi URL đổi → cập nhật dropdown
 */
export function ProjectSelector() {
  const [activeProjectId, setActiveProjectId] = useActiveProjectId()
  const navigate = useNavigate()
  const location = useLocation()
  const [selectedProject, setSelectedProject] = useState<ProjectSummaryDto | null>(null)
  const autoSelectedRef = useRef(false)

  // Fetch danh sách project lần đầu để auto-select project mới nhất
  const { data: initialData } = useQuery({
    queryKey: qk.projects.paging({ pageNumber: 1, pageSize: 1 }),
    queryFn: () =>
      getProjectsWithPaging({
        pageNumber: 1,
        pageSize: 1,
        sorts: [{ field: 'CreationTime', direction: SortDirection.Desc }],
      }),
  })

  // Auto-select project mới nhất nếu chưa có project nào được chọn
  useEffect(() => {
    if (autoSelectedRef.current) return
    if (!initialData?.items?.length) return

    const latest = initialData.items[0]

    if (!activeProjectId) {
      autoSelectedRef.current = true
      setSelectedProject(latest)
      setActiveProjectId(latest.id)
      // Nếu đang ở trang không phải pipeline/dashboard, chuyển về dashboard
      if (
        location.pathname !== '/dashboard' &&
        !location.pathname.startsWith('/pipeline')
      ) {
        navigate('/dashboard?projectId=' + encodeURIComponent(latest.id), { replace: true })
      }
    }
  }, [initialData, activeProjectId, setActiveProjectId, navigate, location.pathname])

  // Đồng bộ selectedProject khi activeProjectId thay đổi từ nơi khác
  useEffect(() => {
    if (!activeProjectId) {
      setSelectedProject(null)
      return
    }
    if (selectedProject?.id === activeProjectId) return

    // Fetch tất cả projects và tìm bằng id (tránh bug JsonElement filter)
    getProjectsWithPaging({
      pageNumber: 1,
      pageSize: 100,
      sorts: [{ field: 'CreationTime', direction: SortDirection.Desc }],
    })
      .then((result) => {
        const found = result.items.find((p) => p.id === activeProjectId)
        if (found) {
          setSelectedProject(found)
        } else {
          setSelectedProject(null)
          setActiveProjectId(null)
        }
      })
      .catch(() => {
        setSelectedProject(null)
        setActiveProjectId(null)
      })
  }, [activeProjectId])

  // Khi initial fetch cho auto-select trả về rỗng (không còn project nào)
  // mà vẫn còn activeProjectId cũ → clear nó
  useEffect(() => {
    if (initialData && initialData.items.length === 0 && activeProjectId) {
      setSelectedProject(null)
      setActiveProjectId(null)
    }
  }, [initialData, activeProjectId])

  const fetchProjects = useCallback(
    async (params: PagedRequestParams): Promise<BackendPagedResponse<ProjectSummaryDto>> => {
      const result = await getProjectsWithPaging(params)
      return {
        items: result.items,
        pageInfo: {
          pageNumber: result.pageNumber,
          pageSize: result.pageSize,
          totalRecords: result.totalCount,
          totalPages: result.totalPages,
          hasPreviousPage: result.pageNumber > 1,
          hasNextPage: result.pageNumber < result.totalPages,
        },
      }
    },
    [],
  )

  return (
    <div className="min-w-[120px] max-w-[200px] sm:min-w-[160px] sm:max-w-[280px] lg:min-w-[200px] lg:max-w-[320px]">
      <SelectDropdown<ProjectSummaryDto>
        fetchData={fetchProjects}
        extraParams={{
          sorts: [{ field: 'CreationTime', direction: SortDirection.Desc }],
          includeTotalCount: true,
          includeItems: true,
        }}
        getOptionId={(p) => p.id}
        getOptionLabel={(p) => p.name}
        renderValue={(p) => (
          <div className="flex items-center gap-2 truncate">
            <FolderOpen className="h-3.5 w-3.5 shrink-0 text-slate-400" />
            <span className="truncate text-sm">{p.name}</span>
          </div>
        )}
        renderOption={(p, isSelected) => (
          <div
            className={`flex items-center gap-2 px-2 py-1.5 text-sm ${
              isSelected ? 'font-medium' : ''
            }`}
          >
            <FolderOpen className="h-3.5 w-3.5 shrink-0 text-slate-400" />
            <div className="flex-1 min-w-0">
              <p className="truncate text-slate-700 dark:text-slate-200">{p.name}</p>
              <p className="text-[10px] text-slate-400">
                {p.creationTime ? new Date(p.creationTime).toLocaleDateString('vi-VN') : ''}
              </p>
            </div>
          </div>
        )}
        value={selectedProject}
        onChange={(project) => {
          if (!project) return
          setSelectedProject(project)
          setActiveProjectId(project.id)
          if (
            !location.pathname.startsWith('/pipeline') &&
            location.pathname !== '/dashboard'
          ) {
            navigate('/dashboard?projectId=' + encodeURIComponent(project.id))
          }
        }}
        allowSearch={true}
        searchPlaceholder="Tìm project..."
        placeholder="Chọn project..."
        emptyText="Không có project nào"
        pageSize={20}
      />
    </div>
  )
}