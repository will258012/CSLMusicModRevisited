using AlgernonCommons;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CSLMusicMod
{
    /// <summary>
    /// This class initializes the custom music stations when the game is loading.
    /// </summary>
    public class ChannelInitializer : MonoBehaviour
    {
        private bool _isInitialized;
        private Dictionary<string, RadioChannelInfo> _customPrefabs;

        /// <summary>
        /// Initializes the custom radio channels
        /// </summary>
        protected void InitializeImpl()
        {
            UserRadioCollection collection = Loading.UserRadioContainer;

            foreach (UserRadioChannel channel in collection.m_Stations.Values)
            {
                // Creates the actual radio station object that the games uses.
                CreatePrefab(channel.m_Name, "Default", new Action<RadioChannelInfo>((RadioChannelInfo obj) =>
                {
                    obj.m_stateChain = channel.m_StateChain;
                    obj.m_Atlas = channel.GetThumbnailAtlas(obj.m_Atlas.material);
                    obj.m_Thumbnail = "thumbnail";
                }));
            }
        }

        public void Awake()
        {
            DontDestroyOnLoad(this);
            _customPrefabs = new Dictionary<string, RadioChannelInfo>();
        }

        public void OnEnable()
        {
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
        }

        public void OnDisable()
        {
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }

        void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single)
            {
                _customPrefabs.Clear();
                _isInitialized = false;
            }
        }

        public void Update()
        {
            if (!_isInitialized)
            {
                // Wait until the game got its radio channels.
                // Then we can inject our custom channels.
                RadioChannelCollection collection = Resources.FindObjectsOfTypeAll<RadioChannelCollection>().FirstOrDefault();

                if (collection != null && collection.isActiveAndEnabled)
                {
                    LoadingManagerHelper.QueueLoadingAction(() =>
                        {
                            InitializeImpl();
                            PrefabCollection<RadioChannelInfo>.InitializePrefabs("CSLMusicChannel ", _customPrefabs.Values.ToArray(), null);
                        });
                    _isInitialized = true;
                }
            }
        }

        protected void CreatePrefab(string newPrefabName, string originalPrefabName, Action<RadioChannelInfo> setupAction)
        {
            var originalPrefab = FindOriginalPrefab(originalPrefabName);

            if (originalPrefab == null)
            {
                Logging.Error(string.Format("AbstractInitializer#CreatePrefab - Prefab '{0}' not found (required for '{1}')", originalPrefabName, newPrefabName));
                return;
            }
            if (_customPrefabs.ContainsKey(newPrefabName))
            {
                return;
            }
            var newPrefab = ClonePrefab(originalPrefab, newPrefabName, transform);
            if (newPrefab == null)
            {
                Logging.Error($"AbstractInitializer#CreatePrefab - Couldn't make prefab '{newPrefabName}'");
                return;
            }
            setupAction.Invoke(newPrefab);
            _customPrefabs.Add(newPrefabName, newPrefab);
        }

        protected static RadioChannelInfo ClonePrefab(RadioChannelInfo originalPrefab, string newName, Transform parentTransform)
        {
            var instance = Instantiate(originalPrefab.gameObject);
            instance.name = newName;
            var newPrefab = instance.GetComponent<RadioChannelInfo>();
            newPrefab.m_Atlas = originalPrefab.m_Atlas;
            newPrefab.m_Thumbnail = originalPrefab.m_Thumbnail;
            instance.SetActive(false);
            newPrefab.m_prefabInitialized = false;
            return newPrefab;
        }

        protected static RadioChannelInfo FindOriginalPrefab(string originalPrefabName)
        {
            RadioChannelInfo foundPrefab;
            foundPrefab = Resources.FindObjectsOfTypeAll<RadioChannelInfo>().FirstOrDefault(netInfo => netInfo.name == originalPrefabName);
            return foundPrefab ?? null;
        }
    }
}

