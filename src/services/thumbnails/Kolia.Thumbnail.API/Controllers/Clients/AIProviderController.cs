using Kolia.Thumbnail.API.AIs;
using Kolia.Thumbnail.API.Models.AIs;
using Kolia.Thumbnail.API.Models.Commons;
using Microsoft.AspNetCore.Mvc;

namespace Kolia.Thumbnail.API.Controllers.Clients
{
    [ApiController]
    [Route("api/v1/ai-providers")]
    public class AIProviderController : ControllerBase
    {
        private readonly IAIProviderService _aiProviderService;
        private readonly ILogger<AIProviderController> _logger;

        public AIProviderController(
            IAIProviderService aiProviderService,
            ILogger<AIProviderController> logger)
        {
            _aiProviderService = aiProviderService;
            _logger = logger;
        }

        [HttpGet("paging")]
        public async Task<IActionResult> GetPagingAsync(
            [FromQuery]PagedRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _aiProviderService.GetWithPagingAsync(request, cancellationToken);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(
            [FromBody]AIProviderCreateDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _aiProviderService.CreateAsync(request, cancellationToken);

            return Ok(result);
        }
    }
}