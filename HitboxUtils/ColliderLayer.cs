using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ariadne.HitboxUtils
{
    public class ColliderLayer : IEnumerable<List<Vector2>>
    {
        public Color Color { get; }
        public int Depth { get; }
        public string Name { get; }
        public ShowHitbox MinShowLevel { get; }
        public bool IsStatic { get; }
        public List<Collider2D> Colliders { get; }

        public ColliderLayer(HitboxType hitboxType)
        {
            Colliders = new List<Collider2D>();
            Color = hitboxType.GetColor();
            Depth = hitboxType.GetDepth();
            Name = hitboxType.GetName();
            MinShowLevel = hitboxType.GetMinShowLevel();
            IsStatic = hitboxType.GetIsStatic();
        }

        public void AddCollider(Collider2D collider)
        {
            Colliders.Add(collider);
        }

        //public IEnumerable<KeyValuePair<Collider2D, List<Vector2>>> GetColliderMapping()
        //{
        //    return Colliders
        //        .Where(col => col != null && col.isActiveAndEnabled)
        //        .Select(col => new KeyValuePair<Collider2D, List<Vector2>>(col, 
        //            ColliderPaths.GetColliderWorldPath(col, true)));
        //}

        public virtual IEnumerator<List<Vector2>> GetEnumerator()
        {
            return Colliders
                .Where(col => col != null && col.isActiveAndEnabled)
                .Select(col => ColliderPaths.GetColliderWorldPath(col, true))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Colliders
                .Where(col => col != null && col.isActiveAndEnabled)
                .Select(col => ColliderPaths.GetColliderWorldPath(col, true))
                .GetEnumerator();
        }

        public static ColliderLayer From(HitboxType hitboxType,
            IEnumerable<ColliderLayer> previousLayers)
        {
            return hitboxType.GetIsStatic() switch
            {
                false => new ColliderLayer(hitboxType),
                true => new StaticColliderLayer(hitboxType, previousLayers)
            };
        }
    }
}
