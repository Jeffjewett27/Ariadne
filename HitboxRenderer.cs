using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

        private void OnGUI()
        {
            if (Event.current?.type != EventType.Repaint || Camera.main == null 
                || GameManager.instance == null || GameManager.instance.isPaused)
            {
                return;
            }

            GUI.depth = int.MaxValue;
            Camera camera = Camera.main;
            float lineWidth = LineWidth;
            foreach (var (hitboxType, colliderLayer) 
                in hitboxTracker.ColliderLayers.Select(pair => (pair.Key, pair.Value)))
            {
                if (Ariadne.settings.ShowHitBoxes < colliderLayer.MinShowLevel)
                    continue;
                foreach (var path in colliderLayer)
                {
                    DrawWorldPointSequence(path, camera, colliderLayer.Color, lineWidth);
                }
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