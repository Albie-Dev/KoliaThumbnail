import { useEffect, useRef, useState } from 'react';
import { useSidebarContext } from '../lib/sidebar-context';
import { Sidebar, SidebarHeader, SidebarBody, SidebarFooter } from './ui/sidebar';
import { Button } from './ui/button';
import { getSidebarEntry, type SidebarFormHandle } from '../lib/sidebar-registry';

// ─────────────────────────────────────────────────────────────────────────
// Tự động nạp mọi file "*.sidebar.tsx" bên trong src/features/**.
// Mỗi file như vậy tự gọi registerSidebarEntry(...) để đăng ký loại sidebar
// mà feature đó cung cấp (xem src/lib/sidebar-registry.tsx).
//
// ⚠️ QUY TẮC KIẾN TRÚC: Khi thêm một sub-page mới cần dùng sidebar dùng chung,
// KHÔNG được sửa file này. Chỉ cần tạo file "<feature>.sidebar.tsx" trong
// thư mục feature đó — component sẽ tự nhận diện.
//
// LƯU Ý VỀ BUNDLE SIZE: cố tình dùng import.meta.glob() KHÔNG kèm
// { eager: true } — mỗi file "*.sidebar.tsx" (và toàn bộ form/zod/
// react-hook-form mà nó kéo theo) sẽ được Rollup tách thành chunk riêng,
// chỉ tải bất đồng bộ SAU khi trang đầu tiên đã render xong, thay vì
// bị nhét cứng vào main bundle và chặn tải trang lần đầu. Vì AppSidebar
// luôn mount cùng App (không nằm trong route), việc tải diễn ra gần như
// ngay lập tức nên không ảnh hưởng trải nghiệm khi người dùng bấm mở sidebar.
// ─────────────────────────────────────────────────────────────────────────
const sidebarModuleLoaders = import.meta.glob('../features/**/*.sidebar.tsx');

let sidebarModulesLoaded = false;
let sidebarModulesLoadingPromise: Promise<void> | null = null;

function loadSidebarModules(): Promise<void> {
  if (sidebarModulesLoaded) return Promise.resolve();
  if (!sidebarModulesLoadingPromise) {
    sidebarModulesLoadingPromise = Promise.all(
      Object.values(sidebarModuleLoaders).map((load) => load()),
    ).then(() => {
      sidebarModulesLoaded = true;
    });
  }
  return sidebarModulesLoadingPromise;
}

export function AppSidebar() {
  const { isOpen, content, close } = useSidebarContext();
  const formRef = useRef<SidebarFormHandle>(null);
  // Chỉ dùng để ép re-render sau khi các module sidebar tải xong lần đầu.
  const [, setModulesReadyTick] = useState(0);

  useEffect(() => {
    let cancelled = false;
    void loadSidebarModules().then(() => {
      if (!cancelled) setModulesReadyTick((tick) => tick + 1);
    });
    return () => {
      cancelled = true;
    };
  }, []);

  const entry = sidebarModulesLoaded ? getSidebarEntry(content?.type) : undefined;

  return (
    <Sidebar isOpen={isOpen} onClose={close}>
      <SidebarHeader title={entry && content ? entry.title(content) : ''} onClose={close} />
      <SidebarBody>
        <div
          className={`transition-opacity duration-400 ease-in-out ${
            isOpen ? 'opacity-100' : 'opacity-0'
          }`}
        >
          {entry && content && entry.render({ content, onClose: close, formRef })}
        </div>
      </SidebarBody>
      <SidebarFooter>
        <Button variant="outline" onClick={close}>
          Hủy
        </Button>
        {entry && (
          <Button
            onClick={() => formRef.current?.submit()}
            disabled={formRef.current?.isSubmitting}
          >
            {formRef.current?.isSubmitting
              ? (entry.submittingLabel ?? 'Đang xử lý…')
              : entry.submitLabel}
          </Button>
        )}
      </SidebarFooter>
    </Sidebar>
  );
}
