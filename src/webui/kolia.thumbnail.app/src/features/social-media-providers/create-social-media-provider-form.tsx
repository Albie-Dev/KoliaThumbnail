import { forwardRef, useImperativeHandle, useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'
import { createSocialMediaProvider } from './api'
import { createSocialMediaProviderSchema, type CreateSocialMediaProviderInput } from './schema'
import { SelectDropdown } from '../../components/selects/select-dropdown'
import { Input } from '../../components/ui/input'
import { FormField, FormGroup, FormLabel } from '../../components/ui/form'
import { FormSection } from '../../components/ui/form-section'
import { ApiError } from '../../lib/api/api-error'
import { SOCIAL_MEDIA_PROVIDER_TYPE_OPTIONS } from './social-media-provider-type'

interface CreateSocialMediaProviderFormProps {
  onClose?: () => void
}

export interface CreateSocialMediaProviderFormHandle {
  submit: () => Promise<void>
  isSubmitting: boolean
}

export const CreateSocialMediaProviderForm = forwardRef<CreateSocialMediaProviderFormHandle, CreateSocialMediaProviderFormProps>(
  ({ onClose }, ref) => {
    const queryClient = useQueryClient()
    const [isSubmitting, setIsSubmitting] = useState(false)

    const {
      register,
      handleSubmit,
      formState: { errors },
      setError,
      reset,
      setValue,
      watch,
    } = useForm<CreateSocialMediaProviderInput>({
      resolver: zodResolver(createSocialMediaProviderSchema),
      defaultValues: {
        name: '',
        shortName: '',
        providerType: undefined as unknown as number,
        imageUrl: '',
        baseUrl: '',
      },
    });

    const { mutateAsync } = useMutation({
      mutationFn: (data: CreateSocialMediaProviderInput) => createSocialMediaProvider(data),
      onError: (error) => {
        if (error instanceof ApiError && error.isValidationError) {
          error.errors?.forEach((validationError) => {
            setError(validationError.property as keyof CreateSocialMediaProviderInput, {
              message: validationError.message,
            })
          })
          toast.warning('Vui lòng kiểm tra lại thông tin đã nhập.')
        }
      },
      onSuccess: () => {
        onClose?.()
        reset()
        queryClient.invalidateQueries({ queryKey: ['social-media-providers'] })
      },
    })

    const onSubmit = async (data: CreateSocialMediaProviderInput) => {
      setIsSubmitting(true)
      try {
        await mutateAsync(data)
        toast.success('Tạo nhà cung cấp thành công!')
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
            <FormLabel htmlFor="name" required>Tên nhà cung cấp</FormLabel>
            <Input {...register('name')} id="name" placeholder="VD: Youtube, Facebook, Tiktok..." />
            <FormField error={errors.name?.message} />
          </FormGroup>

          <div className="grid grid-cols-1 gap-4">
            <FormGroup>
              <FormLabel htmlFor="shortName" required>Mã nhà cung cấp</FormLabel>
              <Input {...register('shortName')} id="shortName" placeholder="VD: youtube" />
              <FormField error={errors.shortName?.message} />
            </FormGroup>

            <FormGroup>
              <FormLabel htmlFor="providerType" required>Loại nhà cung cấp</FormLabel>
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
              <FormLabel htmlFor="imageUrl">URL hình ảnh</FormLabel>
              <Input {...register('imageUrl')} id="imageUrl" placeholder="https://example.com/logo.png" />
              <FormField error={errors.imageUrl?.message} />
            </FormGroup>
          </div>

          <FormGroup>
            <FormLabel htmlFor="baseUrl" required>Base URL</FormLabel>
            <Input {...register('baseUrl')} id="baseUrl" placeholder="https://youtube.com" />
            <FormField error={errors.baseUrl?.message} />
          </FormGroup>
        </FormSection>
      </form>
    )
  },
)

CreateSocialMediaProviderForm.displayName = 'CreateSocialMediaProviderForm'