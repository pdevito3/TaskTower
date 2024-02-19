namespace TaskTowerSandbox.Sandboxing;

using System.Text.Json;

public record Pokemon(string name);
public class PokeApiService(IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("PokeAPI");

    public async Task<(int, string)> GetRandomPokemonAsync()
    {
        var randomPokemonId = new Random().Next(1, 899);
        var response = await _httpClient.GetAsync($"pokemon/{randomPokemonId}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var pokemon = JsonSerializer.Deserialize<Pokemon>(content);
            return (randomPokemonId, pokemon.name);
        }

        return (randomPokemonId, "Pokemon not found");
    }
}