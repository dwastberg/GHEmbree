using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Embree;

namespace GHEmbree
{
    class EmbreeTools
    {
        

        private static TriangleMesh BuildEmbreeMesh(Device device, Rhino.Geometry.Mesh mesh)
        {
            List<IEmbreePoint> vertices = new List<IEmbreePoint>();
            List<int> indices = new List<int>();
            mesh.Faces.ConvertQuadsToTriangles();
            mesh.Compact();


            foreach (var v in mesh.Vertices)
            {
                vertices.Add(new EPoint(v.X, v.Y, v.Z));
            }
            foreach (var f in mesh.Faces)
            {
                indices.Add(f.A);
                indices.Add(f.B);
                indices.Add(f.C);
            }
            return new TriangleMesh(device, indices, vertices);

        }

        public static Scene<Model> BuildScene(List<Rhino.Geometry.Mesh> meshes)
        {
            var merged_mesh = new Rhino.Geometry.Mesh();
            foreach (var m in meshes)
            {
                merged_mesh.Append(m);
            }
            return BuildScene(merged_mesh);
        }



        public static Scene<Model> BuildScene(Rhino.Geometry.Mesh mesh)
        {
            Device device = new Device();
            Model model = new Model(device);
            Scene<Model> scene = new Scene<Model>(device, Flags.SCENE, Flags.TRAVERSAL);
            TriangleMesh embreeMesh = BuildEmbreeMesh(device, mesh);
            model.AddMesh(embreeMesh);

            scene.Add(model);
            scene.Commit();

            return scene;
        }

        public static List<int> OcclusionHits(Scene<Model> scene,List<Rhino.Geometry.Point3d> pts, List<Rhino.Geometry.Vector3d> viewVectors, bool reverseView = false)
        {
            int ptCount = pts.Count;
            var hitCount = new List<int>(new int[ptCount]); // initialzie with zeros
            var Eviews = new List<EVector>();

            foreach (var v in viewVectors)
            {
                if (reverseView) { v.Reverse(); }
                Eviews.Add(new EVector(v.X, v.Y, v.Z));
            }
           
            Parallel.For(0, ptCount,
                idx =>
                {
                    // int hits = 0;
                    var p = pts[idx];
                    var ePt = new EPoint(p.X, p.Y, p.Z);
                    foreach (var v in Eviews)
                    {
                        bool hit = scene.Occludes(new Ray(ePt, v));
                        if (hit) { hitCount[idx]++; }
                    }
                });

            return hitCount;
        }

        public static List<int> OcclusionHits4(Scene<Model> scene, List<Rhino.Geometry.Point3d> pts, List<Rhino.Geometry.Vector3d> viewVectors, bool reverseView = false)
        {
            int ray_packet_size = 4;
            int ptCount = pts.Count;
            int viewCount = viewVectors.Count;
            var hitCount = new List<int>(new int[ptCount]); // initialzie with zeros
            var EViews = new List<EVector>();
            bool[] hits = null;

            foreach (var v in viewVectors)
            {
                if (reverseView) { v.Reverse(); }
                EViews.Add(new EVector(v.X, v.Y, v.Z));
            }

            int ray_padding = (ray_packet_size - (viewCount % ray_packet_size)) % 4;
            for (int i = 0; i < ray_padding; i++)
            {
                EViews.Add(new EVector(0, 0, -1));
            }


            Parallel.For(0, ptCount,
               idx =>
               {
                   var p = pts[idx];
                   var ePt = new EPoint(p.X, p.Y, p.Z);
                   int processedRays = 0;
                   for (int i = 0; i < viewCount; i += ray_packet_size)
                   {
                       
                       var rays = new Ray[]
                       {
                           new Ray(ePt, EViews[i]),
                           new Ray(ePt, EViews[i+1]),
                           new Ray(ePt, EViews[i+2]),
                           new Ray(ePt, EViews[i+3])
                       };
                       hits = scene.Occludes4(rays);
                       foreach (var hit in hits)
                       {
                           processedRays++;
                           if (processedRays>viewCount) { break; }
                           if (hit) { hitCount[idx]++; }
                       }
                       
                   }
               });
            return hitCount;
        }

        public static List<Rhino.Geometry.Point> Intersections(Scene<Model> scene, List<Rhino.Geometry.Point3d> pts, List<Rhino.Geometry.Vector3d> directions, ref List<bool> hitMask)
        {
            int outputCount = Math.Max(pts.Count, directions.Count);
            var meshIntersections = new Rhino.Geometry.Point[outputCount];
            //var meshIntersections = new List<Rhino.Geometry.Point>();
            var eDirections = new List<EVector>(directions.Count);
            

            // needed because we cannot access ref variables inside Parallel.For lambda
            var localHitMask = new bool[outputCount];

            foreach (var d in directions)
            {
                eDirections.Add(new EVector(d.X, d.Y, d.Z));
            }

            // Intersection<Model>[] hits = null;
            Parallel.For(0, outputCount, idx =>
            {
                var pt = pts[Math.Min(idx, pts.Count - 1)];
                var direction = eDirections[Math.Min(idx, eDirections.Count - 1)];

                var ray = new Ray(new EPoint(pt.X, pt.Y, pt.Z), direction);
                var packet = scene.Intersects(ray);
                var hits = new Intersection<Model>[] { packet.ToIntersection<Model>(scene) };
                localHitMask[idx] = (hits[0].HasHit);
                if (hits[0].HasHit)
                {
                    var hitPos = ray.PointAt(hits[0].Distance);
                    meshIntersections[idx] = (new Rhino.Geometry.Point(new Rhino.Geometry.Point3d(hitPos.X, hitPos.Y, hitPos.Z)));
                }
                else
                {
                    meshIntersections[idx] = null;
                }
            });

            hitMask.AddRange(localHitMask);

            return new List<Rhino.Geometry.Point>(meshIntersections);
            //return meshIntersections;
        }
    }
}
