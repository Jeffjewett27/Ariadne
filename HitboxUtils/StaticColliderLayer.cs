using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ariadne.HitboxUtils
{
    public class StaticColliderLayer : ColliderLayer, IEnumerable<List<Vector2>>
    {
        private readonly IEnumerable<StaticColliderLayer> previousLayers;

        public bool IsUnion { get; }
        public bool IsIntersection { get; }

        private ColliderUnion colliderUnion;

        public StaticColliderLayer(HitboxType hitboxType, IEnumerable<ColliderLayer> prevLayers)
            : base(hitboxType)
        {
            IsUnion = hitboxType.GetIsUnion();
            IsIntersection = hitboxType.GetIsIntersection();
            previousLayers = prevLayers
                .Where(layer => layer.IsStatic && layer.Name != Name)
                .Select(layer => (StaticColliderLayer)layer);
            Ariadne.MLog($"Static layer {hitboxType.GetName()}: union {IsUnion}");
        }

        public void UpdatePaths()
        {
            colliderUnion = new ColliderUnion(Colliders);
            if (IsUnion)
            {
                Ariadne.MLog($"Layer {Name}: Unioning paths");
                colliderUnion.UnionAllPaths();
            }
            if (IsIntersection)
            {
                Ariadne.MLog($"Layer {Name}: Intersection");
                foreach (var layer in previousLayers)
                {
                    Ariadne.MLog($"Layer {Name}: Intersecting {layer.Name}");
                    colliderUnion.ClipOverlap(layer.colliderUnion);
                }
            }
        }

        public override IEnumerator<List<Vector2>> GetEnumerator()
        {
            if (colliderUnion is null) UpdatePaths();
            return colliderUnion.GetDisjointPaths().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (colliderUnion is null) UpdatePaths();
            return colliderUnion.GetDisjointPaths().GetEnumerator();
        }
    }
}
