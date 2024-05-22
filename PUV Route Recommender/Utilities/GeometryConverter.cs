using NetTopologySuite.IO;
using NetTopologySuite.Geometries;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CommuteMate.Utilities
{
    public class GeometryConverter : JsonConverter<Geometry>
    {
        private readonly GeoJsonReader _reader = new GeoJsonReader();
        private readonly GeoJsonWriter _writer = new GeoJsonWriter();

        public override Geometry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                string geoJson = document.RootElement.GetRawText();
                return _reader.Read<Geometry>(geoJson);
            }
        }

        public override void Write(Utf8JsonWriter writer, Geometry value, JsonSerializerOptions options)
        {
            string geoJson = _writer.Write(value);
            using (JsonDocument document = JsonDocument.Parse(geoJson))
            {
                document.WriteTo(writer);
            }   
        }
    }

    public static class HttpClientExtensions
    {
        public static async Task<T> ReadFromJsonAsyncWithCustomConverter<T>(this HttpContent content, JsonSerializerOptions options = null)
        {
            var data = await content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<T>(data, options);
        }
    }
}
