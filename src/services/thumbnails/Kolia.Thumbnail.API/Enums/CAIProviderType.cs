namespace Kolia.Thumbnail.API.Enums
{
    public enum CAIProviderType
    {
        /// <summary>
        /// Mặc định hệ thống
        /// </summary>
        System = 0,

        // ===== LLM / Chat Providers =====
        /// <summary>Open AI (GPT-4o, GPT-4.1, o-series...)</summary>
        OpenAI = 1,
        /// <summary>Google Gemini</summary>
        Gemini = 2,
        /// <summary>Deepseek</summary>
        Deepseek = 3,
        /// <summary>Groq (LPU inference - Llama, Mixtral...)</summary>
        Groq = 4,
        /// <summary>X AI (Grok)</summary>
        XAI = 5,
        /// <summary>Anthropic (Claude)</summary>
        Anthropic = 6,
        /// <summary>Mistral AI</summary>
        Mistral = 7,
        /// <summary>Cohere (Command R+)</summary>
        Cohere = 8,
        /// <summary>Perplexity AI</summary>
        Perplexity = 9,
        /// <summary>Together AI (host nhiều open-source model)</summary>
        TogetherAI = 10,
        /// <summary>OpenRouter (gateway tổng hợp nhiều model)</summary>
        OpenRouter = 11,
        /// <summary>Azure OpenAI Service</summary>
        AzureOpenAI = 12,
        /// <summary>AWS Bedrock</summary>
        AWSBedrock = 13,
        /// <summary>Ollama (chạy model local)</summary>
        Ollama = 14,
        /// <summary>Hugging Face Inference API</summary>
        HuggingFace = 15,
        /// <summary>Alibaba Qwen</summary>
        Qwen = 16,
        /// <summary>Baidu ERNIE</summary>
        ErnieBaidu = 17,
        /// <summary>Moonshot AI (Kimi)</summary>
        Moonshot = 18,
        /// <summary>Zhipu AI (GLM)</summary>
        ZhipuAI = 19,
        /// <summary>MiniMax</summary>
        MiniMax = 20,
        /// <summary>Fireworks AI</summary>
        FireworksAI = 21,
        /// <summary>Replicate (host model theo container)</summary>
        Replicate = 22,

        // ===== Image Generation Providers =====
        /// <summary>Stability AI (Stable Diffusion, SD3)</summary>
        StabilityAI = 100,
        /// <summary>Black Forest Labs (Flux)</summary>
        BlackForestLabs = 101,
        /// <summary>Ideogram AI</summary>
        Ideogram = 102,
        /// <summary>Leonardo AI</summary>
        LeonardoAI = 103,
        /// <summary>Recraft AI</summary>
        Recraft = 104,

        // ===== Audio Providers =====
        /// <summary>ElevenLabs - TTS chất lượng cao, clone giọng</summary>
        ElevenLabs = 200,
        /// <summary>AssemblyAI - Speech to text, phụ đề</summary>
        AssemblyAI = 201,
        /// <summary>Deepgram - Speech to text realtime</summary>
        Deepgram = 202,
        /// <summary>PlayHT - Text to speech</summary>
        PlayHT = 203,

        // ===== Video Providers =====
        /// <summary>Runway ML</summary>
        RunwayML = 300,
        /// <summary>Kling AI (Kuaishou)</summary>
        KlingAI = 301,
        /// <summary>Luma AI (Dream Machine)</summary>
        LumaAI = 302,
        /// <summary>Pika Labs</summary>
        PikaLabs = 303,
        /// <summary>Google Veo</summary>
        GoogleVeo = 304,
        /// <summary>OpenAI Sora</summary>
        OpenAISora = 305,
        /// <summary>HeyGen - AI avatar video</summary>
        HeyGen = 306,
        /// <summary>Synthesia - AI avatar video</summary>
        Synthesia = 307
    }
}