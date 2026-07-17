import React, { createContext, useContext, useEffect, useRef, useState } from 'react';
import type { ReactNode } from 'react';
import { X } from 'lucide-react';
import { cn } from '../../lib/utils';

// ── Resize config ───────────────────────────────────────────────────────
const SIDEBAR_MIN_WIDTH = 320;
const SIDEBAR_MAX_WIDTH = 720;
const SIDEBAR_DEFAULT_WIDTH = 384; // tương đương w-96 trước đây
const SIDEBAR_WIDTH_STORAGE_KEY = 'kolia:sidebar-width';

function clampWidth(width: number): number {
  return Math.min(SIDEBAR_MAX_WIDTH, Math.max(SIDEBAR_MIN_WIDTH, width));
}

function readStoredWidth(): number {
  if (typeof window === 'undefined') return SIDEBAR_DEFAULT_WIDTH;
  try {
    const stored = window.localStorage.getItem(SIDEBAR_WIDTH_STORAGE_KEY);
    const parsed = stored ? Number(stored) : NaN;
    return Number.isFinite(parsed) ? clampWidth(parsed) : SIDEBAR_DEFAULT_WIDTH;
  } catch {
    // localStorage có thể bị chặn (chế độ ẩn danh nghiêm ngặt...) — bỏ qua, dùng mặc định
    return SIDEBAR_DEFAULT_WIDTH;
  }
}

function persistWidth(width: number): void {
  if (typeof window === 'undefined') return;
  try {
    window.localStorage.setItem(SIDEBAR_WIDTH_STORAGE_KEY, String(width));
  } catch {
    // bỏ qua nếu không ghi được
  }
}

interface SidebarContextType {
  isOpen: boolean;
  open: () => void;
  close: () => void;
}

const SidebarContext = createContext<SidebarContextType | undefined>(undefined);

export const SidebarProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <SidebarContext.Provider
      value={{
        isOpen,
        open: () => setIsOpen(true),
        close: () => setIsOpen(false),
      }}
    >
      {children}
    </SidebarContext.Provider>
  );
};

export const useSidebar = () => {
  const context = useContext(SidebarContext);
  if (!context) {
    throw new Error('useSidebar must be used within SidebarProvider');
  }
  return context;
};

interface SidebarTriggerProps {
  asChild?: boolean;
  children: ReactNode;
}

export const SidebarTrigger: React.FC<SidebarTriggerProps> = ({ children, asChild }) => {
  const { open } = useSidebar();

  if (asChild && React.isValidElement(children)) {
    return React.cloneElement(children as React.ReactElement<any>, {
      onClick: open,
    });
  }

  return <button onClick={open}>{children}</button>;
};

import * as Portal from '@radix-ui/react-portal';

interface SidebarProps {
  isOpen: boolean;
  onClose: () => void;
  children: ReactNode;
  side?: 'left' | 'right';
}

export const Sidebar: React.FC<SidebarProps> = ({
  isOpen,
  onClose,
  children,
  side = 'right',
}) => {
  const [width, setWidth] = useState<number>(readStoredWidth);
  const [isResizing, setIsResizing] = useState(false);
  const resizeStartRef = useRef({ pointerX: 0, startWidth: SIDEBAR_DEFAULT_WIDTH });

  const handleResizePointerDown = (e: React.PointerEvent<HTMLDivElement>) => {
    e.preventDefault();
    resizeStartRef.current = { pointerX: e.clientX, startWidth: width };
    setIsResizing(true);
    e.currentTarget.setPointerCapture(e.pointerId);
  };

  const handleResizePointerMove = (e: React.PointerEvent<HTMLDivElement>) => {
    if (!isResizing) return;
    const delta = e.clientX - resizeStartRef.current.pointerX;
    // Sidebar bên phải: kéo sang trái (delta âm) sẽ mở rộng.
    // Sidebar bên trái: kéo sang phải (delta dương) sẽ mở rộng.
    const nextWidth = side === 'right'
      ? resizeStartRef.current.startWidth - delta
      : resizeStartRef.current.startWidth + delta;
    setWidth(clampWidth(nextWidth));
  };

  const stopResizing = (e: React.PointerEvent<HTMLDivElement>) => {
    if (!isResizing) return;
    setIsResizing(false);
    e.currentTarget.releasePointerCapture(e.pointerId);
    persistWidth(width);
  };

  const handleResizeDoubleClick = () => {
    setWidth(SIDEBAR_DEFAULT_WIDTH);
    persistWidth(SIDEBAR_DEFAULT_WIDTH);
  };

  // Trong lúc đang kéo, vô hiệu hoá text-selection và đổi con trỏ toàn trang
  // để trải nghiệm kéo mượt kể cả khi chuột đi ra ngoài thanh resize.
  useEffect(() => {
    if (!isResizing) return;
    const previousCursor = document.body.style.cursor;
    const previousUserSelect = document.body.style.userSelect;
    document.body.style.cursor = 'col-resize';
    document.body.style.userSelect = 'none';
    return () => {
      document.body.style.cursor = previousCursor;
      document.body.style.userSelect = previousUserSelect;
    };
  }, [isResizing]);

  return (
    <Portal.Root>
      {/* Backdrop */}
      <div
        className={cn(
          'fixed inset-0 z-40 bg-black transition-all duration-400 ease-in-out',
          isOpen ? 'opacity-50' : 'pointer-events-none opacity-0'
        )}
        onClick={onClose}
        aria-hidden="true"
      />

      {/* Sidebar */}
      <div
        style={{ width: `${width}px` }}
        className={cn(
          'fixed top-0 z-50 h-screen bg-white dark:bg-slate-900 shadow-2xl flex flex-col',
          side === 'right' ? 'right-0' : 'left-0',
          // Khi đang resize thì bỏ transition để thanh sidebar bám theo chuột tức thời,
          // không bị "trễ" do easing của transition mở/đóng.
          isResizing ? '' : 'transition-all duration-400 ease-in-out',
          side === 'right' ? (isOpen ? 'translate-x-0' : 'translate-x-full') : (isOpen ? 'translate-x-0' : '-translate-x-full')
        )}
      >
        {/* Resize handle */}
        <div
          role="separator"
          aria-orientation="vertical"
          aria-label="Kéo để đổi kích thước sidebar"
          title="Kéo để đổi kích thước, nhấp đôi để về mặc định"
          onPointerDown={handleResizePointerDown}
          onPointerMove={handleResizePointerMove}
          onPointerUp={stopResizing}
          onPointerCancel={stopResizing}
          onDoubleClick={handleResizeDoubleClick}
          className={cn(
            'absolute top-0 z-10 h-full w-1.5 cursor-col-resize touch-none group',
            side === 'right' ? '-left-0.5' : '-right-0.5'
          )}
        >
          <div
            className={cn(
              'h-full w-px mx-auto bg-transparent group-hover:bg-blue-400 transition-colors',
              isResizing ? 'bg-blue-500' : ''
            )}
          />
        </div>

        {children}
      </div>
    </Portal.Root>
  );
};

interface SidebarHeaderProps {
  title: string;
  onClose: () => void;
}

export const SidebarHeader: React.FC<SidebarHeaderProps> = ({ title, onClose }) => {
  return (
    <div className="flex items-center justify-between border-b px-6 py-4">
      <h2 className="text-xl font-semibold text-slate-900 dark:text-slate-100">{title}</h2>
      <button
        onClick={onClose}
        className="inline-flex items-center justify-center rounded-md p-2 text-slate-500 dark:text-slate-400 hover:bg-slate-100 hover:dark:bg-slate-800 hover:text-slate-900 hover:dark:text-slate-100 focus:outline-none"
      >
        <X className="h-5 w-5" />
      </button>
    </div>
  );
};

interface SidebarBodyProps {
  children: ReactNode;
}

export const SidebarBody: React.FC<SidebarBodyProps> = ({ children }) => {
  return <div className="flex-1 overflow-y-auto px-6 py-4">{children}</div>;
};

interface SidebarFooterProps {
  children: ReactNode;
}

export const SidebarFooter: React.FC<SidebarFooterProps> = ({ children }) => {
  return (
    <div className="border-t px-6 py-4 flex items-center justify-end gap-3">
      {children}
    </div>
  );
};
