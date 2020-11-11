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
    }
}
