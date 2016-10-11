using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;

namespace Common.Logging
{
    public sealed class LogProviderExceptionLogger : IExceptionLogger
    {
        private static readonly ILog Logger;

        static LogProviderExceptionLogger()
        {
            Logger = LogProvider.GetCurrentClassLogger();
        }

        public async Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            await Task.Run(() => Logger.ErrorException("Unhandled exception", context.Exception), cancellationToken);
        }
    }
}
