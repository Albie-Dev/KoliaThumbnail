import type { Ref } from 'react'
import { registerSidebarEntry } from '../../lib/sidebar-registry'
import { CreateScheduledJobForm, type CreateScheduledJobFormHandle } from './create-scheduled-job-form'
import { UpdateScheduledJobForm, type UpdateScheduledJobFormHandle } from './update-scheduled-job-form'

registerSidebarEntry<{ type: 'create-scheduled-job' }>('create-scheduled-job', {
  title: () => 'Tạo Scheduled Import Job',
  submitLabel: 'Tạo',
  render: ({ onClose, formRef }) => (
    <CreateScheduledJobForm
      ref={formRef as Ref<CreateScheduledJobFormHandle>}
      onSuccess={onClose}
    />
  ),
})

registerSidebarEntry<{ type: 'edit-scheduled-job'; id: string }>('edit-scheduled-job', {
  title: () => 'Sửa Scheduled Import Job',
  submitLabel: 'Lưu',
  render: ({ content, onClose, formRef }) => (
    <UpdateScheduledJobForm
      ref={formRef as Ref<UpdateScheduledJobFormHandle>}
      editJobId={content.id}
      onSuccess={onClose}
    />
  ),
})
