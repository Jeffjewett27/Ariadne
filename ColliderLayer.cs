using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ariadne
{
    public class ColliderLayer : IEnumerable<Collider2D>
    {
        public Color Color { get; }
        public int Depth { get; }
        public string Name { get; }

        private List<Collider2D> colliders;

        public ColliderLayer(Color color, int depth, string name)
        {
            Color = color;
            Depth = depth;
            Name = name;
        }

        public ColliderLayer(HitboxType hitboxType)
        {
            colliders = new List<Collider2D>();
            Color = hitboxType.GetColor();
            Depth = hitboxType.GetDepth();
            Name = hitboxType.GetName();
        }

        public void AddCollider(Collider2D collider)
        {
            colliders.Add(collider);
        }

        public IEnumerator<Collider2D> GetEnumerator()
        {
            return colliders.Where(col => col != null && col.isActiveAndEnabled).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return colliders.Where(col => col != null && col.isActiveAndEnabled).GetEnumerator();
        }
    }
}
