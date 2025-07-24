using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MockApi.Runtime.Timing;
using System.Linq.Expressions;

namespace MockApi.Data.ValueConverters
{
    public class DateTimeValueConverter : ValueConverter<DateTime?, DateTime?>
    {
        public DateTimeValueConverter(ConverterMappingHints mappingHints = null)
            : base(Normalize, Normalize, mappingHints)
        {
        }

        private static readonly Expression<Func<DateTime?, DateTime?>> Normalize = x =>
            x.HasValue ? Clock.Normalize(x.Value) : x;
    }
}
