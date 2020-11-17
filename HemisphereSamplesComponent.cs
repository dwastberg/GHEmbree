using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GHEmbree
{
    public class HemisphereSamplesComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public HemisphereSamplesComponent()
          : base("HemisphereSample", "HemiS",
              "Sample points on a unit hemisphere",
              "Mesh", "Embree")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Samples", "N", "Number of Samples",GH_ParamAccess.item);
            pManager.AddAngleParameter("Cone", "A", "Angle of sample cone",GH_ParamAccess.item,Math.PI);
            pManager.AddVectorParameter("Up", "U", "Direction of the hemisphere", GH_ParamAccess.item, new Vector3d(0.0, 0.0, 1.0));
            pManager.AddBooleanParameter("Reverse", "R", "Point sample vectors towards hemisphere center", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Sample Vectors","V","Direction vectors",GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int numSamples = 0;
            double angle = Math.PI;
            var up = new Rhino.Geometry.Vector3d();

            if (!DA.GetData(0, ref numSamples)) { return; }
            if (!DA.GetData(1, ref angle)) { return; }
            if (!DA.GetData(2, ref up)) { return; }
            var hemisphereSample = new List<Vector3d>();

            if (up.EpsilonEquals(new Vector3d(0, 0, 1), 1e-5))
            {
                hemisphereSample = HemisphereSampling.Sample(numSamples);
            } else
            {
                hemisphereSample = HemisphereSampling.Sample(numSamples,up);
            }
            

            DA.SetDataList(0, hemisphereSample);
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
            get { return new Guid("0317446a-89e0-4c6a-be0a-449920094501"); }
        }
    }
}