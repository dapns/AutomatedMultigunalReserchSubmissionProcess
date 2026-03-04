using AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;

namespace AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Services
{
    public class ProcessingService
    {
        private readonly IIngestionAgent _ingestion;
        private readonly IPreProcessAgent _preProcess;
        private readonly ITranslationAgent _translation;
        private readonly IExtractionAgent _extraction;
        private readonly IValidationAgent _validation;
        private readonly ISummaryAgent _summary;
        private readonly IRAGAgent _rag;
        private readonly IHumanFeedbackAgent _humanFeedback;
        private readonly ILogger<ProcessingService> _logger;

        public ProcessingService(
            IIngestionAgent ingestion,
            IPreProcessAgent preProcess,
            ITranslationAgent translation,
            IExtractionAgent extraction,
            IValidationAgent validation,
            ISummaryAgent summary,
            IRAGAgent rag,
            IHumanFeedbackAgent humanFeedback,
            ILogger<ProcessingOrchestrator> logger)
        {
            _ingestion = ingestion;
            _preProcess = preProcess;
            _translation = translation;
            _extraction = extraction;
            _validation = validation;
            _summary = summary;
            _rag = rag;
            _humanFeedback = humanFeedback;
            _logger = logger;
        }

        public async Task<Submission> ProcessSubmissionAsync(IFormFile file)
        {
            // Step 1: Ingest
            var submission = await _ingestion.IngestAsync(file);

            // Step 2: Pre-process (language detection, OCR, text extraction)
            submission = await _preProcess.PreProcessAsync(submission);

            // Step 3: Translate if needed
            submission = await _translation.TranslateAsync(submission);

            // Step 4: Extract structured info
            var extracted = await _extraction.ExtractAsync(submission.EnglishTranslation);
            submission.ExtractedInfo = extracted;

            // Step 5: Validate
            var validation = await _validation.ValidateAsync(extracted, submission.EnglishTranslation);
            submission.ValidationResult = validation;

            // Step 6: Check if human review needed
            submission.NeedsHumanReview = await _humanFeedback.NeedsReviewAsync(submission);

            // Step 7: Generate summary
            var summary = await _summary.GenerateSummaryAsync(submission);
            submission.Summary = summary;

            // Step 8: Index in RAG
            await _rag.IndexSubmissionAsync(submission);

            _logger.LogInformation("Processing complete for submission {Id}", submission.Id);
            return submission;
        }
    }
}
