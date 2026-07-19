import { z } from 'zod'

export const saveManualBriefSchema = z.object({
  overviewInput: z.string().min(1, 'Tổng quan không được để trống'),
  viewpointInput: z.string().min(1, 'Quan điểm không được để trống'),
  keyDataInput: z.string().min(1, 'Dữ liệu quan trọng không được để trống'),
})

export type SaveManualBriefInput = z.infer<typeof saveManualBriefSchema>
