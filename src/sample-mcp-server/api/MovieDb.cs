using System.Net.Http.Json;
using Dumpify;
using Microsoft.AspNetCore.WebUtilities;

public sealed class MovieDb : IMovieDb, IDisposable
{
	private readonly HttpClient _httpClient;

	public MovieDb(IHttpClientFactory httpClientFactory)
	{
		_httpClient = httpClientFactory.CreateClient(nameof(MovieDb));
		_httpClient.BaseAddress = new Uri("http://www.omdbapi.com");
	}

	public async Task<Movie> GetMovieByIdAsync(string apiKey, string id, Type? type = null, int? year = null, Plot plot = Plot.Short, ReturnType returnType = ReturnType.Json)
	{
		var args = BuildUrl(apiKey: apiKey, id: id, type: type, year: year, plot: plot, returnType: returnType);

		return (await _httpClient.GetFromJsonAsync<Movie>(args))!;
	}

	public async Task<Movie> GetMovieByNameAsync(string apiKey, string name, Type? type = null, int? year = null, Plot plot = Plot.Short, ReturnType returnType = ReturnType.Json)
	{
		var args = BuildUrl(apiKey: apiKey, title: name, type: type, year: year, plot: plot, returnType: returnType).Dump();

		return (await _httpClient.GetFromJsonAsync<Movie>(args))!;
	}

	public async Task<SeasonInfo> GetSeasonByMovieNameAsync(string apiKey, string movieName, int season, int? year = null, ReturnType returnType = ReturnType.Json)
	{
		var args = BuildUrl(apiKey: apiKey, title: movieName, type: null, year: year, returnType: returnType, season: season);

		return (await _httpClient.GetFromJsonAsync<SeasonInfo>(args))!;
	}

	public async Task<SeasonInfo> GetSeasonByMovieIdAsync(string apiKey, string movieId, int season, int? year = null, ReturnType returnType = ReturnType.Json)
	{
		var args = BuildUrl(apiKey: apiKey, title: movieId, type: null, year: year, returnType: returnType, season: season);

		return (await _httpClient.GetFromJsonAsync<SeasonInfo>(args))!;
	}

	public async Task<SearchResult> SearchAsync(string apiKey, string query, Type? type = null, int? year = null, ReturnType returnType = ReturnType.Json, int? page = null)
	{
		var result = await FetchItemsAsync(apiKey, query, type, year, returnType, page ?? 1);

		if(!string.Equals(result.Response, "True", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrWhiteSpace(result.Error))
		{
			return result;
		}

		if(page is null && result.Search?.Length < result.TotalResults)
		{
			var list = new List<SearchResultItem>(result.TotalResults.Value);
			list.AddRange(result.Search ?? Array.Empty<SearchResultItem>());

			for(int currentPage = 2; list.Count < result.TotalResults; ++currentPage)
			{
				var nextPage = await FetchItemsAsync(apiKey, query, type, year, returnType, page: currentPage);
				if(!string.Equals(nextPage.Response, "True", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrWhiteSpace(nextPage.Error))
				{
					break;
				}

				list.AddRange(nextPage.Search ?? Array.Empty<SearchResultItem>());
			}

			result.Search = list.ToArray();
		}

		return result;
	}

	private async Task<SearchResult> FetchItemsAsync(string apiKey, string query, Type? type = null, int? year = null, ReturnType returnType = ReturnType.Json, int page = 1, int? season = null)
	{
		var args = BuildUrl(apiKey: apiKey, query: query, type: type, year: year, returnType: returnType, page: page, season: season);

		return (await _httpClient.GetFromJsonAsync<SearchResult>(args))!;
	}

	private static string BuildUrl(string apiKey, string? id = default, string? title = default, string? query = default, Type? type = default, int? year = default, Plot? plot = default, ReturnType? returnType = default, int? page = null, int? season = null)
	{
		return QueryHelpers.AddQueryString("", GetQueryParams(apiKey, id, title, query, type, year, plot, returnType, page, season));

		static IEnumerable<KeyValuePair<string, string?>> GetQueryParams(string apiKey, string? id = default, string? title = default, string? query = default ,Type? type = default, int? year = default, Plot? plot = default, ReturnType? returnType = default, int? page = null, int? season = null)
		{
			yield return new("apikey", apiKey);

			if (id is not null)
			{
				yield return new ("i", id);
			}

			if (query is not null)
			{
				yield return new ("s", query);
			}

			if (title is not null) 
			{
				yield return new ("t", title);
			}

			if (season is not null) 
			{
				yield return new ("season", season.ToString());
			}

			if (page is not null) 
			{
				yield return new ("page", page.ToString());
			}

			if (type is not null) 
			{
				yield return new ("type", type.ToString()!.ToLowerInvariant());
			}

			if (year is not null)
			{
				yield return new ("y", year.Value.ToString());
			}

			if (plot is not null) 
			{
				yield return new ("plot", plot.ToString()!.ToLowerInvariant());
			}

			if (returnType is not null) 
			{
				yield return new ("r", returnType.ToString()!.ToLowerInvariant());
			}
		}
	}

	public void Dispose()
	{
	}
}
