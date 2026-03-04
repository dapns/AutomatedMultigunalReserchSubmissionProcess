using AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedMultigunalReserchSubmissionProcess.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IHumanFeedbackAgent _humanFeedback;
        private readonly LoggingService _logging;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IHumanFeedbackAgent humanFeedback, LoggingService logging, ILogger<AdminController> logger)
        {
            _humanFeedback = humanFeedback;
            _logging = logging;
            _logger = logger;
        }

        [HttpGet("pending")]
        public IActionResult GetPendingReviews()
        {
            var pending = InMemoryStore.Submissions.Where(s => s.NeedsHumanReview).ToList();
            return Ok(pending);
        }

        [HttpPost("correct")]
        public async Task<IActionResult> ApplyCorrection([FromBody] CorrectionRequest request)
        {
            await _humanFeedback.ApplyCorrectionAsync(request.SubmissionId, request.Field, request.CorrectedValue);
            _logging.Log("admin", "Correction", $"Submission {request.SubmissionId} field {request.Field} corrected.");
            return Ok();
        }

        [HttpGet("logs")]
        public IActionResult GetLogs() => Ok(InMemoryStore.Logs);

        public class CorrectionRequest
        {
            public Guid SubmissionId { get; set; }
            public string Field { get; set; }
            public string CorrectedValue { get; set; }
        }
    }
}
