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
    public class Clipping
    {
        private class UnionFind<T>
        {
            private int[] parent;
            private int[] rank;
            private List<T> objects;

            public UnionFind(List<T> objectList)
            {
                int size = objectList.Count();
                parent = new int[size];
                rank = new int[size];
                objects = objectList;

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
                    return;
                }

                if (rank[rootX] < rank[rootY])
                {
                    parent[rootX] = rootY;
                }
                else if (rank[rootX] > rank[rootY])
                {
                    parent[rootY] = rootX;
                }
                else
                {
                    parent[rootY] = rootX;
                    rank[rootX]++;
                }
            }

            public T GetObject(int x)
            {
                return objects[Find(x)];
            }

            public void SetObject(int x, T obj)
            {
                objects[Find(x)] = obj;
            }

            public List<T> GetDisjointObjects()
            {
                var disjointObjects = new List<T>();
                for (int i = 0; i < parent.Length; i++)
                {
                    if (parent[i] == i)
                    {
                        disjointObjects.Add(objects[i]);
                    }
                }
                return disjointObjects;
            }
        
        }

        public static List<List<Vector2>> UnionAllPaths(List<Collider2D> colliders)
        {
            var paths = colliders
                .Select(col => ColliderPaths.GetColliderWorldPath(col, true))
                .Select(ColliderPaths.VectorPathToPathsD)
                .ToList();
            //var paths = GetTestPaths();
            var unionFinder = new UnionFind<PathsD>(paths);
            int numCollisions = 0;
            for (int i = 0; i < paths.Count - 1;  i++)
            {
                for (int j = i + 1; j < paths.Count; j++)
                {
                    var colA = colliders[i];
                    var colB = colliders[j];
                    var shapeA = unionFinder.GetObject(i);
                    var shapeB = unionFinder.GetObject(j);
                    if (shapeA == shapeB) continue;
                    var intersection = Clipper.Intersect(
                        Clipper.InflatePaths(shapeA, 0.01f, JoinType.Square, EndType.Polygon), 
                        shapeB, 
                        FillRule.NonZero
                    );
                    if (Clipper.Area(intersection) > 0)
                    {
                        unionFinder.Union(i, j);
                        var unionShape = Clipper.Union(shapeA, shapeB, FillRule.NonZero);
                        unionFinder.SetObject(i, unionShape);
                        numCollisions++;
                    }
                }
            }
            return unionFinder.GetDisjointObjects().Select(ColliderPaths.PathsDToVectorPath).ToList();
        }

        private static List<PathsD> GetTestPaths()
        {
            var path1 = new PathD(new List<PointD> {
                new PointD(0, 0),
                new PointD(0, 4),
                new PointD(4, 4),
                new PointD(4, 0),
                new PointD(0, 0)
            });
            var path2 = new PathD(new List<PointD> {
                new PointD(1, 1),
                new PointD(1, 2),
                new PointD(2, 3),
                new PointD(2, 1),
                new PointD(1, 1)
            });
            var path3 = new PathD(new List<PointD> {
                new PointD(0, 2),
                new PointD(0, 3),
                new PointD(1, 4),
                new PointD(1, 2),
                new PointD(0, 2)
            });
            var path4 = new PathD(new List<PointD> {
                new PointD(0, 1),
                new PointD(0, -4),
                new PointD(-2, -4),
                new PointD(-2, 1),
                new PointD(0, 1)
            });
            var path5 = new PathD(new List<PointD> {
                new PointD(10, 1),
                new PointD(10, -4),
                new PointD(8, -4),
                new PointD(8, 1),
                new PointD(10, 1)
            });
            return new List<PathsD>() {
                new PathsD(new List<PathD> { path1 }),
                new PathsD(new List<PathD> { path2 }),
                new PathsD(new List<PathD> { path3 }),
                new PathsD(new List<PathD> { path4 }),
                new PathsD(new List<PathD> { path5 }),
            };
        }

        //    private struct BoundedPaths
        //{
        //    public PathsD paths;
        //    public Bounds bounds;

        //    public BoundedPaths(PathsD paths, Bounds bounds)
        //    {
        //        this.paths = paths;
        //        this.bounds = bounds;
        //    }
        //}

        //private static BoundedPaths ColliderToPaths(PolygonCollider2D collider)
        //{
        //    List<PathD> paths = new();
        //    for (int i = 0; i < collider.pathCount; i++)
        //    {
        //        var polygonPoints = collider.GetPath(i).Select(point => new PointD(point.x, point.y)).ToList();
        //        if (polygonPoints.Count > 0)
        //        {
        //            polygonPoints.Add(polygonPoints[0]);
        //        }
        //        paths.Add(new(polygonPoints));
        //    }
        //    return new(new(paths), collider.bounds);
        //}

        //private static IEnumerable<List<Vector2>> PathsToVectors(IEnumerable<PathsD> uniquePaths)
        //{
        //    var vectorPaths = new List<List<Vector2>>();
        //    foreach (var paths in uniquePaths)
        //    {
        //        foreach (var path in paths)
        //        {
        //            vectorPaths.Add(path.Select(p => new Vector2((float)p.x, (float)p.y)).ToList());
        //        }
        //    }
        //    return vectorPaths;
        //}

        //public static IEnumerable<List<Vector2>> UnionAllPolygonColliders(IEnumerable<Collider2D> colliders)
        //{
        //    List<BoundedPaths> boundedPaths = new(polygons.Select(ColliderToPaths));
        //    Queue<PathsD> exclusivePaths = new Queue<PathsD>();

        //    Ariadne.MLog($"#poly: ${boundedPaths.Count}");
        //    //var union = boundedPaths.Dequeue().paths;
        //    while (boundedPaths.Count > 0)
        //    {
        //        var bp1 = boundedPaths.First();
        //        boundedPaths.Remove(bp1);
        //        //union = Clipper.Union(union, bp2, FillRule.NonZero);
        //        bool foundIntersection = false;
        //        foreach (var bp2 in boundedPaths)
        //        {
        //            //if (bp1.bounds.Intersects(bp2.bounds))
        //            //{
        //                var interpath = Clipper.Intersect(bp1.paths, bp2.paths, FillRule.NonZero);
        //                if (interpath.Count > 0)
        //                {
        //                    Ariadne.MLog("Found intersection");
        //                    var union = Clipper.Union(bp1.paths, bp2.paths, FillRule.NonZero);
        //                    boundedPaths.Remove(bp2);
        //                    boundedPaths.Add(new(union, new Bounds()));
        //                    foundIntersection = true;
        //                    break;
        //                }

        //            //}
        //        }
        //        if (!foundIntersection)
        //        {
        //            exclusivePaths.Enqueue(bp1.paths);
        //        }
        //    }
        //    //Ariadne.MLog($"#union: ${union.Count}");
        //    //return PathsToVectors(union);
        //    Ariadne.MLog($"Found ${exclusivePaths.Count} distinct shapes");
        //    return PathsToVectors(exclusivePaths);
        //}


    }
}
