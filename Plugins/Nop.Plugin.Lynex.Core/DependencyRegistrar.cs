using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;

namespace Nop.Plugin.Lynex.Core
{
	public partial class DependencyRegistrar : IDependencyRegistrar
	{
	    public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
	    {

            //builder.RegisterType<NopPictureService>().As<IPictureService>().InstancePerLifetimeScope();

            ////data context
            //this.RegisterPluginDataContext<ShippingByWeightObjectContext>(builder, "nop_object_context_shipping_weight_zip");

            ////override required repository with our custom context
            //builder.RegisterType<EfRepository<ShippingByWeightRecord>>()
            //    .As<IRepository<ShippingByWeightRecord>>()
            //    .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_shipping_weight_zip"))
            //    .InstancePerLifetimeScope();
        }

	    public int Order
		{
			get
			{
				return 1000;
			}
		}
	}
}
