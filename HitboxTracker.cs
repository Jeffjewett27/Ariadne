using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{
    public class HitboxTracker: MonoBehaviour
    {
        public static SortedDictionary<HitboxType, ColliderLayer> BuildColliderLayers()
        {
            var layers = new SortedDictionary<HitboxType, ColliderLayer>();
            foreach (var hbtype in (HitboxType[])Enum.GetValues(typeof(HitboxType)))
            {
                layers[hbtype] = ColliderLayer.From(hbtype, layers.Values);
            }
            return layers;
        }

        private IColliderClassifier classifier;

        public readonly SortedDictionary<HitboxType, ColliderLayer> ColliderLayers = BuildColliderLayers();
        public Collider2D ClosestCollider { get; private set; }

        public static string debugPattern = null;
        public static int debugDraws = 0;

        private void Start()
        {
            classifier = new DetailedClassifier();
            foreach (Collider2D col in Resources.FindObjectsOfTypeAll<Collider2D>())
            {
                var hbType = classifier.ClassifyCollider(col);
                if (ColliderLayers.ContainsKey(hbType))
                {
                    ColliderLayers[hbType].AddCollider(col);
                }
            }
        }

        public void UpdateHitbox(GameObject go)
        {
            foreach (Collider2D col in go.GetComponentsInChildren<Collider2D>(true))
            {
                var hbType = classifier.ClassifyCollider(col);
                if (ColliderLayers.ContainsKey(hbType))
                {
                    ColliderLayers[hbType].AddCollider(col);
                }
            }
        }

        private void FixedUpdate()
        {   
            var oldClosest = ClosestCollider;
            ClosestCollider = GetClosestCollider();
            if (ClosestCollider != null && oldClosest != ClosestCollider)
            {
                Ariadne.MLog($"New closest collider: {ClosestCollider.name}");
            }
        }

        private Collider2D GetClosestCollider()
        {
            var players = ColliderLayers[HitboxType.Knight].Colliders;
            var player = players.Count > 0 ? players[0] : null;

            if (player == null)
            {
                //Ariadne.MLog("No player found");
                return null;
            }

            Collider2D closest = null;
            float closestDist = float.PositiveInfinity;
            foreach (var pair in ColliderLayers)
            {
                if (pair.Key == HitboxType.Knight
                    || pair.Key == HitboxType.Other
                    || pair.Key == HitboxType.Attack
                    || pair.Key != HitboxType.Terrain) continue;
                foreach (Collider2D collider2D in pair.Value.Colliders)
                {
                    if (collider2D == null || !collider2D.isActiveAndEnabled) continue;
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