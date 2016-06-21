using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Autofac;
using Autofac.Core;
using dotless.Core.Parser.Infrastructure;
using HtmlAgilityPack;
using NHibernate.Impl;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Core.Plugins;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using NopImport.Common;
using NopImport.Common.Services;
using NopImport.Console.Common;
using NopImport.Console.Helper;
using NopImport.GoogleTranslate;
using SevenSpikes.Nop.Conditions.Helpers;
using SevenSpikes.Nop.Conditions.Services;
using SevenSpikes.Nop.Mappings.Data;
using SevenSpikes.Nop.Mappings.Domain;
using SevenSpikes.Nop.Mappings.Services;
using SevenSpikes.Nop.Plugins.NopQuickTabs.Database;
using SevenSpikes.Nop.Plugins.NopQuickTabs.Domain;
using SevenSpikes.Nop.Plugins.NopQuickTabs.EFMapping;
using SevenSpikes.Nop.Plugins.NopQuickTabs.Services;
using SevenSpikes.Nop.Services.Configuration;
using NopProduct = Nop.Core.Domain.Catalog.Product;
using Product = NopImport.Model.Data.Product;

namespace NopImport.Console.Import
{
    public class NopLinker : BaseWorker
    {

        public NopLinker()
        {

            Initialise();


            //EngineContext.Current.Resolve<ITabService>();
        }
        #region Initialise
        private void Initialise()
        {
            EngineContext.Initialize(false);
            var builder = new ContainerBuilder();
            builder.RegisterType<EntityMappingService>().As<IEntityMappingService>().InstancePerLifetimeScope();
            builder.RegisterType<EntityConditionService>().As<IEntityConditionService>().InstancePerLifetimeScope();
            builder.RegisterType<TabService>().As<ITabService>().InstancePerLifetimeScope();
            builder.RegisterType<ConditionService>().As<IConditionService>().InstancePerLifetimeScope();
            builder.RegisterType<ConditionChecker>().As<IConditionChecker>().InstancePerLifetimeScope();
            builder.RegisterType<LocalizedSettingService>().As<ILocalizedSettingService>().InstancePerLifetimeScope();
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
        }

        private void RegisterHelper<T>(ContainerBuilder builder, string contextName)
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

        #endregion



        #region properties

        private IProductService _productService;
        protected IProductService ProductService
        {
            get
            {
                if (_productService == null)
                {
                    _productService = EngineContext.Current.Resolve<IProductService>();
                }
                return _productService;
            }
        }

        private ITabService _tabService;
        protected ITabService TabService
        {
            get
            {
                if (_tabService == null)
                {
                    _tabService = EngineContext.Current.Resolve<ITabService>();
                }
                return _tabService;
            }
        }

        private IPictureService _pictureService;
        protected IPictureService PictureService
        {
            get
            {
                if (_pictureService == null)
                {
                    _pictureService = EngineContext.Current.Resolve<IPictureService>();
                }
                return _pictureService;
            }
        }




        private IUrlRecordService _urlRecordService;
        protected IUrlRecordService UrlRecordService
        {
            get
            {
                if (_urlRecordService == null)
                {
                    _urlRecordService = EngineContext.Current.Resolve<IUrlRecordService>();
                }
                return _urlRecordService;
            }
        }


        private ILocalizationService _localizationService;
        protected ILocalizationService LocalizationService
        {
            get
            {
                if (_localizationService == null)
                {
                    _localizationService = EngineContext.Current.Resolve<ILocalizationService>();
                }
                return _localizationService;
            }
        }


        private ILocalizedSettingService _localizedSettingService;
        protected ILocalizedSettingService LocalizedSettingService
        {
            get
            {
                if (_localizedSettingService == null)
                {
                    _localizedSettingService = EngineContext.Current.Resolve<ILocalizedSettingService>();
                }
                return _localizedSettingService;
            }
        }



        private ILocalizedEntityService _localizedEntityService;
        protected ILocalizedEntityService LocalizedEntityService
        {
            get
            {
                if (_localizedEntityService == null)
                {
                    _localizedEntityService = EngineContext.Current.Resolve<ILocalizedEntityService>();
                }
                return _localizedEntityService;
            }
        }

        private Translator _translator;

        protected Translator Translator
        {
            get
            {
                if (_translator == null)
                {
                    _translator = new Translator();
                }
                return _translator;
            }
        }

        #endregion

        public bool IsProductExists(string code, string id)
        {
            return ProductService.GetProductBySku(string.Format("{0}-{1}", code,
                id)) != null;
        }

        private static string GetMimeTypeFromFilePath(string filePath)
        {
            var mimeType = MimeMapping.GetMimeMapping(filePath);

            //little hack here because MimeMapping does not contain all mappings (e.g. PNG)
            if (mimeType == "application/octet-stream")
                mimeType = "image/jpeg";

            return mimeType;
        }

        private void TranslateMetaKeywords(NopProduct nopProduct)
        {
            foreach (var googleLanguage in GoogleLanguage.Languages)
            {
                if (googleLanguage.Id != 1)
                {
                    string translatedText = null;
                    if (googleLanguage.Id == 2)
                    {
                        translatedText =
                            @"保健品 维生素 抗衰老 医药 在线 护理 健康 美容 产品 减肥 处方药 药物 扑热息痛 皮肤 流感 阿司匹林 奶粉 婴儿食品 母婴用品 儿童 男人 女人 老人 小孩 自然 有机";
                    }
                    else
                    {
                        var keywords = nopProduct.MetaKeywords.Replace(" ", "/ ");
                        var language = GoogleLanguage.GetLanguageById(googleLanguage.Id);



                        if (language != null && googleLanguage.Id != 1)
                        {
                            translatedText = NopDictionary.GetTranslate(keywords, googleLanguage.Id);

                            if (string.IsNullOrWhiteSpace(translatedText))
                            {
                                translatedText = Translator.Translate(keywords, "English", GoogleLanguage.GetLanguageById(googleLanguage.Id).Name);

                                translatedText = translatedText.Replace("/", " ");
                            }
                        }
                    }

       
                    var lan = new LocalizedProperty
                    {
                        EntityId = nopProduct.Id,
                        LanguageId = googleLanguage.Id,
                        LocaleKey = "MetaKeywords",
                        LocaleKeyGroup = "Prodcut",
                        LocaleValue = translatedText
                    };

                    LocalizedEntityService.InsertLocalizedProperty(lan);
                }
            }
        }

        private void TranslateToAllLanguages<T>(T property, string propertyName) where T : BaseEntity
        {
            foreach (var googleLanguage in GoogleLanguage.Languages)
            {
                if (googleLanguage.Id != 1)
                {
                    TranslateProperty(property, propertyName, googleLanguage.Id);
                }
            }
        }

        private void TranslateProperty<T>(T property, string propertyName, int languageId) where T : BaseEntity
        {
            var propertyValue = (typeof (T)).GetProperty(propertyName).GetValue(property, null).ToString();
            var lan = GetLocalizedProperty(property.Id, typeof(T).Name, propertyName, propertyValue, languageId);
            LocalizedEntityService.InsertLocalizedProperty(lan);
        }


        private LocalizedProperty GetLocalizedProperty(int entityId, string keyGroup, string key, string value, int languageId)
        {
            var language = GoogleLanguage.GetLanguageById(languageId);

            string translatedText = null;

            if (language != null && languageId != 1)
            {
                translatedText = NopDictionary.GetTranslate(value, languageId);

                if (string.IsNullOrWhiteSpace(translatedText))
                {
                    if (value.Contains("<") && value.Contains(">"))
                    {
                        translatedText = GetTranslateHtml(value, languageId);
                    }
                    else
                    {
                        translatedText = Translator.Translate(value, "English", GoogleLanguage.GetLanguageById(languageId).Name);
                    }

                    translatedText = translatedText.Replace("\\r \\n", "<br/>").Replace("\\r\\n", "<br/>").Replace("\\r", "<br/>").Replace("\\n", "<br/>");

                }
            }





            return  new LocalizedProperty
            {
                EntityId = entityId,
                LanguageId = languageId,
                LocaleKey = key,
                LocaleKeyGroup = keyGroup,
                LocaleValue = translatedText
            };

            



            //LocalizedSettingService.SaveSetting(lan, 1);
        }

        private string GetTranslateHtml(string text, int languageId)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(text);


            TranslateNode(htmlDocument.DocumentNode, languageId);


            return htmlDocument.DocumentNode.OuterHtml;
        }

        private void TranslateNode(HtmlNode node, int languageId)
        {
            if (!node.HasChildNodes)
            {
                node.InnerHtml = Translator.Translate(node.InnerText, "English",
                    GoogleLanguage.GetLanguageById(languageId).Name);
            }
            else
            {
                foreach (var childNode in node.ChildNodes)
                {
                    TranslateNode(childNode, languageId);
                }
            }
        }


        private List<Tab> GetTabs(Product product)
        {
            var output = new List<Tab>();

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

        public override void Process()
        {
            using (var db = new DatabaseService("DefaultConnectionString", "NopImport"))
            {

                var products = db.Session.QueryOver<Product>().Where(q => q.IsUpdated && !q.IsSynced ).List();
                var count = 0;
                foreach (var product in products)
                {
                    count ++;
                    try
                    {
                        var nopProduct =
                            ProductService.GetProductBySku(string.Format("{0}-{1}", product.ExternalStoreCode,
                                product.ExternalId));
                        if (nopProduct == null)
                        {
                            nopProduct = new NopProduct();
                            nopProduct.UpdateFrom(product);

                            ProductService.InsertProduct(nopProduct);

                            TranslateToAllLanguages(nopProduct, "Name");
                            TranslateToAllLanguages(nopProduct, "ShortDescription");
                            TranslateToAllLanguages(nopProduct, "FullDescription");
                            TranslateToAllLanguages(nopProduct, "MetaDescription");
                            TranslateMetaKeywords(nopProduct);
                            TranslateToAllLanguages(nopProduct, "MetaTitle");

                            var slug = nopProduct.ValidateSeName(StringExtension.GenerateSlug(product.Name),
                                product.Name, true);
                            UrlRecordService.SaveSlug(nopProduct, slug, 0);
                            UrlRecordService.SaveSlug(nopProduct, slug, 1);
                            UrlRecordService.SaveSlug(nopProduct, slug, 2);
                            UrlRecordService.SaveSlug(nopProduct, slug, 3);
                            if (!string.IsNullOrWhiteSpace(product.LocalPicture))
                            {
                                var directory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\..\\Presentation\\Nop.Web\\Content\\Images\\Thumbs"));

                                var fullPath = Path.Combine(directory, product.LocalPicture);

                                var newPictureBinary = File.ReadAllBytes(fullPath);
                                var newPicture = PictureService.InsertPicture(newPictureBinary,
                                    GetMimeTypeFromFilePath(fullPath), PictureService.GetPictureSeName(product.Name));
                                nopProduct.ProductPictures.Add(new ProductPicture
                                {
                                    PictureId = newPicture.Id,
                                    DisplayOrder = 1,
                                });

                            }

                            nopProduct.ProductManufacturers.Add(new ProductManufacturer
                            {
                                ManufacturerId = int.Parse(product.Manufacturer)
                            });

                            nopProduct.ProductCategories.Add(new ProductCategory
                            {
                                CategoryId = int.Parse(product.Category)
                            });


                            ProductService.UpdateProduct(nopProduct);


                            var tabs = GetTabs(product);

                            foreach (var tab in tabs)
                            {
                                TabService.InsertTab(tab);
                                TranslateToAllLanguages(tab, "Description");
                                TranslateToAllLanguages(tab, "DisplayName");
                            }
                            TabService.AddTabsForProductByIds(nopProduct.Id, tabs.Select(q => q.Id).ToArray());
                        }
                        else
                        {
                            System.Console.WriteLine("product exists");
                        }

                        db.BeginTransaction();
                        product.NopId = nopProduct.Id.ToString(CultureInfo.InvariantCulture);
                        db.Session.Save(product);
                        db.CommitTransaction();
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine("Error when syncing product to NopCommerce, Rolling back");
                        db.RollBackTransaction();
                        throw ex;
                    }

                    ChangeProgress(count * 100 / products.Count);

                }
            }
        }
    }
}
