using System.ComponentModel;
using Dumpify;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

public class RecipeTools
{
	private const string RecipeLocation = @"C:\Users\moaid\OneDrive\Documents\Obsidian Vault\Cooking";

    [McpServerTool]
    [Description("Returns a recipe according to the recipe or food name")]
    public async Task<object?> GetRecipe(IMcpServer server, [Description("The name of the recipe or the food required")] string recipeName)
    {
		var files = System.IO.Directory.EnumerateFiles(RecipeLocation, "*.md", System.IO.SearchOption.AllDirectories)
			.Select(filePath => (name: filePath, content: System.IO.File.ReadAllText(filePath)))
			.Where(tuple  => tuple.name.Contains(recipeName, StringComparison.OrdinalIgnoreCase) || tuple.content.Contains(recipeName, StringComparison.OrdinalIgnoreCase))
			.Select(tuple => new { 
				Name = System.IO.Path.GetFileNameWithoutExtension(tuple.name), 
				Content = tuple.content 
			});

		var file = files.FirstOrDefault();

		Console.Error.WriteLine($"Requesting recipe for: {file?.Content}");
		if(file is null)
		{
			return null;
		}

		var args = new CreateMessageRequestParams
        {
            Messages = [new SamplingMessage
                {
                    Role = Role.User,
                    Content = new TextContentBlock { Text = $"Make the following recipe only use pounds, ounces, ferenhite and only american units {file.Content}" },
                }],
				MaxTokens = 10000,
            SystemPrompt = "You are a helpful test server.",
        };


		var result = await server.SampleAsync(args);
		Console.Error.WriteLine(result.DumpText());
		return (result.Content as TextContentBlock)?.Text ?? "No content returned from the model";
    }

	[McpServerTool]
    [Description("Returns a recipe according to the recipe or food name")]
    public object[] GetAllRecipes()
    {
		var files = System.IO.Directory.EnumerateFiles(RecipeLocation, "*.md", System.IO.SearchOption.AllDirectories)
			.Select(filePath => (name: filePath, content: System.IO.File.ReadAllText(filePath)))
			.Select(tuple => new { 
				Name = System.IO.Path.GetFileNameWithoutExtension(tuple.name), 
				Content = tuple.content 
			});

		return files.ToArray();
    }
}
