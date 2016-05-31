using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NopImport.Model.Data
{
    public interface IDbModel
    {

    }

    public interface IBaseEntity : IDbModel
    {
        long Id { get; set; }
    }

    public interface IBaseEntity<T> : IBaseEntity
    {
    }

    public class EnumTable<T> : IBaseEntity<T> where T : struct ,IConvertible
    {
        public virtual long Id { get; set; }

        public virtual string Name { get; set; }
    }
}
