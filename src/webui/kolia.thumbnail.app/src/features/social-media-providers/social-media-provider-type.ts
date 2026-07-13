export const CSocialMediaProviderType = {
  Youtube: 1,
  Facebook: 2,
  Tiktok: 3,
  X: 4
} as const

export type CSocialMediaProviderType = (typeof CSocialMediaProviderType)[keyof typeof CSocialMediaProviderType]

export interface CSocialMediaProviderTypeOption {
  id: CSocialMediaProviderType
  label: string
}

export const SOCIAL_MEDIA_PROVIDER_TYPE_OPTIONS: CSocialMediaProviderTypeOption[] = [
  // ===== LLM / Chat Providers =====
  { id: CSocialMediaProviderType.Youtube, label: 'Youtube' },
  { id: CSocialMediaProviderType.Facebook, label: 'Facebook' },
  { id: CSocialMediaProviderType.Tiktok, label: 'Tiktok' },
  { id: CSocialMediaProviderType.X, label: 'X' },
]

export function getSocialMediaProviderTypeLabel(type: CSocialMediaProviderType): string | undefined {
  return SOCIAL_MEDIA_PROVIDER_TYPE_OPTIONS.find((o) => o.id === type)?.label;
}

const BADGE_COLORS = [
  'bg-blue-100 text-blue-700',
  'bg-green-100 text-green-700',
  'bg-amber-100 text-amber-700',
  'bg-rose-100 text-rose-700',
  'bg-purple-100 text-purple-700',
  'bg-cyan-100 text-cyan-700',
  'bg-teal-100 text-teal-700',
  'bg-pink-100 text-pink-700',
  'bg-indigo-100 text-indigo-700',
  'bg-orange-100 text-orange-700',
  'bg-lime-100 text-lime-700',
  'bg-violet-100 text-violet-700',
] as const

export function getSocialMediaProviderTypeBadgeClass(type: number): string {
  return BADGE_COLORS[Math.abs(type) % BADGE_COLORS.length]
}
