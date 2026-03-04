using AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedMultigunalReserchSubmissionProcess.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QnAController : ControllerBase
    {
        private readonly IQnAAgent _qnaAgent;

        public QnAController(IQnAAgent qnaAgent)
        {
            _qnaAgent = qnaAgent;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] QnARequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest("Question is required.");

            var answer = await _qnaAgent.AskAsync(request.Question, request.SubmissionId);
            return Ok(new { request.Question, Answer = answer });
        }

        public class QnARequest
        {
            public string Question { get; set; }
            public string? SubmissionId { get; set; }
        }
    }
}
