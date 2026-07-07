using Kolia.Thumbnail.API.AIs;
using Microsoft.AspNetCore.Mvc;

namespace Kolia.Thumbnail.API.Controllers.Admins
{
    public class AdminAIProviderController : ControllerBase
    {
        private readonly IAIProviderService _aiProviderService;
        private readonly ILogger<AdminAIProviderController> _logger;

        public AdminAIProviderController(
            IAIProviderService aiProviderService,
            ILogger<AdminAIProviderController> logger)
        {
            _aiProviderService = aiProviderService;
            _logger = logger;
        }

        
    }
}