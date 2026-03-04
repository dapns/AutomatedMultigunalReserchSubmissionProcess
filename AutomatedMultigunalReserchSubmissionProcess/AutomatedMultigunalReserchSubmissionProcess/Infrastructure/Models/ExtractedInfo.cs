namespace AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models
{
    public class ExtractedInfo
    {
        public string Title { get; set; }
        public List<string> Authors { get; set; } = new();
        public List<string> Affiliations { get; set; } = new();
        public string Abstract { get; set; }
        public List<string> Keywords { get; set; } = new();
        public List<string> Sections { get; set; } = new();
        public List<string> References { get; set; } = new();
        public int PageCount { get; set; }
        public double PlagiarismScore { get; set; }
    }
}
