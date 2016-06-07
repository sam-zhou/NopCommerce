using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Core.Plugins;
using Nop.Plugin.ExternalAuth.Google.Core;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Nop.Plugin.ExternalAuth.Google
{
    /// <summary>
    /// Dependency registrar
    /// </summary>
    public class DependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            var typeFinders = typeFinder.FindClassesOfType<IPluginFinder>(true);
            if (typeFinders.Count() == 1)
            {
                if (null == Expression.Lambda<Func<IPluginFinder>>(Expression.New(typeFinders.First())).Compile()().GetPluginDescriptorBySystemName("ExternalAuth.Google", LoadPluginsMode.InstalledOnly))
                {
                    return;
                }
            }

            builder.RegisterType<GoogleProviderAuthorizer>().As<IOAuthProviderGoogleAuthorizer>().InstancePerLifetimeScope();

        }

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order
        {
            get { return 5; }
        }
    }
}
