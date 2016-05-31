using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NopImport.Model.Common;

namespace NopImport.Model.SearchModel
{
    public abstract class BaseSeachModel
    {
        private List<Identifier> _identifiers;

        public List<Identifier> Identifiers
        {
            get
            {
                if (_identifiers == null)
                {
                    _identifiers = new List<Identifier>();
                }
                return _identifiers;
            }
        }

        public void AddIdentifier(string name, IdentifierType type, string value)
        {
            Identifiers.Add(new Identifier
            {
                Type = type,
                Value = value,
                Name = name,
            });
        }

        public string GetValue(string name)
        {
            var found = Identifiers.FirstOrDefault(q => q.Name == name);
            
            if (found != null)
            {
                return found.Value;
            }

            return null;
        }
    }
}
