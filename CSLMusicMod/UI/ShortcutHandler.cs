using AlgernonCommons;
using ColossalFramework.UI;
using CSLMusicMod.Helpers;
using UnityEngine;

namespace CSLMusicMod.UI
{
    /// <summary>
    /// Behavior that handles all shortcuts
    /// </summary>
    public class ShortcutHandler : MonoBehaviour
    {
        // Shortcut key variables
        private bool m_OpenPanelKey_IsDown = false;
        private bool m_NextTrackKey_IsDown = false;
        private bool m_NextStationKey_IsDown = false;

        private bool m_ModifierCtrl = false;
        private bool m_ModifierShift = false;
        private bool m_ModiferAlt = false;

        public void Start()
        {
            Logging.Message(ModOptions.Instance.ShortcutNextTrack);
            Logging.Message(ModOptions.Instance.ShortcutNextStation);
            Logging.Message(ModOptions.Instance.ShortcutOpenRadioPanel);
        }

        private bool ShortcutDown(ModOptions.Shortcut shortcut)
        {
            return shortcut.Key != KeyCode.None && Input.GetKeyDown(shortcut.Key) &&
            (shortcut.ModifierControl == m_ModifierCtrl) &&
            (shortcut.ModifierShift == m_ModifierShift) &&
            (shortcut.ModifierAlt == m_ModiferAlt);
        }

        private bool ShortcutUp(ModOptions.Shortcut shortcut)
        {
            return shortcut.Key == KeyCode.None || Input.GetKeyUp(shortcut.Key);
        }

        public void Update()
        {
            // Check if some other UI has the focus
            if (UIView.HasInputFocus())
            {
                m_NextTrackKey_IsDown = false;
                m_OpenPanelKey_IsDown = false;
                m_NextStationKey_IsDown = false;
                return;
            }

            m_ModifierCtrl = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
            m_ModifierShift = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
            m_ModiferAlt = (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt));

            //Next track
            if (ShortcutDown(ModOptions.Instance.ShortcutNextTrack))
            {
                m_NextTrackKey_IsDown = true;
            }
            else if (m_NextTrackKey_IsDown && ShortcutUp(ModOptions.Instance.ShortcutNextTrack))
            {
                m_NextTrackKey_IsDown = false;
                Logging.Message("Pressed shortcut for next track");
                AudioManagerHelper.NextTrack();
            }

            //Next station
            if (ShortcutDown(ModOptions.Instance.ShortcutNextStation))
            {
                m_NextStationKey_IsDown = true;
            }
            else if (m_NextStationKey_IsDown && ShortcutUp(ModOptions.Instance.ShortcutNextStation))
            {
                Logging.Message("Pressed shortcut for next station");
                m_NextStationKey_IsDown = false;
                AudioManagerHelper.NextStation();
            }

            //Panel
            if (ShortcutDown(ModOptions.Instance.ShortcutOpenRadioPanel))
            {
                m_OpenPanelKey_IsDown = true;
            }
            else if (m_OpenPanelKey_IsDown && ShortcutUp(ModOptions.Instance.ShortcutOpenRadioPanel))
            {
                m_OpenPanelKey_IsDown = false;
                Logging.Message("Pressed shortcut for hide/show panel");

                var radioPanel = AudioManagerHelper.CurrentRadioPanel;
                if (radioPanel != null)
                {
                    var visible = ReflectionHelper.GetPrivateField<bool>(radioPanel, "m_isVisible");

                    if (visible)
                        radioPanel.HideRadio();
                    else
                        radioPanel.ShowRadio();
                }
            }
        }
    }
}

