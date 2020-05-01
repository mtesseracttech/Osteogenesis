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
            var angles = GetInternalAnglesPolygonOnUnitSphere(new List<Vector3>
            {
                a, b, c, d
            });

            return new Vector4(angles[0], angles[1], angles[2], angles[3]);
        }


        public static float AreaPolygonOnUnitSphere(List<Vector3> vertices)
        {
            if (vertices == null || vertices.Count == 0) return 0;
            var angles = GetInternalAnglesPolygonOnUnitSphere(vertices);
            return angles.Sum() - (vertices.Count - 2) * Mathf.PI;
        }

        private static List<float> GetInternalAnglesPolygonOnUnitSphere(List<Vector3> vertices)
        {
            if (vertices == null) return null;
            if (vertices.Count == 0) return new List<float>();

            //Generating the intersecting plane normals
            var normals = new List<Vector3>();
            for (int i = 0; i < vertices.Count; i++)
            {
                normals.Add(
                    Vector3.Cross(-vertices[i], vertices[(i + 1) % vertices.Count] - vertices[i]).normalized
                );
            }

            var angles = new List<float>();

            //Calculating dihedral angles
            for (int i = 0; i < normals.Count; i++)
            {
                float angle;
                if (Vector3.Dot(normals[i], -normals[(i + 1) % normals.Count]) <= 0f)
                {
                    angle = Vector3.Angle(normals[i], -normals[(i + 1) % normals.Count]) * Mathf.Deg2Rad;
                }
                else
                {
                    angle = (360f - Vector3.Angle(normals[i], -normals[(i + 1) % normals.Count])) * Mathf.Deg2Rad;
                }

                angles.Add(
                    angle
                );
            }

            //DrawInternalPolygonWithNormals(vertices, normals);

            return angles;
        }

        private static void DrawInternalPolygonWithNormals(List<Vector3> vertices, List<Vector3> normals)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                //More efficient variant of:
                var tri = new Triangle(
                    vertices[i],
                    Vector3.zero,
                    vertices[(i + 1) % vertices.Count]
                );

                //Drawing the triangle
                switch (i)
                {
                    case 0:
                        tri.DebugDraw(Color.red);
                        break;
                    case 1:
                        tri.DebugDraw(Color.yellow);
                        break;
                    default:
                        tri.DebugDraw(Color.green);
                        break;
                }

                //Drawing its normal
                Debug.DrawRay((vertices[i] + vertices[(i + 1) % vertices.Count]) / 3f, normals[i]);
            }
        }
    }
}