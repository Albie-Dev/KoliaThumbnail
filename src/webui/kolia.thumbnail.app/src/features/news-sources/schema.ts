import { z } from 'zod'
import { CNewsSourceGroup } from './news-source-group-type'
import { CSourceFetchMode } from './news-source-fetch-mode-type'

const NEWS_SOURCE_GROUP_VALUES = Object.values(CNewsSourceGroup) as [number, ...number[]]
const SOURCE_FETCH_MODE_VALUES = Object.values(CSourceFetchMode) as [number, ...number[]]

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
})

export type CreateNewsSourceInput = z.infer<typeof createNewsSourceSchema>

export const updateNewsSourceSchema = createNewsSourceSchema

export type UpdateNewsSourceInput = z.infer<typeof updateNewsSourceSchema> & { id: string }
