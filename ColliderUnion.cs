using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Clipper2Lib;
using static Ariadne.HitboxTracker;
using System.Collections;

namespace Ariadne
{
    public class ColliderUnion
    {
        private readonly int[] parent;
        private readonly int[] rank;
        private readonly List<Collider2D> colliders;
        private readonly List<PathsD> shapes;

        public ColliderUnion(List<Collider2D> objectList)
        {
            int size = objectList.Count();
            parent = new int[size];
            rank = new int[size];
            colliders = objectList;

            shapes = colliders
                .Select(col => ColliderPaths.GetColliderWorldPath(col, true))
                .Select(ColliderPaths.VectorPathToPathsD)
                .ToList();

            for (int i = 0; i < size; i++)
            {
                parent[i] = i;
                rank[i] = 0;
            }
        }

        public int Find(int x)
        {
            if (parent[x] != x)
            {
                parent[x] = Find(parent[x]);
            }
            return parent[x];
        }

        public void Union(int x, int y)
        {
            int rootX = Find(x);
            int rootY = Find(y);

            if (rootX == rootY)
            {
                //already unioned
                return;
            }

            if (rank[rootX] < rank[rootY])
            {
                ClipperUnion(x, y, true);
                parent[rootX] = rootY;
            }
            else if (rank[rootX] > rank[rootY])
            {
                ClipperUnion(y, x, true);
                parent[rootY] = rootX;
            }
            else
            {
                ClipperUnion(y, x, true);
                parent[rootY] = rootX;
                rank[rootX]++;
            }
        }

        private PathsD GetShape(int x)
        {
            return shapes[Find(x)];
        }

        private List<PathsD> GetDisjointShapes()
        {
            var disjointObjects = new List<PathsD>();
            for (int i = 0; i < parent.Length; i++)
            {
                if (parent[i] == i)
                {
                    disjointObjects.Add(shapes[i]);
                }
            }
            return disjointObjects;
        }

        public List<List<Vector2>> GetDisjointPaths()
        {
            return GetDisjointShapes().SelectMany(ColliderPaths.PathsDToVectorPath).ToList();
        }

        /// <summary>
        /// Unions the shapes associated with the parent and child
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        /// <param name="isRoot"></param>
        private void ClipperUnion(int parent, int child, bool isRoot)
        {
            if (true || !isRoot)
            {
                parent = Find(parent);
                child = Find(child);
            }

            var shapeA = shapes[parent];
            var shapeB = shapes[child];
            var union = Clipper.Union(shapeA, shapeB, FillRule.NonZero);
            shapes[parent] = union;
            shapes[child] = union;
        }
        
        /// <summary>
        /// Creates union links between colliders that overlap or share edges
        /// </summary>
        public void UnionAllPaths()
        {
            for (int i = 0; i < shapes.Count - 1;  i++)
            {
                for (int j = i + 1; j < shapes.Count; j++)
                {
                    var shapeA = GetShape(i);
                    var shapeB = GetShape(j);
                    if (shapeA == shapeB) continue;
                    var intersection = Clipper.Intersect(
                        Clipper.InflatePaths(shapeA, 0.01f, JoinType.Square, EndType.Polygon), 
                        shapeB, 
                        FillRule.NonZero
                    );
                    if (Clipper.Area(intersection) > 0)
                    {
                        Ariadne.MLog($"Intersection found between {colliders[i].name} and {colliders[j].name}");
                        Union(i, j);
                    }
                }
            }
        }

        public void ClipOverlap(ColliderUnion other)
        {
            for (int i = 0; i < shapes.Count; i++ )
            {
                if (parent[i] != i) continue;
                foreach (var tpath in other.GetDisjointShapes())
                {
                    var diff = Clipper.Difference(
                        shapes[i],
                        tpath,
                        FillRule.NonZero,
                        2);
                    if (Clipper.Area(diff) < 0.001)
                    {
                        Ariadne.MLog($"Box completely removed: {shapes[i]}");
                    }
                    shapes[i] = diff;
                }
            }

            //return lowerPaths.SelectMany(ColliderPaths.PathsDToVectorPath).ToList();
        }

        //private static List<PathsD> GetTestPaths()
        //{
        //    var path1 = new PathD(new List<PointD> {
        //        new PointD(0, 0),
        //        new PointD(0, 4),
        //        new PointD(4, 4),
        //        new PointD(4, 0),
        //        new PointD(0, 0)
        //    });
        //    var path2 = new PathD(new List<PointD> {
        //        new PointD(1, 1),
        //        new PointD(1, 2),
        //        new PointD(2, 3),
        //        new PointD(2, 1),
        //        new PointD(1, 1)
        //    });
        //    var path3 = new PathD(new List<PointD> {
        //        new PointD(0, 2),
        //        new PointD(0, 3),
        //        new PointD(1, 4),
        //        new PointD(1, 2),
        //        new PointD(0, 2)
        //    });
        //    var path4 = new PathD(new List<PointD> {
        //        new PointD(0, 1),
        //        new PointD(0, -4),
        //        new PointD(-2, -4),
        //        new PointD(-2, 1),
        //        new PointD(0, 1)
        //    });
        //    var path5 = new PathD(new List<PointD> {
        //        new PointD(10, 1),
        //        new PointD(10, -4),
        //        new PointD(8, -4),
        //        new PointD(8, 1),
        //        new PointD(10, 1)
        //    });
        //    return new List<PathsD>() {
        //        new PathsD(new List<PathD> { path1 }),
        //        new PathsD(new List<PathD> { path2 }),
        //        new PathsD(new List<PathD> { path3 }),
        //        new PathsD(new List<PathD> { path4 }),
        //        new PathsD(new List<PathD> { path5 }),
        //    };
        //}

    }
}
