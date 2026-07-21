import { forwardRef, useImperativeHandle, useState, useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { Loader2, ShieldAlert, ShieldCheck } from 'lucide-react'
import { z } from 'zod'
import { Button } from '../../components/ui/button'
import { Input } from '../../components/ui/input'
import { Textarea } from '../../components/ui/textarea'
import { SelectDropdown } from '../../components/selects/select-dropdown'
import { FormField, FormGroup, FormLabel } from '../../components/ui/form'
import { FormSection } from '../../components/ui/form-section'
import { CronBuilder } from './cron-builder'
import { updateScheduledJob, getScheduledJob, checkAccess, type CheckAccessResult, type UpdateScheduledJobInput } from './api'
import { getGoogleServiceAccountsWithPaging } from '../google-services/api'
import { ApiError } from '../../lib/api/api-error'

// Schema riêng cho update — KHÔNG dùng chung với create
const baseUpdateSchema = z.object({
  name: z.string().min(1, 'Tên không được để trống'),
  description: z.string().optional().nullable(),
  sourceType: z.number().optional(),
  sourceUrl: z.string().min(1, 'URL không được để trống').url('URL không hợp lệ'),
  googleServiceAccountId: z.string().min(1, 'Vui lòng chọn service account'),
  scheduleType: z.enum(['now', 'once', 'cron']),
  scheduledAt: z.string().optional().nullable(),
  cronExpression: z.string().optional().nullable(),
  cronDescription: z.string().optional().nullable(),
  maxRetries: z.number().min(0).max(10),
})

const updateScheduledJobSchema = baseUpdateSchema.superRefine((val, ctx) => {
  if (val.scheduleType === 'cron' && !val.cronExpression) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      path: ['cronExpression'],
      message: 'Vui lòng nhập cron expression',
    })
  }
})

type UpdateFormInput = z.infer<typeof updateScheduledJobSchema>

interface Props {
  editJobId: string
  onSuccess?: () => void
}

export interface UpdateScheduledJobFormHandle {
  submit: () => Promise<void>
  isSubmitting: boolean
}

export const UpdateScheduledJobForm = forwardRef<UpdateScheduledJobFormHandle, Props>(
  ({ editJobId, onSuccess }, ref) => {
    const queryClient = useQueryClient()
    const [isSubmitting, setIsSubmitting] = useState(false)
    const [accessResult, setAccessResult] = useState<CheckAccessResult | null>(null)
    const [isChecking, setIsChecking] = useState(false)

    // Fetch full job details
    const { data: jobData, isLoading: isJobLoading } = useQuery({
      queryKey: ['scheduled-jobs', editJobId],
      queryFn: () => getScheduledJob(editJobId),
      enabled: !!editJobId,
    })

    const {
      register,
      handleSubmit,
      formState: { errors },
      setError,
      setValue,
      watch,
    } = useForm<UpdateFormInput>({
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      resolver: zodResolver(updateScheduledJobSchema) as any,
      defaultValues: {
        name: '',
        description: '',
        sourceType: undefined,
        sourceUrl: '',
        googleServiceAccountId: '',
        scheduleType: 'now',
        scheduledAt: null,
        cronExpression: '',
        cronDescription: '',
        maxRetries: 3,
      },
    })

    // Pre-populate form khi job data loaded
    useEffect(() => {
      if (!jobData) return
      setValue('name', jobData.name)
      setValue('description', jobData.description || '')
      setValue('sourceUrl', jobData.sourceUrl)
      setValue('googleServiceAccountId', jobData.googleServiceAccountId)
      setValue('maxRetries', jobData.maxRetries)
      setValue('scheduleType', jobData.cronExpression ? 'cron' : jobData.scheduledAt ? 'once' : 'now')
      setValue('scheduledAt', jobData.scheduledAt ? jobData.scheduledAt.slice(0, 16) : null)
      setValue('cronExpression', jobData.cronExpression || '')
      setValue('cronDescription', jobData.cronDescription || '')
      // Auto-detect source type from URL
      if (jobData.sourceUrl.includes('/spreadsheets/')) setValue('sourceType', 1)
      else if (jobData.sourceUrl.includes('/document/')) setValue('sourceType', 2)
    }, [jobData, setValue])

    const { data: saData } = useQuery({
      queryKey: ['google-services', 'select'],
      queryFn: () =>
        getGoogleServiceAccountsWithPaging({
          pageNumber: 1,
          pageSize: 100,
          includeDeleted: false,
        }),
    })

    const serviceAccountOptions = (saData?.items ?? []).map((sa) => ({
      id: sa.id,
      label: `${sa.name} (${sa.clientEmail})`,
    }))

    const { mutateAsync: save } = useMutation({
      mutationFn: (data: UpdateScheduledJobInput) => updateScheduledJob(editJobId, data),
      onError: (error) => {
        if (error instanceof ApiError && error.isValidationError) {
          error.errors?.forEach((ve) => {
            setError(ve.property as keyof UpdateFormInput, { message: ve.message })
          })
          toast.warning('Vui lòng kiểm tra lại thông tin đã nhập.')
        }
      },
      onSuccess: () => {
        toast.success('Cập nhật job thành công!')
        onSuccess?.()
        queryClient.invalidateQueries({ queryKey: ['scheduled-jobs'] })
      },
    })

    const onSubmit = async (data: UpdateFormInput) => {
      const detectedType = data.sourceType ?? (
        data.sourceUrl.includes('/spreadsheets/') ? 1 :
        data.sourceUrl.includes('/document/') ? 2 :
        undefined
      )

      if (!detectedType) {
        toast.error('URL không hợp lệ. Vui lòng nhập link Google Sheets hoặc Google Docs.')
        return
      }

      // Map scheduleType to API contract — chỉ gửi field phù hợp, không gửi null
      const payload: UpdateScheduledJobInput = {
        name: data.name,
        description: data.description || null,
        sourceUrl: data.sourceUrl,
        googleServiceAccountId: data.googleServiceAccountId,
        maxRetries: data.maxRetries,
        ...(data.scheduleType === 'once' && data.scheduledAt
          ? { scheduledAt: new Date(data.scheduledAt).toISOString() }
          : {}),
        ...(data.scheduleType === 'cron'
          ? {
              cronExpression: data.cronExpression,
              cronDescription: data.cronDescription,
              timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone,
            }
          : {}),
      }

      setIsSubmitting(true)
      try {
        await save(payload)
      } finally {
        setIsSubmitting(false)
      }
    }

    useImperativeHandle(ref, () => ({
      // eslint-disable-next-line @typescript-eslint/no-misused-promises
      submit: handleSubmit(onSubmit as unknown as (data: Record<string, unknown>) => Promise<void>),
      isSubmitting,
    }))

    async function handleCheckAccess() {
      const sourceUrl = watch('sourceUrl')
      const sourceType = watch('sourceType')
      const saId = watch('googleServiceAccountId')

      if (!sourceUrl || !saId) {
        toast.warning('Vui lòng nhập URL và chọn Service Account trước.')
        return
      }

      if (!sourceType) {
        toast.error('Không thể nhận diện loại nguồn từ URL. Vui lòng kiểm tra URL Google Sheets hoặc Google Docs.')
        return
      }

      setIsChecking(true)
      setAccessResult(null)
      try {
        const result = await checkAccess({ sourceUrl, sourceType, googleServiceAccountId: saId })
        setAccessResult(result)
        if (result.hasAccess) {
          toast.success('Service account có quyền truy cập vào tài liệu này.')
        } else {
          toast.error(result.errorMessage || 'Không thể truy cập.')
        }
      } catch {
        toast.error('Lỗi khi kiểm tra quyền.')
      } finally {
        setIsChecking(false)
      }
    }

    if (isJobLoading) {
      return (
        <div className="flex items-center justify-center py-12">
          <Loader2 className="h-6 w-6 animate-spin text-slate-400" />
        </div>
      )
    }

    return (
      <form className="space-y-5" onSubmit={handleSubmit(onSubmit as unknown as (data: Record<string, unknown>) => Promise<void>)}>
        <FormSection title="Thông tin job" description="Cập nhật tên và mô tả của scheduled import job">
          <FormGroup>
            <FormLabel htmlFor="name" required>Tên job</FormLabel>
            <Input {...register('name')} id="name" placeholder="VD: Import nội dung livestream tuần 30" />
            <FormField error={errors.name?.message} />
          </FormGroup>

          <FormGroup>
            <FormLabel htmlFor="description">Mô tả</FormLabel>
            <Textarea {...register('description')} id="description" placeholder="Mô tả ngắn về job..." />
            <FormField error={errors.description?.message} />
          </FormGroup>
        </FormSection>

        <FormSection title="Nguồn dữ liệu" description="Nhập URL Google Sheets hoặc Google Docs — hệ thống tự nhận diện loại nguồn">
          <div className="grid grid-cols-1 gap-4">
            <FormGroup>
              <FormLabel htmlFor="googleServiceAccountId" required>Service Account</FormLabel>
              <SelectDropdown<{ id: string; label: string }>
                items={serviceAccountOptions}
                getOptionId={(opt) => opt.id}
                getOptionLabel={(opt) => opt.label}
                value={serviceAccountOptions.find((opt) => opt.id === watch('googleServiceAccountId')) ?? null}
                onChange={(opt) => setValue('googleServiceAccountId', opt?.id ?? '', { shouldValidate: true })}
                allowSearch
                searchPlaceholder="Tìm service account..."
                placeholder="Chọn service account..."
              />
              <FormField error={errors.googleServiceAccountId?.message} />
            </FormGroup>
          </div>

          <FormGroup>
            <FormLabel htmlFor="sourceUrl" required>URL nguồn</FormLabel>
            <div className="flex gap-2 mb-2">
              {([
                { id: 1, label: 'Google Sheets', icon: '📊' },
                { id: 2, label: 'Google Docs', icon: '📝' },
              ] as const).map((opt) => {
                const detected = watch('sourceType') === opt.id
                return (
                  <span
                    key={opt.id}
                    className={`inline-flex items-center gap-1 rounded-full px-3 py-1 text-xs font-medium transition-colors ${
                      detected
                        ? 'bg-blue-100 text-blue-700 ring-1 ring-blue-300 dark:bg-blue-900/40 dark:text-blue-300 dark:ring-blue-700'
                        : 'bg-slate-100 text-slate-400 dark:bg-slate-800 dark:text-slate-500'
                    }`}
                  >
                    {opt.icon} {opt.label}
                  </span>
                )
              })}
            </div>
            <div className="flex gap-2">
              <Input
                {...register('sourceUrl', {
                  onChange: (e) => {
                    const url = e.target.value
                    if (url.includes('/spreadsheets/')) {
                      setValue('sourceType', 1, { shouldValidate: true })
                    } else if (url.includes('/document/')) {
                      setValue('sourceType', 2, { shouldValidate: true })
                    } else {
                      setValue('sourceType', undefined, { shouldValidate: true })
                    }
                  },
                })}
                id="sourceUrl"
                placeholder="https://docs.google.com/spreadsheets/d/... hoặc /document/d/..."
                className="flex-1"
              />
              <Button
                type="button"
                variant="outline"
                onClick={handleCheckAccess}
                disabled={isChecking}
                className="shrink-0"
              >
                {isChecking ? (
                  <Loader2 className="h-4 w-4 animate-spin" />
                ) : accessResult?.hasAccess ? (
                  <ShieldCheck className="h-4 w-4 text-green-500" />
                ) : (
                  <ShieldAlert className="h-4 w-4" />
                )}
                Kiểm tra
              </Button>
            </div>
            {accessResult && !accessResult.hasAccess && accessResult.instruction && (
              <div className="mt-2 rounded border border-red-200 bg-red-50 dark:border-red-800 dark:bg-red-900/20 p-3 text-xs text-red-700 dark:text-red-300 whitespace-pre-line">
                {accessResult.instruction}
              </div>
            )}
            <FormField error={errors.sourceUrl?.message} />
          </FormGroup>
        </FormSection>

        <FormSection title="Lịch trình" description="Thay đổi lịch chạy cho job">
          <FormGroup>
            <FormLabel required>Kiểu lịch</FormLabel>
            <div className="flex gap-2">
              {([
                { value: 'now', label: 'Chạy ngay' },
                { value: 'once', label: 'Một lần' },
                { value: 'cron', label: 'Định kỳ (Cron)' },
              ] as const).map((opt) => (
                <Button
                  key={opt.value}
                  type="button"
                  variant={watch('scheduleType') === opt.value ? 'default' : 'outline'}
                  onClick={() => setValue('scheduleType', opt.value)}
                  size="sm"
                >
                  {opt.label}
                </Button>
              ))}
            </div>
          </FormGroup>

          {watch('scheduleType') === 'once' && (
            <FormGroup>
              <FormLabel htmlFor="scheduledAt">Thời gian chạy</FormLabel>
              <Input
                type="datetime-local"
                id="scheduledAt"
                {...register('scheduledAt')}
              />
              <FormField error={errors.scheduledAt?.message} />
            </FormGroup>
          )}

          {watch('scheduleType') === 'cron' && (
            <CronBuilder
              value={watch('cronExpression') || ''}
              description={watch('cronDescription') || ''}
              onChange={(cron, desc) => {
                setValue('cronExpression', cron, { shouldValidate: true })
                setValue('cronDescription', desc, { shouldValidate: true })
              }}
            />
          )}
          {watch('scheduleType') === 'cron' && <FormField error={errors.cronExpression?.message} />}

          <FormGroup>
            <FormLabel htmlFor="maxRetries">Số lần thử lại</FormLabel>
            <Input
              type="number"
              id="maxRetries"
              min={0}
              max={10}
              {...register('maxRetries', { valueAsNumber: true })}
            />
            <FormField error={errors.maxRetries?.message} />
          </FormGroup>
        </FormSection>
      </form>
    )
  },
)

UpdateScheduledJobForm.displayName = 'UpdateScheduledJobForm'
