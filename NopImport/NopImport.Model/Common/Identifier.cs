namespace NopImport.Model.Common
{
    public class Identifier
    {
        public IdentifierType Type { get; set; }

        public string Value { get; set; }

        public string Name { get; set; }

        public string AttributeElement { get; set; }

        public bool SearchParent { get; set; }

        public int CharactersToRemove { get; set; }

        public string OrginalText { get; set; }

        public string ReplaceWith { get; set; }
    }

    public enum IdentifierType
    {
        Text,
        ElementContent,
        Attribute,
    }
}
