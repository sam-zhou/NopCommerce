using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NopImport.Model.Common;
using NopImport.Model.Data;

namespace NopImport.Model.SearchModel
{
    public abstract class BaseSeachModel<T> where T: BaseEntity
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

        public void AddIdentifier(string name, IdentifierType type, string value, string attributeElement = null, bool searchParent = false, int charactersToRemove = 0, string originalText = null, string replaceWith = null)
        {
            Identifiers.Add(new Identifier
            {
                Type = type,
                Value = value,
                Name = name,
                AttributeElement = attributeElement,
                SearchParent = searchParent,
                CharactersToRemove = charactersToRemove,
                OrginalText = originalText,
                ReplaceWith = replaceWith
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
