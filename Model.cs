using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Embree;

namespace GHEmbree
{
    public class Flags
    {
        public const SceneFlags SCENE = SceneFlags.Static | SceneFlags.Coherent | SceneFlags.HighQuality | SceneFlags.Robust;
        public const TraversalFlags TRAVERSAL = TraversalFlags.Single | TraversalFlags.Packet4 | TraversalFlags.Packet8;
    }

    public class Model : IInstance, IDisposable
    {
        private readonly Geometry geometry;
        private Matrix transform, inverseTranspose;

        /// <summary>
        /// Gets the wrapped Geometry collection.
        /// </summary>
        public Geometry Geometry { get { return geometry; } }

        /// <summary>
        /// Gets or sets whether this model is enabled.
        /// </summary>
        public Boolean Enabled { get; set; }


        /// <summary>
        /// Gets the transform associated with this model.
        /// </summary>
        public IEmbreeMatrix Transform { get { return transform; } }

        /// <summary>
        /// Creates a new empty model.
        /// </summary>
        public Model(Device device)
        {
            Enabled = true;
            this.transform = Matrix.Identity;
            inverseTranspose = Matrix.InverseTranspose(transform);
            geometry = new Geometry(device, Flags.SCENE, Flags.TRAVERSAL);
        }

        /// <summary>
        /// Adds a mesh to this model
        /// </summary>
        public void AddMesh(IMesh mesh)
        {
            geometry.Add(mesh);

        }


        /// <summary>
        /// Corrects an Embree.NET normal, which is unnormalized
        /// and in object space, to a world space normal vector.
        /// </summary>
        public EVector CorrectNormal(EVector normal)
        {
            return (inverseTranspose * normal).Normalize();
        }

        #region IDisposable

        ~Model()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                geometry.Dispose();
            }
        }

        #endregion
    }
}
