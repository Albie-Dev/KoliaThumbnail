import { forwardRef, useImperativeHandle, useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Controller, useForm, useWatch } from 'react-hook-form'
import { toast } from 'sonner'
import { Plus, Trash2 } from 'lucide-react'

import { Input } from '../../components/ui/input'
import { Button } from '../../components/ui/button'
import { Badge } from '../../components/ui/badge'
import { FormField, FormGroup, FormLabel } from '../../components/ui/form'
import { SelectDropdown } from '../../components/selects/select-dropdown'
import { Checkbox } from '../../components/ui/checkbox'

import { updateFunctionConfig, getProviderModels, type AIFunctionConfigDetailDto, type AIModelInfo } from './api'
import { getFunctionTypeLabel } from './function-type'
import { getAIProvidersWithPaging, type AIProviderBaseDto } from '../ai-providers/api'
import { fetchAIProviderConfigurations, type AIProviderConfigurationBaseDto } from '../ai-configurations/api'
import { ApiError } from '../../lib/api/api-error'

interface EditAIFunctionConfigFormProps {
  config: AIFunctionConfigDetailDto
  onClose?: () => void
}

export interface EditAIFunctionConfigFormHandle {
  submit: () => Promise<void>
  isSubmitting: boolean
}

export const EditAIFunctionConfigForm = forwardRef<
  EditAIFunctionConfigFormHandle,
  EditAIFunctionConfigFormProps
>(({ config, onClose }, ref) => {
  const queryClient = useQueryClient()

  // ── Load providers & configs ──────────────────────────────────────────
  const { data: providerList = [] } = useQuery({
    queryKey: ['ai-providers', 'all'],
    queryFn: async () => {
      const res = await getAIProvidersWithPaging({ pageNumber: 1, pageSize: 1000, includeDeleted: false })
      return res.items ?? []
    },
    staleTime: 5 * 60 * 1000,
  })

  const { data: configList = [] } = useQuery({
    queryKey: ['ai-configurations', 'all'],
    queryFn: async () => {
      const res = await fetchAIProviderConfigurations({ pageNumber: 1, pageSize: 1000, includeDeleted: false })
      return res.items ?? []
    },
    staleTime: 5 * 60 * 1000,
  })

  // ── Models state — auto-fetch khi provider + config thay đổi ────────
  const [modelsByItem, setModelsByItem] = useState<Record<string, AIModelInfo[]>>({})
  const [loadingModels, setLoadingModels] = useState<Record<string, boolean>>({})
  const [modelErrors, setModelErrors] = useState<Record<string, string>>({})

  // ── Build default items ──────────────────────────────────────────────
  const defaultItems = useMemo(() => {
    const items = config.items?.length > 0 ? config.items : []
    return items.length > 0
      ? items.map((i) => ({
          key: i.id ?? crypto.randomUUID(),
          priority: i.priority,
          aiProviderId: i.aiProviderId || '',
          aiProviderConfigurationId: i.aiProviderConfigurationId || '',
          model: i.model ?? '',
          temperature: i.temperature != null ? String(i.temperature) : '',
          maxTokens: i.maxTokens != null ? String(i.maxTokens) : '',
          isEnabled: i.isEnabled,
        }))
      : [
          {
            key: crypto.randomUUID(),
            priority: 0,
            aiProviderId: '',
            aiProviderConfigurationId: '',
            model: '',
            temperature: '',
            maxTokens: '',
            isEnabled: true,
          },
        ]
  }, [config.items])

  // ── Form ─────────────────────────────────────────────────────────────
  type FormValues = {
    model: string
    temperature: string
    maxTokens: string
    items: typeof defaultItems
  }

  const { control, register, handleSubmit, formState, setError, reset } = useForm<FormValues>({
    defaultValues: {
      model: config.model ?? '',
      temperature: config.temperature != null ? String(config.temperature) : '',
      maxTokens: config.maxTokens != null ? String(config.maxTokens) : '',
      items: defaultItems,
    },
  })

  const watchedItems = useWatch({ control, name: 'items' }) ?? defaultItems

  // Auto-fetch models whenever providerId + configurationId change
  useEffect(() => {
    watchedItems.forEach((item) => {
      if (!item.aiProviderId || !item.aiProviderConfigurationId) return

      // Tạo cache key dựa trên provider+config để tránh dùng sai cache khi đổi provider
      const cacheKey = `${item.key}_${item.aiProviderId}_${item.aiProviderConfigurationId}`
      if (modelsByItem[cacheKey]) {
        // Đã fetch cho cặp provider+config này rồi, không fetch lại
        return
      }

      const doFetch = async () => {
        setLoadingModels((prev) => ({ ...prev, [item.key]: true }))
        setModelErrors((prev) => ({ ...prev, [item.key]: '' }))
        try {
          const models = await getProviderModels(item.aiProviderId, item.aiProviderConfigurationId)
          setModelsByItem((prev) => ({ ...prev, [cacheKey]: models }))
        } catch {
          setModelErrors((prev) => ({ ...prev, [item.key]: 'Không thể tải models' }))
        } finally {
          setLoadingModels((prev) => ({ ...prev, [item.key]: false }))
        }
      }
      doFetch()
    })
  }, [watchedItems, modelsByItem])

  // ── Mutation ─────────────────────────────────────────────────────────
  const { mutateAsync } = useMutation({
    mutationFn: (data: FormValues) =>
      updateFunctionConfig(config.id, {
        model: data.model || null,
        temperature: data.temperature ? Number(data.temperature) : null,
        maxTokens: data.maxTokens ? Number(data.maxTokens) : null,
        items: data.items.map((item, idx) => ({
          id: config.items?.[idx]?.id ?? null,
          priority: item.priority,
          aiProviderId: item.aiProviderId,
          aiProviderConfigurationId: item.aiProviderConfigurationId,
          model: item.model || null,
          temperature: item.temperature ? Number(item.temperature) : null,
          maxTokens: item.maxTokens ? Number(item.maxTokens) : null,
          isEnabled: item.isEnabled,
        })),
      }),
    onSuccess: () => {
      toast.success('Đã cập nhật cấu hình chức năng!')
      queryClient.invalidateQueries({ queryKey: ['ai-function-configs'] })
      onClose?.()
    },
    onError: (error) => {
      if (error instanceof ApiError && error.isValidationError) {
        error.errors?.forEach((ve) => setError(ve.property as keyof FormValues, { message: ve.message }))
      }
    },
  })

  const onSubmit = async (data: FormValues) => {
    try {
      await mutateAsync(data)
    } catch { /* global handler */ }
  }

  useImperativeHandle(ref, () => ({
    submit: async () => { await handleSubmit(onSubmit)() },
    isSubmitting: formState.isSubmitting,
  }))

  // ── Helpers ──────────────────────────────────────────────────────────
  function getConfigsForProvider(providerId: string): AIProviderConfigurationBaseDto[] {
    if (!providerId) return []
    return configList.filter((c) => c.aiProviderId === providerId)
  }

  return (
    <div className="space-y-5">
      {/* ── Function type (read-only) ──────────────────────────────────── */}
      <FormGroup>
        <FormLabel>Chức năng</FormLabel>
        <Badge variant="secondary" className="text-sm px-3 py-1.5 w-fit">
          {getFunctionTypeLabel(config.functionType)}
        </Badge>
      </FormGroup>

      {/* ── Items list ─────────────────────────────────────────────────── */}
      <div className="space-y-3">
        <div className="flex items-center justify-between">
          <span className="text-sm font-medium text-slate-700 dark:text-slate-200">
            Provider &amp; Config
          </span>
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={() =>
              reset({
                ...control._formValues,
                items: [
                  ...watchedItems,
                  {
                    key: crypto.randomUUID(),
                    priority: watchedItems.length,
                    aiProviderId: '',
                    aiProviderConfigurationId: '',
                    model: '',
                    temperature: '',
                    maxTokens: '',
                    isEnabled: true,
                  },
                ],
              } as FormValues)
            }
          >
            <Plus className="h-3.5 w-3.5 mr-1" />
            Thêm fallback
          </Button>
        </div>

        {watchedItems.map((item, index) => {
          const filteredConfigs = getConfigsForProvider(item.aiProviderId)
          const modelsCacheKey = `${item.key}_${item.aiProviderId}_${item.aiProviderConfigurationId}`
          const itemModels = modelsByItem[modelsCacheKey] ?? []

          return (
            <div
              key={item.key}
              className="rounded-lg border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 p-4 space-y-3"
            >
              {/* ── Header ───────────────────────────────────────────── */}
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <Badge variant={item.priority === 0 ? 'default' : 'secondary'}>
                    {item.priority === 0 ? 'Primary' : `Fallback ${item.priority}`}
                  </Badge>
                  <Controller
                    control={control}
                    name={`items.${index}.isEnabled`}
                    render={({ field }) => (
                      <label className="flex items-center gap-1.5 text-xs text-slate-500 cursor-pointer">
                        <Checkbox checked={field.value} onCheckedChange={(v) => field.onChange(v)} />
                        Enabled
                      </label>
                    )}
                  />
                </div>
                {item.priority > 0 && (
                  <button
                    type="button"
                    className="text-red-500 hover:text-red-700 transition-colors"
                    onClick={() => {
                      const updated = watchedItems.filter((_, i) => i !== index)
                      reset({ ...control._formValues, items: updated } as FormValues)
                    }}
                    title="Xoá"
                  >
                    <Trash2 className="h-4 w-4" />
                  </button>
                )}
              </div>

              {/* ── Provider ──────────────────────────────────────────── */}
              <Controller
                control={control}
                name={`items.${index}.aiProviderId`}
                render={({ field }) => (
                  <SelectDropdown<AIProviderBaseDto>
                    items={providerList}
                    getOptionId={(p) => p.id}
                    getOptionLabel={(p) => `${p.name} (${p.shortName})`}
                    renderValue={(p) => (
                      <div className="flex items-center gap-2">
                        {p.imageUrl && <img src={p.imageUrl} alt="" className="h-5 w-5 rounded-full object-contain" />}
                        <span>{p.name}</span>
                      </div>
                    )}
                    value={providerList.find((p) => p.id === field.value) ?? null}
                    onChange={(val) => {
                      field.onChange(val?.id ?? '')
                      control._formValues.items[index].aiProviderConfigurationId = ''
                    }}
                    placeholder="Chọn provider..."
                  />
                )}
              />

              {/* ── Config ────────────────────────────────────────────── */}
              <Controller
                control={control}
                name={`items.${index}.aiProviderConfigurationId`}
                render={({ field }) => (
                  <SelectDropdown<AIProviderConfigurationBaseDto>
                    items={filteredConfigs}
                    getOptionId={(c) => c.id}
                    getOptionLabel={(c) => c.name}
                    value={filteredConfigs.find((c) => c.id === field.value) ?? null}
                    onChange={(val) => field.onChange(val?.id ?? '')}
                    placeholder="Chọn config..."
                    disabled={!item.aiProviderId}
                    emptyText={item.aiProviderId ? 'Không có config nào' : 'Chọn provider trước'}
                  />
                )}
              />

              {/* ── Model picker (auto-fetch khi có provider + config) ── */}
              <Controller
                control={control}
                name={`items.${index}.model`}
                render={({ field }) => (
                  <SelectDropdown<AIModelInfo>
                    items={itemModels}
                    getOptionId={(m) => m.modelId}
                    getOptionLabel={(m) => m.displayName || m.modelId}
                    renderOption={(m) => (
                      <div className="flex flex-col">
                        <span className="text-sm">{m.displayName || m.modelId}</span>
                        <span className="text-xs text-slate-400">
                          Input: {m.inputTokenLimit?.toLocaleString() ?? '?'} · Output:{' '}
                          {m.outputTokenLimit?.toLocaleString() ?? '?'}
                        </span>
                      </div>
                    )}
                    value={itemModels.find((m) => m.modelId === field.value || m.displayName === field.value) ?? null}
                    onChange={(val) => field.onChange(val?.modelId ?? '')}
                    placeholder={
                      loadingModels[item.key]
                        ? 'Đang tải models…'
                        : modelErrors[item.key]
                          ? 'Lỗi tải models'
                          : 'Chọn model…'
                    }
                    disabled={!item.aiProviderConfigurationId || loadingModels[item.key]}
                    emptyText={
                      !item.aiProviderConfigurationId
                        ? 'Chọn config trước'
                        : loadingModels[item.key]
                          ? 'Đang tải…'
                          : modelErrors[item.key] ?? 'Không tìm thấy model'
                    }
                  />
                )}
              />

              {/* ── Overrides ──────────────────────────────────────────── */}
              <div className="grid grid-cols-2 gap-2">
                <FormGroup>
                  <FormLabel>Temperature</FormLabel>
                  <Input {...register(`items.${index}.temperature`)} type="number" step="0.01" min="0" max="2" placeholder="mặc định" />
                </FormGroup>
                <FormGroup>
                  <FormLabel>Max tokens</FormLabel>
                  <Input {...register(`items.${index}.maxTokens`)} type="number" min="0" placeholder="mặc định" />
                </FormGroup>
              </div>
            </div>
          )
        })}
      </div>

      <FormField error={formState.errors.model?.message} />
    </div>
  )
})
