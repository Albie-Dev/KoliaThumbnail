using System.Text.Json;
using Kolia.Thumbnail.API.Services.News;
using Microsoft.AspNetCore.Mvc;

namespace Kolia.Thumbnail.API.Controllers.Clients
{
    /// <summary>
    /// SSE endpoint cho FE stream log realtime khi search / deep-analyze đang chạy.
    /// </summary>
    [ApiController]
    [Route("api/v1/progress")]
    public class ProgressController : ControllerBase
    {
        private readonly OperationProgressStore _store;

        public ProgressController(OperationProgressStore store)
        {
            _store = store;
        }

        /// <summary>
        /// Lấy trạng thái hiện tại của 1 operation (dùng cho polling, không SSE).
        /// </summary>
        [HttpGet("{operationId:guid}")]
        public IActionResult GetProgress(Guid operationId)
        {
            var progress = _store.Get(operationId);
            if (progress == null)
                return NotFound(new { error = "Operation not found" });

            return Ok(new
            {
                progress.OperationId,
                progress.Title,
                progress.Status,
                progress.ErrorMessage,
                progress.Logs,
                progress.StartedAt,
                progress.LastUpdated
            });
        }

        /// <summary>
        /// SSE stream: client dùng EventSource để nhận log realtime.
        /// </summary>
        [HttpGet("{operationId:guid}/stream")]
        public async Task StreamProgress(Guid operationId, CancellationToken ct)
        {
            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";
            Response.Headers["X-Accel-Buffering"] = "no";

            var lastLogCount = 0;
            var waitStart = DateTimeOffset.UtcNow;

            while (!ct.IsCancellationRequested)
            {
                var progress = _store.Get(operationId);

                // Nếu chưa có progress entry, đợi tối đa 10s (search API chưa kịp tạo)
                if (progress == null)
                {
                    if ((DateTimeOffset.UtcNow - waitStart).TotalSeconds > 10)
                    {
                        await Response.WriteAsync("event: error\ndata: Operation not found or timed out\n\n", ct);
                        await Response.Body.FlushAsync(ct);
                        break;
                    }
                    await Response.WriteAsync("data: \n\n", ct); // keep-alive
                    await Response.Body.FlushAsync(ct);
                    await Task.Delay(300, ct);
                    continue;
                }

                // Gửi các log mới
                for (var i = lastLogCount; i < progress.Logs.Count; i++)
                {
                    var logJson = JsonSerializer.Serialize(progress.Logs[i]);
                    await Response.WriteAsync($"data: {logJson}\n\n", ct);
                    lastLogCount++;
                }

                // Gửi status event khi hoàn thành
                if (progress.Status != "running")
                {
                    var statusPayload = JsonSerializer.Serialize(new
                    {
                        type = "status",
                        progress.Status,
                        progress.ErrorMessage
                    });
                    await Response.WriteAsync($"event: done\ndata: {statusPayload}\n\n", ct);
                    await Response.Body.FlushAsync(ct);
                    break;
                }

                await Response.WriteAsync("data: \n\n", ct); // keep-alive
                await Response.Body.FlushAsync(ct);
                await Task.Delay(500, ct);
            }
        }
    }
}
