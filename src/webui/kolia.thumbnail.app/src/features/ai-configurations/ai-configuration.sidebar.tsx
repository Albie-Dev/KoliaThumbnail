import type { Ref } from 'react'
import { registerSidebarEntry } from '../../lib/sidebar-registry'
import { CreateAIConfigurationForm, type CreateAIConfigurationFormHandle } from './create-ai-configuration-form'
import { EditAIConfigurationForm, type EditAIConfigurationFormHandle } from './edit-ai-configuration-form'
import type { AIConfigurationBaseDto } from './api'

// ── Tạo mới cấu hình ──────────────────────────────────────────────────
registerSidebarEntry<{ type: 'create-ai-configuration' }>('create-ai-configuration', {
  title: () => 'Tạo cấu hình mới',
  submitLabel: 'Tạo',
  render: ({ onClose, formRef }) => (
    <CreateAIConfigurationForm
      ref={formRef as Ref<CreateAIConfigurationFormHandle>}
      onClose={onClose}
    />
  ),
})

// ── Chỉnh sửa cấu hình ────────────────────────────────────────────────
registerSidebarEntry<{ type: 'edit-ai-configuration'; configuration: AIConfigurationBaseDto }>(
  'edit-ai-configuration',
  {
    title: (content) => `Chỉnh sửa: ${content.configuration.name}`,
    submitLabel: 'Lưu',
    render: ({ content, onClose, formRef }) => (
      <EditAIConfigurationForm
        ref={formRef as Ref<EditAIConfigurationFormHandle>}
        configuration={content.configuration}
        onClose={onClose}
      />
    ),
  },
)
