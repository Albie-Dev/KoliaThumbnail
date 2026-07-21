import { useState, useEffect, useRef } from 'react'
import { Input } from '../../components/ui/input'
import { SelectDropdown } from '../../components/selects/select-dropdown'
import { FormGroup, FormLabel } from '../../components/ui/form'

const WEEKDAYS = [
  { id: 1, label: 'Thứ 2' },
  { id: 2, label: 'Thứ 3' },
  { id: 3, label: 'Thứ 4' },
  { id: 4, label: 'Thứ 5' },
  { id: 5, label: 'Thứ 6' },
  { id: 6, label: 'Thứ 7' },
  { id: 0, label: 'CN' },
]

const FREQ_OPTIONS = [
  { id: 'minutes', label: 'Phút' },
  { id: 'hours', label: 'Giờ' },
  { id: 'daily', label: 'Hàng ngày' },
  { id: 'weekly', label: 'Hàng tuần' },
  { id: 'monthly', label: 'Hàng tháng' },
  { id: 'custom', label: 'Tuỳ chỉnh (Cron)' },
]

interface CronBuilderProps {
  value: string
  description: string
  onChange: (cron: string, desc: string) => void
}

export function CronBuilder({ value, description, onChange }: CronBuilderProps) {
  const [freq, setFreq] = useState('minutes')
  const [intervalVal, setIntervalVal] = useState(5)
  const [hour, setHour] = useState(9)
  const [minute, setMinute] = useState(0)
  const [selectedDays, setSelectedDays] = useState<number[]>([1])
  const [monthDay, setMonthDay] = useState(1)
  const [customCron, setCustomCron] = useState(value || '')
  const [customDesc, setCustomDesc] = useState(description || '')
  const mounted = useRef(false)
  const isParsingFromProp = useRef(false)
  const lastGeneratedCron = useRef<string | null>(null)
  const onChangeRef = useRef(onChange)
  onChangeRef.current = onChange

  // Đánh dấu mounted sau lần render đầu — giúp sync effect hoạt động
  // ở create mode (khi value='' thì parse effect không set flag)
  useEffect(() => {
    mounted.current = true
  }, [])

  // Parse cron expression khi value thay đổi (từ edit data)
  useEffect(() => {
    if (!value) return

    // Skip nếu value là cron ta vừa generate ra (tránh re-parse vô hạn)
    if (lastGeneratedCron.current === value) return

    isParsingFromProp.current = true
    const parts = value.trim().split(/\s+/)
    if (parts.length !== 5) { setFreq('custom'); isParsingFromProp.current = false; return }

    const [min, hr, day, mon, dow] = parts

    if (min.startsWith('*/') && hr === '*' && day === '*' && mon === '*' && dow === '*') {
      setFreq('minutes'); setIntervalVal(parseInt(min.slice(2)) || 5)
    } else if (min === '0' && hr.startsWith('*/') && day === '*' && mon === '*' && dow === '*') {
      setFreq('hours'); setIntervalVal(parseInt(hr.slice(2)) || 1)
    } else if (min !== '*' && hr !== '*' && day === '*' && mon === '*' && dow === '*') {
      setFreq('daily'); setHour(parseInt(hr) || 9); setMinute(parseInt(min) || 0)
    } else if (min !== '*' && hr !== '*' && day === '*' && mon === '*' && dow !== '*') {
      setFreq('weekly'); setHour(parseInt(hr) || 9); setMinute(parseInt(min) || 0)
      setSelectedDays(dow.split(',').map(Number))
    } else if (min !== '*' && hr !== '*' && day !== '*' && mon === '*' && dow === '*') {
      setFreq('monthly'); setHour(parseInt(hr) || 9); setMinute(parseInt(min) || 0)
      setMonthDay(parseInt(day) || 1)
    } else {
      setFreq('custom')
    }
    // Reset flag after state updates are queued
    setTimeout(() => { isParsingFromProp.current = false }, 0)
    // Không gọi onChange ở đây — parent đã setValue rồi, chỉ parse để hiển thị UI
  }, [value])

  // Sync lên parent khi user thay đổi UI control (freq, intervalVal, hour, ...)
  useEffect(() => {
    if (!mounted.current || freq === 'custom' || isParsingFromProp.current) return

    const { cron, desc } = generateCron()
    if (cron !== value) {
      lastGeneratedCron.current = cron
      onChangeRef.current(cron, desc)
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [freq, intervalVal, hour, minute, selectedDays, monthDay])

  const generateCron = () => {
    let cron = ''
    let desc = ''

    switch (freq) {
      case 'minutes':
        cron = `*/${intervalVal} * * * *`
        desc = `Mỗi ${intervalVal} phút`
        break
      case 'hours':
        cron = `0 */${intervalVal} * * *`
        desc = `Mỗi ${intervalVal} giờ`
        break
      case 'daily':
        cron = `${minute} ${hour} * * *`
        desc = `Hàng ngày lúc ${String(hour).padStart(2, '0')}:${String(minute).padStart(2, '0')}`
        break
      case 'weekly': {
        const dayLabels = selectedDays.map(d => WEEKDAYS.find(w => w.id === d)?.label || '').filter(Boolean)
        cron = `${minute} ${hour} * * ${selectedDays.sort().join(',')}`
        desc = `Hàng tuần vào ${dayLabels.join(', ')} lúc ${String(hour).padStart(2, '0')}:${String(minute).padStart(2, '0')}`
        break
      }
      case 'monthly':
        cron = `${minute} ${hour} ${monthDay} * *`
        desc = `Hàng tháng vào ngày ${monthDay} lúc ${String(hour).padStart(2, '0')}:${String(minute).padStart(2, '0')}`
        break
      case 'custom':
        cron = customCron
        desc = customDesc
        break
    }

    return { cron, desc }
  }

  const handleCustomChange = (cron: string, desc: string) => {
    setCustomCron(cron)
    setCustomDesc(desc)
    if (freq === 'custom') {
      lastGeneratedCron.current = cron
      onChangeRef.current(cron, desc)
    }
  }

  const toggleDay = (day: number) => {
    setSelectedDays(prev =>
      prev.includes(day) ? prev.filter(d => d !== day) : [...prev, day]
    )
  }

  const renderOptions = () => {
    switch (freq) {
      case 'minutes':
        return (
          <FormGroup>
            <FormLabel>Khoảng cách (phút)</FormLabel>
            <div className="flex items-center gap-2">
              <span className="text-sm text-slate-500">Mỗi</span>
              <Input
                type="number"
                min={1}
                max={59}
                value={intervalVal}
                onChange={(e) => setIntervalVal(Math.max(1, parseInt(e.target.value) || 1))}
                className="w-20"
              />
              <span className="text-sm text-slate-500">phút</span>
            </div>
          </FormGroup>
        )

      case 'hours':
        return (
          <FormGroup>
            <FormLabel>Khoảng cách (giờ)</FormLabel>
            <div className="flex items-center gap-2">
              <span className="text-sm text-slate-500">Mỗi</span>
              <Input
                type="number"
                min={1}
                max={23}
                value={intervalVal}
                onChange={(e) => setIntervalVal(Math.max(1, parseInt(e.target.value) || 1))}
                className="w-20"
              />
              <span className="text-sm text-slate-500">giờ</span>
            </div>
          </FormGroup>
        )

      case 'daily':
        return (
          <FormGroup>
            <FormLabel>Giờ chạy</FormLabel>
            <div className="flex items-center gap-2">
              <Input
                type="number"
                min={0}
                max={23}
                value={hour}
                onChange={(e) => setHour(Math.min(23, Math.max(0, parseInt(e.target.value) || 0)))}
                className="w-20"
              />
              <span className="text-sm text-slate-500">giờ</span>
              <Input
                type="number"
                min={0}
                max={59}
                value={minute}
                onChange={(e) => setMinute(Math.min(59, Math.max(0, parseInt(e.target.value) || 0)))}
                className="w-20"
              />
              <span className="text-sm text-slate-500">phút</span>
            </div>
          </FormGroup>
        )

      case 'weekly':
        return (
          <>
            <FormGroup>
              <FormLabel>Giờ chạy</FormLabel>
              <div className="flex items-center gap-2">
                <Input
                  type="number"
                  min={0}
                  max={23}
                  value={hour}
                  onChange={(e) => setHour(Math.min(23, Math.max(0, parseInt(e.target.value) || 0)))}
                  className="w-20"
                />
                <span className="text-sm text-slate-500">giờ</span>
                <Input
                  type="number"
                  min={0}
                  max={59}
                  value={minute}
                  onChange={(e) => setMinute(Math.min(59, Math.max(0, parseInt(e.target.value) || 0)))}
                  className="w-20"
                />
                <span className="text-sm text-slate-500">phút</span>
              </div>
            </FormGroup>
            <FormGroup>
              <FormLabel>Chọn ngày trong tuần</FormLabel>
              <div className="flex flex-wrap gap-1.5">
                {WEEKDAYS.map((day) => (
                  <button
                    key={day.id}
                    type="button"
                    onClick={() => toggleDay(day.id)}
                    className={`px-3 py-1.5 rounded-md text-xs font-medium transition-colors ${
                      selectedDays.includes(day.id)
                        ? 'bg-blue-500 text-white'
                        : 'bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'
                    }`}
                  >
                    {day.label}
                  </button>
                ))}
              </div>
              {selectedDays.length === 0 && (
                <p className="text-xs text-rose-500">Vui lòng chọn ít nhất 1 ngày</p>
              )}
            </FormGroup>
          </>
        )

      case 'monthly':
        return (
          <>
            <FormGroup>
              <FormLabel>Giờ chạy</FormLabel>
              <div className="flex items-center gap-2">
                <Input
                  type="number"
                  min={0}
                  max={23}
                  value={hour}
                  onChange={(e) => setHour(Math.min(23, Math.max(0, parseInt(e.target.value) || 0)))}
                  className="w-20"
                />
                <span className="text-sm text-slate-500">giờ</span>
                <Input
                  type="number"
                  min={0}
                  max={59}
                  value={minute}
                  onChange={(e) => setMinute(Math.min(59, Math.max(0, parseInt(e.target.value) || 0)))}
                  className="w-20"
                />
                <span className="text-sm text-slate-500">phút</span>
              </div>
            </FormGroup>
            <FormGroup>
              <FormLabel>Ngày trong tháng</FormLabel>
              <div className="flex items-center gap-2">
                <span className="text-sm text-slate-500">Ngày</span>
                <Input
                  type="number"
                  min={1}
                  max={28}
                  value={monthDay}
                  onChange={(e) => setMonthDay(Math.min(28, Math.max(1, parseInt(e.target.value) || 1)))}
                  className="w-20"
                />
              </div>
            </FormGroup>
          </>
        )

      case 'custom':
        return (
          <>
            <FormGroup>
              <FormLabel htmlFor="cronExpression">Cron Expression</FormLabel>
              <Input
                id="cronExpression"
                value={customCron}
                onChange={(e) => handleCustomChange(e.target.value, customDesc)}
                placeholder="VD: */5 * * * *"
                className="font-mono"
              />
              <p className="text-xs text-slate-400 mt-1">
                Định dạng 5 trường: phút giờ ngày tháng thứ.
                <a href="https://crontab.guru" target="_blank" rel="noopener noreferrer" className="ml-1 text-blue-500 hover:underline">crontab.guru</a>
              </p>
            </FormGroup>
            <FormGroup>
              <FormLabel htmlFor="cronDesc">Mô tả</FormLabel>
              <Input
                id="cronDesc"
                value={customDesc}
                onChange={(e) => handleCustomChange(customCron, e.target.value)}
                placeholder="VD: Mỗi 5 phút"
              />
            </FormGroup>
          </>
        )
    }
  }

  const userTimeZone = Intl.DateTimeFormat().resolvedOptions().timeZone
  const { cron: previewCron } = generateCron()

  return (
    <div className="space-y-4">
      {/* Hiển thị múi giờ tự động detect */}
      <div className="flex items-center gap-2 rounded border border-blue-200 dark:border-blue-800 bg-blue-50 dark:bg-blue-900/20 px-3 py-2 text-xs text-blue-700 dark:text-blue-300">
        <span>🕐</span>
        <span>Múi giờ phát hiện: <strong>{userTimeZone}</strong></span>
        <span className="text-blue-400">(cron chạy theo giờ địa phương của bạn)</span>
      </div>

      <FormGroup>
        <FormLabel required>Tần suất</FormLabel>
        <SelectDropdown<{ id: string; label: string }>
          items={FREQ_OPTIONS}
          getOptionId={(opt) => opt.id}
          getOptionLabel={(opt) => opt.label}
          value={FREQ_OPTIONS.find((o) => o.id === freq) ?? null}
          onChange={(opt) => {
            if (opt) setFreq(opt.id)
          }}
          placeholder="Chọn tần suất..."
        />
      </FormGroup>

      {renderOptions()}

      {freq !== 'custom' && (
        <div className="rounded border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800/50 px-3 py-2">
          <div className="text-xs text-slate-500 dark:text-slate-400">
            Cron: <code className="font-mono text-blue-600 dark:text-blue-400 ml-1">{previewCron}</code>
          </div>
          <div className="text-xs text-slate-500 mt-0.5">
            Mô tả: <span className="text-slate-700 dark:text-slate-300 ml-1">{generateCron().desc}</span>
          </div>
        </div>
      )}
    </div>
  )
}
