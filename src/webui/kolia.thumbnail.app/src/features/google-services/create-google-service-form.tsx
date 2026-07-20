import { forwardRef, useImperativeHandle, useRef, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { Upload, FileJson } from 'lucide-react'
import { Input } from '../../components/ui/input'
import { Textarea } from '../../components/ui/textarea'
import { FormField, FormGroup, FormLabel } from '../../components/ui/form'
import { FormSection } from '../../components/ui/form-section'
import { Button } from '../../components/ui/button'
import { createGoogleServiceAccountSchema, type CreateGoogleServiceAccountInput } from './schema'
import { createGoogleServiceAccount, updateGoogleServiceAccount, importGoogleServiceAccountFile, type GoogleServiceAccountDto } from './api'
import { ApiError } from '../../lib/api/api-error'

interface Props {
  editTarget?: GoogleServiceAccountDto
  onSuccess?: () => void
}

export interface CreateGoogleServiceFormHandle {
  submit: () => Promise<void>
  isSubmitting: boolean
}

export const CreateGoogleServiceForm = forwardRef<CreateGoogleServiceFormHandle, Props>(
  ({ editTarget, onSuccess }, ref) => {
    const queryClient = useQueryClient()
    const isEdit = !!editTarget
    const [isSubmitting, setIsSubmitting] = useState(false)
    const [isUploading, setIsUploading] = useState(false)
    const [selectedFile, setSelectedFile] = useState<File | null>(null)
    const fileInputRef = useRef<HTMLInputElement>(null)

    const {
      register,
      handleSubmit,
      formState: { errors },
      setError,
      reset,
      setValue,
    } = useForm<CreateGoogleServiceAccountInput>({
      resolver: zodResolver(createGoogleServiceAccountSchema),
      defaultValues: {
        name: editTarget?.name ?? '',
        description: editTarget?.description ?? '',
        credentialJson: '',
        scopes: editTarget?.scopes ?? 'https://www.googleapis.com/auth/spreadsheets.readonly,https://www.googleapis.com/auth/documents.readonly',
      },
    })

    const { mutateAsync } = useMutation({
      mutationFn: (data: CreateGoogleServiceAccountInput) =>
        isEdit && editTarget
          ? updateGoogleServiceAccount(editTarget.id, { ...data, isEnabled: editTarget.isEnabled, credentialJson: data.credentialJson || undefined })
          : createGoogleServiceAccount(data),
      onError: (error) => {
        if (error instanceof ApiError && error.isValidationError) {
          error.errors?.forEach((ve) => {
            setError(ve.property as keyof CreateGoogleServiceAccountInput, { message: ve.message })
          })
          toast.warning('Vui lòng kiểm tra lại thông tin đã nhập.')
        }
      },
      onSuccess: () => {
        onSuccess?.()
        reset()
        setSelectedFile(null)
        queryClient.invalidateQueries({ queryKey: ['google-services'] })
      },
    })

    const onSubmit = async (data: CreateGoogleServiceAccountInput) => {
      // Nếu có file và chưa được đọc vào textarea, upload trực tiếp bằng form-data
      if (selectedFile && !isEdit) {
        setIsUploading(true)
        try {
          await importGoogleServiceAccountFile(
            data.name,
            selectedFile,
            data.description || null,
            data.scopes || null,
          )
          toast.success('Đã tạo service account từ file JSON thành công!')
          onSuccess?.()
          reset()
          setSelectedFile(null)
          queryClient.invalidateQueries({ queryKey: ['google-services'] })
          return
        } catch (err) {
          toast.error((err as Error).message || 'Upload file thất bại.')
          return
        } finally {
          setIsUploading(false)
        }
      }

      setIsSubmitting(true)
      try {
        await mutateAsync(data)
        toast.success(isEdit ? 'Đã cập nhật service account.' : 'Đã tạo service account thành công!')
      } finally {
        setIsSubmitting(false)
      }
    }

    const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
      const file = e.target.files?.[0]
      if (!file) return

      if (!file.name.endsWith('.json')) {
        toast.error('Chỉ hỗ trợ file .json.')
        return
      }

      if (file.size > 5 * 1024 * 1024) {
        toast.error('File không được vượt quá 5MB.')
        return
      }

      setSelectedFile(file)

      // Đọc file và populate textarea
      const reader = new FileReader()
      reader.onload = (ev) => {
        const content = ev.target?.result as string
        try {
          JSON.parse(content) // validate JSON
          setValue('credentialJson', content, { shouldValidate: true })
          toast.success(`Đã đọc file: ${file.name}`)
        } catch {
          toast.error('File JSON không hợp lệ.')
        }
      }
      reader.onerror = () => {
        toast.error('Không thể đọc file.')
      }
      reader.readAsText(file)
    }

    useImperativeHandle(ref, () => ({
      submit: handleSubmit(onSubmit),
      isSubmitting: isSubmitting || isUploading,
    }))

    return (
      <form className="space-y-5" onSubmit={handleSubmit(onSubmit)}>
        <FormSection
          title={isEdit ? 'Cập nhật Service Account' : 'Thêm Service Account mới'}
          description="Upload file JSON credential từ Google Cloud Console hoặc dán trực tiếp nội dung JSON"
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

          {!isEdit && (
            <FormGroup>
              <FormLabel>Upload file JSON</FormLabel>
              <div className="flex items-center gap-3">
                <input
                  ref={fileInputRef}
                  type="file"
                  accept=".json"
                  className="hidden"
                  onChange={handleFileSelect}
                />
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => fileInputRef.current?.click()}
                  className="gap-2"
                >
                  <Upload className="h-4 w-4" />
                  Chọn file .json
                </Button>
                {selectedFile && (
                  <span className="flex items-center gap-1 text-sm text-green-600 dark:text-green-400">
                    <FileJson className="h-4 w-4" />
                    {selectedFile.name}
                  </span>
                )}
              </div>
              <p className="text-xs text-slate-400 dark:text-slate-500 mt-1">
                Tải file .json từ Google Cloud Console → IAM & Admin → Service Accounts → Create Key
              </p>
            </FormGroup>
          )}

          <FormGroup>
            <FormLabel htmlFor="credentialJson" required>
              {isEdit
                ? 'JSON Credential (để trống nếu không đổi)'
                : selectedFile
                  ? 'Nội dung JSON (đã đọc từ file)'
                  : 'JSON Credential (hoặc upload file bên trên)'}
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
        </FormSection>
      </form>
    )
  },
)

CreateGoogleServiceForm.displayName = 'CreateGoogleServiceForm'
