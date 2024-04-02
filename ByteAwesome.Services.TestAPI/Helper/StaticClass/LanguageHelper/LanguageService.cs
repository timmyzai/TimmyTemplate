using System.Xml.Linq;
using Serilog;

namespace ByteAwesome.Services.TestAPI
{
    public static class LanguageService
    {
        public static Dictionary<string, Dictionary<string, string>> translations = new Dictionary<string, Dictionary<string, string>>();
        private static IHttpContextAccessor _httpContextAccessor;
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            LoadLanguages();
        }
        private static void LoadLanguages()
        {
            try
            {
                string[] filePaths = Directory.GetFiles("../Localisation", "*.xml");
                foreach (string filePath in filePaths)
                {
                    XDocument doc = XDocument.Load(filePath);
                    string culture = Path.GetFileNameWithoutExtension(filePath).Split('_').Last();
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
                Log.Error(ex, $"{ErrorCodes.General.LoadLanguages} - Error loading translations: {ex.Message}");
            }
        }
        public static string Translate(string key, string userLangSetting = null)
        {
            if (userLangSetting != null && translations.ContainsKey(key) && translations[key].ContainsKey(userLangSetting))
            {
                return translations[key][userLangSetting];
            }
            var languages = _httpContextAccessor?.HttpContext?.Request?.Headers["Accept-Language"].ToString().Split(',')
                .Select(s => new
                {
                    Code = s.Split(';')[0],
                    Quality = double.TryParse(s.Split(';').ElementAtOrDefault(1)?.Replace("q=", ""), out var q) ? q : 1
                })
                .OrderByDescending(l => l.Quality).ToList();
            if (languages == null || !languages.Any())
            {
                if (translations.ContainsKey(key) && translations[key].ContainsKey("en"))
                {
                    return translations[key]["en"];
                }
            }
            foreach (var lang in languages)
            {
                if (translations.ContainsKey(key) && translations[key].ContainsKey(lang.Code))
                {
                    return translations[key][lang.Code];
                }
                var langRoot = lang.Code.Split('-')[0];
                if (translations.ContainsKey(key) && translations[key].ContainsKey(langRoot))
                {
                    return translations[key][langRoot];
                }
            }
            return key;
        }
    }
}