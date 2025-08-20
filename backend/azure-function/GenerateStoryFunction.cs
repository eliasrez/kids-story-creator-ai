using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OpenAI.Images;

namespace KidsStoryCreator;

public class GenerateStoryFunction
{
    private readonly ILogger _logger;
    private readonly AzureOpenAIClient _openAiClient;
    private const string ChatDeploymentName = "gpt-4";     // Update to your deployment name
    private const string DalleDeploymentName = "dall-e-3"; // Update to your DALL·E deployment
    private readonly string templatePath; 

    public GenerateStoryFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<GenerateStoryFunction>();

        // Initialize OpenAIClient (replace with your actual endpoint and key)
        var endpoint = new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!);
        var key = new AzureKeyCredential(Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY")!);
        _openAiClient = new AzureOpenAIClient(endpoint, key);

        templatePath = Path.Combine(AppContext.BaseDirectory, "prompts/story-template.txt");
    }

    [Function("GenerateStoryFunction")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        _logger.LogInformation("GenerateStoryFunction triggered.");

        using var reader = new StreamReader(req.Body);
        var requestBody = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(requestBody))
        {
            var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await badResponse.WriteStringAsync("Request body is empty.");
            return badResponse;
        }

        _logger.LogInformation(requestBody);
        var jsonDoc = JsonDocument.Parse(requestBody);
        //string json = jsonDoc.RootElement.ToString();
        // _logger.LogInformation(json);

        string? userPrompt = null;
        if (jsonDoc.RootElement.TryGetProperty("ideas", out JsonElement promptElement))
        {
            userPrompt = promptElement.ValueKind != JsonValueKind.Null
                           ? promptElement.GetString()
                           : "";
        }

        string? userCategory = null;
        if (jsonDoc.RootElement.TryGetProperty("category", out JsonElement categoryElement))
        {
            userCategory = categoryElement.ValueKind != JsonValueKind.Null
                           ? categoryElement.GetString()
                           : "";
        }

        string? illustrationDescription = null;
        if (jsonDoc.RootElement.TryGetProperty("drawing_description", out JsonElement drawingDescriptionElement))
        {
            illustrationDescription = drawingDescriptionElement.ValueKind != JsonValueKind.Null
                           ? drawingDescriptionElement.GetString()
                           : "";
        }
        var finalPrompt = "";

        _logger.LogInformation(userPrompt);

        if (string.IsNullOrWhiteSpace(userPrompt) || string.IsNullOrWhiteSpace(userCategory))
        {
            var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await badResponse.WriteStringAsync("Missing 'prompt' or 'category' in request.");
            return badResponse;
        }

        try
        {
            // Read the template file
            var templateContent = await File.ReadAllTextAsync(templatePath);

            // Replace the placeholders
            finalPrompt = templateContent
                .Replace("{category}", userCategory)
                .Replace("{ideas}", userPrompt)
                .Replace("{drawing_description}", illustrationDescription);

            _logger.LogInformation("Final Prompt: {finalPrompt}", finalPrompt);


        }
        catch (FileNotFoundException)
        {
            _logger.LogError($"Template file not found at: {templatePath}");
            var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("The story template file could not be found.");
            return errorResponse;
        }


        // Get a ChatClient for your specific deployment
        ChatClient chatClient = _openAiClient.GetChatClient(ChatDeploymentName);

        // Prepare chat messages
        List<ChatMessage> messages = new()
        {
            new SystemChatMessage("You are a creative assistant helping children write fun and imaginative stories."),
            new UserChatMessage(finalPrompt!)
        };


        try
        {
            // Complete the chat
            ChatCompletion completion = await chatClient.CompleteChatAsync(messages);

            // Extract and display the assistant's response
            string generatedStory = completion.Content.Last().Text;
            Console.WriteLine($"Story Returned: {generatedStory}");


            var imagePrompt = $"Illustrate this children's story: {generatedStory.Substring(0, Math.Min(300, generatedStory.Length))}";
            var imageClient = _openAiClient.GetImageClient(DalleDeploymentName);


            var imageGeneration = await imageClient.GenerateImageAsync(
                    imagePrompt,
                    options: new ImageGenerationOptions()
                    {
                        Size = GeneratedImageSize.W1024xH1024,
                        Style = GeneratedImageStyle.Vivid,
                        Quality = GeneratedImageQuality.High
                    }
                );

            Console.WriteLine(imageGeneration.Value.ImageUri);
            var imageUrl = imageGeneration.Value?.ImageUri;

            // === 3. Return JSON response ===
            var result = new
            {
                story = generatedStory.Trim(),
                illustration = imageUrl
            };

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonSerializer.Serialize(result));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate story from OpenAI.");
            var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("Failed to generate story.");
            return errorResponse;
        }
    }
}
