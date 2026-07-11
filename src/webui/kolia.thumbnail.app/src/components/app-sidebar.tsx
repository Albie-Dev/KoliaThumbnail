import { useRef } from 'react';
import { useSidebarContext } from '../lib/sidebar-context';
import { Sidebar, SidebarHeader, SidebarBody, SidebarFooter } from './ui/sidebar';
import { CreateAIProviderForm, type CreateAIProviderFormHandle } from '../features/ai-providers/create-ai-provider-form';
import { EditAIProviderForm, type EditAIProviderFormHandle } from '../features/ai-providers/edit-ai-provider-form';
import { CreateAIConfigurationForm, type CreateAIConfigurationFormHandle } from '../features/ai-configurations/create-ai-configuration-form';
import { EditAIConfigurationForm, type EditAIConfigurationFormHandle } from '../features/ai-configurations/edit-ai-configuration-form';
import { Button } from './ui/button';

export function AppSidebar() {
  const { isOpen, content, close } = useSidebarContext();
  const createFormRef = useRef<CreateAIProviderFormHandle>(null);
  const editFormRef = useRef<EditAIProviderFormHandle>(null);
  const createConfigFormRef = useRef<CreateAIConfigurationFormHandle>(null);
  const editConfigFormRef = useRef<EditAIConfigurationFormHandle>(null);

  const getTitle = () => {
    if (!content) return '';
    if (content.type === 'create-ai-provider') return 'Tạo nhà cung cấp mới';
    if (content.type === 'edit-ai-provider') {
      const provider = content.provider as { name: string };
      return `Chỉnh sửa: ${provider.name}`;
    }
    if (content.type === 'create-ai-configuration') return 'Tạo cấu hình mới';
    if (content.type === 'edit-ai-configuration') {
      const configuration = content.configuration as { name: string };
      return `Chỉnh sửa: ${configuration.name}`;
    }
    return '';
  };

  const isEditMode = content?.type === 'edit-ai-provider';
  const isCreateMode = content?.type === 'create-ai-provider';
  const isCreateConfigMode = content?.type === 'create-ai-configuration';
  const isEditConfigMode = content?.type === 'edit-ai-configuration';

  return (
    <Sidebar isOpen={isOpen} onClose={close}>
      <SidebarHeader title={getTitle()} onClose={close} />
      <SidebarBody>
        <div
          className={`transition-opacity duration-400 ease-in-out ${
            isOpen ? 'opacity-100' : 'opacity-0'
          }`}
        >
          {isCreateMode && (
            <CreateAIProviderForm ref={createFormRef} onClose={close} />
          )}
          {isEditMode && (
            <EditAIProviderForm ref={editFormRef} provider={content.provider as import('../features/ai-providers/api').AIProviderBaseDto} onClose={close} />
          )}
          {isCreateConfigMode && (
            <CreateAIConfigurationForm ref={createConfigFormRef} onClose={close} />
          )}
          {isEditConfigMode && (
            <EditAIConfigurationForm ref={editConfigFormRef} configuration={content.configuration as import('../features/ai-configurations/api').AIConfigurationBaseDto} onClose={close} />
          )}
        </div>
      </SidebarBody>
      <SidebarFooter>
        <Button variant="outline" onClick={close}>
          Hủy
        </Button>
        {isCreateMode && (
          <Button
            onClick={() => createFormRef.current?.submit()}
            disabled={createFormRef.current?.isSubmitting}
          >
            {createFormRef.current?.isSubmitting ? 'Đang xử lý…' : 'Tạo'}
          </Button>
        )}
        {isEditMode && (
          <Button
            onClick={() => editFormRef.current?.submit()}
            disabled={editFormRef.current?.isSubmitting}
          >
            {editFormRef.current?.isSubmitting ? 'Đang xử lý…' : 'Lưu'}
          </Button>
        )}
        {isCreateConfigMode && (
          <Button
            onClick={() => createConfigFormRef.current?.submit()}
            disabled={createConfigFormRef.current?.isSubmitting}
          >
            {createConfigFormRef.current?.isSubmitting ? 'Đang xử lý…' : 'Tạo'}
          </Button>
        )}
        {isEditConfigMode && (
          <Button
            onClick={() => editConfigFormRef.current?.submit()}
            disabled={editConfigFormRef.current?.isSubmitting}
          >
            {editConfigFormRef.current?.isSubmitting ? 'Đang xử lý…' : 'Lưu'}
          </Button>
        )}
      </SidebarFooter>
    </Sidebar>
  );
}
