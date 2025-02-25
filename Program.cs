using HotelAppSearchApi.Models;
using Nest;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/search", async (string? city, int? rating) =>
{
    var host = Environment.GetEnvironmentVariable("host");
    var userName = Environment.GetEnvironmentVariable("userName");
    var password = Environment.GetEnvironmentVariable("password");
    var indexName = Environment.GetEnvironmentVariable("event");
    
    var connSettings = new ConnectionSettings(new Uri(host));
    connSettings.BasicAuthentication(userName, password);
    connSettings.DefaultIndex(indexName);
    connSettings.DefaultMappingFor<Hotel>(m => m.IdProperty(p => p.Id));
    
    var esClient = new Nest.ElasticClient(connSettings);

    // Match - Prefix - Range - Fuzzy Match
    ISearchResponse<Hotel> result;
    
    if (city is null)
    {
        result = await esClient.SearchAsync<Hotel>(s => s.Query(q =>
            q.MatchAll() && // return all docs
            q.Range(r => r.Field(f => f.Rating).GreaterThanOrEquals(rating))
        ));
    }
    else
    {
        result = await esClient.SearchAsync<Hotel>(s => s.Query(q => 
            q.Prefix(p => p.Field(f => f.CityName).Value(city).CaseInsensitive()) &&
            q.Range(r => r.Field(f => f.Rating).GreaterThanOrEquals(rating))
        ));
    }
    
    return result.Hits.Select(x => x.Source).ToList();
});

app.Run();

