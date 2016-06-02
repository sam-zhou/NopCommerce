using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NopImport.GoogleTranslate
{
    public class NopResource
    {
        public string English { get; set; }

        public string ChineseSimplified { get; set; }

        public string ChineseTraditional { get; set; }

        public string GetTranslatedText(int languageId)
        {
            var output = English;

            if (languageId == 2 && !string.IsNullOrWhiteSpace(ChineseSimplified))
            {
                output = ChineseSimplified;
            }

            if (languageId == 3 && !string.IsNullOrWhiteSpace(ChineseTraditional))
            {
                output = ChineseTraditional;
            }
            return output;
        }
    }
}
