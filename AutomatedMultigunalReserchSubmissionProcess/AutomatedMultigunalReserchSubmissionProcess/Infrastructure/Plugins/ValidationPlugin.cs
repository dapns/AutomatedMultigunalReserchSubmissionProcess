using Microsoft.SemanticKernel;

namespace AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Plugins
{
    public class ValidationPlugin
    {
        [KernelFunction("check_page_count")]
        public string CheckPageCount(int pageCount)
        {
            if (pageCount < 8) return "Page count is less than minimum (8).";
            if (pageCount > 25) return "Page count exceeds maximum (25).";
            return "Page count OK";
        }

        [KernelFunction("check_sections_present")]
        public string CheckSectionsPresent(string sections)
        {
            var required = new[] { "title", "abstract", "keywords", "author", "references" };
            var missing = required.Where(r => !sections.Contains(r, StringComparison.OrdinalIgnoreCase)).ToList();
            return missing.Any() ? $"Missing sections: {string.Join(", ", missing)}" : "All required sections present";
        }
    }
}
