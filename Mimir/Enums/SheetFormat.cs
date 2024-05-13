namespace Mimir.Enums
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
                    return "text/csv";
                case SheetFormat.Json:
                    return "application/json";
                default:
                    throw new KeyNotFoundException(format.ToString());
            }
        }
    }
}
