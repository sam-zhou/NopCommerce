using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Autofac;
using Autofac.Core;
using dotless.Core.Parser.Infrastructure;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Core.Plugins;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Media;
using Nop.Services.Seo;
using NopImport.Common;
using NopImport.Common.Services;
using NopImport.Console.Helper;
using SevenSpikes.Nop.Conditions.Helpers;
using SevenSpikes.Nop.Conditions.Services;
using SevenSpikes.Nop.Mappings.Data;
using SevenSpikes.Nop.Mappings.Domain;
using SevenSpikes.Nop.Mappings.Services;
using SevenSpikes.Nop.Plugins.NopQuickTabs.Database;
using SevenSpikes.Nop.Plugins.NopQuickTabs.Domain;
using SevenSpikes.Nop.Plugins.NopQuickTabs.EFMapping;
using SevenSpikes.Nop.Plugins.NopQuickTabs.Services;
using NopProduct = Nop.Core.Domain.Catalog.Product;
using Product = NopImport.Model.Data.Product;

namespace NopImport.Console.Import
{
    public class NopLinker
    {
        public NopLinker()
        {

            EngineContext.Initialize(false);
            var builder = new ContainerBuilder();
            builder.RegisterType<EntityMappingService>().As<IEntityMappingService>().InstancePerLifetimeScope();
            builder.RegisterType<EntityConditionService>().As<IEntityConditionService>().InstancePerLifetimeScope();
            builder.RegisterType<TabService>().As<ITabService>().InstancePerLifetimeScope();
            builder.RegisterType<ConditionService>().As<IConditionService>().InstancePerLifetimeScope();
            builder.RegisterType<ConditionChecker>().As<IConditionChecker>().InstancePerLifetimeScope();
            RegisterHelper<NopQuickTabsObjectContext>(builder, "nop_object_context_quick_tabs");

            //override required repository with our custom context
            builder.RegisterType<EfRepository<Tab>>()
                .As<IRepository<Tab>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_quick_tabs"))
                .InstancePerLifetimeScope();

            RegisterHelper<MappingsObjectContext>(builder, "nop_object_context_entity_mapping");

            //override required repository with our custom context
            builder.RegisterType<EfRepository<EntityMapping>>()
                .As<IRepository<EntityMapping>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_entity_mapping"))
                .InstancePerLifetimeScope();

            builder.Update(EngineContext.Current.ContainerManager.Container);
            
            
            //EngineContext.Current.Resolve<ITabService>();
        }

        public void RegisterHelper<T>(ContainerBuilder builder, string contextName)
             where T : IDbContext
        {
            //data layer
            var dataSettingsManager = new DataSettingsManager();
            var dataProviderSettings = dataSettingsManager.LoadSettings();

            if (dataProviderSettings != null && dataProviderSettings.IsValid())
            {
                //register named context
                builder.Register(c => (IDbContext)Activator.CreateInstance(typeof(T), new object[] { dataProviderSettings.DataConnectionString }))
                    .Named<IDbContext>(contextName)
                    .InstancePerLifetimeScope();

                builder.Register(c => (T)Activator.CreateInstance(typeof(T), new object[] { dataProviderSettings.DataConnectionString }))
                    .InstancePerLifetimeScope();
            }
            else
            {
                //register named context
                builder.Register(c => (T)Activator.CreateInstance(typeof(T), new object[] { c.Resolve<DataSettings>().DataConnectionString }))
                    .Named<IDbContext>(contextName)
                    .InstancePerLifetimeScope();

                builder.Register(c => (T)Activator.CreateInstance(typeof(T), new object[] { c.Resolve<DataSettings>().DataConnectionString }))
                    .InstancePerLifetimeScope();
            }
        }


        public void ImportItems()
        {
            using (var db = new DatabaseService("DefaultConnectionString", "NopImport"))
            {
                var productService = EngineContext.Current.Resolve<IProductService>();
                var tabService = EngineContext.Current.Resolve<ITabService>();
                var pictureService = EngineContext.Current.Resolve<IPictureService>();
                var urlRecordService = EngineContext.Current.Resolve<IUrlRecordService>();
                var products = db.Session.QueryOver<Product>().Where(q => q.IsUpdated && !q.IsSynced).List();
                foreach (var product in products)
                {
                    try
                    {
                        var nopProduct =
                            productService.GetProductBySku(string.Format("{0}-{1}", product.ExternalStoreCode,
                                product.ExternalId));
                        if (nopProduct == null)
                        {
                            nopProduct = new NopProduct();
                            nopProduct.UpdateFrom(product);

                            productService.InsertProduct(nopProduct);
                            urlRecordService.SaveSlug(nopProduct, nopProduct.ValidateSeName(StringExtension.GenerateSlug(product.Name), product.Name, true), 0);
                            if (!string.IsNullOrWhiteSpace(product.LocalPicture))
                            {
                                var fullPath = Path.Combine(
                                    @"I:\NopCommerce\Presentation\Nop.Web\Content\Images\Thumbs", product.LocalPicture);

                                var newPictureBinary = File.ReadAllBytes(fullPath);
                                var newPicture = pictureService.InsertPicture(newPictureBinary,
                                    GetMimeTypeFromFilePath(fullPath), pictureService.GetPictureSeName(product.Name));
                                nopProduct.ProductPictures.Add(new ProductPicture
                                {
                                    PictureId = newPicture.Id,
                                    DisplayOrder = 1,
                                });
                                
                            }


                            nopProduct.ProductCategories.Add(new ProductCategory
                            {
                                CategoryId = 5
                            });
                            productService.UpdateProduct(nopProduct);


                            var tabs = GetTabs(product);

                            foreach (var tab in tabs)
                            {
                                tabService.InsertTab(tab);
                            }
                            
                            

                            tabService.AddTabsForProductByIds(nopProduct.Id, tabs.Select(q => q.Id).ToArray());

                        }
                        else
                        {
                            
                            System.Console.WriteLine("product exists");
                            //nopProduct.UpdateFrom(product);
                            //productService.UpdateProduct(nopProduct);
                        }


                        db.BeginTransaction();
                        product.IsSynced = true;
                        db.Session.Save(product);
                        db.CommitTransaction();
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine("Error when syncing product to NopCommerce, Rolling back");
                        db.RollBackTransaction();
                        throw ex;
                    }
                    
                    
                }
            }
        }

        public static string GetMimeTypeFromFilePath(string filePath)
        {
            var mimeType = MimeMapping.GetMimeMapping(filePath);

            //little hack here because MimeMapping does not contain all mappings (e.g. PNG)
            if (mimeType == "application/octet-stream")
                mimeType = "image/jpeg";

            return mimeType;
        }


        private List<Tab> GetTabs(Product product)
        {
            var output = new List<Tab>();

            if (!string.IsNullOrWhiteSpace(product.GeneralInfo))
            {
                output.Add(new Tab
                {
                    SystemName = "general_info_" + product.ExternalStoreCode + "_" + product.ExternalId,
                    DisplayName = "General Information",
                    Description = product.GeneralInfo,
                    TabMode = TabMode.Mappings,
                    DisplayOrder = 3
                });

            }

            if (!string.IsNullOrWhiteSpace(product.Directions))
            {
                output.Add(new Tab
                {
                    SystemName = "directions_" + product.ExternalStoreCode + "_" + product.ExternalId,
                    DisplayName = "Directions",
                    Description = product.Directions,
                    TabMode = TabMode.Mappings,
                    DisplayOrder = 4
                });
            }

            

            if (!string.IsNullOrWhiteSpace(product.Ingredients))
            {
                output.Add(new Tab
                {
                    SystemName = "ingredients_" + product.ExternalStoreCode + "_" + product.ExternalId,
                    DisplayName = "Ingredients",
                    Description = product.Ingredients,
                    TabMode = TabMode.Mappings,
                    DisplayOrder = 5
                });

            }

            if (!string.IsNullOrWhiteSpace(product.Warnings))
            {
                output.Add(new Tab
                {
                    SystemName = "warnings_" + product.ExternalStoreCode + "_" + product.ExternalId,
                    DisplayName = "Warnings",
                    Description = product.Warnings,
                    TabMode = TabMode.Mappings,
                    DisplayOrder = 6
                });

            }

            return output;
        } 
    }
}
