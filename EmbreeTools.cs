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
            // var eDirections = new List<EVector>(directions.Count);
            int ray_packet_size = 4;
            var rayPackets = new List<Ray[]>();             

            // needed because we cannot access ref variables inside Parallel.For lambda
            var localHitMask = new bool[outputCount];

            int total_pack_count = 0;
            for (int i = 0; i < outputCount; i=i+ray_packet_size)
            {
                var ray_pack = new Ray[4];
                for (int j = 0; j < ray_packet_size; j++)
                {
                    var pt = pts[Math.Min(i+j, pts.Count - 1)];
                    var direction = directions[Math.Min(i+j, directions.Count - 1)];
                    ray_pack[j] = new Ray(new EPoint(pt.X, pt.Y, pt.Z), new EVector(direction.X, direction.Y, direction.Z));
                    total_pack_count++;
                }
                rayPackets.Add(ray_pack);
            }
            // we have this many 'extra' rays in out last ray packet that we need to ignore
            int overPack = total_pack_count - outputCount;

            Parallel.For(0, rayPackets.Count, idx => {
                var packet = scene.Intersects4(rayPackets[idx]);
                var hits = packet.ToIntersection<Model>(scene);
                for (int i = 0; i < ray_packet_size; i++)
                {
                    if ( (idx * 4 + i) >= outputCount ) { break;  } 
                    localHitMask[idx * 4 + i] = hits[i].HasHit;
                    if (hits[i].HasHit)
                    {
                        var hitPos = rayPackets[idx][i].PointAt(hits[i].Distance);
                        meshIntersections[idx * 4 + i] = (new Rhino.Geometry.Point(new Rhino.Geometry.Point3d(hitPos.X, hitPos.Y, hitPos.Z)));
                    } else
                    {
                        meshIntersections[idx * 4 + i] = null;
                    }
                }

            });

            hitMask.AddRange(localHitMask);

            return new List<Rhino.Geometry.Point>(meshIntersections);
            //return meshIntersections;
        }
    }
}
