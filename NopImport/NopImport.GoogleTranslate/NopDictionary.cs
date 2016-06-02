using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NopImport.GoogleTranslate
{
    public static class NopDictionary
    {
        private static Dictionary<string, NopResource> _dictionary;

        public static Dictionary<string, NopResource> Dictionary
        {
            get
            {
                if (_dictionary == null)
                {
                    _dictionary = new Dictionary<string, NopResource>();
                    _dictionary.Add("General Information", new NopResource { ChineseSimplified = "基本介绍", ChineseTraditional = "基本介紹" });
                    _dictionary.Add("Directions", new NopResource { ChineseSimplified = "用法说明", ChineseTraditional = "用法說明" });
                    _dictionary.Add("Ingredients", new NopResource { ChineseSimplified = "成分", ChineseTraditional = "成分" });
                    _dictionary.Add("Warnings", new NopResource { ChineseSimplified = "注意事项", ChineseTraditional = "注意事項" });
                }
                return _dictionary;
            }
        } 

        public static string GetTranslate(string originalText, int languageId)
        {
            string output;
            if (Dictionary.ContainsKey(originalText))
            {
                output = Dictionary.FirstOrDefault(q => q.Key == originalText).Value.GetTranslatedText(languageId);
            }
            else
            {
                output = null;
            }
            return output;
        }
    }
}
