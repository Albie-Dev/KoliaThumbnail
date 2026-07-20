import type { Ref } from 'react'
import { registerSidebarEntry } from '../../lib/sidebar-registry'
import { CreateScheduledJobForm, type CreateScheduledJobFormHandle } from './create-scheduled-job-form'

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
