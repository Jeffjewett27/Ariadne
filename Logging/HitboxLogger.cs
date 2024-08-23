using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Ariadne.Visual;
using Ariadne.HitboxUtils;
using Ariadne.Logging;
using Newtonsoft.Json;
using System.Data;
using System.IO;
using Newtonsoft.Json.Converters;
using UnityEngine.Profiling;
using HutongGames.PlayMaker;

namespace Ariadne.Logging
{
    [RequireComponent(typeof(HitboxTracker))]
    public class HitboxLogger : MonoBehaviour
    {
        private HitboxTracker hitboxTracker;
        private Dictionary<Collider2D, HitboxData> colliderLogs;
        private List<HitboxCategoryData> staticLayerLogs;
        private List<short> deviations;
        private List<int> skippedFrames;
        private Camera camera;

        private int logCounter;
        private int timestep;
        private int numRecords;
        private float loggingIntervalMs;
        private float entryUpdateTime;

        private string logIdentifier;
        private string sceneName;
        private DateTime entryTime;
        private DateTime lastTime;
        private List<ColliderRecord> cameraPositions;
        private ColliderRecord lastCameraPosition;

        private void Start()
        {
            hitboxTracker = GetComponent<HitboxTracker>();
            colliderLogs = new Dictionary<Collider2D, HitboxData>();
            staticLayerLogs = new List<HitboxCategoryData>();
            deviations = new List<short>();
            skippedFrames = new List<int>();

            loggingIntervalMs = Ariadne.settings.LoggingIntervalMS;
            logCounter = 0;
            timestep = -1;
            numRecords = 0;

            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            camera = Camera.main;
            cameraPositions = new List<ColliderRecord>();

            if (camera != null)
            {
                // Attach the CameraScreenshotHelper script to the main camera
                CameraLoggerHelper helper = camera.gameObject.AddComponent<CameraLoggerHelper>();
                // Set the reference to this script
                helper.hitboxLogger = this;
            }
            else
            {
                Debug.LogError("Main camera not found!");
            }
        }

        public void LogUpdate()
        {
            if (Camera.main == null || GameManager.instance == null
                || GameManager.instance.isPaused || GameManager.instance.IsInSceneTransition
                || GameManager.instance.inventoryFSM.ActiveStateName == "Opened")
            {
                return;
            }
            float realTimeSinceStartup = Time.realtimeSinceStartup;
            if (timestep == -1)
            {
                entryTime = DateTime.Now;
                entryUpdateTime = realTimeSinceStartup;
            }

            float targetMs = timestep * loggingIntervalMs;
            float actualMs = (realTimeSinceStartup - entryUpdateTime) * 1000;
            int timestepIncrement = Mathf.CeilToInt((actualMs - targetMs) / loggingIntervalMs);
            if (timestep == -1)
            {
                // weird edge case idk
                timestepIncrement = 1;
                actualMs = targetMs;
            }
            if (timestepIncrement < 1)
            {
                return;
            }
                lastTime = DateTime.Now;
            if (timestepIncrement > 1)
            {
                // Skipped a frame
                Ariadne.MLog($"[{timestep}] Skipped {timestepIncrement - 1} frames: {actualMs - targetMs}, {deviations.Count}");

                skippedFrames.Add(timestep);
                skippedFrames.Add(timestep + timestepIncrement - 1);
            }
            timestep += timestepIncrement;

            deviations.Add((short)(actualMs - targetMs));
            
            bool shouldLogStatic = staticLayerLogs.Count == 0;

            foreach (var layerPair in hitboxTracker.ColliderLayers) 
            {
                var hbType = layerPair.Key;
                var layer = layerPair.Value;
                if (hbType.GetMinShowLevel() > ShowHitbox.Show) continue;
                if (hbType.GetIsStatic())
                {
                    // only log static once
                    if (!shouldLogStatic) continue;

                    var staticLayer = (StaticColliderLayer)layer;
                    LogStaticLayer(timestep, hbType, staticLayer.GetShapesPaths());
                } else
                {
                    foreach (var collider in layer.Colliders)
                    {
                        if (collider == null) continue;
                        LogCollider(timestep, hbType, collider);
                    }
                }
            }
            LogCamera();

            ScreenCapture.CaptureScreenshot(GetLogImagePath(timestep));
            logCounter++;
        }


        private void LogCamera()
        {
            if (camera == null) return;
            //camera is at roughly z=-38. calculate the world coordinate range covered by half FOV
            float worldHeight = 2 * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView / 2) * -camera.transform.GetPositionZ();
            var newRecord = new ColliderRecord()
            {
                timestamp = timestep,
                posX = camera.transform.position.x,
                posY = camera.transform.position.y,
                height = worldHeight,
                width = worldHeight * camera.aspect
            };
            numRecords++;
            if (lastCameraPosition == null)
            {
                lastCameraPosition = newRecord;
                cameraPositions.Add(newRecord);
                return;
            }
            var diff = lastCameraPosition.Diff(newRecord);
            if (!diff.IsEmpty())
            {
                cameraPositions.Add(diff);
                lastCameraPosition = newRecord;
            }
        }

        private void LogCollider(int timestamp, HitboxType hbType, Collider2D collider)
        {
            if (!colliderLogs.ContainsKey(collider))
            {
                if (!collider.isActiveAndEnabled) return; //only start logging once active
                var newRecord = RecordFromCollider(timestamp, collider);
                colliderLogs[collider] = new HitboxData()
                {
                    hitboxType = hbType,
                    name = collider.name,
                    records = new()
                    {
                        newRecord
                    },
                    current = newRecord
                };
                numRecords++;
            } else
            {
                var log = colliderLogs[collider];
                var records = log.records;
                var newRecord = RecordFromCollider(timestamp, collider);
                var diff = log.current.Diff(newRecord);
                if (!diff.IsEmpty())
                {
                    records.Add(diff);
                    log.current = newRecord;
                    numRecords++;
                }
            }
        }

        private void LogStaticLayer(int timestamp, HitboxType hitboxType, IEnumerable<List<List<Vector2>>> shapesPaths)
        {
            var hitboxDatas = new List<HitboxData>();
            foreach (var shapePaths in shapesPaths)
            {
                var newRecord = RecordFromShapePaths(timestamp, shapePaths);
                var hitboxData = new HitboxData()
                {
                    hitboxType = hitboxType,
                    name = "",
                    records = new()
                    {
                        newRecord
                    },
                    current = newRecord,
                    segmentationBounds = shapePaths.Select(innerList => innerList
                        .SelectMany(v => new List<float> { v.x, v.y }).ToList()
                    ).ToList()
                };
                hitboxDatas.Add(hitboxData);
            }
            // Don't log if empty
            if (hitboxDatas.Count == 0) return;

            var hitboxCategory = new HitboxCategoryData()
            {
                hitboxType = hitboxType,
                entities = hitboxDatas
            };
            staticLayerLogs.Add(hitboxCategory);
        }

        private ColliderRecord RecordFromCollider(int timestamp, Collider2D collider)
        {
            var center = (Vector2)collider.transform.position + collider.offset;
            var extent = collider.bounds.extents; //TODO account for rotation
            var fsm = collider.transform.gameObject.GetComponent<PlayMakerFSM>();
            string fsmState = fsm?.ActiveStateName;

            var record = new ColliderRecord()
            {
                timestamp = timestamp,
                posX = center.x,
                posY = center.y,
                width = extent.x,
                height = extent.y,
                isActive = collider.isActiveAndEnabled,
                fsmState = fsmState
            };
            return record;
        }

        private ColliderRecord RecordFromShapePaths(int timestamp, List<List<Vector2>> shapePaths)
        {
            float minX, maxX, minY, maxY;
            (minX, maxX, minY, maxY) = GetMinMaxCoordinates(shapePaths);
            float centerX = (maxX - minX) / 2;
            float centerY = (maxY - minY) / 2;
            float width = maxX - centerX;
            float height = maxY - centerY;

            var record = new ColliderRecord()
            {
                timestamp = timestamp,
                posX = centerX,
                posY = centerY,
                width = width,
                height = height,
                isActive = true
            };
            return record;
        }

        private static (float minX, float maxX, float minY, float maxY) GetMinMaxCoordinates(List<List<Vector2>> shapePaths)
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (var path in shapePaths)
            {
                foreach (var point in path)
                {
                    if (point.x < minX) minX = point.x;
                    if (point.x > maxX) maxX = point.x;
                    if (point.y < minY) minY = point.y;
                    if (point.y > maxY) maxY = point.y;
                }
            }

            return (minX, maxX, minY, maxY);
        }

        void OnDestroy() => LogToFile();

        void LogToFile()
        {
            Ariadne.MLog($"Logged {colliderLogs.Count} objects for {timestep} ticks totaling {numRecords}");
            string logfilename = GetLogFilePath();
            if (logfilename == null) return;

            var categories = colliderLogs.Select(pair => pair.Value)
                .GroupBy(hbData => hbData.hitboxType).Select(group => new HitboxCategoryData()
                {
                    hitboxType = group.Key,
                    entities = group.ToList()
                }).Concat(staticLayerLogs).ToList();

            var sceneData = new SceneData()
            {
                sceneName = sceneName,
                entryTime = entryTime,
                exitTime = lastTime,
                intervalMs = loggingIntervalMs,
                camera = cameraPositions,
                categories = categories,
                deviationsMs = deviations,
                skippedFrames = skippedFrames
            };

            string jsonString = JsonHandler.Serialize(sceneData);
            File.WriteAllText(logfilename, jsonString);
        }

        string GetLogDirPath()
        {
            var baseDir = Ariadne.settings.LogFolder;
            if (!Directory.Exists(baseDir)) return null;
            var saveDir = Path.Combine(baseDir, Ariadne.saveSettings.saveId);
            if (!Directory.Exists(saveDir)) Directory.CreateDirectory(saveDir);
            return saveDir;
        }

        string GetLogIdentifier()
        {
            TimeSpan t = entryTime - new DateTime(2023, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;
            return $"{sceneName}_{secondsSinceEpoch}";
        }

        string GetLogFilePath()
        {
            var saveDir = GetLogDirPath();
            var filename = $"{GetLogIdentifier()}.json";
            return Path.Combine(saveDir, filename);
        }

        string GetLogImagePath(int logCount)
        {
            var saveDir = GetLogDirPath();
            if (saveDir == null) return null;

            var imageDir = Path.Combine(saveDir, "images");
            if (!Directory.Exists(imageDir)) Directory.CreateDirectory(imageDir);

            var logImageDir = Path.Combine(imageDir, sceneName);
            if (!Directory.Exists(logImageDir)) Directory.CreateDirectory(logImageDir);

            var filename = $"{GetLogIdentifier()}_{logCount}.png";
            return Path.Combine(logImageDir, filename);
        }
    }
}