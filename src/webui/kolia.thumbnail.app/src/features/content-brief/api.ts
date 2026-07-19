import { httpClient } from '../../lib/api/http-client'
import type { CImportContentSource } from '../../types/enums/pipeline.enums'

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

export async function analyzeBrief(projectId: string, manualPrompt?: string): Promise<ContentBriefDto> {
  return httpClient.post<ContentBriefDto>(`/api/v1/projects/${projectId}/brief/analyze`, { manualPrompt })
}

export async function syncSheetBrief(projectId: string, sheetUrl: string): Promise<ContentBriefDto> {
  return httpClient.post<ContentBriefDto>(`/api/v1/projects/${projectId}/brief/sync-sheet`, { sheetUrl })
}

export async function confirmBrief(projectId: string): Promise<void> {
  await httpClient.post(`/api/v1/projects/${projectId}/brief/confirm`, {})
}
