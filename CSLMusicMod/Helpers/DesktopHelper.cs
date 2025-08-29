using AlgernonCommons;
using System;
using System.Diagnostics;
using System.Globalization;
using UnityEngine;

namespace CSLMusicMod.Helpers
{
    public static class DesktopHelper
    {
        /// <summary>
        /// Opens file with external program
        /// </summary>
        /// <param name="path"></param>
        public static void OpenFileExternally(string path)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    Process.Start(path);
                    break;
                case PlatformID.Win32Windows:
                    Process.Start(path);
                    break;
                case PlatformID.Unix:
                    Process.Start("xdg-open", path);
                    break;
                case PlatformID.MacOSX:
                    Process.Start("open", path);
                    break;
            }
        }
        /// <summary>
        /// Unity's Mono runtime has a known limitation where <see cref="CultureInfo.CurrentCulture"/> 
        /// and <see cref="CultureInfo.CurrentUICulture"/> consistently return "en-US", regardless of 
        /// the system's actual locale settings.
        /// 
        /// As a partial workaround, <see cref="GetUserDefaultLCID()"/> and <see cref="Application.systemLanguage"/> can be used to detect 
        /// the system language, though note that regional formats (such as time, date, and number 
        /// formatting) may still default to <see cref="CultureInfo"/>'s conventions rather than reflecting system preferences.
        /// </summary>
        public static CultureInfo GetCorrectCultureInfo() => Environment.OSVersion.Platform == PlatformID.Win32NT
                ? CultureInfo.GetCultureInfo(GetUserDefaultLCID())
                : CultureInfo.GetCultureInfo(MapSystemLanguageToCultureCode(Application.systemLanguage));

        [System.Runtime.InteropServices.DllImport("KERNEL32.DLL")]
        private static extern int GetUserDefaultLCID();

        private static string MapSystemLanguageToCultureCode(SystemLanguage lang)
        {
            //Supports languages supported by AlgernonCommons for now
            switch (lang)
            {
                case SystemLanguage.Czech: return "cs-CZ";
                case SystemLanguage.Chinese: return "zh-CN";
                case SystemLanguage.ChineseSimplified: return "zh-Hans";
                case SystemLanguage.ChineseTraditional: return "zh-Hant";
                case SystemLanguage.Dutch: return "nl-NL";
                case SystemLanguage.English: return "en-US";
                case SystemLanguage.Finnish: return "fi-FI";
                case SystemLanguage.French: return "fr-FR";
                case SystemLanguage.German: return "de-DE";
                case SystemLanguage.Hungarian: return "hu-HU";
                case SystemLanguage.Indonesian: return "id-ID";
                case SystemLanguage.Italian: return "it-IT";
                case SystemLanguage.Japanese: return "ja-JP";
                case SystemLanguage.Korean: return "ko-KR";
                case SystemLanguage.Polish: return "pl-PL";
                case SystemLanguage.Portuguese: return "pt-BR";
                case SystemLanguage.Russian: return "ru-RU";
                case SystemLanguage.Slovak: return "sk-SK";
                case SystemLanguage.Spanish: return "es-ES";
                case SystemLanguage.Turkish: return "tr-TR";
                case SystemLanguage.Ukrainian: return "uk-UA";

                default:
                    Logging.KeyMessage("GetCorrectCultureInfo(): Not supported SystemLanguage" + lang);
                    return "en-US";
            }
        }
    }
}