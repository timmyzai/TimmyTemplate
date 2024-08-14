using System.Xml.Linq;
using Microsoft.AspNetCore.Http;

namespace ByteAwesome
{
    public static class LanguageService
    {
        public static Dictionary<string, Dictionary<string, string>> translations = new();
        private static IHttpContextAccessor _httpContextAccessor;
        private static HashSet<string> availableCultures = new();
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            LoadLanguages();
        }
        private static void LoadLanguages()
        {
            try
            {
                string[] filePaths = Directory.GetFiles("../ByteAwesome.Shared.Files/Localisation", "*.xml");
                foreach (string filePath in filePaths)
                {
                    XDocument doc = XDocument.Load(filePath);
                    string culture = Path.GetFileNameWithoutExtension(filePath).Split('_').Last();
                    availableCultures.Add(culture);
                    foreach (XElement element in doc.Descendants("text"))
                    {
                        string key = element.Attribute("name").Value;
                        string value = element.Value;
                        if (!translations.ContainsKey(key))
                        {
                            translations[key] = new Dictionary<string, string>();
                        }
                        translations[key][culture] = value;
                    }
                }
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
            }
        }
        public static string Translate(string key, string userLangSetting = null)
        {
            if (userLangSetting is not null && TryGetTranslation(key, userLangSetting, out var translation))
            {
                return translation;
            }
            var primaryLanguageCode = CurrentSession.GetUserLanguage();
            if (primaryLanguageCode is not null)
            {
                string cultureCode = primaryLanguageCode;
                if (availableCultures.Contains(cultureCode) && TryGetTranslation(key, cultureCode, out translation))
                {
                    return translation;
                }
                else
                {
                    string baseLang = cultureCode.Split('-')[0];
                    if (availableCultures.Contains(baseLang) && TryGetTranslation(key, baseLang, out translation))
                    {
                        return translation;
                    }
                }
            }

            return TryGetTranslation(key, "en", out translation) ? translation : key;
        }
        public static string DeterminePrimaryLanguageCode(string languages)
        {
            var primaryLanguage = languages
                .Split(',')
                .Select(lang => lang.Split(';'))
                .Select(parts => new
                {
                    Code = parts[0].Trim(),
                    Quality = double.TryParse(parts.ElementAtOrDefault(1)?.Replace("q=", ""), out var q) ? q : 1
                })
                .OrderByDescending(lang => lang.Quality) // Order by quality score
                .FirstOrDefault(lang => availableCultures.Contains(lang.Code) || availableCultures.Contains(lang.Code.Split('-')[0])); // Find the first valid language

            return primaryLanguage?.Code ?? "en"; // Return the determined code or default to English
        }
        private static bool TryGetTranslation(string key, string language, out string result)
        {
            result = null;
            if (translations.ContainsKey(key) && translations[key].ContainsKey(language))
            {
                result = translations[key][language];
                return true;
            }
            return false;
        }
    }
}