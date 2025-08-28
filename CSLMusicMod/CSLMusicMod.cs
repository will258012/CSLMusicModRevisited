using AlgernonCommons;
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

        /// <summary>
        /// Logs into the debug log if enabled.
        /// </summary>
        /// <returns>The log.</returns>
        /// <param name="message">Message.</param>
        public static void Log(object message) => Logging.Message(message);

        public override void LoadSettings() => ModOptions.Instance.LoadSettings();

        public override void SaveSettings() => ModOptions.Instance.SaveSettings();

    }
}

