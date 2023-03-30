using System;
using System.Collections.Generic;
using GlobalEnums;
using UnityEngine;
using System.Linq;
using static Ariadne.HitboxTracker;

namespace Ariadne
{
    public class HitboxTracker: MonoBehaviour
    {
        // ReSharper disable once StructCanBeMadeReadOnly
        public struct HitboxType : IComparable<HitboxType>
        {
            public static readonly HitboxType Knight = new("Knight", Color.yellow, 0);                     // yellow
            public static readonly HitboxType Enemy = new("Enemy", new Color(0.8f, 0, 0), 1);       // red      
            public static readonly HitboxType Attack = new("Attack", Color.cyan, 2);                       // cyan
            public static readonly HitboxType Terrain = new("Terrain", new Color(0.6f, 0.6f, 0), 3);     // green
            public static readonly HitboxType Trigger = new("Trigger", new Color(0.5f, 0.5f, 1f), 4); // blue
            public static readonly HitboxType Breakable = new("Breakable", new Color(1f, 0.75f, 0.8f), 5); // pink
            public static readonly HitboxType Transition = new("Transition", new Color(0.0f, 0.0f, 0.5f), 6); // dark blue
            public static readonly HitboxType Switch = new("Switch", new Color(0.8f, 0.8f, 0.8f), 3); // light gray
            public static readonly HitboxType Gate = new("Gate", new Color(0.5f, 0.5f, 0.5f), 3); // gray
            public static readonly HitboxType Bottle = new("GrubBottle", new Color(0.5f, 0.9f, 0.5f), 3); // greenish
            public static readonly HitboxType Bench = new("Bench", new Color(0.2f, 0.2f, 0.2f), 3); // dark gray
            public static readonly HitboxType HazardRespawn = new("HazardRespawn", new Color(0.5f, 0.0f, 0.5f), 3); // purple 
            public static readonly HitboxType Other = new("Other", new Color(0.9f, 0.6f, 0.4f), 3); // orange
            public static readonly HitboxType Highlighted = new("Highlighted", new Color(1f, 1f, 1f), 3); // white
            public static readonly HitboxType GeoStore = new("GeoStore", new Color(0.6f, 0.6f, 0.2f), 3); // yellowish
            public static readonly HitboxType GeoToken = new("GeoToken", new Color(0.9f, 0.9f, 0.2f), 3); // more yellowish
            public static readonly HitboxType SoulStore = new("SoulStore", new Color(0.9f, 0.9f, 0.9f), 3); // whitish
            public static readonly HitboxType Grass = new("Grass", new Color(0.4f, 1f, 0.4f), 3); // green
            public static readonly HitboxType Elevator = new("Elevator", new Color(0.6f, 0.6f, 0.8f), 3); // blue-gray
            public static readonly HitboxType MaskShard = new("MaskShard", new Color(0.9f, 0.7f, 0.7f), 3); // pinkish
            public static readonly HitboxType SoulShard = new("SoulShard", new Color(0.7f, 0.7f, 0.9f), 3); // blueish
            public static readonly HitboxType SecretArea = new("SecretArea", new Color(0.4f, 0.4f, 0.0f), 3); // dark
            public static readonly HitboxType Lifeblood = new("Lifeblood", new Color(0.2f, 0.3f, 0.8f), 3); // light blue
            public static readonly HitboxType HotSpring = new("HotSpring", new Color(0.9f, 0.9f, 0.9f), 3); // whitish
            public static readonly HitboxType Upgrade = new("Upgrade", new Color(0.9f, 0.9f, 0.9f), 3); // whitish


            public readonly Color Color;
            public readonly int Depth;
            public readonly string Name;

            private HitboxType(string name, Color color, int depth)
            {
                Color = color;
                Depth = depth;
                Name = name;
            }

            public int CompareTo(HitboxType other)
            {
                return other.Name.CompareTo(Name);
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public readonly SortedDictionary<HitboxType, HashSet<Collider2D>> colliders = new()
        {
            {HitboxType.Knight, new HashSet<Collider2D>()},
            {HitboxType.Enemy, new HashSet<Collider2D>()},
            {HitboxType.Attack, new HashSet<Collider2D>()},
            {HitboxType.Terrain, new HashSet<Collider2D>()},
            {HitboxType.Trigger, new HashSet<Collider2D>()},
            {HitboxType.Breakable, new HashSet<Collider2D>()},
            {HitboxType.Transition, new HashSet<Collider2D>()},
            {HitboxType.Switch, new HashSet<Collider2D>()},
            {HitboxType.Gate, new HashSet<Collider2D>()},
            {HitboxType.Bench, new HashSet<Collider2D>()},
            {HitboxType.Bottle, new HashSet<Collider2D>()},
            {HitboxType.HazardRespawn, new HashSet<Collider2D>()},
            {HitboxType.Other, new HashSet<Collider2D>()},
            {HitboxType.GeoStore, new HashSet<Collider2D>()},
            {HitboxType.GeoToken, new HashSet<Collider2D>()},
            {HitboxType.SoulStore, new HashSet<Collider2D>()},
            {HitboxType.Grass, new HashSet<Collider2D>()},
            {HitboxType.Elevator, new HashSet<Collider2D>()},
            {HitboxType.SoulShard, new HashSet<Collider2D>()},
            {HitboxType.MaskShard, new HashSet<Collider2D>()},
            {HitboxType.SecretArea, new HashSet<Collider2D>()},
            {HitboxType.Lifeblood, new HashSet<Collider2D>()},
            {HitboxType.HotSpring, new HashSet<Collider2D>()},
            {HitboxType.Upgrade, new HashSet<Collider2D>()},
        };

        public Collider2D closestCollider;
        public List<List<Vector2>> terrainOutlines = new();

        public static string debugPattern = null;
        public static int debugDraws = 0;

        public static float LineWidth => Math.Max(0.6f, Screen.width / 960f * GameCameras.instance.tk2dCam.ZoomFactor);

        private void Start()
        {
            foreach (Collider2D col in Resources.FindObjectsOfTypeAll<Collider2D>())
            {
                TryAddHitboxes(col);
            }

            foreach (var pair in colliders)
            {
                foreach (var collider in pair.Value)
                {
                    if (!collider.isActiveAndEnabled) continue;
                    Ariadne.MLog($"[{pair.Key.Name}] Collider '{collider.name}'");
                }
            }

            var terrainPaths = colliders[HitboxType.Terrain]
                .Where(col => col.isActiveAndEnabled)
                .ToList();
            terrainOutlines = Clipping.UnionAllPaths(terrainPaths);
        }

        public void UpdateHitbox(GameObject go)
        {
            foreach (Collider2D col in go.GetComponentsInChildren<Collider2D>(true))
            {
                var success = TryAddHitboxes(col);
                if (success) Ariadne.MLog($"[Unknown] Collider '{col.name}'");
            }
        }

        private Vector2 LocalToScreenPoint(Camera camera, Collider2D collider2D, Vector2 point)
        {
            Vector2 result = camera.WorldToScreenPoint((Vector2)collider2D.transform.TransformPoint(point + collider2D.offset));
            return new Vector2((int)Math.Round(result.x), (int)Math.Round(Screen.height - result.y));
        }

        private bool TryAddHitboxes(Collider2D collider2D)
        {
            if (collider2D == null)
            {
                return false;
            }

            if (debugPattern != null && collider2D.name.Contains(debugPattern))
            {
                Ariadne.MLog($"Debugging initializer: {collider2D.name}");
            }

            if (collider2D is BoxCollider2D or PolygonCollider2D or EdgeCollider2D or CircleCollider2D)
            {
                GameObject go = collider2D.gameObject;
                if (collider2D.GetComponent<DamageHero>()
                    || collider2D.gameObject.LocateMyFSM("damages_hero"))
                {
                    colliders[HitboxType.Enemy].Add(collider2D);
                }
                else if (go.GetComponent<HealthManager>()
                    || go.LocateMyFSM("health_manager_enemy")
                    || go.LocateMyFSM("health_manager"))
                {
                    colliders[HitboxType.Other].Add(collider2D);
                }
                else if (go.layer == (int)PhysLayers.TERRAIN)
                {
                    if (go.GetComponent<GeoControl>() != null)
                        colliders[HitboxType.GeoStore].Add(collider2D);
                    else if (go.GetComponent<HealthCocoon>() != null)
                    {
                        colliders[HitboxType.Lifeblood].Add(collider2D);
                    }
                    else if (go.name.Contains("Break")
                        || go.name.Contains("Collapse")
                        || go.name.StartsWith("Loose Floor")
                        || go.name.StartsWith("Quake Floor")
                        || go.GetComponent<Breakable>() != null)
                        colliders[HitboxType.Breakable].Add(collider2D);
                    else if (collider2D.name.Contains("Gate")
                        || collider2D.name.StartsWith("sliding_wall")
                        || collider2D.name.StartsWith("Bottom Block")
                        || collider2D.name.Contains("Solid")
                        || collider2D.name.StartsWith("full_wall")
                        || collider2D.name.StartsWith("Inactive Block")
                        || collider2D.name.StartsWith("Bottle Physical")
                        || collider2D.name.StartsWith("Rising Pillar")
                        || go.GetComponent<SlopePlat>() != null
                        //TODO find common denominator
                        )
                    {
                        if (collider2D.isTrigger)
                            colliders[HitboxType.Switch].Add(collider2D);
                        else
                            colliders[HitboxType.Gate].Add(collider2D);
                    }
                    else if (go.tag == "Platform" || collider2D.name.StartsWith("elev_main"))
                    {
                        colliders[HitboxType.Elevator].Add(collider2D);
                    }
                    else
                        colliders[HitboxType.Terrain].Add(collider2D);
                }
                else if (go == HeroController.instance?.gameObject && !collider2D.isTrigger)
                {
                    colliders[HitboxType.Knight].Add(collider2D);
                }
                else if (go.GetComponent<DamageEnemies>() || go.LocateMyFSM("damages_enemy") || go.name == "Damager" && go.LocateMyFSM("Damage"))
                {
                    colliders[HitboxType.Attack].Add(collider2D);
                }
                else if (collider2D.isTrigger && collider2D.GetComponent<HazardRespawnTrigger>())
                {
                    colliders[HitboxType.HazardRespawn].Add(collider2D);
                }
                else if (collider2D.isTrigger && collider2D.GetComponent<TransitionPoint>())
                {
                    colliders[HitboxType.Transition].Add(collider2D);
                }
                else if (collider2D.GetComponent<Breakable>() != null)
                {
                    NonBouncer bounce = collider2D.GetComponent<NonBouncer>();
                    if (bounce == null || !bounce.active)
                    {
                        colliders[HitboxType.Trigger].Add(collider2D);
                    }
                }
                else if (collider2D.GetComponent<RestBench>() != null)
                {
                    colliders[HitboxType.Bench].Add(collider2D);
                }
                else if (collider2D.name.StartsWith("Grub Bottle"))
                {
                    colliders[HitboxType.Bottle].Add(collider2D);
                }
                else if (collider2D.name.StartsWith("Spa Floor"))
                {
                    colliders[HitboxType.HotSpring].Add(collider2D);
                }
                else if (collider2D.name.Contains("Geo"))
                {
                    if (go.GetComponent<GeoControl>() != null)
                        colliders[HitboxType.GeoStore].Add(collider2D);
                    else
                        colliders[HitboxType.GeoToken].Add(collider2D);
                }
                else if (collider2D.name.Contains("Soul"))
                {
                    //var components = go.GetComponents(typeof(Component));
                    //Ariadne.MLog(components.ToString());
                    if (go.GetComponent<PersistentIntItem>() != null)
                        colliders[HitboxType.SoulStore].Add(collider2D);
                    else
                        colliders[HitboxType.Other].Add(collider2D);
                }
                else if (collider2D.name.ToLower().Contains("grass"))
                {
                    colliders[HitboxType.Grass].Add(collider2D);
                }
                else if (go.GetComponent<PersistentBoolItem>() != null)
                {
                    if (collider2D.name.StartsWith("Heart Piece"))
                    {
                        colliders[HitboxType.MaskShard].Add(collider2D);
                    }
                    else if (collider2D.name.StartsWith("Vessel Fragment"))
                    {
                        colliders[HitboxType.SoulShard].Add(collider2D);
                    }
                    else if (collider2D.name.Contains("Mask"))
                    {
                        colliders[HitboxType.SecretArea].Add(collider2D);
                    }
                }
                else if (collider2D.name.StartsWith("Lift Call Lever"))
                {
                    colliders[HitboxType.Switch].Add(collider2D);
                }
                else if (go.GetComponent<SpriteRenderer>()?.sprite.texture.name.StartsWith("fireball_collect") ?? false )
                {
                    colliders[HitboxType.Upgrade].Add(collider2D);
                }
                else if (Ariadne.settings.ShowHitBoxes == ShowHitbox.Verbose)
                {
                    colliders[HitboxType.Other].Add(collider2D);
                }
            }

            return true;
        }

        private void OnGUI()
        {
            if (Event.current?.type != EventType.Repaint || Camera.main == null || GameManager.instance == null || GameManager.instance.isPaused)
            {
                return;
            }

            closestCollider = ClosestCollider();

            GUI.depth = int.MaxValue;
            Camera camera = Camera.main;
            float lineWidth = LineWidth;
            foreach (var pair in colliders)
            {
                if (pair.Key.Equals(HitboxType.Terrain)) continue;
                foreach (Collider2D collider2D in pair.Value)
                {
                    var hbtype = collider2D == closestCollider ? HitboxType.Highlighted : pair.Key;
                    DrawHitbox(camera, collider2D, hbtype, lineWidth);
                }
            }

            foreach (var path in terrainOutlines)
            {
                DrawWorldPointSequence(path, camera, HitboxType.Highlighted, lineWidth);
            }

        }

        private void DrawHitbox(Camera camera, Collider2D collider2D, HitboxType hitboxType, float lineWidth)
        {
            if (collider2D == null || !collider2D.isActiveAndEnabled)
            {
                return;
            }

            if (debugDraws > 0 && debugPattern != null && collider2D.name.Contains(debugPattern))
            {
                debugDraws--;
                Ariadne.MLog($"Debugging draw: {collider2D.name}");
            }

            int origDepth = GUI.depth;
            GUI.depth = hitboxType.Depth;
            if (collider2D is BoxCollider2D or EdgeCollider2D or PolygonCollider2D)
            {
                switch (collider2D)
                {
                    case BoxCollider2D boxCollider2D:
                        Vector2 halfSize = boxCollider2D.size / 2f;
                        Vector2 topLeft = new(-halfSize.x, halfSize.y);
                        Vector2 topRight = halfSize;
                        Vector2 bottomRight = new(halfSize.x, -halfSize.y);
                        Vector2 bottomLeft = -halfSize;
                        List<Vector2> boxPoints = new List<Vector2>
                        {
                            topLeft, topRight, bottomRight, bottomLeft, topLeft
                        };
                        DrawPointSequence(boxPoints, camera, collider2D, hitboxType, lineWidth);
                        break;
                    case EdgeCollider2D edgeCollider2D:
                        DrawPointSequence(new(edgeCollider2D.points), camera, collider2D, hitboxType, lineWidth);
                        break;
                    case PolygonCollider2D polygonCollider2D:
                        for (int i = 0; i < polygonCollider2D.pathCount; i++)
                        {
                            List<Vector2> polygonPoints = new(polygonCollider2D.GetPath(i));
                            if (polygonPoints.Count > 0)
                            {
                                polygonPoints.Add(polygonPoints[0]);
                            }
                            DrawPointSequence(polygonPoints, camera, collider2D, hitboxType, lineWidth);
                        }
                        break;
                }
            }
            else if (collider2D is CircleCollider2D circleCollider2D)
            {
                Vector2 center = LocalToScreenPoint(camera, collider2D, Vector2.zero);
                Vector2 right = LocalToScreenPoint(camera, collider2D, Vector2.right * circleCollider2D.radius);
                int radius = (int)Math.Round(Vector2.Distance(center, right));
                Drawing.DrawCircle(center, radius, hitboxType.Color, lineWidth, true, Mathf.Clamp(radius / 16, 4, 32));
            }

            //if (hitboxType.Equals(HitboxType.SecretArea))
            //{
            //    foreach (var sr in collider2D.gameObject.GetComponentsInChildren<SpriteRenderer>())
            //    {
            //        var bounds = sr.bounds;
            //        Vector2 extent = (Vector2)bounds.extents;
            //        Vector2 center = (Vector2)bounds.center;
            //        Vector2 topLeft = new(-extent.x, extent.y);
            //        Vector2 topRight = extent;
            //        Vector2 bottomRight = new(extent.x, -extent.y);
            //        Vector2 bottomLeft = -extent;
            //        List<Vector2> boxPoints = new List<Vector2>
            //                {
            //                    topLeft + center , topRight + center, bottomRight + center, bottomLeft + center, topLeft + center
            //                };
            //        if (Ariadne.settings.DebugAB)
            //            DrawWorldPointSequence(boxPoints, camera, hitboxType, lineWidth);
            //        else
            //            DrawPointSequence(boxPoints, camera, collider2D, hitboxType, lineWidth);
            //    }
                //var sr = collider2D.gameObject.GetComponent<SpriteRenderer>();
            //}

            GUI.depth = origDepth;
        }

        private void DrawPointSequence(List<Vector2> points, Camera camera, Collider2D collider2D, HitboxType hitboxType, float lineWidth)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 pointA = LocalToScreenPoint(camera, collider2D, points[i]);
                Vector2 pointB = LocalToScreenPoint(camera, collider2D, points[i + 1]);
                Drawing.DrawLine(pointA, pointB, hitboxType.Color, lineWidth, true);
            }
        }

        private void DrawWorldPointSequence(List<Vector2> points, Camera camera, HitboxType hitboxType, float lineWidth)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 pointA = camera.WorldToScreenPoint(points[i]);
                pointA.y = Screen.height - pointA.y;
                Vector2 pointB = camera.WorldToScreenPoint(points[i+1]);
                pointB.y = Screen.height - pointB.y;
                Drawing.DrawLine(pointA, pointB, hitboxType.Color, lineWidth, true);
            }
        }

        private Collider2D ClosestCollider()
        {
            var players = colliders[HitboxType.Knight].ToList();
            var player = players.Count > 0 ? players[0] : null;

            if (player == null) {
                Ariadne.MLog("No player found");
                return null; 
            }

            Collider2D closest = null;
            float closestDist = float.PositiveInfinity;
            foreach (var pair in colliders)
            {
                if (pair.Key.Equals(HitboxType.Knight) || !pair.Key.Equals(HitboxType.Terrain))
                {
                    continue;
                }
                foreach (Collider2D collider2D in pair.Value)
                {
                    if (collider2D == null || !collider2D.isActiveAndEnabled || collider2D.isTrigger) continue;
                    var dist = collider2D.Distance(player).distance;
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = collider2D;
                    }
                }
            }
            return closest;
        }
    }
}