using Spotify.Library.Core;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Spotify.Web
{
    public class ItemTypeJsonConverter : JsonConverter<ItemType>
    {
        public override ItemType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Most of JSON reading is handled by ServiceStack which already has the capabilities of going from string to enum.
            // Leaving this not implemented as I should not need to implement this one.
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, ItemType value, JsonSerializerOptions options)
        {
            var stringValue = value switch
            {
                ItemType.Track => nameof(ItemType.Track),
                ItemType.Album => nameof(ItemType.Album),
                ItemType.Artist => nameof(ItemType.Artist),

                _ => throw new IndexOutOfRangeException(nameof(ItemType))
            };

            writer.WriteStringValue(stringValue);
        }
    }
}
