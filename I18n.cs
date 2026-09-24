using System;
using System.IO;

namespace ParamToolbox
{
    public enum AppLanguage
    {
        English,
        Vietnamese
    }

    public static class I18n
    {
        private static readonly string ConfigPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            ".paramtoolbox_lang"
        );

        private static AppLanguage _currentLanguage = AppLanguage.English;

        static I18n()
        {
            // Default to English. Check if a saved preference exists.
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string saved = File.ReadAllText(ConfigPath).Trim();
                    if (saved.Equals("vi", StringComparison.OrdinalIgnoreCase))
                    {
                        _currentLanguage = AppLanguage.Vietnamese;
                    }
                }
            }
            catch
            {
                _currentLanguage = AppLanguage.English;
            }
        }

        public static AppLanguage CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                _currentLanguage = value;
                try
                {
                    File.WriteAllText(ConfigPath, _currentLanguage == AppLanguage.Vietnamese ? "vi" : "en");
                }
                catch
                {
                    // Ignore persistence errors
                }
            }
        }

        public static void ToggleLanguage()
        {
            CurrentLanguage = _currentLanguage == AppLanguage.English ? AppLanguage.Vietnamese : AppLanguage.English;
        }

        public static string T(string en, string vi)
        {
            return _currentLanguage == AppLanguage.Vietnamese ? vi : en;
        }
    }
}
