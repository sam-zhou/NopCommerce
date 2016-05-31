using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NopImport.Model.Common;
using NopImport.Model.Data;
using NopImport.Model.SearchModel;

namespace NopImport.UrlSearcher
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
            if (identifier.Type == IdentifierType.Attribute)
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
            if (identifier.Type == IdentifierType.Attribute)
            {

                if (identifier.AttributeElement != null)
                {
                    targetNode = targetNode.SelectSingleNode(identifier.AttributeElement);
                }
                result = targetNode.Attributes[identifier.Value].Value;


            }
            else
            {
                result = targetNode.SelectSingleNode(identifier.Value).InnerHtml;
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
