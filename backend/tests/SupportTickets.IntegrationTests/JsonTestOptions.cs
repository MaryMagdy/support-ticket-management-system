using System.Text.Json;
using System.Text.Json.Serialization;

namespace SupportTickets.IntegrationTests;

public static class JsonTestOptions
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
