using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Embree;

namespace GHEmbree
{
    class HemisphereSampling
    {
        public static List<Rhino.Geometry.Vector3d> Sample(int n, Rhino.Geometry.Vector3d up)
        {
            var hemisphereSample = Sample(n);
            var T = TransformFromUpVector(up);
            var transformedSample = new List<Rhino.Geometry.Vector3d>(n);
            foreach (var v in hemisphereSample)
            {
                v.Transform(T);
                transformedSample.Add(v);

            }
           
            return transformedSample;
        } 

        public static List<Rhino.Geometry.Vector3d> Sample(int n)
        {
            var hemisphereSample = new List<Rhino.Geometry.Vector3d>(n);
            List<EPoint> Hn = hammersly_points(n);
            foreach (EPoint p in Hn)
            {
                hemisphereSample.Add(HemsphereSampleCosine(p.X, p.Y));
            }

            return hemisphereSample;
        }

        private static Rhino.Geometry.Transform TransformFromUpVector(Rhino.Geometry.Vector3d up)
        // http://math.stackexchange.com/questions/180418/calculate-rotation-matrix-to-align-vector-a-to-vector-b-in-3d/
        {
            var T = new Rhino.Geometry.Transform(1.0);
            up.Unitize();
            var unitZ = new Rhino.Geometry.Vector3d(0, 0, 1);
            var v = Rhino.Geometry.Vector3d.CrossProduct(up, unitZ);
            var s = v.Length;
            var c = unitZ * up;
            var I = new Rhino.Geometry.Matrix(3, 3);
            var R = new Rhino.Geometry.Matrix(3, 3);
            I.SetDiagonal(1);
            if (up.EpsilonEquals(new Rhino.Geometry.Vector3d(0, 0, -1), 1e-6))
            { 
                R = I;
                R.Scale(-1);
            } else
            {
                var v_x = new Rhino.Geometry.Matrix(3, 3);
                v_x.SetDiagonal(0);
                var x = v_x[1, 2];
                v_x[0, 1] = v.Z;
                v_x[0, 2] = -v.Y;

                v_x[1, 0] = -v.Z;
                v_x[1, 2] = v.X;

                v_x[2, 0] = v.Y;
                v_x[2, 1] = -v.X;

                var v_x2 = v_x * v_x;
                v_x2.Scale((1 - c) / (s * s));
                R = I + v_x + v_x2;
            }
            
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    T[i, j] = R[i, j];
                }
            }
            return T;
        }

        private static List<EPoint> hammersly_points(int n)
        {
            List<EPoint> Hn = new List<EPoint>(n);
            for (int i = 0; i < n; i++)
            {
                Hn.Add(new EPoint((double)i / n, VDC(i), 0));
            }
            return Hn;

        }

        private static double VDC(int n)
        {
            int b = 2;
            int remainder = 0;
            double vdc = 0.0;
            double denom = 1.0;
            while (n > 0)
            {
                denom *= b;
                n = Math.DivRem(n, b, out remainder);
                // Console.WriteLine(n.ToString());
                vdc += remainder / denom;
            }
            return vdc;
        }

        private static Rhino.Geometry.Vector3d HemsphereSampleCosine(double u1, double u2)
        {
            double r, theta, x, y, z;
            r = Math.Sqrt(u1);
            theta = 2 * Math.PI * u2;
            x = r * Math.Cos(theta);
            y = r * Math.Sin(theta);
            z = Math.Sqrt(Math.Max(0, 1 - u1));
            return new Rhino.Geometry.Vector3d(x, y, z);
        }
    }
}
