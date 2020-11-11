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
            
            foreach (var v in mesh.Vertices)
            {
                vertices.Add(new EPoint(v.X, v.Y, v.Z));
            }
            foreach (var f in mesh.Faces)
            {
                indices.Add(f.A);
                indices.Add(f.C);
                indices.Add(f.B);
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
            Ray[] rays = null;
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
                       
                       rays = new[]
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
    }
}
