using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Clipper2Lib;
using static Ariadne.HitboxTracker;
using System.IO;
using UnityEngine.UIElements;

namespace Ariadne.HitboxUtils
{
    internal class ColliderPaths
    {
        public static int NumCircleSamples = 16;

        public static List<Vector2> GetColliderWorldPath(Collider2D collider2D, bool closeShape)
        {
            List<Vector2> path = new List<Vector2>();
            switch (collider2D)
            {
                case BoxCollider2D boxCollider2D:
                    Vector2 halfSize = boxCollider2D.size / 2f;
                    Vector2 topLeft = new(-halfSize.x, halfSize.y);
                    Vector2 topRight = halfSize;
                    Vector2 bottomRight = new(halfSize.x, -halfSize.y);
                    Vector2 bottomLeft = -halfSize;
                    Vector2 cent = boxCollider2D.offset;
                    var boxPath = new List<Vector2>
                        {
                            cent + topLeft, cent + topRight, cent + bottomRight, cent + bottomLeft, cent + topLeft
                        };
                    //var boxPath = new List<Vector2>()
                    //{
                    //    new(-1, 1), new(1, 1), new(1, -1), new Vector2(-1, -1), new Vector2(-1, 1)
                    //};
                    path = boxPath.Select(p => (Vector2)boxCollider2D.transform.TransformPoint(p)).ToList();
                    //path = boxPath;
                    break;
                case EdgeCollider2D edgeCollider2D:
                    var edgePath = new List<Vector2>(edgeCollider2D.points);
                    if (closeShape && edgePath.Count > 0)
                    {
                        edgePath.Add(edgePath[0]);
                    }
                    path = edgePath.Select(p => (Vector2)edgeCollider2D.transform.TransformPoint(p)).ToList();
                    break;
                case PolygonCollider2D polygonCollider2D:
                    //assume polygon has no holes
                    var polyPath = new List<Vector2>(polygonCollider2D.GetPath(0));
                    if (polyPath.Count > 0)
                    {
                        polyPath.Add(polyPath[0]);
                    }
                    path = polyPath.Select(p => (Vector2)polygonCollider2D.transform.TransformPoint(p)).ToList();
                    break;
                case CircleCollider2D circleCollider2D:
                    float rad = circleCollider2D.radius;
                    Vector2 center = circleCollider2D.transform.position;
                    float angleDiff = 2 * Mathf.PI / NumCircleSamples;
                    float angle = 0;

                    var circlePath = new List<Vector2>();
                    for (int i = 0; i <= NumCircleSamples; i++)
                    {
                        Vector2 point = center + new Vector2(rad * Mathf.Cos(angle), rad * Mathf.Sin(angle));
                        circlePath.Add(point);
                        angle += angleDiff;
                    }
                    path = circlePath.Select(p => (Vector2)circleCollider2D.transform.TransformPoint(p)).ToList();
                    break;
            }

            return path;
        }

        public static List<List<Vector2>> PathsDToVectorPath(PathsD paths)
        {
            var vectorPaths = new List<List<Vector2>>();

            foreach (var path in paths)
            {
                if (Mathf.Abs((float)Clipper.Area(path)) < 0.01)
                {
                    //Ariadne.MLog($"A path with {Clipper.Area(path)} area is being drawn");
                }
                var pathList = path.Select(p => new Vector2((float)p.x, (float)p.y)).ToList();
                // ensure the path loops back to itself
                if (pathList.Count > 0 && pathList[0] != pathList[pathList.Count - 1])
                {
                    pathList.Add(pathList[0]);
                }
                vectorPaths.Add(pathList);
            }
            return vectorPaths;
        }

        public static PathsD VectorPathToPathsD(List<Vector2> vectorPath)
        {
            PathD pathD = new(vectorPath.Select(p => new PointD(p.x, p.y)).ToList());
            List<PathD> pathDs = new List<PathD>() { pathD };
            return new PathsD(pathDs);
        }
    }
}
