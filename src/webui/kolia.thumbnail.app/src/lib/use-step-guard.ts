import { useEffect } from 'react'
import { useQuery } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { useActiveProjectId } from './project-context'
import { getProjectById } from '../features/projects/api'
import { qk } from './query-keys'
import { CProjectStepStatus, CProjectStepNumber } from '../types/enums/pipeline.enums'

/**
 * Map route path → step number
 */
const ROUTE_TO_STEP: Record<string, CProjectStepNumber> = {
  '/pipeline/video-content': CProjectStepNumber.ContentBrief,
  '/pipeline/news': CProjectStepNumber.News,
  '/pipeline/reference': CProjectStepNumber.ThumbnailReference,
  '/pipeline/reference/library': CProjectStepNumber.ThumbnailReference,
  '/pipeline/thumbnail/display-text': CProjectStepNumber.GenerateThumbnail,
  '/pipeline/thumbnail/generate': CProjectStepNumber.GenerateThumbnail,
  '/pipeline/video-title': CProjectStepNumber.VideoTitle,
}

/**
 * Guard kiểm tra xem user có quyền truy cập step hiện tại không.
 * Nếu step chưa tới (cả status undefined và NotStarted) → redirect về dashboard.
 * Chỉ chặn ở client-side; BE tự chặn ở server nếu cần.
 *
 * @param currentRoute - Route hiện tại (thường là location.pathname)
 * @param redirectTo - Đường dẫn redirect khi không có quyền (mặc định /dashboard)
 */
export function useStepGuard(currentRoute: string, redirectTo?: string): { isLoading: boolean; isAllowed: boolean } {
  const [activeProjectId] = useActiveProjectId()
  const navigate = useNavigate()
  const fallback = redirectTo ?? (activeProjectId ? `/dashboard?projectId=${encodeURIComponent(activeProjectId)}` : '/archive')

  const { data: project, isLoading } = useQuery({
    queryKey: activeProjectId ? qk.projects.detail(activeProjectId) : ['projects', 'empty'],
    queryFn: () => getProjectById(activeProjectId!),
    enabled: !!activeProjectId,
    staleTime: 30_000,
  })

  const stepNumber = ROUTE_TO_STEP[currentRoute]
  const isAllowed = !stepNumber || // route không phải pipeline step → luôn cho phép
    !activeProjectId || // không có project → không check
    !project?.steps || // chưa load xong → chưa kết luận
    project.steps.some((s) => s.stepNumber === stepNumber && (
      s.stepStatus === CProjectStepStatus.Completed ||
      s.stepStatus === CProjectStepStatus.InProgress
    ))

  useEffect(() => {
    if (isLoading || !project?.steps) return
    if (!isAllowed) {
      navigate(fallback, { replace: true })
    }
  }, [isAllowed, isLoading, project, navigate, fallback])

  return { isLoading, isAllowed }
}
