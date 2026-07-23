import { forwardRef, useImperativeHandle, useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'
import { updateNewsSource, type NewsSourceListItemDto } from './api'
import { updateNewsSourceSchema } from './schema'
import { SelectDropdown } from '../../components/selects/select-dropdown'
import { Input } from '../../components/ui/input'
import { Checkbox } from '../../components/ui/checkbox'
import { FormField, FormGroup, FormLabel } from '../../components/ui/form'
import { FormSection } from '../../components/ui/form-section'
import { ApiError } from '../../lib/api/api-error'
import { NEWS_SOURCE_GROUP_OPTIONS } from './news-source-group-type'
import { SOURCE_FETCH_MODE_OPTIONS } from './news-source-fetch-mode-type'
import { MARKET_SCOPE_OPTIONS } from '../../types/enums/pipeline.enums'
import type { z } from 'zod'

type FormValues = z.infer<typeof updateNewsSourceSchema>

interface EditNewsSourceFormProps {
  source: NewsSourceListItemDto
  onClose?: () => void
}

export interface EditNewsSourceFormHandle {
  submit: () => Promise<void>
  isSubmitting: boolean
}

export const EditNewsSourceForm = forwardRef<EditNewsSourceFormHandle, EditNewsSourceFormProps>(
  ({ source, onClose }, ref) => {
    const queryClient = useQueryClient()
    const [isSubmitting, setIsSubmitting] = useState(false)

    const {
      register,
      handleSubmit,
      formState: { errors },
      setError,
      reset,
      watch,
      setValue,
    } = useForm<FormValues>({
      resolver: zodResolver(updateNewsSourceSchema),
      defaultValues: {
        name: source.name,
        rssOrFeedUrl: source.rssOrFeedUrl,
        region: source.region,
        isTrusted: source.isTrusted,
        priority: source.priority,
        sourceGroup: source.sourceGroup,
        fetchMode: source.fetchMode,
        domain: source.domain,
      },
    })

    const { mutateAsync } = useMutation({
      mutationFn: (data: FormValues) =>
        updateNewsSource({ ...data, id: source.id }),
      onError: (error) => {
        if (error instanceof ApiError && error.isValidationError) {
          error.errors?.forEach((validationError) => {
            setError(validationError.property as keyof FormValues, {
              message: validationError.message,
            })
          })
          toast.warning('Vui lòng kiểm tra lại thông tin đã nhập.')
        }
      },
      onSuccess: () => {
        onClose?.()
        reset()
        queryClient.invalidateQueries({ queryKey: ['news-sources'] })
      },
    })

    const onSubmit = async (data: FormValues) => {
      setIsSubmitting(true)
      try {
        await mutateAsync(data)
        toast.success('Cập nhật nguồn tin thành công!')
      } finally {
        setIsSubmitting(false)
      }
    }

    useImperativeHandle(ref, () => ({
      submit: handleSubmit(onSubmit),
      isSubmitting,
    }))

    return (
      <form className="space-y-5">
        <FormSection title="Thông tin cơ bản" description="Tên, domain và URL RSS của nguồn tin">
          <FormGroup>
            <FormLabel htmlFor="name" required>Tên nguồn tin</FormLabel>
            <Input {...register('name')} id="name" placeholder="VD: VnExpress, CoinDesk" />
            <FormField error={errors.name?.message} />
          </FormGroup>

          <FormGroup>
            <FormLabel htmlFor="domain" required>Domain</FormLabel>
            <Input {...register('domain')} id="domain" placeholder="VD: vnexpress.net" />
            <FormField error={errors.domain?.message} />
          </FormGroup>

          <FormGroup>
            <FormLabel htmlFor="rssOrFeedUrl" required>URL RSS</FormLabel>
            <Input {...register('rssOrFeedUrl')} id="rssOrFeedUrl" placeholder="https://example.com/rss" />
            <FormField error={errors.rssOrFeedUrl?.message} />
          </FormGroup>
        </FormSection>

        <FormSection title="Phân loại" description="Nhóm, khu vực và phương thức fetch">
          <div className="grid grid-cols-1 gap-4">
            <FormGroup>
              <FormLabel htmlFor="sourceGroup" required>Nhóm nguồn</FormLabel>
              <SelectDropdown<{ id: number; label: string }>
                items={NEWS_SOURCE_GROUP_OPTIONS}
                getOptionId={(opt) => String(opt.id)}
                getOptionLabel={(opt) => opt.label}
                value={NEWS_SOURCE_GROUP_OPTIONS.find((opt) => opt.id === watch('sourceGroup')) ?? null}
                onChange={(opt) => setValue('sourceGroup', opt?.id ?? 0, { shouldValidate: true })}
                placeholder="Chọn nhóm nguồn..."
              />
              <FormField error={errors.sourceGroup?.message} />
            </FormGroup>

            <FormGroup>
              <FormLabel htmlFor="region" required>Khu vực</FormLabel>
              <SelectDropdown<{ id: number; label: string }>
                items={MARKET_SCOPE_OPTIONS}
                getOptionId={(opt) => String(opt.id)}
                getOptionLabel={(opt) => opt.label}
                value={MARKET_SCOPE_OPTIONS.find((opt) => opt.id === watch('region')) ?? null}
                onChange={(opt) => setValue('region', opt?.id ?? 1, { shouldValidate: true })}
                placeholder="Chọn khu vực..."
              />
              <FormField error={errors.region?.message} />
            </FormGroup>

            <FormGroup>
              <FormLabel htmlFor="fetchMode" required>Phương thức fetch</FormLabel>
              <SelectDropdown<{ id: number; label: string }>
                items={SOURCE_FETCH_MODE_OPTIONS}
                getOptionId={(opt) => String(opt.id)}
                getOptionLabel={(opt) => opt.label}
                value={SOURCE_FETCH_MODE_OPTIONS.find((opt) => opt.id === watch('fetchMode')) ?? null}
                onChange={(opt) => setValue('fetchMode', opt?.id ?? 1, { shouldValidate: true })}
                placeholder="Chọn phương thức fetch..."
              />
              <FormField error={errors.fetchMode?.message} />
            </FormGroup>
          </div>
        </FormSection>

        <FormSection title="Cấu hình" description="Độ ưu tiên và trạng thái tin cậy">
          <FormGroup>
            <FormLabel htmlFor="priority" required>Priority</FormLabel>
            <Input {...register('priority', { valueAsNumber: true })} id="priority" type="number" min={0} placeholder="0" />
            <FormField error={errors.priority?.message} />
          </FormGroup>

          <FormGroup>
            <label className="flex items-center gap-2 cursor-pointer">
              <Checkbox
                checked={watch('isTrusted')}
                onCheckedChange={(checked) => setValue('isTrusted', checked === true, { shouldValidate: true })}
              />
              <span className="text-sm text-slate-700 dark:text-slate-300">Nguồn tin cậy</span>
            </label>
            <FormField error={errors.isTrusted?.message} />
          </FormGroup>
        </FormSection>
      </form>
    )
  },
)

EditNewsSourceForm.displayName = 'EditNewsSourceForm'
