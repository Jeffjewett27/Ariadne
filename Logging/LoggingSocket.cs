using Ariadne.HitboxUtils;
using Ariadne.Socket;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ariadne.Logging
{
    [RequireComponent(typeof(HitboxTracker))]
    public class LoggingSocket : MonoBehaviour
    {
        private HitboxTracker hitboxTracker;
        private Camera camera;

        float intervalMs;
        float lastUpdateTime;
        bool isFirst;

        private void Start ()
        {
            intervalMs = Ariadne.settings.SocketIntervalMS;
            lastUpdateTime = Time.realtimeSinceStartup * 1000;
            WebSocketServerManager.Instance.Open();
            camera = Camera.main;
            isFirst = true;
        }

        private void Update ()
        {
            if (hitboxTracker == null)
            {
                hitboxTracker = GetComponent<HitboxTracker>();
            }

            if (Camera.main == null || GameManager.instance == null
                || GameManager.instance.isPaused || GameManager.instance.IsInSceneTransition
                || hitboxTracker == null)
            {
                return;
            }

            float curTimeMs = Time.realtimeSinceStartup * 1000;
            int numIntervals = (int)((curTimeMs - lastUpdateTime) / intervalMs);
            if (numIntervals == 0)
            {
                return;
            }
            lastUpdateTime += numIntervals * intervalMs;

            //string message = $"{lastUpdateTime}, {GameManager.instance.inventoryFSM.ActiveStateName}"; ;
            //Ariadne.MLog($"broadcast {message}");
            string message = GenerateMessage();
            WebSocketServerManager.Instance.BroadcastMessage(message);
            isFirst = false;
        }

        private string GenerateMessage()
        {
            LiveData liveData = new()
            {
                sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                currentTime = Time.realtimeSinceStartup,
                inventoryState = GameManager.instance.inventoryFSM.ActiveStateName,
                camera = CameraColliderRecord(camera),
                worldObjects = new Dictionary<int, EntitySnapshot>()
            };

            var playerData = GameManager.instance.playerData;
            PlayerSnapshot playerSnapshot = new()
            {
                maxHealth = playerData.maxHealth,
                health = playerData.health,
                healthBlue = playerData.healthBlue,
                profileId = playerData.profileID,
                dash = playerData.hasDash,
                claw = playerData.hasWalljump,
                wings = playerData.hasDoubleJump,
                cdash = playerData.hasSuperDash,
                showdowdash = playerData.hasShadowDash,
                isma = playerData.hasAcidArmour
            };
            liveData.playerSnapshot = playerSnapshot;

            foreach (var layerPair in hitboxTracker.ColliderLayers)
            {
                var hbType = layerPair.Key;
                var layer = layerPair.Value;
                if (hbType.GetMinShowLevel() > ShowHitbox.Show) continue;
                if (hbType.GetIsStatic())
                {
                    if (isFirst && hbType == HitboxType.Terrain)
                    {
                        var staticLayer = (StaticColliderLayer)layer;
                        liveData.terrainSegmentation =
                            SegmentationFromStaticLayer(staticLayer.GetShapesPaths());
                    }
                    else if (isFirst && hbType == HitboxType.StaticHazard)
                    {
                        var staticLayer = (StaticColliderLayer)layer;
                        liveData.staticHazardSegmentation =
                            SegmentationFromStaticLayer(staticLayer.GetShapesPaths());
                    }
                }
                else
                {
                    foreach (var collider in layer.Colliders)
                    {

                        if (collider == null || !collider.isActiveAndEnabled) continue;
                        int entityIndex = collider.gameObject.GetInstanceID();
                        EntitySnapshot snapshot = SnapshotFromCollider(collider, hbType);
                        if (!liveData.worldObjects.ContainsKey(entityIndex))
                        {
                            liveData.worldObjects.Add(entityIndex, snapshot);
                        }
                    }
                }
            }
            string message = JsonHandler.Serialize(liveData);
            return message;
        }

        private ColliderRecord CameraColliderRecord(Camera camera)
        {
            float worldHeight = 2 * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView / 2) * -camera.transform.GetPositionZ();
            return new()
            {
                posX = camera.transform.position.x,
                posY = camera.transform.position.y,
                height = worldHeight,
                width = worldHeight * camera.aspect
            };
        }

        private List<List<List<float>>> SegmentationFromStaticLayer(IEnumerable<List<List<Vector2>>> shapesPaths)
        {
            return shapesPaths.SelectMany(paths => paths) // flatten IEnumerable
                .Select(innerList => innerList
                    .Select(v => new List<float> { v.x, v.y }).ToList() // vector to list
                )
                .ToList();
        }

        private EntitySnapshot SnapshotFromCollider(Collider2D collider, HitboxType hitboxType)
        {
            var center = (Vector2)collider.transform.position + collider.offset;
            var extent = collider.bounds.extents; //TODO account for rotation
            var fsm = collider.transform.gameObject.GetComponent<PlayMakerFSM>();
            string fsmState = fsm?.ActiveStateName;

            EntitySnapshot snapshot = new()
            {
                name = collider.name,
                hitboxType = hitboxType,
                posX = center.x,
                posY = center.y,
                width = extent.x,
                height = extent.y,
                isActive = collider.isActiveAndEnabled,
                fsmState = fsmState
            };
            return snapshot;
        }

        private void OnApplicationQuit()
        {
            WebSocketServerManager.Instance.Close();
        }
    }
}
