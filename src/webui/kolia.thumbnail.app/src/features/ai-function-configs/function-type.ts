/**
 * Mirror BE: CAIFunctionType enum.
 * Dùng để hiển thị tên chức năng trong dropdown / bảng.
 */
export const CAIFunctionType = {
  ContentBriefAnalysis: 1,
  NewsScoring: 2,
  ThumbnailGeneration: 3,
  DisplayTextGeneration: 4,
  VideoTitleGeneration: 5,
  CompletePackageGeneration: 6,
} as const

export type CAIFunctionType = (typeof CAIFunctionType)[keyof typeof CAIFunctionType]

export const FUNCTION_TYPE_OPTIONS: { id: CAIFunctionType; label: string }[] = [
  { id: CAIFunctionType.ContentBriefAnalysis, label: 'Content Brief Analysis' },
  { id: CAIFunctionType.NewsScoring, label: 'News Scoring' },
  { id: CAIFunctionType.ThumbnailGeneration, label: 'Thumbnail Generation' },
  { id: CAIFunctionType.DisplayTextGeneration, label: 'Display Text Generation' },
  { id: CAIFunctionType.VideoTitleGeneration, label: 'Video Title Generation' },
  { id: CAIFunctionType.CompletePackageGeneration, label: 'Complete Package Generation' },
]

export function getFunctionTypeLabel(type: number): string {
  return FUNCTION_TYPE_OPTIONS.find((o) => o.id === type)?.label ?? `Unknown (${type})`
}
