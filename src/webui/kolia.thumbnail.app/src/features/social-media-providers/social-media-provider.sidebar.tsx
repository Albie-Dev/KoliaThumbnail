import type { Ref } from 'react'
import { registerSidebarEntry } from '../../lib/sidebar-registry'
import { CreateSocialMediaProviderForm, type CreateSocialMediaProviderFormHandle } from './create-social-media-provider-form'
import { EditSocialMediaProviderForm, type EditSocialMediaProviderFormHandle } from './edit-social-media-provider-form'
import type { SocialMediaProviderBaseDto } from './api'

// ── Tạo mới nhà cung cấp ─────────────────────────────────────────────
registerSidebarEntry<{ type: 'create-social-media-provider' }>('create-social-media-provider', {
  title: () => 'Tạo nhà cung cấp mới',
  submitLabel: 'Tạo',
  render: ({ onClose, formRef }) => (
    <CreateSocialMediaProviderForm
      ref={formRef as Ref<CreateSocialMediaProviderFormHandle>}
      onClose={onClose}
    />
  ),
})

// ── Chỉnh sửa nhà cung cấp ────────────────────────────────────────────
registerSidebarEntry<{ type: 'edit-social-medial-provider'; provider: SocialMediaProviderBaseDto }>('edit-social-media-provider', {
  title: (content) => `Chỉnh sửa: ${content.provider.name}`,
  submitLabel: 'Lưu',
  render: ({ content, onClose, formRef }) => (
    <EditSocialMediaProviderForm
      ref={formRef as Ref<EditSocialMediaProviderFormHandle>}
      provider={content.provider}
      onClose={onClose}
    />
  ),
})
