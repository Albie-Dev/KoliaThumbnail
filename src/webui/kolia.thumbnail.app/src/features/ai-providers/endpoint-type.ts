export const EndpointType = {
  Model: 1,
  Chat: 2,
  Balance: 3
} as const

export type EndpointType = (typeof EndpointType)[keyof typeof EndpointType]

export interface EndpointTypeOption {
  id: EndpointType
  label: string
}

export const ENDPOINT_TYPE_OPTIONS: EndpointTypeOption[] = [
  { id: EndpointType.Chat, label: 'Chat' },
  { id: EndpointType.Model, label: 'Model' },
  { id: EndpointType.Balance, label: 'Balance' },
]

export function getEndpointTypeLabel(type: EndpointType): string {
  return ENDPOINT_TYPE_OPTIONS.find((o) => o.id === type)?.label ?? ''
}
