using Kolia.Thumbnail.API.Enums;

namespace Kolia.Thumbnail.API.Engines
{
    public abstract class AIEngine : IAIEngine
    {
        public abstract CAIProviderType ProviderType { get; }

        public abstract Task<AIBalanceInfo> GetAIBalanceInfoAsync(string apiKey);

        public abstract Task<List<AIModelInfo>> GetAIModelInfosAsync(string apiKey);

        public abstract Task<bool> ValidateApiKeyAsync(string apiKey);

        public abstract AIProviderCapabilities GetCapabilities();
    }
}