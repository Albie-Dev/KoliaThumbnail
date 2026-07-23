// =============================================================================
// Pipeline Enums — mirror BE C# enums dạng as const
// =============================================================================

// ── CProjectStatus ──────────────────────────────────────────────────────────
export const CProjectStatus = {
  /// <summary>Bản nháp</summary>
  Draft: 0,
  /// <summary>Chờ xử lý</summary>
  Pending: 1,
  /// <summary>Đang thực hiện</summary>
  Running: 2,
  /// <summary>Tạm dừng</summary>
  Paused: 3,
  /// <summary>Hoàn thành</summary>
  Completed: 4,
  /// <summary>Thất bại</summary>
  Failed: 5,
  /// <summary>Đã huỷ</summary>
  Cancelled: 6,
} as const

export type CProjectStatus = (typeof CProjectStatus)[keyof typeof CProjectStatus]

export interface CProjectStatusOption {
  id: CProjectStatus
  label: string
  badgeClass: string
}

export const PROJECT_STATUS_OPTIONS: CProjectStatusOption[] = [
  { id: CProjectStatus.Draft, label: 'Bản nháp', badgeClass: 'bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300' },
  { id: CProjectStatus.Pending, label: 'Chờ xử lý', badgeClass: 'bg-amber-50 dark:bg-amber-950/40 text-amber-700 dark:text-amber-300' },
  { id: CProjectStatus.Running, label: 'Đang thực hiện', badgeClass: 'bg-blue-50 dark:bg-blue-950/40 text-blue-700 dark:text-blue-300' },
  { id: CProjectStatus.Paused, label: 'Tạm dừng', badgeClass: 'bg-orange-50 dark:bg-orange-950/40 text-orange-700 dark:text-orange-300' },
  { id: CProjectStatus.Completed, label: 'Hoàn thành', badgeClass: 'bg-emerald-50 dark:bg-emerald-950/40 text-emerald-700 dark:text-emerald-300' },
  { id: CProjectStatus.Failed, label: 'Thất bại', badgeClass: 'bg-rose-50 dark:bg-rose-950/40 text-rose-700 dark:text-rose-300' },
  { id: CProjectStatus.Cancelled, label: 'Đã huỷ', badgeClass: 'bg-slate-100 dark:bg-slate-800 text-slate-500 dark:text-slate-400' },
]

export function getProjectStatusLabel(status: CProjectStatus): string {
  return PROJECT_STATUS_OPTIONS.find((o) => o.id === status)?.label ?? ''
}

export function getProjectStatusBadgeClass(status: CProjectStatus): string {
  return PROJECT_STATUS_OPTIONS.find((o) => o.id === status)?.badgeClass ?? ''
}

// ── CProjectStepNumber ─────────────────────────────────────────────────────
export const CProjectStepNumber = {
  /// <summary>Nội dung video (Content Brief)</summary>
  ContentBrief: 1,
  /// <summary>Tin tức</summary>
  News: 2,
  /// <summary>Thumbnail tham khảo</summary>
  ThumbnailReference: 3,
  /// <summary>Tạo thumbnail</summary>
  GenerateThumbnail: 4,
  /// <summary>Video Title</summary>
  VideoTitle: 5,
} as const

export type CProjectStepNumber = (typeof CProjectStepNumber)[keyof typeof CProjectStepNumber]

export const STEP_NUMBER_LABELS: Record<number, string> = {
  [CProjectStepNumber.ContentBrief]: '1. Nội dung video',
  [CProjectStepNumber.News]: '2. Tin tức',
  [CProjectStepNumber.ThumbnailReference]: '3. Thumbnail tham khảo',
  [CProjectStepNumber.GenerateThumbnail]: '4. Tạo thumbnail',
  [CProjectStepNumber.VideoTitle]: '5. Video Title',
}

export const STEP_NUMBER_ROUTES: Record<number, string> = {
  [CProjectStepNumber.ContentBrief]: '/pipeline/video-content',
  [CProjectStepNumber.News]: '/pipeline/news',
  [CProjectStepNumber.ThumbnailReference]: '/pipeline/reference',
  [CProjectStepNumber.GenerateThumbnail]: '/pipeline/thumbnail/display-text',
  [CProjectStepNumber.VideoTitle]: '/pipeline/video-title',
}

export const NEXT_STEP_HINTS: Record<number, string> = {
  [CProjectStepNumber.ContentBrief]: 'Nhập nội dung tổng quan, quan điểm và dữ liệu quan trọng cho video.',
  [CProjectStepNumber.News]: 'Tìm kiếm tin tức liên quan, phân tích cảm xúc và chọn tin phù hợp.',
  [CProjectStepNumber.ThumbnailReference]: 'Tra cứu thumbnail tham khảo, phân tích mẫu và chọn hướng thiết kế.',
  [CProjectStepNumber.GenerateThumbnail]: 'Tạo display text, sinh thumbnail, chỉnh sửa và chọn mẫu tốt nhất.',
  [CProjectStepNumber.VideoTitle]: 'Tạo tiêu đề video, chọn phong cách và hoàn thiện title.',
}

// ── CProjectStepStatus ─────────────────────────────────────────────────────
export const CProjectStepStatus = {
  /// <summary>Chưa bắt đầu</summary>
  NotStarted: 0,
  /// <summary>Đang thực hiện</summary>
  InProgress: 1,
  /// <summary>Hoàn thành</summary>
  Completed: 2,
  /// <summary>Thất bại</summary>
  Failed: 3,
  /// <summary>Đã bỏ qua</summary>
  Skipped: 4,
} as const

export type CProjectStepStatus = (typeof CProjectStepStatus)[keyof typeof CProjectStepStatus]

// ── CImportContentSource ───────────────────────────────────────────────────
export const CImportContentSource = {
  /// <summary>Dán văn bản</summary>
  PasteText: 1,
  /// <summary>File</summary>
  File: 2,
  /// <summary>Link ngoài</summary>
  ExternalLink: 3,
} as const

export type CImportContentSource = (typeof CImportContentSource)[keyof typeof CImportContentSource]

export const IMPORT_CONTENT_SOURCE_OPTIONS: { id: CImportContentSource; label: string }[] = [
  { id: CImportContentSource.PasteText, label: 'Dán văn bản' },
  { id: CImportContentSource.File, label: 'File' },
  { id: CImportContentSource.ExternalLink, label: 'Link ngoài' },
]

// ── CMarketScope ───────────────────────────────────────────────────────────
export const CMarketScope = {
  /// <summary>Trong nước</summary>
  Domestic: 1,
  /// <summary>Quốc tế</summary>
  International: 2,
  /// <summary>Cả hai</summary>
  Both: 3,
} as const

export type CMarketScope = (typeof CMarketScope)[keyof typeof CMarketScope]

export const MARKET_SCOPE_OPTIONS: { id: CMarketScope; label: string }[] = [
  { id: CMarketScope.Domestic, label: 'Trong nước' },
  { id: CMarketScope.International, label: 'Quốc tế' },
  { id: CMarketScope.Both, label: 'Cả hai' },
]

// ── CNewsTimeRange ─────────────────────────────────────────────────────────
export const CNewsTimeRange = {
  /// <summary>24 giờ qua</summary>
  Last24Hours: 1,
  /// <summary>48 giờ qua</summary>
  Last48Hours: 2,
  /// <summary>72 giờ qua</summary>
  Last72Hours: 3,
  /// <summary>7 ngày gần nhất (khuyến nghị — hiệu quả nhất)</summary>
  Last7Days: 4,
  /// <summary>30 ngày gần nhất (cảnh báo: nặng tài nguyên)</summary>
  Last30Days: 5,
} as const

export type CNewsTimeRange = (typeof CNewsTimeRange)[keyof typeof CNewsTimeRange]

export const NEWS_TIME_RANGE_OPTIONS: { id: CNewsTimeRange; label: string; warning?: string }[] = [
  { id: CNewsTimeRange.Last24Hours, label: '24 giờ qua' },
  { id: CNewsTimeRange.Last48Hours, label: '48 giờ qua' },
  { id: CNewsTimeRange.Last72Hours, label: '72 giờ qua' },
  { id: CNewsTimeRange.Last7Days, label: '7 ngày gần nhất (khuyến nghị — hiệu quả nhất)' },
  { id: CNewsTimeRange.Last30Days, label: '30 ngày gần nhất', warning: 'Khoảng thời gian dài có thể ảnh hưởng hiệu suất hệ thống' },
]

// ── CNewsCountFilter ───────────────────────────────────────────────────────
export const CNewsCountFilter = {
  /// <summary>Top 10</summary>
  Top10: 1,
  /// <summary>Top 20</summary>
  Top20: 2,
  /// <summary>Top 30</summary>
  Top30: 3,
  /// <summary>Tất cả</summary>
  All: 4,
} as const

export type CNewsCountFilter = (typeof CNewsCountFilter)[keyof typeof CNewsCountFilter]

export const NEWS_COUNT_FILTER_OPTIONS: { id: CNewsCountFilter; label: string }[] = [
  { id: CNewsCountFilter.Top10, label: 'Top 10' },
  { id: CNewsCountFilter.Top20, label: 'Top 20' },
  { id: CNewsCountFilter.Top30, label: 'Top 30' },
  { id: CNewsCountFilter.All, label: 'Tất cả' },
]

// ── CNewsRecommendation ────────────────────────────────────────────────────
export const CNewsRecommendation = {
  /// <summary>Nên chọn</summary>
  ShouldSelect: 1,
  /// <summary>Có thể chọn</summary>
  CanSelect: 2,
  /// <summary>Không ưu tiên</summary>
  NotPriority: 3,
} as const

export type CNewsRecommendation = (typeof CNewsRecommendation)[keyof typeof CNewsRecommendation]

export const NEWS_RECOMMENDATION_OPTIONS: { id: CNewsRecommendation; label: string; badgeClass: string }[] = [
  { id: CNewsRecommendation.ShouldSelect, label: 'Nên chọn', badgeClass: 'bg-emerald-50 dark:bg-emerald-950/40 text-emerald-700 dark:text-emerald-300' },
  { id: CNewsRecommendation.CanSelect, label: 'Có thể chọn', badgeClass: 'bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300' },
  { id: CNewsRecommendation.NotPriority, label: 'Không ưu tiên', badgeClass: 'bg-rose-50 dark:bg-rose-950/40 text-rose-700 dark:text-rose-300' },
]

// ── CRelevanceLevel ────────────────────────────────────────────────────────
export const CRelevanceLevel = {
  /// <summary>Cao</summary>
  High: 1,
  /// <summary>Trung bình</summary>
  Medium: 2,
  /// <summary>Thấp</summary>
  Low: 3,
} as const

export type CRelevanceLevel = (typeof CRelevanceLevel)[keyof typeof CRelevanceLevel]

export const RELEVANCE_LEVEL_OPTIONS: { id: CRelevanceLevel; label: string }[] = [
  { id: CRelevanceLevel.High, label: 'Cao' },
  { id: CRelevanceLevel.Medium, label: 'Trung bình' },
  { id: CRelevanceLevel.Low, label: 'Thấp' },
]

export function getRelevanceLevelLabel(level: CRelevanceLevel): string {
  return RELEVANCE_LEVEL_OPTIONS.find((o) => o.id === level)?.label ?? ''
}

// ── CEmotionTag (flags, bitwise) ───────────────────────────────────────────
export const CEmotionTag = {
  /// <summary>Không có</summary>
  None: 0,
  /// <summary>Sợ hãi</summary>
  Fear: 1,
  /// <summary>Nghi ngờ</summary>
  Doubt: 2,
  /// <summary>Tò mò</summary>
  Curiosity: 4,
  /// <summary>Khẩn cấp</summary>
  Urgency: 8,
  /// <summary>Áp lực quyết định</summary>
  DecisionPressure: 16,
  /// <summary>Ngạc nhiên</summary>
  Surprise: 32,
  /// <summary>Giận dữ</summary>
  Anger: 64,
  /// <summary>Hy vọng</summary>
  Hope: 128,
} as const

export type CEmotionTag = (typeof CEmotionTag)[keyof typeof CEmotionTag]

export const EMOTION_TAG_LABELS: Record<number, string> = {
  [CEmotionTag.Fear]: 'Sợ hãi',
  [CEmotionTag.Doubt]: 'Nghi ngờ',
  [CEmotionTag.Curiosity]: 'Tò mò',
  [CEmotionTag.Urgency]: 'Khẩn cấp',
  [CEmotionTag.DecisionPressure]: 'Áp lực quyết định',
  [CEmotionTag.Surprise]: 'Ngạc nhiên',
  [CEmotionTag.Anger]: 'Giận dữ',
  [CEmotionTag.Hope]: 'Hy vọng',
}

/**
 * Giải mã bitwise CEmotionTag flags → mảng tên cảm xúc.
 */
export function decodeEmotionTags(tags: number): string[] {
  if (tags === 0 || tags === CEmotionTag.None) return []
  const result: string[] = []
  // eslint-disable-next-line no-restricted-syntax
  for (const [key, value] of Object.entries(CEmotionTag)) {
    const numVal = value as number
    if (numVal > 0 && (tags & numVal) === numVal) {
      result.push(EMOTION_TAG_LABELS[numVal] ?? key)
    }
  }
  return result
}

// ── CSourceType ────────────────────────────────────────────────────────────
export const CSourceType = {
  /// <summary>Thu thập tự động</summary>
  Crawled: 1,
  /// <summary>Import thủ công</summary>
  ManualLink: 2,
} as const

export type CSourceType = (typeof CSourceType)[keyof typeof CSourceType]

// ── CThumbnailPlatform ─────────────────────────────────────────────────────
export const CThumbnailPlatform = {
  /// <summary>Youtube</summary>
  Youtube: 1,
  /// <summary>Faceless</summary>
  Faceless: 2,
} as const

export type CThumbnailPlatform = (typeof CThumbnailPlatform)[keyof typeof CThumbnailPlatform]

export const THUMBNAIL_PLATFORM_OPTIONS: { id: CThumbnailPlatform; label: string }[] = [
  { id: CThumbnailPlatform.Youtube, label: 'YouTube' },
  { id: CThumbnailPlatform.Faceless, label: 'Faceless' },
]

// ── CThumbnailTimeFilter ───────────────────────────────────────────────────
export const CThumbnailTimeFilter = {
  /// <summary>Tuần này</summary>
  ThisWeek: 1,
  /// <summary>1 tháng</summary>
  OneMonth: 2,
  /// <summary>3 tháng</summary>
  ThreeMonths: 3,
  /// <summary>6 tháng</summary>
  SixMonths: 4,
  /// <summary>1 năm</summary>
  OneYear: 5,
} as const

export type CThumbnailTimeFilter = (typeof CThumbnailTimeFilter)[keyof typeof CThumbnailTimeFilter]

export const THUMBNAIL_TIME_FILTER_OPTIONS: { id: CThumbnailTimeFilter; label: string }[] = [
  { id: CThumbnailTimeFilter.ThisWeek, label: 'Tuần này' },
  { id: CThumbnailTimeFilter.OneMonth, label: '1 tháng' },
  { id: CThumbnailTimeFilter.ThreeMonths, label: '3 tháng' },
  { id: CThumbnailTimeFilter.SixMonths, label: '6 tháng' },
  { id: CThumbnailTimeFilter.OneYear, label: '1 năm' },
]

// ── CThumbnailSortFilter ───────────────────────────────────────────────────
export const CThumbnailSortFilter = {
  /// <summary>Nhiều lượt xem nhất</summary>
  MostViewed: 1,
  /// <summary>Mới nhất</summary>
  Newest: 2,
  /// <summary>Phù hợp nhất</summary>
  MostRelevant: 3,
} as const

export type CThumbnailSortFilter = (typeof CThumbnailSortFilter)[keyof typeof CThumbnailSortFilter]

export const THUMBNAIL_SORT_FILTER_OPTIONS: { id: CThumbnailSortFilter; label: string }[] = [
  { id: CThumbnailSortFilter.MostViewed, label: 'Nhiều lượt xem nhất' },
  { id: CThumbnailSortFilter.Newest, label: 'Mới nhất' },
  { id: CThumbnailSortFilter.MostRelevant, label: 'Phù hợp nhất' },
]

// ── CLibraryUserStatus ─────────────────────────────────────────────────────
export const CLibraryUserStatus = {
  /// <summary>Chờ duyệt</summary>
  Pending: 0,
  /// <summary>Phù hợp</summary>
  Approved: 1,
  /// <summary>Không phù hợp</summary>
  Rejected: 2,
} as const

export type CLibraryUserStatus = (typeof CLibraryUserStatus)[keyof typeof CLibraryUserStatus]

export const LIBRARY_USER_STATUS_OPTIONS: { id: CLibraryUserStatus; label: string }[] = [
  { id: CLibraryUserStatus.Pending, label: 'Chờ duyệt' },
  { id: CLibraryUserStatus.Approved, label: 'Phù hợp' },
  { id: CLibraryUserStatus.Rejected, label: 'Không phù hợp' },
]

// ── CThumbnailEditTool ─────────────────────────────────────────────────────
export const CThumbnailEditTool = {
  /// <summary>Chỉnh ảnh</summary>
  Image: 1,
  /// <summary>Sửa chữ</summary>
  Text: 2,
  /// <summary>Đổi phong cách</summary>
  Style: 3,
  /// <summary>Đổi biểu cảm</summary>
  Avatar: 4,
} as const

export type CThumbnailEditTool = (typeof CThumbnailEditTool)[keyof typeof CThumbnailEditTool]

export const THUMBNAIL_EDIT_TOOL_OPTIONS: { id: CThumbnailEditTool; label: string }[] = [
  { id: CThumbnailEditTool.Image, label: 'Chỉnh ảnh' },
  { id: CThumbnailEditTool.Text, label: 'Sửa chữ' },
  { id: CThumbnailEditTool.Style, label: 'Đổi phong cách' },
  { id: CThumbnailEditTool.Avatar, label: 'Đổi biểu cảm' },
]

// ── CTitleStyle ────────────────────────────────────────────────────────────
export const CTitleStyle = {
  /// <summary>Cảnh báo từ chuyên gia</summary>
  WarningExpert: 1,
  /// <summary>Trung lập, rõ ràng</summary>
  NeutralClear: 2,
  /// <summary>Kích thích tò mò</summary>
  CuriosityClick: 3,
} as const

export type CTitleStyle = (typeof CTitleStyle)[keyof typeof CTitleStyle]

export const TITLE_STYLE_OPTIONS: { id: CTitleStyle; label: string }[] = [
  { id: CTitleStyle.WarningExpert, label: 'Cảnh báo từ chuyên gia' },
  { id: CTitleStyle.NeutralClear, label: 'Trung lập, rõ ràng' },
  { id: CTitleStyle.CuriosityClick, label: 'Kích thích tò mò' },
]
