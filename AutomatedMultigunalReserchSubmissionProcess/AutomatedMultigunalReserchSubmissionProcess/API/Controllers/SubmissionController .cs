using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedMultigunalReserchSubmissionProcess.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubmissionController : ControllerBase
    {
        private readonly ProcessingService _orchestrator;
        private readonly ILogger<SubmissionController> _logger;

        public SubmissionController(ProcessingService orchestrator, ILogger<SubmissionController> logger)
        {
            _orchestrator = orchestrator;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadSubmission(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var submission = await _orchestrator.ProcessSubmissionAsync(file);
            return Ok(new { submission.Id, submission.FileName, submission.NeedsHumanReview });
        }

        [HttpGet("{id}")]
        public IActionResult GetSubmission(Guid id)
        {
            var sub = InMemoryStore.Submissions.FirstOrDefault(s => s.Id == id);
            if (sub == null) return NotFound();
            return Ok(sub);
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(InMemoryStore.Submissions);
    }
}
