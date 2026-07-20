/**
 * Tập trung toàn bộ react-query key thành factory để tránh lệch tay khi
 * invalidate. Mọi useQuery/invalidateQueries bắt buộc dùng qk.*.
 */
export const qk = {
  projects: {
    paging: (params: unknown) => ['projects', 'paging', params] as const,
    detail: (id: string) => ['projects', id] as const,
  },
  brief: (projectId: string) => ['brief', projectId] as const,
  news: {
    list: (projectId: string) => ['news', projectId] as const,
    suggestedKeywords: (projectId: string) => ['news', projectId, 'suggested-keywords'] as const,
    deepAnalysis: (newsItemId: string) => ['news', 'deep-analysis', newsItemId] as const,
  },
  thumbnailLibrary: {
    list: (projectId: string, excludeIrrelevant?: boolean) =>
      ['thumbnail-library', projectId, excludeIrrelevant] as const,
    analysis: (itemId: string) => ['thumbnail-library', 'analysis', itemId] as const,
  },
  googleServices: {
    paging: (params: unknown) => ['google-services', 'paging', params] as const,
    detail: (id: string) => ['google-services', id] as const,
  },
  scheduledJobs: {
    paging: (params: unknown) => ['scheduled-jobs', 'paging', params] as const,
    detail: (id: string) => ['scheduled-jobs', id] as const,
    logs: (id: string) => ['scheduled-jobs', id, 'logs'] as const,
  },
  displayTexts: (projectId: string) => ['display-texts', projectId] as const,
  thumbnailGeneration: (projectId: string) => ['thumbnail-generation', projectId] as const,
  characters: {
    list: () => ['characters'] as const,
    detail: (id: string) => ['characters', id] as const,
  },
  videoTitles: (projectId: string) => ['video-titles', projectId] as const,
  completePackages: (projectId: string) => ['complete-packages', projectId] as const,
}
