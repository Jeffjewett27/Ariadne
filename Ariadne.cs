using Ariadne.Visual;
using Modding;
using System;
using UnityEngine;

namespace Ariadne
{
    public class Ariadne : Mod, IGlobalSettings<GlobalSettings>, ILocalSettings<SaveSettings>, ICustomMenuMod
    {
        public static Ariadne Instance;

        new public string GetName() => "Ariadne";
        public override string GetVersion() => "0.1";


        public static GlobalSettings settings { get; set; } = new GlobalSettings();
        public void OnLoadGlobal(GlobalSettings s) => settings = s;
        public GlobalSettings OnSaveGlobal() => settings;
        public static SaveSettings saveSettings { get; set; } = new SaveSettings();
        public void OnLoadLocal(SaveSettings s) {
            saveSettings = s;
            if (saveSettings.saveId == null || saveSettings.saveId.Length == 0)
            {
                saveSettings.saveId = Guid.NewGuid().ToString().Substring(0, 6);
            }
        }
        public SaveSettings OnSaveLocal() => saveSettings;

        public HitboxTrackerManager TrackerManager { get; set; }


        public override void Initialize()
        {
            Log("Initializing");

            Instance = this;

            TrackerManager = new HitboxTrackerManager();
            if (settings.TrackHitboxes) TrackerManager.Load();

            //UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;

            Log("Initialized");
        }

        private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
        {
            if (HeroController.SilentInstance != null && Camera.main.GetComponent<WhiteOcclusion>() == null)
            {
                Camera.main.gameObject.AddComponent<WhiteOcclusion>();
            }
        }

        public static void MLog(string msg)
        {
            Instance?.Log(msg);
        }

        public static void DebugName(string pattern, int numDraws)
        {
            HitboxTracker.debugDraws = numDraws;
            HitboxTracker.debugPattern = pattern;
        }

        public static void SetVisibility(int vis)
        {
            settings.ShowHitBoxes = (ShowHitbox)vis;
            settings.TrackHitboxes = vis > 0;
        }

        public static void DebugAB()
        {
            settings.DebugAB = !settings.DebugAB;
        }

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates) =>
            ModMenu.CreateMenuScreen(modListMenu);
        public bool ToggleButtonInsideMenu => false;

    }
}