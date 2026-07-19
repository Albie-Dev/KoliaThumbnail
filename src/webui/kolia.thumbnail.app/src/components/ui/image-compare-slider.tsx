import { useRef, useCallback } from 'react'
import { cn } from '../../lib/utils'

interface ImageCompareSliderProps {
  beforeSrc: string
  afterSrc: string
  /** Giá trị kéo 0–100, mặc định 50 */
  value?: number
  onChange?: (value: number) => void
  beforeLabel?: string
  afterLabel?: string
  className?: string
}

export function ImageCompareSlider({
  beforeSrc,
  afterSrc,
  value = 50,
  onChange,
  beforeLabel = 'Before',
  afterLabel = 'After',
  className,
}: ImageCompareSliderProps) {
  const containerRef = useRef<HTMLDivElement>(null)
  const isDragging = useRef(false)

  const handleMove = useCallback(
    (clientX: number) => {
      if (!containerRef.current) return
      const rect = containerRef.current.getBoundingClientRect()
      const x = Math.max(0, Math.min(clientX - rect.left, rect.width))
      const percent = (x / rect.width) * 100
      onChange?.(Math.round(percent))
    },
    [onChange],
  )

  const handleMouseDown = () => {
    isDragging.current = true
  }

  const handleMouseMove = (e: React.MouseEvent) => {
    if (!isDragging.current) return
    handleMove(e.clientX)
  }

  const handleMouseUp = () => {
    isDragging.current = false
  }

  const handleTouchMove = (e: React.TouchEvent) => {
    handleMove(e.touches[0].clientX)
  }

  return (
    <div
      ref={containerRef}
      className={cn('relative select-none overflow-hidden rounded-lg', className)}
      style={{ aspectRatio: '16 / 9' }}
      onMouseMove={handleMouseMove}
      onMouseUp={handleMouseUp}
      onMouseLeave={handleMouseUp}
      onTouchMove={handleTouchMove}
    >
      {/* After image (full) */}
      <img
        src={afterSrc}
        alt={afterLabel}
        className="absolute inset-0 h-full w-full object-cover"
        draggable={false}
      />

      {/* Before image (clipped) */}
      <div
        className="absolute inset-0 h-full overflow-hidden"
        style={{ width: `${value}%` }}
      >
        <img
          src={beforeSrc}
          alt={beforeLabel}
          className="absolute inset-0 h-full w-full object-cover"
          draggable={false}
          style={{ width: `${100 / (value / 100)}%`, maxWidth: 'none' }}
        />
      </div>

      {/* Slider handle */}
      <div
        className="absolute inset-y-0 cursor-ew-resize"
        style={{ left: `${value}%` }}
        onMouseDown={handleMouseDown}
        onTouchStart={handleMouseDown}
      >
        {/* Line */}
        <div className="absolute inset-y-0 left-1/2 w-0.5 -translate-x-1/2 bg-white shadow-md" />
        {/* Circle handle */}
        <div className="absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 flex h-8 w-8 items-center justify-center rounded-full bg-white shadow-lg ring-2 ring-white">
          <svg
            width="16"
            height="16"
            viewBox="0 0 16 16"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            className="text-slate-700"
          >
            <path d="M4 12L8 8L4 4" />
            <path d="M12 12L8 8L12 4" />
          </svg>
        </div>
      </div>

      {/* Labels */}
      <span className="absolute left-2 top-2 rounded bg-black/50 px-1.5 py-0.5 text-xs text-white">
        {beforeLabel}
      </span>
      <span className="absolute right-2 top-2 rounded bg-black/50 px-1.5 py-0.5 text-xs text-white">
        {afterLabel}
      </span>
    </div>
  )
}
