using System.Net.Mime;

namespace Mimir.MongoDB.Enums
{
    public enum SheetFormat
    {
        Json,
        Csv
    }

    static class EnumExtension
    {
        public static string ToMimeType(this SheetFormat format)
        {
            switch (format)
            {
                case SheetFormat.Csv:
                    return MediaTypeNames.Text.Csv;
                case SheetFormat.Json:
                    return MediaTypeNames.Application.Json;
                default:
                    throw new KeyNotFoundException(format.ToString());
            }
        }
    }
}
