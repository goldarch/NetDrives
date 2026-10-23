using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using Hardcodet.NetDrives.Properties;

namespace Hardcodet.NetDrives.Util
{
    public sealed class LocalizationManager : INotifyPropertyChanged
    {
        public sealed class LanguageOption
        {
            public string CultureName { get; set; }
            public string DisplayName { get; set; }
            public override string ToString() { return DisplayName; }
        }

        private static readonly LocalizationManager instance = new LocalizationManager();
        public static LocalizationManager Instance { get { return instance; } }

        private readonly IList<LanguageOption> supportedLanguages;
        public IList<LanguageOption> SupportedLanguages { get { return supportedLanguages; } }

        private CultureInfo currentCulture;
        public string CurrentCultureName
        {
            get { return currentCulture == null ? "en-US" : currentCulture.Name; }
        }

        private LocalizationManager()
        {
            supportedLanguages = new List<LanguageOption>
      {
        new LanguageOption { CultureName = "en-US", DisplayName = "English (US)" },
        new LanguageOption { CultureName = "zh-CN", DisplayName = "中文（简体）" }
      };

            ApplyCulture(Settings.Default.UiCulture);
        }

        public object this[string resourceKey]
        {
            get
            {
                if (String.IsNullOrEmpty(resourceKey)) return String.Empty;
                return Resources.ResourceManager.GetObject(resourceKey, Resources.Culture) ?? resourceKey;
            }
        }

        public void ApplyCulture(string cultureName)
        {
            CultureInfo culture;
            try
            {
                culture = String.IsNullOrWhiteSpace(cultureName)
                  ? new CultureInfo("en-US")
                  : new CultureInfo(cultureName);
            }
            catch
            {
                culture = new CultureInfo("en-US");
            }

            currentCulture = culture;
            Resources.Culture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            OnPropertyChanged("CurrentCultureName");
            OnPropertyChanged("Item[]");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}