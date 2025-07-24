
using MockApi.Localization;

namespace MockApi.Runtime.Exceptions.Handling
{
    public class DefaultErrorInfoConverter : IExceptionToErrorInfoConverter
    {
        private readonly ITranslationService _translationService;

        public DefaultErrorInfoConverter(ITranslationService translationService)
        {
            _translationService = translationService;
        }

        public ErrorInfo Convert(Exception exception)
        {
            var errorInfo = CreateErrorInfo(exception);

            return errorInfo;
        }

        private ErrorInfo CreateErrorInfo(Exception exception)
        {
            if(exception is AggregateException aggException && exception.InnerException != null)
            {
                if (aggException.InnerException is UserFriendlyException)
                {
                    exception = aggException.InnerException;
                }
            }

            if(exception is UserFriendlyException userFriendlyException)
            {
                return new ErrorInfo(userFriendlyException.Code, userFriendlyException.Message);
            }

            return new ErrorInfo(_translationService.Translate("InternalServerError"));
        }
    }
}
