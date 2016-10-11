using Autofac;
using Autofac.Core;

namespace Common.Logging
{
    public sealed class LogRequestModule : Module
    {
        private static readonly ILog Logger;

        static LogRequestModule()
        {
            Logger = LogProvider.GetCurrentClassLogger();
        }
        

        public int Depth = 0;

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry,
            IComponentRegistration registration)
        {
            registration.Preparing += RegistrationOnPreparing;
            registration.Activating += RegistrationOnActivating;
            base.AttachToComponentRegistration(componentRegistry, registration);
        }

        private string GetPrefix()
        {
            return new string('-', Depth*2);
        }

        private void RegistrationOnPreparing(object sender, PreparingEventArgs preparingEventArgs)
        {
            Logger.Info($"{GetPrefix()} Resolving  {preparingEventArgs.Component.Activator.LimitType}");
            Depth++;
        }

        private void RegistrationOnActivating(object sender, ActivatingEventArgs<object> activatingEventArgs)
        {
            Depth--;

        }
    }
}
