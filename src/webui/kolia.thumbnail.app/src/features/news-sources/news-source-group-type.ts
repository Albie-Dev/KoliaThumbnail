// =============================================================================
// CNewsSourceGroup — mirror BE enum
// =============================================================================

export const CNewsSourceGroup = {
  /// <summary>Nguồn tin tài chính quốc tế (Bloomberg, Reuters, WSJ, FT, CNBC...)</summary>
  InternationalFinance: 1,
  /// <summary>Nguồn dữ liệu/chính thống (FED, FOMC, BLS, BEA, IMF, World Bank...)</summary>
  OfficialData: 2,
  /// <summary>Nguồn tin tài chính Việt Nam (CafeF, VnEconomy, Vietstock, SSI, MBS, VNDirect...)</summary>
  VietnamFinance: 3,
  /// <summary>Nguồn biểu đồ/thị trường (TradingView, Investing, Kitco, giá vàng...)</summary>
  ChartMarket: 4,
  /// <summary>YouTube và Google Trends</summary>
  YoutubeSearchTrend: 5,
} as const

export type CNewsSourceGroup = (typeof CNewsSourceGroup)[keyof typeof CNewsSourceGroup]

export interface CNewsSourceGroupOption {
  id: CNewsSourceGroup
  label: string
}

export const NEWS_SOURCE_GROUP_OPTIONS: CNewsSourceGroupOption[] = [
  { id: CNewsSourceGroup.InternationalFinance, label: 'Tin tài chính quốc tế' },
  { id: CNewsSourceGroup.OfficialData, label: 'Dữ liệu/chính thống' },
  { id: CNewsSourceGroup.VietnamFinance, label: 'Tin tài chính Việt Nam' },
  { id: CNewsSourceGroup.ChartMarket, label: 'Biểu đồ/thị trường' },
  { id: CNewsSourceGroup.YoutubeSearchTrend, label: 'YouTube/Search trend' },
]

export function getNewsSourceGroupLabel(v: CNewsSourceGroup): string | undefined {
  return NEWS_SOURCE_GROUP_OPTIONS.find((o) => o.id === v)?.label
}

const BADGE_COLORS = [
  'bg-blue-100 text-blue-700 dark:bg-blue-950/40 dark:text-blue-400',
  'bg-emerald-100 text-emerald-700 dark:bg-emerald-950/40 dark:text-emerald-400',
  'bg-amber-100 text-amber-700 dark:bg-amber-950/40 dark:text-amber-400',
  'bg-violet-100 text-violet-700 dark:bg-violet-950/40 dark:text-violet-400',
  'bg-rose-100 text-rose-700 dark:bg-rose-950/40 dark:text-rose-400',
] as const

export function getNewsSourceGroupBadgeClass(v: number): string {
  return BADGE_COLORS[Math.abs(v) % BADGE_COLORS.length]
}
