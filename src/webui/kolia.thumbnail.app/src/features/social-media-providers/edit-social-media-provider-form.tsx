import { forwardRef, useImperativeHandle, useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'
import { updateSocialMediaProvider, type UpdateSocialMediaProviderInput } from './api'
import type { SocialMediaProviderBaseDto } from './api'
import { updateSocialMediaProviderSchema } from './schema'
import { SelectDropdown } from '../../components/selects/select-dropdown'
import { Input } from '../../components/ui/input'
import { SOCIAL_MEDIA_PROVIDER_TYPE_OPTIONS } from './social-media-provider-type'
import { FormField, FormGroup, FormLabel } from '../../components/ui/form'
import { FormSection } from '../../components/ui/form-section'
import { ApiError } from '../../lib/api/api-error'
import type { z } from 'zod'

type FormValues = z.infer<typeof updateSocialMediaProviderSchema>

interface EditSocialMediaProviderFormProps {
  provider: SocialMediaProviderBaseDto
  onClose?: () => void
}

export interface EditSocialMediaProviderFormHandle {
  submit: () => Promise<void>
  isSubmitting: boolean
}

export const EditSocialMediaProviderForm = forwardRef<EditSocialMediaProviderFormHandle, EditSocialMediaProviderFormProps>(
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
      resolver: zodResolver(updateSocialMediaProviderSchema),
      defaultValues: {
        name: provider.name,
        shortName: provider.shortName,
        providerType: (provider as any).providerType ?? 0,
        imageUrl: provider.imageUrl ?? '',
        baseUrl: provider.baseUrl,
      },
    })

    const { mutate } = useMutation({
      mutationFn: (data: FormValues) =>
        updateSocialMediaProvider({ ...data, id: provider.id } as UpdateSocialMediaProviderInput),
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
        toast.success('Cập nhật nhà cung cấp thành công!')
        onClose?.()
        reset()
        queryClient.invalidateQueries({ queryKey: ['social-media-providers'] })
      },
    })

    const onSubmit = async (data: FormValues) => {
      setIsSubmitting(true)
      try {
        await toast.promise(
          new Promise((resolve, reject) => {
            mutate(data, {
              onSuccess: () => resolve(null),
              onError: (error) => reject(error),
            })
          }),
          {
            loading: 'Đang cập nhật…',
            success: 'Cập nhật thành công!',
            error: (error: Error) => error.message || 'Có lỗi xảy ra',
          },
        )
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
            <Input {...register('name')} id="edit-name" placeholder="VD: Youtube, Facebook, Tiktok" />
            <FormField error={errors.name?.message} />
          </FormGroup>

          <div className="grid grid-cols-1 gap-4">
            <FormGroup>
              <FormLabel htmlFor="edit-shortName" required>Mã nhà cung cấp</FormLabel>
              <Input {...register('shortName')} id="edit-shortName" placeholder="VD: youtube" />
              <FormField error={errors.shortName?.message} />
            </FormGroup>

            <FormGroup>
              <FormLabel htmlFor="edit-providerType" required>Loại nhà cung cấp</FormLabel>
              <SelectDropdown<{ id: number; label: string }>
                items={SOCIAL_MEDIA_PROVIDER_TYPE_OPTIONS}
                getOptionId={(opt) => String(opt.id)}
                getOptionLabel={(opt) => opt.label}
                value={SOCIAL_MEDIA_PROVIDER_TYPE_OPTIONS.find((opt) => opt.id === watch('providerType')) ?? null}
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
            <Input {...register('baseUrl')} id="edit-baseUrl" placeholder="https://youtube.com" />
            <FormField error={errors.baseUrl?.message} />
          </FormGroup>
        </FormSection>
      </form>
    )
  },
)

EditSocialMediaProviderForm.displayName = 'EditSocialMediaProviderForm'
