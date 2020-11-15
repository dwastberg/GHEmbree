using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GHEmbree
{
    public class EmbreeMeshRayIntersectComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public EmbreeMeshRayIntersectComponent()
          : base("EmbreeMesh|Ray", "EmMeshRay",
              "Intersect mesh with ray",
              "Mesh", "Embree")
        {
        }
        
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Meshes to intersect", GH_ParamAccess.list);
            pManager.AddPointParameter("Point", "P", "Ray start point", GH_ParamAccess.list);
            pManager.AddVectorParameter("Direction", "D", "Ray direction", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "X", "First intersection point", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Hit", "H", "Hit or miss", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var pts = new List<Rhino.Geometry.Point3d>();
            var obstructions = new List<Rhino.Geometry.Mesh>();
            var directions = new List<Rhino.Geometry.Vector3d>();

            
            if (!DA.GetDataList(0, obstructions)) { return; }
            if (!DA.GetDataList(1, pts)) { return; }
            if (!DA.GetDataList(2, directions)) { return; }

            var hitList = new List<bool>();

            var scene = EmbreeTools.BuildScene(obstructions);
            var intersectionPoints = EmbreeTools.Intersections(scene, pts, directions, ref hitList);

            DA.SetDataList(0, intersectionPoints);
            DA.SetDataList(1, hitList);
            
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
            get { return new Guid("f5b0f24a-66ec-4c7d-b5dc-b533754d2073"); }
        }
    }
}