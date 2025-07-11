using Microsoft.EntityFrameworkCore;

namespace MockApi.Data
{
    public class DbContextFilterScope<TContext> : IDisposable where TContext : DbContext
    {
        private readonly TContext _context;
        private readonly string _propertyName;
        private readonly object? _originalValue;

        public DbContextFilterScope(TContext context, string propertyName, object temporaryValue)
        {
            _context = context;
            _propertyName = propertyName;

            var prop = typeof(TContext).GetProperty(_propertyName);
            if (prop == null)
                throw new InvalidOperationException($"Property '{_propertyName}' not found on context '{typeof(TContext).Name}'.");

            _originalValue = prop.GetValue(context);
            prop.SetValue(context, temporaryValue);
        }

        public void Dispose()
        {
            var prop = typeof(TContext).GetProperty(_propertyName);
            prop?.SetValue(_context, _originalValue);
        }
    }
}
