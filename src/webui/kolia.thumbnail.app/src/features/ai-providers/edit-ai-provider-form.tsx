import { forwardRef, useImperativeHandle, useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { toast } from 'sonner'
import { updateAIProvider, type UpdateAIProviderInput } from './api'
import type { ThumbnailItem } from './api'
import { updateAIProviderSchema } from './schema'
import { Input } from '../../components/ui/input'
import { FormField, FormGroup, FormLabel } from '../../components/ui/form'
import { ApiError } from '../../lib/api/api-error'
import type { z } from 'zod'

type FormValues = z.infer<typeof updateAIProviderSchema>

interface EditAIProviderFormProps {
  provider: ThumbnailItem
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
    } = useForm<FormValues>({
      resolver: zodResolver(updateAIProviderSchema),
      defaultValues: {
        name: provider.name,
        shortName: provider.shortName,
        imageUrl: provider.imageUrl ?? '',
      },
    })

    const { mutate } = useMutation({
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
        toast.success('Cập nhật nhà cung cấp thành công!')
        onClose?.()
        reset()
        queryClient.invalidateQueries({ queryKey: ['ai-providers'] })
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
      <form className="space-y-4">
        <FormGroup>
          <FormLabel htmlFor="edit-name">Tên nhà cung cấp</FormLabel>
          <Input {...register('name')} id="edit-name" placeholder="VD: OpenAI, Google, Microsoft" />
          <FormField error={errors.name?.message} />
        </FormGroup>

        <FormGroup>
          <FormLabel htmlFor="edit-shortName">Mã nhà cung cấp</FormLabel>
          <Input {...register('shortName')} id="edit-shortName" placeholder="VD: openai, google, microsoft" />
          <FormField error={errors.shortName?.message} />
        </FormGroup>

        <FormGroup>
          <FormLabel htmlFor="edit-imageUrl">URL hình ảnh (không bắt buộc)</FormLabel>
          <Input {...register('imageUrl')} id="edit-imageUrl" placeholder="https://example.com/logo.png" />
          <FormField error={errors.imageUrl?.message} />
        </FormGroup>
      </form>
    )
  },
)

EditAIProviderForm.displayName = 'EditAIProviderForm'
