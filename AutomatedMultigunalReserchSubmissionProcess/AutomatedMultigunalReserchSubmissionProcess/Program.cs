using AutomatedMultigunalReserchSubmissionProcess.Core.IServices;
using AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Plugins;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Services;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Services.Agents;
using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Microsoft.SemanticKernel;
using OpenAI.Embeddings;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

// Validate Azure OpenAI settings
var azureEndpoint = configuration["AzureOpenAI:Endpoint"];
var azureApiKey = configuration["AzureOpenAI:ApiKey"];
var chatDeployment = configuration["AzureOpenAI:DeploymentName"];          // GPT-4o
var embeddingDeployment = configuration["AzureOpenAI:EmbeddingDeploymentName"]; // text-embedding-3-large

if (string.IsNullOrWhiteSpace(azureEndpoint))
    throw new InvalidOperationException("AzureOpenAI:Endpoint is not configured.");
if (string.IsNullOrWhiteSpace(azureApiKey))
    throw new InvalidOperationException("AzureOpenAI:ApiKey is not configured.");
if (string.IsNullOrWhiteSpace(chatDeployment))
    throw new InvalidOperationException("AzureOpenAI:DeploymentName (chat) is not configured.");
if (string.IsNullOrWhiteSpace(embeddingDeployment))
    throw new InvalidOperationException("AzureOpenAI:EmbeddingDeploymentName is not configured.");

// Build Semantic Kernel with chat completion
var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddAzureOpenAIChatCompletion(chatDeployment, azureEndpoint, azureApiKey);
kernelBuilder.Plugins.AddFromType<ValidationPlugin>("Validation");
var kernel = kernelBuilder.Build();
services.AddSingleton(kernel);

// Create Azure OpenAI client for embeddings
var azureClient = new AzureOpenAIClient(new Uri(azureEndpoint), new AzureKeyCredential(azureApiKey));
var embeddingClient = azureClient.GetEmbeddingClient(embeddingDeployment);
services.AddSingleton(embeddingClient);

// Choose vector store: Azure Cognitive Search if configured, otherwise in-memory
var searchEndpoint = configuration["AzureSearch:Endpoint"];
var searchIndex = configuration["AzureSearch:IndexName"];
var searchApiKey = configuration["AzureSearch:ApiKey"];

if (!string.IsNullOrWhiteSpace(searchEndpoint) &&
    !string.IsNullOrWhiteSpace(searchIndex) &&
    !string.IsNullOrWhiteSpace(searchApiKey))
{
    services.AddSingleton(sp =>
        new SearchClient(new Uri(searchEndpoint), searchIndex, new AzureKeyCredential(searchApiKey)));
    services.AddSingleton<IVectorStore, AzureVectorStore>();
}
else
{
    services.AddSingleton<IVectorStore, InMemoryVectorStore>();
}

// Register controllers and API explorer
services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

// Register all agents and services
services.AddScoped<IIngestionAgent, IngestionAgent>();
services.AddScoped<IPreProcessAgent, PreProcessAgent>();
services.AddScoped<ITranslationAgent, TranslationAgent>();
services.AddScoped<IExtractionAgent, ExtractionAgent>();
services.AddScoped<IValidationAgent, ValidationAgent>();
services.AddScoped<ISummaryAgent, SummaryAgent>();
services.AddScoped<IRAGAgent, RagAgent>();
services.AddScoped<IQnAAgent, QnAAgent>();
services.AddScoped<IHumanFeedbackAgent, HumanFeedbackAgent>();
services.AddScoped<ProcessingService>();
services.AddSingleton<LoggingService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();