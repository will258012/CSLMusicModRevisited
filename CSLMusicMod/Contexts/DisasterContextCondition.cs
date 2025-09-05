﻿using ColossalFramework;
using LitJson;
using System.Collections.Generic;

namespace CSLMusicMod.Contexts
{
    /// <summary>
    /// This condition handles if a disaster is currently in your city
    /// </summary>
    public class DisasterContextCondition : RadioContextCondition
    {
        public int m_DisasterCountFrom = 0;

        public int m_DisasterCountTo = DisasterManager.MAX_DISASTER_COUNT;

        public bool m_Invert = false;

        public HashSet<string> m_DisasterFilter = new HashSet<string>();

        public HashSet<string> m_Collections = new HashSet<string>();

        public override bool Applies()
        {
            return m_Invert ? !_Applies() : _Applies();
        }

        private bool _Applies()
        {
            int disasterCount = Singleton<DisasterManager>.instance.m_disasterCount;

            int count = 0;

            for (int i = 0; i < disasterCount; ++i)
            {
                DisasterData data = Singleton<DisasterManager>.instance.m_disasters[i];

                if (data.Info != null && (data.m_flags & DisasterData.Flags.Active) != DisasterData.Flags.None)
                {
                    if (m_DisasterFilter.Count == 0 || m_DisasterFilter.Contains(data.Info.name))
                    {
                        ++count;
                    }
                }
            }

            return count >= m_DisasterCountFrom && count <= m_DisasterCountTo;
        }

        public static DisasterContextCondition LoadFromJson(JsonData json)
        {
            DisasterContextCondition context = new DisasterContextCondition
            {
                m_DisasterCountFrom = (int)json["from"],
                m_DisasterCountTo = (int)json["to"]
            };

            if (json.Keys.Contains("not"))
            {
                context.m_Invert = (bool)json["not"];
            }

            if (json.Keys.Contains("of"))
            {
                foreach (JsonData e in json["of"])
                {
                    context.m_DisasterFilter.Add((string)e);
                }
            }

            return context;
        }
    }
}

