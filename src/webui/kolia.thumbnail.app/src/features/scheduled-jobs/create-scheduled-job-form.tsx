import { forwardRef, useImperativeHandle, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { Loader2, ShieldAlert, ShieldCheck } from 'lucide-react'
import { Button } from '../../components/ui/button'
import { Input } from '../../components/ui/input'
import { Textarea } from '../../components/ui/textarea'
import { SelectDropdown } from '../../components/selects/select-dropdown'
import { FormField, FormGroup, FormLabel } from '../../components/ui/form'
import { FormSection } from '../../components/ui/form-section'
import { CronBuilder } from './cron-builder'
import { createScheduledJobSchema, type CreateScheduledJobInput, GOOGLE_SERVICE_TYPE_OPTIONS } from './schema'
import { createScheduledJob, checkAccess, type CheckAccessResult } from './api'
import { getGoogleServiceAccountsWithPaging } from '../google-services/api'
import { ApiError } from '../../lib/api/api-error'

interface Props {
  onSuccess?: () => void
}

export interface CreateScheduledJobFormHandle {
  submit: () => Promise<void>
  isSubmitting: boolean
}

export const CreateScheduledJobForm = forwardRef<CreateScheduledJobFormHandle, Props>(
  ({ onSuccess }, ref) => {
    const queryClient = useQueryClient()
    const [isSubmitting, setIsSubmitting] = useState(false)
    const [accessResult, setAccessResult] = useState<CheckAccessResult | null>(null)
    const [isChecking, setIsChecking] = useState(false)

    const {
      register,
      handleSubmit,
      formState: { errors },
      setError,
      reset,
      setValue,
      watch,
    } = useForm<CreateScheduledJobInput>({
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      resolver: zodResolver(createScheduledJobSchema) as any,
      defaultValues: {
        name: '',
        description: '',
        sourceType: 1,
        sourceUrl: '',
        googleServiceAccountId: '',
        scheduleType: 'now',
        scheduledAt: null,
        cronExpression: '',
        cronDescription: '',
        maxRetries: 3,
      },
    })

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

    const { mutateAsync } = useMutation({
      mutationFn: (data: CreateScheduledJobInput) => createScheduledJob(data),
      onError: (error) => {
        if (error instanceof ApiError && error.isValidationError) {
          error.errors?.forEach((ve) => {
            setError(ve.property as keyof CreateScheduledJobInput, { message: ve.message })
          })
          toast.warning('Vui lòng kiểm tra lại thông tin đã nhập.')
        }
      },
      onSuccess: () => {
        onSuccess?.()
        reset()
        queryClient.invalidateQueries({ queryKey: ['scheduled-jobs'] })
      },
    })

    const onSubmit = async (data: CreateScheduledJobInput) => {
      // Map scheduleType to backend fields
      const payload = {
        name: data.name,
        description: data.description || null,
        sourceType: data.sourceType,
        sourceUrl: data.sourceUrl,
        googleServiceAccountId: data.googleServiceAccountId,
        scheduledAt: data.scheduleType === 'once' && data.scheduledAt ? new Date(data.scheduledAt).toISOString() : null,
        cronExpression: data.scheduleType === 'cron' ? data.cronExpression : null,
        cronDescription: data.scheduleType === 'cron' ? data.cronDescription : null,
        maxRetries: data.maxRetries,
      }
      setIsSubmitting(true)
      try {
        await mutateAsync(payload as unknown as CreateScheduledJobInput)
        toast.success('Tạo job thành công!')
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

    return (
      <form className="space-y-5" onSubmit={handleSubmit(onSubmit as unknown as (data: Record<string, unknown>) => Promise<void>)}>
        <FormSection title="Thông tin job" description="Tên và mô tả của scheduled import job">
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

        <FormSection title="Nguồn dữ liệu" description="Loại nguồn và URL Google Sheets/Docs">
          <div className="grid grid-cols-1 gap-4">
            <FormGroup>
              <FormLabel htmlFor="sourceType" required>Loại nguồn</FormLabel>
              <SelectDropdown<{ id: number; label: string }>
                items={GOOGLE_SERVICE_TYPE_OPTIONS}
                getOptionId={(opt) => String(opt.id)}
                getOptionLabel={(opt) => opt.label}
                value={GOOGLE_SERVICE_TYPE_OPTIONS.find((opt) => opt.id === watch('sourceType')) ?? null}
                onChange={(opt) => setValue('sourceType', opt?.id ?? 1, { shouldValidate: true })}
                placeholder="Chọn loại..."
              />
              <FormField error={errors.sourceType?.message} />
            </FormGroup>

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
            <div className="flex gap-2">
              <Input
                {...register('sourceUrl')}
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

        <FormSection title="Lịch trình" description="Chọn cách lên lịch cho job">
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

CreateScheduledJobForm.displayName = 'CreateScheduledJobForm'
