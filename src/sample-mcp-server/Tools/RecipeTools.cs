using System.ComponentModel;
using ModelContextProtocol.Server;

public class RecipeTools
{
	private const string RecipeLocation = @"C:\Users\moaid\OneDrive\Documents\Obsidian Vault\Cooking";

    [McpServerTool]
    [Description("Returns a recipe according to the recipe or food name")]
    public object? GetRecipe([Description("The name of the recipe or the food required")] string recipeName)
    {
		var files = System.IO.Directory.EnumerateFiles(RecipeLocation, "*.md", System.IO.SearchOption.AllDirectories)
			.Select(filePath => (name: filePath, content: System.IO.File.ReadAllText(filePath)))
			.Where(tuple  => tuple.name.Contains(recipeName, StringComparison.OrdinalIgnoreCase) || tuple.content.Contains(recipeName, StringComparison.OrdinalIgnoreCase))
			.Select(tuple => new { 
				Name = System.IO.Path.GetFileNameWithoutExtension(tuple.name), 
				Content = tuple.content 
			});

		var file = files.FirstOrDefault();
		return file;
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
