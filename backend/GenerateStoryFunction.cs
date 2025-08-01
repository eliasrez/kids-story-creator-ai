using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.AI.OpenAI;
using Azure;

public static class GenerateStoryFunction
{
    [FunctionName("GenerateStoryFunction")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        string category = req.Form["category"];
        string ideas = req.Form["ideas"];

        string systemPrompt = $@"
You are a kids’ story writer. Always write creative, age-appropriate, and fun stories for children aged 6–10.
Never include anything scary, violent, or inappropriate. The child wants a story in the '{category}' category.
They described their characters and ideas as follows: {ideas}.
The story should be short (~300 words), exciting, and use simple language.";

        var client = new OpenAIClient(new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")),
            new AzureKeyCredential(Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY")));

        var response = await client.GetChatCompletionsAsync(
            deploymentOrModelName: "gpt-4",
            new ChatCompletionsOptions
            {
                Messages =
                {
                    new ChatMessage(ChatRole.System, systemPrompt),
                    new ChatMessage(ChatRole.User, "Please write the story now.")
                }
            });

        string storyText = response.Value.Choices[0].Message.Content;

        var imagePrompt = $"An illustration for a kids story in the category '{category}' with these ideas: {ideas}";

        var imageResponse = await client.GetImageGenerationsAsync(new ImageGenerationOptions()
        {
            Prompt = imagePrompt,
            DeploymentName = "dall-e-3",
            Size = ImageSize.Size512x512
        });

        string imageUrl = imageResponse.Value.Data[0].Url;

        return new OkObjectResult(new { story = storyText, illustration = imageUrl });
    }
}