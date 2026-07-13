import type { Ref } from 'react'
import { registerSidebarEntry } from '../../lib/sidebar-registry'
import { CreateAIProviderConfigurationForm, type CreateAIProviderConfigurationFormHandle } from './create-ai-configuration-form'
import { EditAIProviderConfigurationForm, type EditAIProviderConfigurationFormHandle } from './edit-ai-configuration-form'
import type { AIProviderConfigurationDetailDto } from './api'

// ── Tạo mới cấu hình ──────────────────────────────────────────────────
registerSidebarEntry<{ type: 'create-ai-configuration' }>('create-ai-configuration', {
  title: () => 'Tạo cấu hình mới',
  submitLabel: 'Tạo',
  render: ({ onClose, formRef }) => (
    <CreateAIProviderConfigurationForm
      ref={formRef as Ref<CreateAIProviderConfigurationFormHandle>}
      onClose={onClose}
    />
  ),
})

// ── Chỉnh sửa cấu hình ────────────────────────────────────────────────
registerSidebarEntry<{ type: 'edit-ai-configuration'; configuration: AIProviderConfigurationDetailDto }>(
  'edit-ai-configuration',
  {
    title: (content) => `Chỉnh sửa: ${content.configuration.name}`,
    submitLabel: 'Lưu',
    render: ({ content, onClose, formRef }) => (
      <EditAIProviderConfigurationForm
        ref={formRef as Ref<EditAIProviderConfigurationFormHandle>}
        configuration={content.configuration}
        onClose={onClose}
      />
    ),
  },
)
