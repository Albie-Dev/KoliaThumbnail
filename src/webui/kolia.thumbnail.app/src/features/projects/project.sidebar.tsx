import type { Ref } from 'react'
import { registerSidebarEntry } from '../../lib/sidebar-registry'
import { CreateProjectForm, type CreateProjectFormHandle } from './create-project-form'

registerSidebarEntry<{ type: 'create-project' }>('create-project', {
  title: () => 'Tạo project mới',
  submitLabel: 'Tạo',
  render: ({ onClose, formRef }) => (
    <CreateProjectForm
      ref={formRef as Ref<CreateProjectFormHandle>}
      onClose={onClose}
    />
  ),
})
