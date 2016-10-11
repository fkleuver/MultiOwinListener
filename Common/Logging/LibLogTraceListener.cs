using System.Diagnostics;

namespace Common.Logging
{
    public sealed class LibLogTraceListener : TraceListener
    {
        private static readonly ILog Logger;

        static LibLogTraceListener()
        {
            Logger = LogProvider.GetCurrentClassLogger();
        }

        public override void WriteLine(string message)
        {
            Logger.Debug(message);
        }

        public override void Write(string message)
        {}
    }
}
