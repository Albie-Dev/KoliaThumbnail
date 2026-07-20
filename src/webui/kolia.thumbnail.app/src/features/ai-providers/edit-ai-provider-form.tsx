import { forwardRef, useImperativeHandle, useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'
import { updateAIProvider, type UpdateAIProviderInput } from './api'
import type { AIProviderBaseDto } from './api'
import { updateAIProviderSchema } from './schema'
import { SelectDropdown } from '../../components/selects/select-dropdown'
import { Input } from '../../components/ui/input'
import { AI_PROVIDER_TYPE_OPTIONS } from './ai-provider-type'
import { FormField, FormGroup, FormLabel } from '../../components/ui/form'
import { FormSection } from '../../components/ui/form-section'
import { ApiError } from '../../lib/api/api-error'
import type { z } from 'zod'

type FormValues = z.infer<typeof updateAIProviderSchema>

interface EditAIProviderFormProps {
  provider: AIProviderBaseDto
  onClose?: () => void
}

export interface EditAIProviderFormHandle {
  submit: () => Promise<void>
  isSubmitting: boolean
}

export const EditAIProviderForm = forwardRef<EditAIProviderFormHandle, EditAIProviderFormProps>(
  ({ provider, onClose }, ref) => {
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
      resolver: zodResolver(updateAIProviderSchema),
      defaultValues: {
        name: provider.name,
        shortName: provider.shortName,
        providerType: (provider as any).providerType ?? 0,
        imageUrl: provider.imageUrl ?? '',
        baseUrl: provider.baseUrl,
      },
    })

    const { mutateAsync } = useMutation({
      mutationFn: (data: FormValues) =>
        updateAIProvider({ ...data, id: provider.id } as UpdateAIProviderInput),
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
        queryClient.invalidateQueries({ queryKey: ['ai-providers'] })
      },
    })

    const onSubmit = async (data: FormValues) => {
      setIsSubmitting(true)
      try {
        await mutateAsync(data)
        toast.success('Cập nhật nhà cung cấp thành công!')
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
        <FormSection title="Thông tin cơ bản" description="Tên, mã và thông tin chung của nhà cung cấp AI">
          <FormGroup>
            <FormLabel htmlFor="edit-name" required>Tên nhà cung cấp</FormLabel>
            <Input {...register('name')} id="edit-name" placeholder="VD: OpenAI, Google, Microsoft" />
            <FormField error={errors.name?.message} />
          </FormGroup>

          <div className="grid grid-cols-1 gap-4">
            <FormGroup>
              <FormLabel htmlFor="edit-shortName" required>Mã nhà cung cấp</FormLabel>
              <Input {...register('shortName')} id="edit-shortName" placeholder="VD: openai" />
              <FormField error={errors.shortName?.message} />
            </FormGroup>

            <FormGroup>
              <FormLabel htmlFor="edit-providerType" required>Loại nhà cung cấp</FormLabel>
              <SelectDropdown<{ id: number; label: string }>
                items={AI_PROVIDER_TYPE_OPTIONS}
                getOptionId={(opt) => String(opt.id)}
                getOptionLabel={(opt) => opt.label}
                value={AI_PROVIDER_TYPE_OPTIONS.find((opt) => opt.id === watch('providerType')) ?? null}
                onChange={(opt) => setValue('providerType', opt?.id ?? 0, { shouldValidate: true })}
                allowSearch={true}
                searchPlaceholder="Tìm loại..."
                placeholder="Chọn loại nhà cung cấp..."
              />
              <FormField error={errors.providerType?.message} />
            </FormGroup>

            <FormGroup>
              <FormLabel htmlFor="edit-imageUrl">URL hình ảnh</FormLabel>
              <Input {...register('imageUrl')} id="edit-imageUrl" placeholder="https://example.com/logo.png" />
              <FormField error={errors.imageUrl?.message} />
            </FormGroup>
          </div>

          <FormGroup>
            <FormLabel htmlFor="edit-baseUrl" required>Base URL</FormLabel>
            <Input {...register('baseUrl')} id="edit-baseUrl" placeholder="https://api.openai.com/v1" />
            <FormField error={errors.baseUrl?.message} />
          </FormGroup>
        </FormSection>
      </form>
    )
  },
)

EditAIProviderForm.displayName = 'EditAIProviderForm'
