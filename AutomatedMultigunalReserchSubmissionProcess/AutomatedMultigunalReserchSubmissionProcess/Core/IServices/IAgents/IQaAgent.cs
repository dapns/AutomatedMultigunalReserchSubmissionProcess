namespace AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents
{
    public interface IQnAAgent
    {
        Task<string> AskAsync(string question, string? submissionId = null);
    }
}
