using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Osteogenesis
{
    public class GeometryUtils
    {
        public static float AreaTriangleOnUnitSphere(Vector3 a, Vector3 b, Vector3 c)
        {
            var angles = GetInternalAnglesTriangleUnitSphere(a, b, c);
            return angles.x + angles.y + angles.z - Mathf.PI;
        }

        /*
         * Faster version of the algorithm, because triangles are simpler and cannot be concave
         */
        public static Vector3 GetInternalAnglesTriangleUnitSphere(Vector3 a, Vector3 b, Vector3 c)
        {
            //Calculate normals
            var normalA = Vector3.Cross(-b, c - b).normalized;
            var normalB = Vector3.Cross(-c, a - c).normalized;
            var normalC = Vector3.Cross(-a, b - a).normalized;

            //Calculate their dihedral angles
            //Invert the one pointing out and measure angle
            var angleA = Vector3.Angle(normalB, -normalC);
            var angleB = Vector3.Angle(normalC, -normalA);
            var angleC = Vector3.Angle(normalA, -normalB);

            //Returning the angles
            return new Vector3(angleA, angleB, angleC) * Mathf.Deg2Rad;
        }

        public static Vector3 GetInternalAnglesTriangleUnitSphere(Triangle tri)
        {
            return GetInternalAnglesTriangleUnitSphere(tri.A, tri.B, tri.C);
        }

        public static float AreaQuadOnUnitSphere(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            var angles = GetInternalAnglesQuadUnitSphere(a, b, c, d);
            return angles.x + angles.y + angles.z + angles.w - 2.0f * Mathf.PI;
        }

        public static float AreaQuadOnUnitySphere(Quad quad)
        {
            return AreaQuadOnUnitSphere(quad.A, quad.B, quad.C, quad.D);
        }

        public static Vector4 GetInternalAnglesQuadUnitSphere(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            var angles = new UnitSpherePolygon(new List<Vector3>
            {
                a, b, c, d
            }).GetInternalAngles();

            return new Vector4(angles[0], angles[1], angles[2], angles[3]);
        }
        
    }
}