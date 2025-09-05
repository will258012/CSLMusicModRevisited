using AlgernonCommons;
using CSLMusicMod.Contexts;
using LitJson;

namespace CSLMusicMod
{
    /// <summary>
    /// General interface for a context condition.
    /// </summary>
    public abstract class RadioContextCondition
    {
        public abstract bool Applies();

        public static RadioContextCondition LoadFromJsonUsingType(JsonData json)
        {
            RadioContextCondition context = null;

            switch ((string)json["type"])
            {
                case "time":
                    context = TimeContextCondition.LoadFromJson(json);
                    break;
                case "weather":
                    context = WeatherContextCondition.LoadFromJson(json);
                    break;
                case "mood":
                    context = MoodContextCondition.LoadFromJson(json);
                    break;
                case "disaster":
                    context = DisasterContextCondition.LoadFromJson(json);
                    break;
                default:
                    Logging.Error($"Unknown context type: {(string)json["type"]}");
                    break;
            }

            return context;
        }
    }
}

