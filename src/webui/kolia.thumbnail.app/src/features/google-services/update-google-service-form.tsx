import { forwardRef, useImperativeHandle, useState, useEffect } from 'react'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { Loader2 } from 'lucide-react'
import { Input } from '../../components/ui/input'
import { Textarea } from '../../components/ui/textarea'
import { Checkbox } from '../../components/ui/checkbox'
import { FormField, FormGroup, FormLabel } from '../../components/ui/form'
import { FormSection } from '../../components/ui/form-section'
import { updateGoogleServiceAccountSchema, type UpdateGoogleServiceAccountInput } from './schema'
import { getGoogleServiceAccount, updateGoogleServiceAccount } from './api'
import { ApiError } from '../../lib/api/api-error'

interface Props {
  editId: string
  onSuccess?: () => void
}

export interface UpdateGoogleServiceFormHandle {
  submit: () => Promise<void>
  isSubmitting: boolean
}

export const UpdateGoogleServiceForm = forwardRef<UpdateGoogleServiceFormHandle, Props>(
  ({ editId, onSuccess }, ref) => {
    const queryClient = useQueryClient()
    const [isSubmitting, setIsSubmitting] = useState(false)

    // Fetch full service account details
    const { data: saData, isLoading: isSaLoading } = useQuery({
      queryKey: ['google-services', editId],
      queryFn: () => getGoogleServiceAccount(editId),
      enabled: !!editId,
    })

    const {
      register,
      control,
      handleSubmit,
      formState: { errors },
      setError,
      setValue,
    } = useForm<UpdateGoogleServiceAccountInput>({
      resolver: zodResolver(updateGoogleServiceAccountSchema),
      defaultValues: {
        name: '',
        description: '',
        credentialJson: '',
        scopes: 'https://www.googleapis.com/auth/spreadsheets.readonly,https://www.googleapis.com/auth/documents.readonly',
        isEnabled: true,
      },
    })

    // Pre-populate form when SA data loaded
    useEffect(() => {
      if (!saData) return
      setValue('name', saData.name)
      setValue('description', saData.description || '')
      setValue('scopes', saData.scopes || 'https://www.googleapis.com/auth/spreadsheets.readonly,https://www.googleapis.com/auth/documents.readonly')
      setValue('isEnabled', saData.isEnabled)
    }, [saData, setValue])

    const { mutateAsync } = useMutation({
      mutationFn: (data: UpdateGoogleServiceAccountInput) =>
        updateGoogleServiceAccount(editId, {
          name: data.name,
          description: data.description || null,
          scopes: data.scopes || null,
          credentialJson: data.credentialJson || undefined,
          isEnabled: data.isEnabled,
        }),
      onError: (error) => {
        if (error instanceof ApiError && error.isValidationError) {
          error.errors?.forEach((ve) => {
            setError(ve.property as keyof UpdateGoogleServiceAccountInput, { message: ve.message })
          })
          toast.warning('Vui lòng kiểm tra lại thông tin đã nhập.')
        }
      },
      onSuccess: () => {
        toast.success('Đã cập nhật service account.')
        onSuccess?.()
        queryClient.invalidateQueries({ queryKey: ['google-services'] })
      },
    })

    const onSubmit = async (data: UpdateGoogleServiceAccountInput) => {
      setIsSubmitting(true)
      try {
        await mutateAsync(data)
      } finally {
        setIsSubmitting(false)
      }
    }

    useImperativeHandle(ref, () => ({
      submit: handleSubmit(onSubmit),
      isSubmitting,
    }))

    if (isSaLoading) {
      return (
        <div className="flex items-center justify-center py-12">
          <Loader2 className="h-6 w-6 animate-spin text-slate-400" />
        </div>
      )
    }

    return (
      <form className="space-y-5" onSubmit={handleSubmit(onSubmit)}>
        <FormSection
          title="Cập nhật Service Account"
          description="Cập nhật thông tin service account. Credential JSON chỉ cần điền khi muốn thay đổi."
        >
          <FormGroup>
            <FormLabel htmlFor="name" required>Tên hiển thị</FormLabel>
            <Input {...register('name')} id="name" placeholder="VD: Google Production SA" />
            <FormField error={errors.name?.message} />
          </FormGroup>

          <FormGroup>
            <FormLabel htmlFor="description">Mô tả</FormLabel>
            <Textarea {...register('description')} id="description" placeholder="Mô tả mục đích sử dụng..." />
            <FormField error={errors.description?.message} />
          </FormGroup>

          <FormGroup>
            <FormLabel htmlFor="credentialJson">
              JSON Credential (để trống nếu không đổi)
            </FormLabel>
            <Textarea
              {...register('credentialJson')}
              id="credentialJson"
              placeholder={`{\n  "type": "service_account",\n  "project_id": "...",\n  "private_key_id": "...",\n  "private_key": "-----BEGIN PRIVATE KEY-----\\n...",\n  "client_email": "...",\n  ...\n}`}
              className="font-mono text-xs min-h-[200px]"
            />
            <FormField error={errors.credentialJson?.message} />
          </FormGroup>

          <FormGroup>
            <FormLabel htmlFor="scopes">Scopes (cách nhau bằng dấu phẩy)</FormLabel>
            <Textarea
              {...register('scopes')}
              id="scopes"
              placeholder="https://www.googleapis.com/auth/spreadsheets.readonly,https://www.googleapis.com/auth/documents.readonly"
              className="text-xs"
            />
            <FormField error={errors.scopes?.message} />
          </FormGroup>

          <FormGroup>
            <div className="flex items-center gap-2">
              <Controller
                control={control}
                name="isEnabled"
                render={({ field }) => (
                  <Checkbox
                    checked={field.value}
                    onCheckedChange={(checked) => field.onChange(!!checked)}
                  />
                )}
              />
              <FormLabel>Đang hoạt động</FormLabel>
            </div>
            <FormField error={errors.isEnabled?.message} />
          </FormGroup>
        </FormSection>
      </form>
    )
  },
)

UpdateGoogleServiceForm.displayName = 'UpdateGoogleServiceForm'
