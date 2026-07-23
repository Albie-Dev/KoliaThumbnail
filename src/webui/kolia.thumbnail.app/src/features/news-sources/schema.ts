import { z } from 'zod'
import { CNewsSourceGroup } from './news-source-group-type'
import { CSourceFetchMode } from './news-source-fetch-mode-type'
import { CApiPaginationType } from './news-source-api-pagination-type'

const NEWS_SOURCE_GROUP_VALUES = Object.values(CNewsSourceGroup) as [number, ...number[]]
const SOURCE_FETCH_MODE_VALUES = Object.values(CSourceFetchMode) as [number, ...number[]]
const API_PAGINATION_TYPE_VALUES = Object.values(CApiPaginationType) as [number, ...number[]]

export const createNewsSourceSchema = z.object({
  name: z.string().min(1, 'Tên không được để trống'),
  rssOrFeedUrl: z.string().min(1, 'URL RSS không được để trống').url('URL không hợp lệ'),
  region: z.number().refine((val) => [1, 2, 3].includes(val), { message: 'Khu vực không hợp lệ' }),
  isTrusted: z.boolean(),
  priority: z.number().min(0, 'Priority phải >= 0'),
  sourceGroup: z.number().refine(
    (val) => (NEWS_SOURCE_GROUP_VALUES as readonly number[]).includes(val),
    { message: 'Nhóm nguồn không hợp lệ' },
  ),
  fetchMode: z.number().refine(
    (val) => (SOURCE_FETCH_MODE_VALUES as readonly number[]).includes(val),
    { message: 'Phương thức fetch không hợp lệ' },
  ),
  domain: z.string().min(1, 'Domain không được để trống'),
  // REST API fields
  apiEndpoint: z.string().url('URL không hợp lệ').optional().or(z.literal('')),
  apiKey: z.string().optional().or(z.literal('')),
  apiQueryParamsTemplate: z.string().optional().or(z.literal('')),
  apiResponseJsonPath: z.string().optional().or(z.literal('')),
  apiPaginationType: z.number().refine(
    (val) => (API_PAGINATION_TYPE_VALUES as readonly number[]).includes(val),
    { message: 'Kiểu phân trang không hợp lệ' },
  ).optional(),
  apiRequestHeaders: z.string().optional().or(z.literal('')),
})

export type CreateNewsSourceInput = z.infer<typeof createNewsSourceSchema>

export const updateNewsSourceSchema = createNewsSourceSchema

export type UpdateNewsSourceInput = z.infer<typeof updateNewsSourceSchema> & { id: string }
