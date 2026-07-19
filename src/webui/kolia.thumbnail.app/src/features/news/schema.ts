import { z } from 'zod'
import { CMarketScope, CNewsTimeRange, CNewsCountFilter } from '../../types/enums/pipeline.enums'

const MARKET_SCOPE_VALUES = Object.values(CMarketScope) as [number, ...number[]]
const NEWS_TIME_RANGE_VALUES = Object.values(CNewsTimeRange) as [number, ...number[]]
const NEWS_COUNT_FILTER_VALUES = Object.values(CNewsCountFilter) as [number, ...number[]]

export const searchNewsSchema = z.object({
  marketScope: z.number().refine((val) => (MARKET_SCOPE_VALUES as readonly number[]).includes(val), {
    message: 'Phạm vi thị trường không hợp lệ',
  }),
  timeRange: z.number().refine((val) => (NEWS_TIME_RANGE_VALUES as readonly number[]).includes(val), {
    message: 'Khoảng thời gian không hợp lệ',
  }),
  countFilter: z.number().refine((val) => (NEWS_COUNT_FILTER_VALUES as readonly number[]).includes(val), {
    message: 'Số lượng không hợp lệ',
  }),
  keywordsRaw: z.string().min(1, 'Từ khoá không được để trống'),
})

export type SearchNewsInput = z.infer<typeof searchNewsSchema>
