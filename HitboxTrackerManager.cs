using Ariadne.Visual;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Ariadne
{
    public class HitboxTrackerManager
    {
        public static bool TrackingState { get; private set; }

        public HitboxTracker HitboxTracker { get; private set; }
        public HitboxRenderer HitboxRenderer { get; private set; }

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
                var hitboxGO = new GameObject();
                HitboxTracker = hitboxGO.AddComponent<HitboxTracker>();
                HitboxRenderer = hitboxGO.AddComponent<HitboxRenderer>();
                Ariadne.Instance.Log($"Created HitboxTracker for scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
            } else
            {
                Ariadne.Instance.Log("Could not create HitboxTracker in non-gameplay scene");
            }
        }

        private void DestroyHitboxRender()
        {
            if (HitboxTracker != null)
            {
                Object.Destroy(HitboxTracker);
                HitboxTracker = null;
            }
        }

        private void UpdateHitboxRender(GameObject go)
        {
            if (HitboxTracker != null)
            {
                HitboxTracker.UpdateHitbox(go);
            }
        }
    }
}