export const CAIProviderType = {
  // ===== LLM / Chat Providers =====
  /// <summary>Open AI (GPT-4o, GPT-4.1, o-series...)</summary>
  OpenAI: 1,
  /// <summary>Google Gemini</summary>
  Gemini: 2,
  /// <summary>Deepseek</summary>
  Deepseek: 3,
  /// <summary>Groq (LPU inference - Llama, Mixtral...)</summary>
  Groq: 4,
  /// <summary>X AI (Grok)</summary>
  XAI: 5,
  /// <summary>Anthropic (Claude)</summary>
  Anthropic: 6,
  /// <summary>Mistral AI</summary>
  Mistral: 7,
  /// <summary>Cohere (Command R+)</summary>
  Cohere: 8,
  /// <summary>Perplexity AI</summary>
  Perplexity: 9,
  /// <summary>Together AI (host nhiều open-source model)</summary>
  TogetherAI: 10,
  /// <summary>OpenRouter (gateway tổng hợp nhiều model)</summary>
  OpenRouter: 11,
  /// <summary>Azure OpenAI Service</summary>
  AzureOpenAI: 12,
  /// <summary>AWS Bedrock</summary>
  AWSBedrock: 13,
  /// <summary>Ollama (chạy model local)</summary>
  Ollama: 14,
  /// <summary>Hugging Face Inference API</summary>
  HuggingFace: 15,
  /// <summary>Alibaba Qwen</summary>
  Qwen: 16,
  /// <summary>Baidu ERNIE</summary>
  ErnieBaidu: 17,
  /// <summary>Moonshot AI (Kimi)</summary>
  Moonshot: 18,
  /// <summary>Zhipu AI (GLM)</summary>
  ZhipuAI: 19,
  /// <summary>MiniMax</summary>
  MiniMax: 20,
  /// <summary>Fireworks AI</summary>
  FireworksAI: 21,
  /// <summary>Replicate (host model theo container)</summary>
  Replicate: 22,

  // ===== Image Generation Providers =====
  /// <summary>Stability AI (Stable Diffusion, SD3)</summary>
  StabilityAI: 100,
  /// <summary>Black Forest Labs (Flux)</summary>
  BlackForestLabs: 101,
  /// <summary>Ideogram AI</summary>
  Ideogram: 102,
  /// <summary>Leonardo AI</summary>
  LeonardoAI: 103,
  /// <summary>Recraft AI</summary>
  Recraft: 104,

  // ===== Audio Providers =====
  /// <summary>ElevenLabs - TTS chất lượng cao, clone giọng</summary>
  ElevenLabs: 200,
  /// <summary>AssemblyAI - Speech to text, phụ đề</summary>
  AssemblyAI: 201,
  /// <summary>Deepgram - Speech to text realtime</summary>
  Deepgram: 202,
  /// <summary>PlayHT - Text to speech</summary>
  PlayHT: 203,

  // ===== Video Providers =====
  /// <summary>Runway ML</summary>
  RunwayML: 300,
  /// <summary>Kling AI (Kuaishou)</summary>
  KlingAI: 301,
  /// <summary>Luma AI (Dream Machine)</summary>
  LumaAI: 302,
  /// <summary>Pika Labs</summary>
  PikaLabs: 303,
  /// <summary>Google Veo</summary>
  GoogleVeo: 304,
  /// <summary>OpenAI Sora</summary>
  OpenAISora: 305,
  /// <summary>HeyGen - AI avatar video</summary>
  HeyGen: 306,
  /// <summary>Synthesia - AI avatar video</summary>
  Synthesia: 307
} as const

export type CAIProviderType = (typeof CAIProviderType)[keyof typeof CAIProviderType]

export interface CAIProviderTypeOption {
  id: CAIProviderType
  label: string
}

export const AI_PROVIDER_TYPE_OPTIONS: CAIProviderTypeOption[] = [
  // ===== LLM / Chat Providers =====
  { id: CAIProviderType.OpenAI, label: 'Open AI' },
  { id: CAIProviderType.Gemini, label: 'Gemini' },
  { id: CAIProviderType.Deepseek, label: 'Deepseek' },
  { id: CAIProviderType.Groq, label: 'Groq' },
  { id: CAIProviderType.XAI, label: 'X AI (Grok)' },
  { id: CAIProviderType.Anthropic, label: 'Anthropic (Claude)' },
  { id: CAIProviderType.Mistral, label: 'Mistral AI' },
  { id: CAIProviderType.Cohere, label: 'Cohere' },
  { id: CAIProviderType.Perplexity, label: 'Perplexity AI' },
  { id: CAIProviderType.TogetherAI, label: 'Together AI' },
  { id: CAIProviderType.OpenRouter, label: 'OpenRouter' },
  { id: CAIProviderType.AzureOpenAI, label: 'Azure OpenAI' },
  { id: CAIProviderType.AWSBedrock, label: 'AWS Bedrock' },
  { id: CAIProviderType.Ollama, label: 'Ollama' },
  { id: CAIProviderType.HuggingFace, label: 'Hugging Face' },
  { id: CAIProviderType.Qwen, label: 'Qwen (Alibaba)' },
  { id: CAIProviderType.ErnieBaidu, label: 'ERNIE (Baidu)' },
  { id: CAIProviderType.Moonshot, label: 'Moonshot (Kimi)' },
  { id: CAIProviderType.ZhipuAI, label: 'Zhipu AI (GLM)' },
  { id: CAIProviderType.MiniMax, label: 'MiniMax' },
  { id: CAIProviderType.FireworksAI, label: 'Fireworks AI' },
  { id: CAIProviderType.Replicate, label: 'Replicate' },

  // ===== Image Generation Providers =====
  { id: CAIProviderType.StabilityAI, label: 'Stability AI' },
  { id: CAIProviderType.BlackForestLabs, label: 'Black Forest Labs (Flux)' },
  { id: CAIProviderType.Ideogram, label: 'Ideogram AI' },
  { id: CAIProviderType.LeonardoAI, label: 'Leonardo AI' },
  { id: CAIProviderType.Recraft, label: 'Recraft AI' },

  // ===== Audio Providers =====
  { id: CAIProviderType.ElevenLabs, label: 'ElevenLabs' },
  { id: CAIProviderType.AssemblyAI, label: 'AssemblyAI' },
  { id: CAIProviderType.Deepgram, label: 'Deepgram' },
  { id: CAIProviderType.PlayHT, label: 'PlayHT' },

  // ===== Video Providers =====
  { id: CAIProviderType.RunwayML, label: 'Runway ML' },
  { id: CAIProviderType.KlingAI, label: 'Kling AI' },
  { id: CAIProviderType.LumaAI, label: 'Luma AI (Dream Machine)' },
  { id: CAIProviderType.PikaLabs, label: 'Pika Labs' },
  { id: CAIProviderType.GoogleVeo, label: 'Google Veo' },
  { id: CAIProviderType.OpenAISora, label: 'OpenAI Sora' },
  { id: CAIProviderType.HeyGen, label: 'HeyGen' },
  { id: CAIProviderType.Synthesia, label: 'Synthesia' },
]

export function getAIProviderTypeLabel(type: CAIProviderType): string | undefined {
  return AI_PROVIDER_TYPE_OPTIONS.find((o) => o.id === type)?.label;
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

export function getAIProviderTypeBadgeClass(type: number): string {
  return BADGE_COLORS[Math.abs(type) % BADGE_COLORS.length]
}
