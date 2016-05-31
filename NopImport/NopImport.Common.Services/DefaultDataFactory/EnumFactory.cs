using System;
using System.Collections.Generic;
using System.Reflection;
using NHibernate;
using NopImport.Model.Data;

namespace NopImport.Common.Services.DefaultDataFactory
{
    internal class EnumFactory : DefaultDataFactoryBase<IBaseEntity>
    {
        public EnumFactory(ISession session, Assembly assembly) : base(session, assembly)
        {
        }

        protected override IEnumerable<IBaseEntity> GetData(Assembly assembly = null)
        {
            if (assembly != null)
            {
                foreach (var mapableEnum in assembly.GetMapableEnumTypes())
                {

                    foreach (var enumItem in Enum.GetValues(mapableEnum))
                    {
                        var d1 = typeof(EnumTable<>);
                        Type[] typeArgs = { mapableEnum };
                        var makeme = d1.MakeGenericType(typeArgs);
                        var o = (IBaseEntity)Activator.CreateInstance(makeme);
                        var type = o.GetType();
                        var idProperty = type.GetProperty("Id");
                        var nameProperty = type.GetProperty("Name");

                        idProperty.SetValue(o, (int)enumItem, null);
                        nameProperty.SetValue(o, enumItem.ToString(), null);

                        yield return o;
                    }
                }
            }
        }
    }
}
