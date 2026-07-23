import { httpClient } from '../../lib/api/http-client'
import { CImportContentSource } from '../../types/enums/pipeline.enums'

// ── DTOs ──────────────────────────────────────────────────────────────────

export interface ContentBriefDto {
  id: string
  projectId: string
  overviewInput?: string | null
  viewpointInput?: string | null
  keyDataInput?: string | null
  topicOutput?: string | null
  mainMessageOutput?: string | null
  highlightDataOutput?: string | null
  isConfirmed: boolean
  importSource?: number | null
  sheetUrl?: string | null
  creationTime?: string | null
  lastModificationTime?: string | null
}

// ── API functions ─────────────────────────────────────────────────────────

export async function getBrief(projectId: string): Promise<ContentBriefDto> {
  return httpClient.get<ContentBriefDto>(`/api/v1/projects/${projectId}/brief`)
}

export async function saveManualBrief(
  projectId: string,
  data: { overviewInput: string; viewpointInput: string; keyDataInput: string },
): Promise<void> {
  await httpClient.put(`/api/v1/projects/${projectId}/brief/manual`, data)
}

export async function importBrief(
  projectId: string,
  data: {
    source: CImportContentSource
    rawText?: string
    fileUrl?: string
    externalLink?: string
  },
): Promise<void> {
  await httpClient.post(`/api/v1/projects/${projectId}/brief/import`, data)
}

/**
 * Import dữ liệu từ PasteText và tự động gọi AI Agent để phân tích,
 * trích xuất toàn bộ 6 trường nội dung ngay trong một lần gọi.
 */
export async function importAndAnalyzeBrief(
  projectId: string,
  rawText: string,
): Promise<ContentBriefDto> {
  return httpClient.post<ContentBriefDto>(
    `/api/v1/projects/${projectId}/brief/import-and-analyze`,
    { source: CImportContentSource.PasteText, rawText },
  )
}

/**
 * Upload file text và tự động gọi AI Agent để phân tích,
 * trích xuất toàn bộ 6 trường nội dung.
 * Gửi trực tiếp file dưới dạng multipart/form-data.
 */
export async function importFileAndAnalyzeBrief(
  projectId: string,
  file: File,
): Promise<ContentBriefDto> {
  const formData = new FormData()
  formData.append('file', file)

  const res = await fetch(`${import.meta.env.VITE_API_BASE_URL ?? 'https://holes-interactive-variations-given.trycloudflare.com'}/api/v1/projects/${projectId}/brief/import-file`, {
    method: 'POST',
    body: formData,
  })

  const text = await res.text()
  const body = text ? JSON.parse(text) : null

  if (!res.ok) {
    throw new Error(body?.message ?? 'Upload file thất bại!')
  }

  return body as ContentBriefDto
}

export async function analyzeBrief(projectId: string, manualPrompt?: string): Promise<ContentBriefDto> {
  return httpClient.post<ContentBriefDto>(`/api/v1/projects/${projectId}/brief/analyze`, { manualPrompt })
}

export async function syncSheetBrief(projectId: string, sheetUrl: string): Promise<ContentBriefDto> {
  return httpClient.post<ContentBriefDto>(`/api/v1/projects/${projectId}/brief/sync-sheet`, { sheetUrl })
}

export async function confirmBrief(projectId: string): Promise<void> {
  await httpClient.post(`/api/v1/projects/${projectId}/brief/confirm`, {})
}
