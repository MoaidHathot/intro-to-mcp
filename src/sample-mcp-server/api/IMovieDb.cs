public interface IMovieDb
{
	public Task<Movie> GetMovieByIdAsync(string apiKey, string id, Type? type = null, int? year = null, Plot plot = Plot.Short, ReturnType returnType = ReturnType.Json);
	public Task<Movie> GetMovieByNameAsync(string apiKey, string name, Type? type = null, int? year = null, Plot plot = Plot.Short, ReturnType returnType = ReturnType.Json);
	public Task<SeasonInfo> GetSeasonByMovieNameAsync(string apiKey, string movieName, int season, int? year = null, ReturnType returnType = ReturnType.Json);
	public Task<SeasonInfo> GetSeasonByMovieIdAsync(string apiKey, string movieIdd, int season, int? year = null, ReturnType returnType = ReturnType.Json);

	public Task<SearchResult> SearchAsync(string apiKey, string query, Type? type = null, int? year = null, ReturnType returnType = ReturnType.Json, int? page = null);
}
