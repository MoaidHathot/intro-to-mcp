using System.ComponentModel;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

[McpServerToolType]
public sealed class MovieTools
{
	private static string ApiKey => Environment.GetEnvironmentVariable("OMDB_API_KEY") ?? throw new InvalidOperationException("OMDB_API_KEY environment variable is not set.");

	[McpServerTool, Description("Get a movie, an show or an episode by id. If Year is unknown provide 0 for year. If Type is unknown, provide -1 for type.")]
	public static async Task<Movie> GetMovieById(IMovieDb movieDb, string id, Type? type = null, int? yearOfMovie = null, Plot plot = Plot.Full, ReturnType returnType = ReturnType.Json)
		=> await movieDb.GetMovieByIdAsync(ApiKey, id, type is Type.Unkown ? null : type, yearOfMovie is 0 ? null : yearOfMovie, plot, returnType);

	[McpServerTool, Description("Get a movie or a show by name. If Year is unknown provide 0 for year. If Type is unknown, provide -1 for type.")]
	public static async Task<Movie> GetMovieByTitle(IMovieDb movieDb, string name, Type? type = null, int? yearOfMovie = null, Plot plot = Plot.Full, ReturnType returnType = ReturnType.Json)
		=> await movieDb.GetMovieByNameAsync(ApiKey, name, type is Type.Unkown ? null : type, yearOfMovie is 0 ? null : yearOfMovie, plot, returnType);

	[McpServerTool, Description("Get info about a season of a show name and season number. If Year is unknown provide 0 for year")]
	public static async Task<SeasonInfo> GetSeasonByMovieName(IMovieDb movieDb, string titleName, int season, int? yearOfMovie = null, ReturnType returnType = ReturnType.Json)
		=> await movieDb.GetSeasonByMovieNameAsync(ApiKey, titleName, season, yearOfMovie is 0 ? null : yearOfMovie, returnType);

	[McpServerTool, Description("Get info about a season of a show by show id and season number. If Year is unknown provide 0 for year")]
	public static async Task<SeasonInfo> GetSeasonByMovieId(IMovieDb movieDb, string titleId, int season, int? yearOfMovie = null, ReturnType returnType = ReturnType.Json)
		=> await movieDb.GetSeasonByMovieIdAsync(ApiKey, titleId, season, yearOfMovie is 0 ? null : yearOfMovie, returnType);

	[McpServerTool, Description("Search for movies or shows a query. ReturnType 0 = json, 1 = xml. Type -1 = Unknown/unspecified, 0 = Movie, 1 = series/show, 2 = Episode. If user didn't specify year/date use 0 for date, the same regarding type.")]
	public static async Task<SearchResult> SearchMovies(IMovieDb movieDb, string query, Type? type = null, int? yearOfMovie = null, ReturnType returnType = ReturnType.Json)
		=> await movieDb.SearchAsync(ApiKey, query, type is Type.Unkown ? null : type, yearOfMovie is 0 ? null : yearOfMovie, returnType);

	[McpServerTool, Description("Suggests to the user what to watch next based on the user's query and prompt")]
    public static async Task<string> MovieSelector(IMcpServer llm, IMovieDb movieDb, [Description("The user query containing names of movies or shows the user is considering")] string query, CancellationToken cancellationToken)
    {
		var rules = """
			According to the prompt containing the movies or shows they are considering to watch
			If the list contain movies, get the rating of all movies and suggest the highest rated movie to show. Add a Summary about the selected movie.
			If the list contains shows, get the rating of all shows and suggest the highest rated show, but also get the ratings of all seasons and episodes of that show and suggest the highest rated season and episodes to watch first. Add a Summary about the selected show.
			If both movies and shows are preset, show results for both movies and shows.
		""";

        ChatMessage[] messages =
        [
            new(ChatRole.User, "You are a helpful assistant that suggests movies or shows to watch based on the user's prompt, based on the following rules: " + rules),
            new(ChatRole.User, $"User Prompt: {query}"),
        ];


        var samplingResponse = await llm.AsSamplingChatClient().GetResponseAsync(messages, options: null, cancellationToken);

		return samplingResponse.ToString();
    }
}
