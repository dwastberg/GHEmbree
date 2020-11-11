﻿using System;
using System.Collections.Generic;
using Embree;
using System.Threading;
using System.Threading.Tasks;



using Grasshopper.Kernel;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace GHEmbree
{
    public class EmbreeOcclusionComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public EmbreeOcclusionComponent()
          : base("EmbreeOcclusion", "EmOcc",
              "Fast mesh ray occlusion testing ",
              "Mesh", "Embree")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Samples", "S", "Sample points for occlusion testing", GH_ParamAccess.list);
            pManager.AddMeshParameter("Obstructions", "O", "Obstructing geometry    ", GH_ParamAccess.list);
            pManager.AddVectorParameter("Rays", "R", "View Rays", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Hits", "H", "Number of occluded rays per sample", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Occlusions", "O", "Occlusion topology for every individual sample", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var pts = new List<Rhino.Geometry.Point3d>();
            var obstructions = new List<Rhino.Geometry.Mesh>();
            var rays = new List<Rhino.Geometry.Vector3d>();
            var Erays = new List<EVector>();

            if (!DA.GetDataList(0, pts)) { return; }
            if (!DA.GetDataList(1, obstructions)) { return; }
            if (!DA.GetDataList(2, rays)) { return; }

            var scene = EmbreeTools.BuildScene(obstructions);

            int ptCount = pts.Count;
            var hitCount = new List<int>(new int[ptCount]); // initialzie with zeros


            foreach (var v in rays)
            {
                v.Reverse();
                Erays.Add(new EVector(v.X, v.Y, v.Z));
            }

            Parallel.For(0,ptCount,
                idx => 
                {
                    // int hits = 0;
                    var p = pts[idx];
                    var ePt = new EPoint(p.X, p.Y, p.Z);
                    foreach (var r in Erays)
                    {
                        bool hit = scene.Occludes(new Ray(ePt, r));
                        if(hit) { hitCount[idx]++;  }
                    }
                });
            DA.SetDataList(0, hitCount);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("34ff032d-6cf7-4ca7-9c51-79d1a9cf42a2"); }
        }
    }
}
