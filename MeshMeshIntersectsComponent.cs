using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace GHEmbree
{
    public class MeshMeshIntersectsComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public MeshMeshIntersectsComponent()
          : base("MeshMeshIntesects", "M|M",
              "Test for intersection between two meshes",
              "Mesh", "GHEmbree")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("MeshA", "A", "Mesh A", GH_ParamAccess.item);
            pManager.AddMeshParameter("MeshB", "B", "Mesh B", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Fast", "F", "Use faster method, can give false negative in some corner cases", GH_ParamAccess.item,true);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Intersect", "I", "Do meshes intersect", GH_ParamAccess.item);
            pManager.AddPointParameter("Intersection point", "P", "First intersection point found", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var meshA = new Rhino.Geometry.Mesh();
            var meshB = new Rhino.Geometry.Mesh();
            bool fastApprox = false;

            if (!DA.GetData(0, ref meshA)) { return; }
            if (!DA.GetData(1, ref meshB)) { return; }
            if (!DA.GetData(2, ref fastApprox)) { return; }

            var iPt = new Point3d();
            bool intersects = MeshMeshIntersect.TestIntersect(meshA, meshB, ref iPt);
            

            DA.SetData(0, intersects);
            if (intersects)
            {
                DA.SetData(1, iPt);
            }
            
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("12bff981-9d80-41e8-8964-789f1e959448"); }
        }
    }
}