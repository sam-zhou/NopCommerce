namespace NopImport.Model.Common
{
    public class Identifier
    {
        public IdentifierType Type { get; set; }

        public string Value { get; set; }

        public string Name { get; set; }

        public string AttributeElement { get; set; }
    }

    public enum IdentifierType
    {
        Text,
        Attribute,
    }
}
