import type { Ref } from 'react'
import { registerSidebarEntry } from '../../lib/sidebar-registry'
import { CreateAIProviderForm, type CreateAIProviderFormHandle } from './create-ai-provider-form'
import { EditAIProviderForm, type EditAIProviderFormHandle } from './edit-ai-provider-form'
import type { AIProviderBaseDto } from './api'

// ── Tạo mới nhà cung cấp ─────────────────────────────────────────────
registerSidebarEntry<{ type: 'create-ai-provider' }>('create-ai-provider', {
  title: () => 'Tạo nhà cung cấp mới',
  submitLabel: 'Tạo',
  render: ({ onClose, formRef }) => (
    <CreateAIProviderForm
      ref={formRef as Ref<CreateAIProviderFormHandle>}
      onClose={onClose}
    />
  ),
})

// ── Chỉnh sửa nhà cung cấp ────────────────────────────────────────────
registerSidebarEntry<{ type: 'edit-ai-provider'; provider: AIProviderBaseDto }>('edit-ai-provider', {
  title: (content) => `Chỉnh sửa: ${content.provider.name}`,
  submitLabel: 'Lưu',
  render: ({ content, onClose, formRef }) => (
    <EditAIProviderForm
      ref={formRef as Ref<EditAIProviderFormHandle>}
      provider={content.provider}
      onClose={onClose}
    />
  ),
})
