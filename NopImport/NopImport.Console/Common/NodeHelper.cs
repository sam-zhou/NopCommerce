using System;
using HtmlAgilityPack;
using NopImport.Model.Common;
using NopImport.Model.Data;
using NopImport.Model.SearchModel;

namespace NopImport.Console.Common
{
    public static class NodeHelper
    {
        public static TEntity GetEntity<TEntity>(this HtmlNode node, BaseSeachModel<TEntity> searchModel , TEntity entity = null) where TEntity: BaseEntity
        {
            if (entity == null)
            {
                entity = Activator.CreateInstance<TEntity>();
            }


            foreach (var identifier in searchModel.Identifiers)
            {
                var property = (typeof(TEntity)).GetProperty(identifier.Name);
                if (property != null)
                {
                    property.SetValue(entity, node.GetValueFromNode(identifier));
                }

            }

            return entity;
        }


        public static HtmlNodeCollection GetNodesFromIdentifier(this HtmlNode node, Identifier identifier)
        {
            if (identifier.Type == IdentifierType.Attribute || identifier.Type == IdentifierType.Text)
            {
                throw new Exception("Cannot get HtmlNodeCollection from attribute");
            }

            return node.SelectNodes(identifier.Value);
        }

        public static string GetValueFromNode(this HtmlNode node, Identifier identifier)
        {

            var targetNode = node;
            if (identifier.SearchParent)
            {
                targetNode = node.ParentNode;
            }

            string result;
            if (identifier.Type == IdentifierType.Text)
            {
                return identifier.Value;
            }


            if (identifier.Type == IdentifierType.Attribute)
            {

                if (identifier.AttributeElement != null)
                {
                    targetNode = targetNode.SelectSingleNode(identifier.AttributeElement);
                }


                if (targetNode == null)
                {
                    result = string.Empty;
                }
                else
                {
                    result = targetNode.Attributes[identifier.Value].Value;
                }
            }
            else
            {
                var selectedNode = targetNode.SelectSingleNode(identifier.Value);

                if (selectedNode != null)
                {
                    result = selectedNode.InnerHtml;
                }
                else
                {
                    result = string.Empty;
                }

            }

            result = result.Trim();
            if (identifier.CharactersToRemove > 0)
            {
                if (result.Length > identifier.CharactersToRemove)
                {
                    result = result.Substring(identifier.CharactersToRemove,
                        result.Length - identifier.CharactersToRemove);
                }
                else
                {
                    result = string.Empty;
                }
            }

            if (identifier.OrginalText != null && identifier.ReplaceWith != null)
            {
                result = result.Replace(identifier.OrginalText, identifier.ReplaceWith);
            }

            return result;
        }
    }
}
