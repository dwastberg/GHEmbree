using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;
using Embree;

namespace GHEmbree
{
    class MeshMeshIntersect
    {
        public static bool TestIntersect(Mesh meshA, Mesh meshB, ref Point3d pt, bool fast = true)
        {
            bool meshInterects = false;

            bool bboxIntersection = TestBBIntersect(meshA, meshB);
            if (!bboxIntersection)
            {
                return false; // if bounding boxes don't intersect then meshes don't interesect
            }
            Mesh largeMesh;
            Mesh smallMesh;
            if (meshA.Faces.Count > meshB.Faces.Count)
            {
                largeMesh = meshA;
                smallMesh = meshB;
            } else
            {
                largeMesh = meshB;
                smallMesh = meshA;
            }

            var scene = EmbreeTools.BuildScene(largeMesh);

            var meshVertices = smallMesh.Vertices.ToPoint3fArray();
            foreach (var face in smallMesh.Faces)
            {
                int numVertices = face.IsTriangle ? 3 : 4;
                for (int v = 0; v < numVertices; v++)
                {   
                    var ray = RayFromVertices(meshVertices[face[v]], meshVertices[face[(v+1)%numVertices]]);
                    var packet = scene.Intersects(ray.Item1, 0, ray.Item2);
                    bool hit = (packet.geomID != RTC.InvalidGeometryID);
                    if (hit)
                    {
                        meshInterects = true;
                        var intersect = packet.ToIntersection<Model>(scene);
                        var hitPos = ray.Item1.PointAt(intersect.Distance);
                        pt = new Rhino.Geometry.Point3d(hitPos.X, hitPos.Y, hitPos.Z);
                        break;
                    }
                }
                if (meshInterects)
                {
                    break;
                }
            }

            return meshInterects;

        }

        private static Tuple<Ray,float> RayFromVertices(Point3f a, Point3f b)
        {

            EPoint pt = new EPoint(a.X, a.Y, a.Z);
            EVector vec = new EVector(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
            Ray ray = new Ray(pt, vec);
            float mag = EVector.Length(vec);
            return new Tuple<Ray, float>(ray, mag);

        }

        static bool TestBBIntersect(Mesh meshA, Mesh meshB, bool fast = true)
        {
            var BboxA = meshA.GetBoundingBox(!fast);
            var BboxB = meshB.GetBoundingBox(!fast);
            return BBoxIntersect(BboxA, BboxB);

        }

        private static bool BBoxIntersect(BoundingBox bboxA, BoundingBox bboxB)
        {
            if (bboxA.Max.X < bboxB.Min.X) return false; 
            if (bboxA.Min.X > bboxB.Max.X) return false; 
            if (bboxA.Max.Y < bboxB.Min.Y) return false; 
            if (bboxA.Min.Y > bboxB.Max.Y) return false;
            if (bboxA.Max.Z < bboxB.Min.Z) return false;
            if (bboxA.Min.Z > bboxB.Max.Z) return false;
            return true; // boxes overlap
        }
    }
}
