namespace AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models
{
    // Vector record abstraction
    public class VectorRecord
    {
        public string Id { get; set; } = string.Empty;
        public IList<float> Vector { get; set; } = new List<float>();
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
