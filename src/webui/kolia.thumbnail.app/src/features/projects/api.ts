import { httpClient } from '../../lib/api/http-client'
import { buildPagedQuery } from '../../lib/api/build-paged-query'
import type { BackendPagedResponse, PagedResult, PagedRequestParams } from '../../types/paging.types'

// ─────────────────────────────────────────────────────────────────────────────
// LƯU Ý: Chưa lấy được OpenAPI spec thực tế từ
// https://blast-case-skeptic.ngrok-free.dev/scalar (ngrok chặn fetch tự động).
// Endpoint/field bên dưới được đặt theo đúng convention REST đang dùng trong
// dự án (`/api/v1/{resource}/paging`, DTO có `isDeleted`/`creationTime`/
// `lastModificationTime`...). Khi có spec thật, chỉ cần chỉnh path + field trong
// file này — các phần còn lại (project-card, projects-page) dùng qua ProjectBaseDto
// nên không cần sửa UI.
// ─────────────────────────────────────────────────────────────────────────────

export interface ProjectBaseDto {
  id: string
  /** Tên project (hiển thị ở phần nội dung dưới card) */
  name: string
  /** Tiêu đề video — hiển thị nổi bật nhất trong phần nội dung dưới card */
  videoTitle?: string | null
  /** Thumbnail preview của project */
  thumbnailUrl?: string | null
  /** Text hiển thị đè lên góc dưới-trái ảnh (kết quả bước 4.1 — Tạo display text) */
  displayText?: string | null
  /** CProjectStatus — xem project-type.ts */
  status: number
  /** Bước hiện tại trong pipeline 6 bước (1-6) */
  currentStep: number
  /** Số bước đã hoàn thành (0-6) */
  completedSteps: number
  isDeleted: boolean
  creationTime?: string | null
  lastModificationTime?: string | null
}

function toPagedResult(payload: BackendPagedResponse<ProjectBaseDto>): PagedResult<ProjectBaseDto> {
  return {
    items: payload.items,
    pageNumber: payload.pageInfo.pageNumber,
    pageSize: payload.pageInfo.pageSize,
    totalCount: payload.pageInfo.totalRecords,
    totalPages: payload.pageInfo.totalPages,
  }
}

export async function getProjectsWithPaging(params: PagedRequestParams) {
  const query = buildPagedQuery({
    includeTotalCount: true,
    includeItems: true,
    ...params,
  })

  const response = await httpClient.get<BackendPagedResponse<ProjectBaseDto>>(`/api/v1/projects/paging?${query.toString()}`)
  return toPagedResult(response)
}

export async function deleteProject(id: string): Promise<void> {
  await httpClient.delete(`/api/v1/projects/${id}`)
}
