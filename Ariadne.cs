﻿using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Ariadne
{
    public class Ariadne : Mod
    {
        public static Ariadne Instance;

        new public string GetName() => "Ariadne";
        public override string GetVersion() => "v0.1";


        public static GlobalSettings settings { get; set; } = new GlobalSettings();

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

    }
}