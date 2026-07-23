import type { Ref } from 'react'
import { registerSidebarEntry } from '../../lib/sidebar-registry'
import { CreateGoogleServiceForm, type CreateGoogleServiceFormHandle } from './create-google-service-form'
import { UpdateGoogleServiceForm, type UpdateGoogleServiceFormHandle } from './update-google-service-form'

registerSidebarEntry<{ type: 'create-google-service' }>('create-google-service', {
  title: () => 'Thêm Service Account',
  submitLabel: 'Tạo',
  render: ({ onClose, formRef }) => (
    <CreateGoogleServiceForm
      ref={formRef as Ref<CreateGoogleServiceFormHandle>}
      onSuccess={onClose}
    />
  ),
})

registerSidebarEntry<{ type: 'edit-google-service'; id: string }>('edit-google-service', {
  title: () => 'Cập nhật Service Account',
  submitLabel: 'Lưu',
  render: ({ content, onClose, formRef }) => (
    <UpdateGoogleServiceForm
      ref={formRef as Ref<UpdateGoogleServiceFormHandle>}
      editId={content.id}
      onSuccess={onClose}
    />
  ),
})
