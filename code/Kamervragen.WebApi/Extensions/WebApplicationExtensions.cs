using Azure.Identity;
using Azure.Search.Documents.Indexes;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel;

namespace WebApi.Extensions
{
    public static class WebApplicationBuilderExtensions
    {
        public static void AddSemanticKernel(this WebApplicationBuilder builder, DefaultAzureCredential azureCredential, HttpClient? httpClient)
        {
            var searchConfig = builder.Configuration.GetSection("Search");
            var openAIConfig = builder.Configuration.GetSection("OpenAI");

            if (searchConfig != null && openAIConfig != null)
            {
                Uri serviceUri = new Uri(searchConfig["EndPoint"]);
                string? indexName = searchConfig["IndexName"];
                string? endpoint = openAIConfig["EndPoint"];
                string? completionModel = openAIConfig["CompletionModel"];
                string? embeddingModel = openAIConfig["EmbeddingModel"];
                string? apiKey = openAIConfig["ApiKey"];

                var kernelBuilder = Kernel.CreateBuilder();
                if (apiKey == null)
                    kernelBuilder.AddAzureOpenAIChatCompletion(completionModel, endpoint, azureCredential);
                else
                    kernelBuilder.AddAzureOpenAIChatCompletion(completionModel, endpoint, apiKey);

                var kernel = kernelBuilder.Build();
                builder.Services.AddSingleton(kernel);
            }
        }
    }
}
