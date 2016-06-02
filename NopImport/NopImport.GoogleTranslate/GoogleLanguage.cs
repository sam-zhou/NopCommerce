using System.Collections.Generic;
using System.Linq;

namespace NopImport.GoogleTranslate
{
    public class GoogleLanguage
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }


        private static List<GoogleLanguage> _languages;

        public static List<GoogleLanguage> Languages
        {
            get
            {
                if (_languages == null)
                {
                    _languages = new List<GoogleLanguage>();
                    _languages.Add(new GoogleLanguage
                    {
                        Id = 1,
                        Name = "English",
                        Code = "en",
                    });
                    _languages.Add(new GoogleLanguage
                    {
                        Id = 2,
                        Name = "Chinese (Simplified)",
                        Code = "zh-CN",
                    });
                    _languages.Add(new GoogleLanguage
                    {
                        Id = 3,
                        Name = "Chinese (Traditional)",
                        Code = "zh-TW",
                    });
                }
                return _languages;
            }
        } 

        public static GoogleLanguage English
        {
            get { return GetLanguageById(1); }
        }

        public static GoogleLanguage ChineseSimplified
        {
            get { return GetLanguageById(2); }
        }

        public static GoogleLanguage ChineseTraditional
        {
            get { return GetLanguageById(3); }
        }

        public static GoogleLanguage GetLanguageById(int id)
        {
            return Languages.FirstOrDefault(q => q.Id == id);
        }
    }
}
