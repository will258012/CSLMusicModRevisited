using System.IO;

namespace CSLMusicMod
{
    /// <summary>
    /// Class that describes a custom song.
    /// </summary>
    public class UserRadioContent
    {
        public RadioContentInfo.ContentType m_ContentType;

        public string m_Name;

        public string m_DisplayName;

        public string m_Collection;

        public string m_FileName;

        public bool m_isVanilla;

        public UserRadioChannel[] m_Channels;

        // Post-launch

        public RadioContentInfo m_VanillaContentInfo;
        public UserRadioContent(string collection, string filename)
        {
            m_Name = "CSLMusic" + "/" + collection + "/" + Path.GetFileNameWithoutExtension(filename);
            m_DisplayName = Path.GetFileNameWithoutExtension(filename);
            m_FileName = filename;
            m_Collection = collection;

            string basename = Path.GetFileNameWithoutExtension(filename);

            if (basename.EndsWith("#blurb"))
            {
                m_ContentType = RadioContentInfo.ContentType.Blurb;
                m_DisplayName = m_DisplayName.Substring(0, m_DisplayName.Length - "#blurb".Length);
            }
            else if (basename.EndsWith("#talk"))
            {
                m_ContentType = RadioContentInfo.ContentType.Talk;
                m_DisplayName = m_DisplayName.Substring(0, m_DisplayName.Length - "#talk".Length);
            }
            else if (basename.EndsWith("#commercial"))
            {
                m_ContentType = RadioContentInfo.ContentType.Commercial;
                m_DisplayName = m_DisplayName.Substring(0, m_DisplayName.Length - "#commercial".Length);
            }
            else if (basename.EndsWith("#broadcast"))
            {
                m_ContentType = RadioContentInfo.ContentType.Broadcast;
                m_DisplayName = m_DisplayName.Substring(0, m_DisplayName.Length - "#broadcast".Length);
            }
            else
            {
                m_ContentType = RadioContentInfo.ContentType.Music;
            }
        }
    }
}

