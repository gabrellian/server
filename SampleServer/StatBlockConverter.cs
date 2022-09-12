using Data.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SampleServer;

internal class StatBlockConverter : JsonConverter<StatBlock>
{
    public override StatBlock Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var data = JsonSerializer.Deserialize<SampleStatBlock>(ref reader, options);
        return data;
    }

    public override void Write(Utf8JsonWriter writer, StatBlock value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value as SampleStatBlock, options);
    }
}
