using System;
using ntbs_service.Models.Enums;

namespace ntbs_service.Helpers
{
    public static class Converter
    {
        public static Status? GetStatusFromString(string status)
        {
            return GetEnumValue<Status>(status);
        }

        public static bool? GetNullableBoolValue(int? value)
        {
            return value == null ? (bool?)null : value == 1;
        }

        public static int? ToNullableInt(string stringValue)
        {
            return int.TryParse(stringValue, out var intValue) ? (int?)intValue : null;
        }

        public static T? GetEnumValue<T>(string raw) where T : struct
        {
            return string.IsNullOrEmpty(raw) ?
                null :
                (T?)Enum.Parse<T>(raw);
        }
    }
}
