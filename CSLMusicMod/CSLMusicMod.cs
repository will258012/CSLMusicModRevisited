using AlgernonCommons.Patching;
using AlgernonCommons.Translation;
using CSLMusicMod.UI;
using ICities;

namespace CSLMusicMod
{
    /// <summary>
    /// Main mod class
    /// </summary>
    public class CSLMusicMod : PatcherMod<SettingsUI, PatcherBase>, IUserMod
    {
        public static System.Random RANDOM = new System.Random();
        public override string BaseName => "CSL Music Mod Revisited";
        public string Description => Translations.Translate("MOD_DESCRIPTION");
        public override string HarmonyID => "Will258012.CSLMusicMod";
        public override void LoadSettings() => ModOptions.Instance.LoadSettings();
        public override void SaveSettings() => ModOptions.Instance.SaveSettings();

    }
}

