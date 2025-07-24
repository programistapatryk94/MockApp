
using MockApi.Localization;

namespace MockApi.Runtime.Exceptions.Handling
{
    public class ErrorInfoBuilder : IErrorInfoBuilder
    {
        private readonly IExceptionToErrorInfoConverter _converter;

        public ErrorInfoBuilder(ITranslationService translationService)
        {
            _converter = new DefaultErrorInfoConverter(translationService);
        }

        public ErrorInfo BuildForException(Exception exception)
        {
            return _converter.Convert(exception);
        }
    }
}
