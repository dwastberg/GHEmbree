/* This is an example of implementing the Embree.NET geometric types
 * in order to transparently interoperate with Embree.NET. Please do
 * note this math library was NOT implemented for this sample, it is
 * meant to demonstrate that it is easy to convert your types to use
 * the IEmbreeVector, IEmbreeRay, etc.. interfaces.
 *
 * In this case, the vector and point classes already have a correct
 * interface (float X, Y, Z), however because of contravariance, the
 * signatures for the Ray and Matrix classes will never be valid. To
 * remedy this, a simple wrapper can be implemented, as seen below.
 *
 * Feel free to use this more or less complete library in your code.
*/

using System;
using Embree;

namespace GHEmbree
{
    static class Constants
    {
        /// <summary>
        /// A positive real close to zero.
        /// </summary>
        public const float Epsilon = 1e-4f;
    }

    /// <summary>
    /// An immutable three-dimensional vector.
    /// </summary>
    public struct EVector : IEmbreeVector
    {
        private readonly float x;
        private readonly float y;
        private readonly float z;

        /// <summary>
        /// Gets the vector's x-coordinate.
        /// </summary>
        public float X { get { return x; } }

        /// <summary>
        /// Gets the vector's y-coordinate.
        /// </summary>
        public float Y { get { return y; } }

        /// <summary>
        /// Gets the vector's z-coordinate.
        /// </summary>
        public float Z { get { return z; } }

        /// <summary>
        /// Indexed access to vector components (zero-based).
        /// </summary>
        public float this[int t]
        {
            get
            {
                switch (t)
                {
                    case 0: return X;
                    case 1: return Y;
                    case 2: return Z;
                    default: throw new ArgumentOutOfRangeException("Invalid component index: " + t);
                }
            }
        }

        /// <summary>
        /// Constructs a new three-dimensional vector.
        /// </summary>
        public EVector(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public EVector(double x, double y, double z)
        {
            this.x = (float)x;
            this.y = (float)y;
            this.z = (float)z;
        }

        public static EVector operator +(EVector u, EVector v)
        {
            return new EVector(u.X + v.X, u.Y + v.Y, u.Z + v.Z);
        }

        public static EVector operator +(EVector u)
        {
            return new EVector(u.X, u.Y, u.Z);
        }

        public static EVector operator -(EVector u, EVector v)
        {
            return u + (-v);
        }

        public static EVector operator -(EVector u)
        {
            return new EVector(-u.X, -u.Y, -u.Z);
        }

        public static EVector operator *(EVector u, float s)
        {
            return new EVector(u.X * s, u.Y * s, u.Z * s);
        }

        public static EVector operator *(float s, EVector u)
        {
            return u * s;
        }

        public static EVector operator /(EVector u, float s)
        {
            return u * (1.0f / s);
        }

        public static EVector operator /(float s, EVector u)
        {
            return u / s;
        }

        /// <summary>
        /// Reflects a vector about a normal.
        /// </summary>
        public static EVector Reflect(EVector i, EVector n)
        {
            return i - 2 * n * Dot(i, n);
        }

        /// <summary>
        /// Returns the length of a vector.
        /// </summary>
        public static float Length(EVector u)
        {
            return (float)Math.Sqrt(Dot(u, u));
        }

        /// <summary>
        /// Normalizes a vector to unit length.
        /// </summary>
        public static EVector Normalize(EVector u)
        {
            var len = Length(u);
            if (len > 0) return u / len;
            else throw new InvalidOperationException("Vector has no direction");
        }

        /// <summary>
        /// Returns the inclination of a vector.
        /// </summary>
        /// <remarks>
        /// Vertical angle, zero on the xz-plane.
        /// </remarks>
        public static float Inclination(EVector u)
        {
            var len = Length(u);
            if (len > 0) return (float)(Math.Acos(u.Y / len) - Math.PI / 2);
            else throw new InvalidOperationException("Vector has no direction");
        }

        /// <summary>
        /// Returns the azimuth of a vector.
        /// </summary>
        /// <remarks>
        /// Horizontal angle, zero towards the z-axis.
        /// </remarks>
        public static float Azimuth(EVector u)
        {
            var len = Length(u);
            if (len > 0) return (float)Math.Atan2(u.Z, u.X);
            else throw new InvalidOperationException("Vector has no direction");
        }

        /// <summary>
        /// Returns the dot product of two vectors.
        /// </summary>
        public static float Dot(EVector u, EVector v)
        {
            return u.X * v.X + u.Y * v.Y + u.Z * v.Z;
        }

        /// <summary>
        /// Returns the cross product of two vectors.
        /// </summary>
        public static EVector Cross(EVector u, EVector v)
        {
            return new EVector(u.Y * v.Z - u.Z * v.Y,
                              u.Z * v.X - u.X * v.Z,
                              u.X * v.Y - u.Y * v.X);
        }

        /// <summary>
        /// Reflects this vector about a normal.
        /// </summary>
        public EVector Reflect(EVector n)
        {
            return Reflect(this, n);
        }

        /// <summary>
        /// Returns the length of this vector.
        /// </summary>
        public float Length()
        {
            return Length(this);
        }

        /// <summary>
        /// Returns this vector, normalized.
        /// </summary>
        public EVector Normalize()
        {
            return Normalize(this);
        }

        /// <summary>
        /// Returns the inclination of this vector.
        /// </summary>
        public float Inclination()
        {
            return Inclination(this);
        }

        /// <summary>
        /// Returns the azimuth of this vector.
        /// </summary>
        public float Azimuth()
        {
            return Azimuth(this);
        }

        /// <summary>
        /// Returns the dot product of this vector with another.
        /// </summary>
        public float Dot(EVector v)
        {
            return Dot(this, v);
        }

        /// <summary>
        /// Returns the cross product of this vector with another.
        /// </summary>
        public EVector Cross(EVector v)
        {
            return Cross(this, v);
        }

        /// <summary>
        /// Converts a vector into a point.
        /// </summary>
        /// <remarks>
        /// This is meaningless mathematically.
        /// </remarks>
        public static EPoint ToPoint(EVector u)
        {
            return EPoint.Zero + u;
        }

        /// <summary>
        /// Converts this vector into a point.
        /// </summary>
        public EPoint ToPoint()
        {
            return ToPoint(this);
        }

        /// <summary>
        /// Returns a textual representation of this vector.
        /// </summary>
        public override string ToString()
        {
            return string.Format("[{0:G2}, {1:G2}, {2:G2}]", X, Y, Z);
        }

        /// <summary>
        /// The vector with length zero.
        /// </summary>
        public static EVector Zero = new EVector(0, 0, 0);
    }

    /// <summary>
    /// An immutable three-dimensional point.
    /// </summary>
    public struct EPoint : IEmbreePoint
    {
        private readonly float x;
        private readonly float y;
        private readonly float z;

        /// <summary>
        /// Gets the point's x-coordinate.
        /// </summary>
        public float X { get { return x; } }

        /// <summary>
        /// Gets the point's y-coordinate.
        /// </summary>
        public float Y { get { return y; } }

        /// <summary>
        /// Gets the point's z-coordinate.
        /// </summary>
        public float Z { get { return z; } }

        /// <summary>
        /// Indexed access to point components (zero-based).
        /// </summary>
        public float this[int t]
        {
            get
            {
                switch (t)
                {
                    case 0: return X;
                    case 1: return Y;
                    case 2: return Z;
                    default: throw new ArgumentOutOfRangeException("Invalid component index: " + t);
                }
            }
        }

        /// <summary>
        /// Constructs a new three-dimensional point.
        /// </summary>
        public EPoint(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public EPoint(double x, double y, double z)
        {
            this.x = (float)x;
            this.y = (float)y;
            this.z = (float)z;
        }

        public static EPoint operator +(EPoint p, EVector u)
        {
            return new EPoint(u.X + p.X, u.Y + p.Y, u.Z + p.Z);
        }

        public static EPoint operator -(EPoint p, EVector u)
        {
            return p + (-u);
        }

        public static EPoint operator +(EVector u, EPoint p)
        {
            return p + u;
        }

        public static EPoint operator +(EPoint u)
        {
            return new EPoint(u.X, u.Y, u.Z);
        }

        public static EVector operator -(EPoint u, EPoint v)
        {
            return new EVector(u.X - v.X, u.Y - v.Y, u.Z - v.Z);
        }

        /// <summary>
        /// Converts a point into a vector.
        /// </summary>
        /// <remarks>
        /// This is meaningless mathematically.
        /// </remarks>
        public static EVector ToVector(EPoint p)
        {
            return p - EPoint.Zero;
        }

        /// <summary>
        /// Converts this point into a vector.
        /// </summary>
        public EVector ToVector()
        {
            return ToVector(this);
        }

        /// <summary>
        /// Returns a textual representation of this point.
        /// </summary>
        public override string ToString()
        {
            return string.Format("[{0:G2}, {1:G2}, {2:G2}]", X, Y, Z);
        }

        /// <summary>
        /// The point at the origin.
        /// </summary>
        public static EPoint Zero = new EPoint(0, 0, 0);
    }

    /// <summary>
    /// An immutable ray, of unit length.
    /// </summary>
    public class Ray : IEmbreeRay
    {
        private readonly EPoint origin;
        private readonly EVector direction;

        /// <summary>
        /// Gets the ray's origin.
        /// </summary>
        public EPoint Origin { get { return origin; } }

        /// <summary>
        /// Gets the ray's (unit length) direction.
        /// </summary>
        public EVector Direction { get { return direction; } }

        #region Embree.NET Interop

        IEmbreePoint  IEmbreeRay.Origin    { get { return origin;    } }
        IEmbreeVector IEmbreeRay.Direction { get { return direction; } }

        #endregion

        /// <summary>
        /// Constructs a ray with an origin and a direction.
        /// </summary>
        public Ray(EPoint origin, EVector direction)
        {
            this.origin = origin;
            this.direction = direction.Normalize();
        }

        /// <summary>
        /// Returns the point at a given distance along the ray.
        /// </summary>
        public static EPoint PointAt(Ray ray, float distance)
        {
            return ray.Origin + ray.Direction * distance;
        }

        /// <summary>
        /// Transforms a ray by a given matrix.
        /// </summary>
        public static Ray Transform(Ray ray, Matrix transform)
        {
            EPoint p1 = transform.Transform(ray.Origin);
            EPoint p2 = transform.Transform(ray.Origin + ray.Direction);

            return new Ray(p1, p2 - p1);
        }

        /// <summary>
        /// Returns a point at a given distance along this ray.
        /// </summary>
        public EPoint PointAt(float distance)
        {
            return PointAt(this, distance);
        }

        /// <summary>
        /// Transforms this ray by a given matrix.
        /// </summary>
        public Ray Transform(Matrix transform)
        {
            return Transform(this, transform);
        }

        public static Ray operator *(Matrix transform, Ray ray)
        {
            return Transform(ray, transform);
        }

        /// <summary>
        /// Returns a textual representation of this ray.
        /// </summary>
        public override string ToString()
        {
            return string.Format("O={0}, D={1}", Origin, Direction);
        }
    }

    /// <summary>
    /// An immutable 3x4 transformation matrix.
    /// </summary>
    /// <remarks>
    /// This matrix is laid out in column major order.
    /// </remarks>
    public class Matrix : IEmbreeMatrix
    {
        private readonly EVector u;
        private readonly EVector v;
        private readonly EVector w;
        private readonly EVector t;

        /// <summary>
        /// The first column (the x-axis of the basis).
        /// </summary>
        public EVector U { get { return u; } }

        /// <summary>
        /// The second column (the y-axis of the basis).
        /// </summary>
        public EVector V { get { return v; } }

        /// <summary>
        /// The third column (the z-axis of the basis).
        /// </summary>
        public EVector W { get { return w; } }

        /// <summary>
        /// The fourth column, corresponding to translation.
        /// </summary>
        public EVector T { get { return t; } }

        #region Embree.NET Interop

        IEmbreeVector IEmbreeMatrix.U { get { return u; } }
        IEmbreeVector IEmbreeMatrix.V { get { return v; } }
        IEmbreeVector IEmbreeMatrix.W { get { return w; } }
        IEmbreeVector IEmbreeMatrix.T { get { return t; } }

        #endregion

        /// <summary>
        /// Indexed access to matrix columns (zero-based).
        /// </summary>
        public EVector this[int t]
        {
            get
            {
                switch (t)
                {
                    case 0: return U;
                    case 1: return V;
                    case 2: return W;
                    case 3: return T;
                    default: throw new ArgumentOutOfRangeException("Invalid column index: " + t);
                }
            }
        }

        /// <summary>
        /// Constructs a matrix from four column vectors.
        /// </summary>
        private Matrix(EVector u, EVector v, EVector w, EVector t)
        {
            this.u = u;
            this.v = v;
            this.w = w;
            this.t = t;
        }

        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        public static Matrix Translation(EVector vec)
        {
            return new Matrix(new EVector(1, 0, 0),
                              new EVector(0, 1, 0),
                              new EVector(0, 0, 1),
                              vec);
        }

        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        /// <param name="vec">The point to become the origin.</param>
        public static Matrix Translation(EPoint pt)
        {
            return new Matrix(new EVector(1, 0, 0),
                              new EVector(0, 1, 0),
                              new EVector(0, 0, 1),
                              pt.ToVector());
        }

        /// <summary>
        /// Creates a uniform scaling matrix.
        /// </summary>
        public static Matrix Scaling(float scale)
        {
            return new Matrix(new EVector(scale, 0, 0),
                              new EVector(0, scale, 0),
                              new EVector(0, 0, scale),
                              EVector.Zero);
        }

        /// <summary>
        /// Creates a possibly non-uniform scaling matrix.
        /// </summary>
        public static Matrix Scaling(EVector scale)
        {
            return new Matrix(new EVector(scale.X, 0, 0),
                              new EVector(0, scale.Y, 0),
                              new EVector(0, 0, scale.Z),
                              EVector.Zero);
        }

        /// <summary>
        /// Creates a rotation matrix about the x-axis,
        /// </summary>
        public static Matrix RotationX(float pitch)
        {
            var c = (float)Math.Cos(pitch);
            var s = (float)Math.Sin(pitch);

            return new Matrix(new EVector(+1, +0, +0),
                              new EVector(+0, +c, +s),
                              new EVector(+0, -s, +c),
                              EVector.Zero);
        }

        /// <summary>
        /// Creates a rotation matrix about the y-axis,
        /// </summary>
        public static Matrix RotationY(float yaw)
        {
            var c = (float)Math.Cos(yaw);
            var s = (float)Math.Sin(yaw);

            return new Matrix(new EVector(+c, +0, -s),
                              new EVector(+0, +1, +0),
                              new EVector(+s, +0, +c),
                              EVector.Zero);
        }

        /// <summary>
        /// Creates a rotation matrix about the z-axis,
        /// </summary>
        public static Matrix RotationZ(float roll)
        {
            var c = (float)Math.Cos(roll);
            var s = (float)Math.Sin(roll);

            return new Matrix(new EVector(+c, +s, +0),
                              new EVector(-s, +c, +0),
                              new EVector(+0, +0, +1),
                              EVector.Zero);
        }

        /// <summary>
        /// Creates a rotation matrix from three Euler angles.
        /// </summary>
        public static Matrix Rotation(float pitch, float yaw, float roll)
        {
            return RotationX(pitch) * RotationY(yaw) * RotationZ(roll);
        }

        /// <summary>
        /// Combines the list of transformations matrices such that a vector
        /// transformed by the resulting matrix would undergo each transform
        /// in the order specified by the arguments.
        /// </summary>
        /// <remarks>
        /// For an ordinary transformation matrix, the order should be:
        ///
        /// - any scaling
        /// - rotation(s)
        /// - translation
        /// </remarks>
        public static Matrix Combine(params Matrix[] transformations)
        {
            var mat = Matrix.Identity;

            foreach (Matrix transform in transformations)
                mat = transform * mat;

            return mat;
        }

        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            var m00 = (m1.U.X * m2.U.X) + (m1.V.X * m2.U.Y) + (m1.W.X * m2.U.Z);
            var m01 = (m1.U.X * m2.V.X) + (m1.V.X * m2.V.Y) + (m1.W.X * m2.V.Z);
            var m02 = (m1.U.X * m2.W.X) + (m1.V.X * m2.W.Y) + (m1.W.X * m2.W.Z);
            var m03 = (m1.U.X * m2.T.X) + (m1.V.X * m2.T.Y) + (m1.W.X * m2.T.Z) + m1.T.X;

            var m10 = (m1.U.Y * m2.U.X) + (m1.V.Y * m2.U.Y) + (m1.W.Y * m2.U.Z);
            var m11 = (m1.U.Y * m2.V.X) + (m1.V.Y * m2.V.Y) + (m1.W.Y * m2.V.Z);
            var m12 = (m1.U.Y * m2.W.X) + (m1.V.Y * m2.W.Y) + (m1.W.Y * m2.W.Z);
            var m13 = (m1.U.Y * m2.T.X) + (m1.V.Y * m2.T.Y) + (m1.W.Y * m2.T.Z) + m1.T.Y;

            var m20 = (m1.U.Z * m2.U.X) + (m1.V.Z * m2.U.Y) + (m1.W.Z * m2.U.Z);
            var m21 = (m1.U.Z * m2.V.X) + (m1.V.Z * m2.V.Y) + (m1.W.Z * m2.V.Z);
            var m22 = (m1.U.Z * m2.W.X) + (m1.V.Z * m2.W.Y) + (m1.W.Z * m2.W.Z);
            var m23 = (m1.U.Z * m2.T.X) + (m1.V.Z * m2.T.Y) + (m1.W.Z * m2.T.Z) + m1.T.Z;

            return new Matrix(new EVector(m00, m10, m20),
                              new EVector(m01, m11, m21),
                              new EVector(m02, m12, m22),
                              new EVector(m03, m13, m23));
        }

        /// <summary>
        /// Transforms a vector by a matrix.
        /// </summary>
        public static EVector Transform(Matrix mat, EVector vec)
        {
            return vec.X * mat.U + vec.Y * mat.V + vec.Z * mat.W;
        }

        /// <summary>
        /// Transforms a point by a matrix.
        /// </summary>
        public static EPoint Transform(Matrix mat, EPoint pt)
        {
            return (pt.X * mat.U + pt.Y * mat.V + pt.Z * mat.W).ToPoint() + mat.T;
        }

        /// <summary>
        /// Transforms a vector by this matrix.
        /// </summary>
        public EVector Transform(EVector vec)
        {
            return Transform(this, vec);
        }

        /// <summary>
        /// Transforms a point by this matrix.
        /// </summary>
        public EPoint Transform(EPoint pt)
        {
            return Transform(this, pt);
        }

        public static EVector operator *(Matrix mat, EVector vec)
        {
            return Transform(mat, vec);
        }

        public static EPoint operator *(Matrix mat, EPoint pt)
        {
            return Transform(mat, pt);
        }

        /// <summary>
        /// Returns the inverse of a matrix.
        /// </summary>
        public static Matrix Invert(Matrix mat)
        {
            // Work out determinant of the 3x3 submatrix.
            var det = mat.U.X * (mat.V.Y * mat.W.Z - mat.W.Y * mat.V.Z)
                    - mat.V.X * (mat.W.Z * mat.U.Y - mat.W.Y * mat.U.Z)
                    + mat.W.X * (mat.U.Y * mat.V.Z - mat.V.Y * mat.U.Z);

            if (Math.Abs(det) < 0)
                throw new ArgumentException("Matrix is not invertible");

            // Compute inv(submatrix) = transpose(submatrix) / det.
            var inv_u = new EVector(mat.U.X, mat.V.X, mat.W.X) / det;
            var inv_v = new EVector(mat.U.Y, mat.V.Y, mat.W.Y) / det;
            var inv_w = new EVector(mat.U.Z, mat.V.Z, mat.W.Z) / det;

            // Transform the translation column by this inverse matrix.
            var inv_t = -(mat.T.X * inv_u + mat.T.Y * inv_v + mat.T.Z * inv_w);

            return new Matrix(inv_u, inv_v, inv_w, inv_t);
        }

        /// <summary>
        /// Returns the "transpose" of a matrix, this is the
        /// transpose of the 3x3 submatrix, no translation.
        /// </summary>
        /// <remarks>
        /// In general, Transpose(Transpose(mat)) != mat.
        /// </remarks>
        private static Matrix Transpose(Matrix mat)
        {
            return new Matrix(new EVector(mat.U.X, mat.V.X, mat.W.X),
                              new EVector(mat.U.Y, mat.V.Y, mat.W.Y),
                              new EVector(mat.U.Z, mat.V.Z, mat.W.Z),
                              EVector.Zero);
        }

        /// <summary>
        /// Returns the inverse transpose of this matrix,
        /// for use in transformation of normal vectors.
        /// </summary>
        /// <remarks>
        /// The resulting matrix always has no translation.
        /// </remarks>
        /// <remarks>
        /// If the matrix is orthogonal (rotation only) then
        /// this method returns the same matrix.
        /// </remarks>
        public static Matrix InverseTranspose(Matrix mat)
        {
            return Transpose(Invert(mat));
        }

        /// <summary>
        /// Inverts this matrix.
        /// </summary>
        public Matrix Invert()
        {
            return Invert(this);
        }

        /// <summary>
        /// Transposes this matrix.
        /// </summary>
        private Matrix Transpose()
        {
            return Transpose(this);
        }

        /// <summary>
        /// Returns the inverse transpose of this matrix.
        /// </summary>
        public Matrix InverseTranspose()
        {
            return InverseTranspose(this);
        }

        /// <summary>
        /// Returns a textual representation of this matrix.
        /// </summary>
        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}, {3}]", U, V, W, T);
        }

        /// <summary>
        /// Returns the 3x4 identity transformation matrix.
        /// </summary>
        public static Matrix Identity = new Matrix(new EVector(1, 0, 0),
                                                   new EVector(0, 1, 0),
                                                   new EVector(0, 0, 1),
                                                   new EVector(0, 0, 0));
    }
}
