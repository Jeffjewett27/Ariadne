using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Ariadne
{
    public class HitboxTrackerManager
    {
        public static bool TrackingState { get; private set; }
        public HitboxTracker hitboxTracker;

        public void Load()
        {
            TrackingState = Ariadne.settings.TrackHitboxes;
            Unload();
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += CreateHitboxRender;


            ModHooks.ColliderCreateHook += UpdateHitboxRender;

            CreateHitboxRender();
        }

        public void Unload()
        {
            TrackingState = Ariadne.settings.TrackHitboxes;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= CreateHitboxRender;

            ModHooks.ColliderCreateHook -= UpdateHitboxRender;
            DestroyHitboxRender();
        }

        private void CreateHitboxRender(Scene current, Scene next) => CreateHitboxRender();

        private void CreateHitboxRender()
        {
            DestroyHitboxRender();
            if (GameManager.instance.IsGameplayScene())
            {
                hitboxTracker = new GameObject().AddComponent<HitboxTracker>();
                Ariadne.Instance.Log("Created HitboxTracker");
            } else
            {
                Ariadne.Instance.Log("Could not create HitboxTracker in non-gameplay scene");
            }
        }

        private void DestroyHitboxRender()
        {
            if (hitboxTracker != null)
            {
                Object.Destroy(hitboxTracker);
                hitboxTracker = null;
            }
        }

        private void UpdateHitboxRender(GameObject go)
        {
            if (hitboxTracker != null)
            {
                hitboxTracker.UpdateHitbox(go);
            }
        }
    }
}