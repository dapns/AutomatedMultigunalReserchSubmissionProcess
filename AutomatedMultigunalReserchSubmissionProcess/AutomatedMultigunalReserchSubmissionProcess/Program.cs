using AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Plugins;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Services;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Services.Agents;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Configure Semantic Kernel
var azureConfig = builder.Configuration.GetSection("AzureOpenAI");
var kernelBuilder = Kernel.CreateBuilder();

// Add Azure OpenAI text generation
kernelBuilder.AddAzureOpenAIChatCompletion(
    deploymentName: azureConfig["DeploymentName"],
    endpoint: azureConfig["Endpoint"],
    apiKey: azureConfig["ApiKey"]
);

// Add embedding generation (for memory)
kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(
    deploymentName: azureConfig["EmbeddingDeploymentName"],
    endpoint: azureConfig["Endpoint"],
    apiKey: azureConfig["ApiKey"]
);

// Register plugins (native functions)
kernelBuilder.Plugins.AddFromType<ValidationPlugin>("Validation");

var kernel = kernelBuilder.Build();

// Register kernel as singleton
services.AddSingleton(kernel);

// Set up memory store (in-memory vector store)
var memoryStore = new VolatileMemoryStore();
var memory = new MemoryBuilder()
    .WithAzureOpenAITextEmbeddingGeneration(
        azureConfig["EmbeddingDeploymentName"],
        azureConfig["Endpoint"],
        azureConfig["ApiKey"])
    .WithMemoryStore(memoryStore)
    .Build();

{
    services.AddControllers();
    services.AddOpenApi();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

    services.AddSingleton<IMemoryStore>(memoryStore);
    services.AddSingleton<ISemanticTextMemory>(memory);

    // Register agents and services
    services.AddScoped<IIngestionAgent, IngestionAgent>();
    services.AddScoped<IPreProcessAgent, PreProcessAgent>();
    services.AddScoped<ITranslationAgent, TranslationAgent>();
    services.AddScoped<IExtractionAgent, ExtractionAgent>();
    services.AddScoped<IValidationAgent, ValidationAgent>();
    services.AddScoped<ISummaryAgent, SummaryAgent>();
    services.AddScoped<IRAGAgent, RAGAgent>();
    services.AddScoped<IQnAAgent, QnAAgent>();
    services.AddScoped<IHumanFeedbackAgent, HumanFeedbackAgent>();

    services.AddScoped<ProcessingService>();
    services.AddSingleton<LoggingService>(); // In-memory log
}

var app = builder.Build();
{
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}

