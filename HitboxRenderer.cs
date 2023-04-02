using System;
using System.Collections.Generic;
using GlobalEnums;
using UnityEngine;
using System.Linq;
using static Ariadne.HitboxTracker;

namespace Ariadne
{
    [RequireComponent(typeof(HitboxTracker))]
    public class HitboxRenderer: MonoBehaviour
    {
        public static float LineWidth => Math.Max(0.6f, 
            Screen.width / 960f * GameCameras.instance.tk2dCam.ZoomFactor);

        private HitboxTracker hitboxTracker;

        private void Start()
        {
            hitboxTracker = GetComponent<HitboxTracker>();
        }

        private Vector2 LocalToScreenPoint(Camera camera, Collider2D collider2D, Vector2 point)
        {
            Vector2 result = camera.WorldToScreenPoint((Vector2)collider2D.transform.TransformPoint(point + collider2D.offset));
            return new Vector2((int)Math.Round(result.x), (int)Math.Round(Screen.height - result.y));
        }

        private void OnGUI()
        {
            if (Event.current?.type != EventType.Repaint || Camera.main == null || GameManager.instance == null || GameManager.instance.isPaused)
            {
                return;
            }

            GUI.depth = int.MaxValue;
            Camera camera = Camera.main;
            float lineWidth = LineWidth;
            foreach (var pair in hitboxTracker.ColliderLayers)
            {
                if (pair.Key == HitboxType.Terrain
                    || pair.Key == HitboxType.StaticHazard
                    || (Ariadne.settings.ShowHitBoxes < ShowHitbox.Verbose 
                        && pair.Key == HitboxType.Other)) 
                    continue;
                foreach (Collider2D collider2D in pair.Value)
                {
                    DrawHitbox(camera, collider2D, pair.Key, lineWidth);
                }
            }

            foreach (var path in hitboxTracker.terrainOutlines)
            {
                DrawWorldPointSequence(path, camera, HitboxType.Terrain.GetColor(), lineWidth);
            }

            foreach (var path in hitboxTracker.hazardOutlines)
            {
                DrawWorldPointSequence(path, camera, HitboxType.StaticHazard.GetColor(), lineWidth);
            }

        }

        private void DrawHitbox(Camera camera, Collider2D collider2D, HitboxType hitboxType, float lineWidth)
        {
            if (collider2D == null || !collider2D.isActiveAndEnabled) return;
            int origDepth = GUI.depth;
            GUI.depth = hitboxType.GetDepth();

            var hbColor = hitboxType.GetColor();
            if (collider2D == hitboxTracker.ClosestCollider)
            {
                hbColor = new Color(1 - hbColor.r, 1 - hbColor.g, 1 - hbColor.b);
            }

            if (debugDraws > 0 && debugPattern != null && collider2D.name.Contains(debugPattern))
            {
                //just a spot to put a breakpoint
                debugDraws--;
                Ariadne.MLog($"Debugging draw: {collider2D.name}");
            }

            switch (collider2D)
            {
                case BoxCollider2D boxCollider2D:
                    DrawBoxCollider(boxCollider2D, camera, hbColor, lineWidth);
                    break;
                case EdgeCollider2D edgeCollider2D:
                    DrawPointSequence(new(edgeCollider2D.points), camera, collider2D, 
                        hbColor, lineWidth);
                    break;
                case PolygonCollider2D polygonCollider2D:
                    DrawPolygonCollider(polygonCollider2D, camera, hbColor, lineWidth);
                    break;
                case CircleCollider2D circleCollider2D:
                    DrawCircleCollider(circleCollider2D, camera, hbColor, lineWidth);
                    break;
            }

            GUI.depth = origDepth;
        }

        private void DrawBoxCollider(BoxCollider2D boxCollider, Camera camera, Color color, float lineWidth)
        {
            Vector2 halfSize = boxCollider.size / 2f;
            Vector2 topLeft = new(-halfSize.x, halfSize.y);
            Vector2 topRight = halfSize;
            Vector2 bottomRight = new(halfSize.x, -halfSize.y);
            Vector2 bottomLeft = -halfSize;
            List<Vector2> boxPoints = new List<Vector2>
                    {
                        topLeft, topRight, bottomRight, bottomLeft, topLeft
                    };
            DrawPointSequence(boxPoints, camera, boxCollider, color, lineWidth);
        }

        private void DrawPolygonCollider(PolygonCollider2D polyCollider, Camera camera, Color color, float lineWidth)
        {
            for (int i = 0; i < polyCollider.pathCount; i++)
            {
                List<Vector2> polygonPoints = new(polyCollider.GetPath(i));
                if (polygonPoints.Count > 0)
                {
                    polygonPoints.Add(polygonPoints[0]);
                }
                DrawPointSequence(polygonPoints, camera, polyCollider, color, lineWidth);
            }
        }

        private void DrawCircleCollider(CircleCollider2D circleCollider, Camera camera, Color color, float lineWidth)
        {
            Vector2 center = LocalToScreenPoint(camera, circleCollider, Vector2.zero);
            Vector2 right = LocalToScreenPoint(camera, circleCollider, Vector2.right * circleCollider.radius);
            int radius = (int)Math.Round(Vector2.Distance(center, right));
            Drawing.DrawCircle(center, radius, color, lineWidth, true, Mathf.Clamp(radius / 16, 4, 32));

        }

        private void DrawPointSequence(List<Vector2> points, Camera camera, Collider2D collider2D, Color color, float lineWidth)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 pointA = LocalToScreenPoint(camera, collider2D, points[i]);
                Vector2 pointB = LocalToScreenPoint(camera, collider2D, points[i + 1]);
                Drawing.DrawLine(pointA, pointB, color, lineWidth, true);
            }
        }

        private void DrawWorldPointSequence(List<Vector2> points, Camera camera, Color color, float lineWidth)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 pointA = camera.WorldToScreenPoint(points[i]);
                pointA.y = Screen.height - pointA.y;
                Vector2 pointB = camera.WorldToScreenPoint(points[i+1]);
                pointB.y = Screen.height - pointB.y;
                Drawing.DrawLine(pointA, pointB, color, lineWidth, true);
            }
        }
    }
}