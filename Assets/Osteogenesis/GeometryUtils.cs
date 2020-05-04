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

            var excess = angles.Sum() - ((vertices.Count - 2) * Mathf.PI);

            //DrawInternalPolygonWithNormals(vertices, null);

            // if (excess > 1f)
            // {
            //     string debugInfo = "Overly large output" + excess + " for: ";
            //     foreach (var vertex in vertices)
            //     {
            //         debugInfo += vertex.ToString("F4") + " ";
            //     }
            //
            //     debugInfo += '\n';
            //
            //     foreach (var angle in angles)
            //     {
            //         debugInfo += angle + " ";
            //     }
            //     
            //     debugInfo += '\n';
            //
            //     debugInfo += "Angle sum: " + angles.Sum();
            //     
            //     Debug.Log(debugInfo);
            //     
            //     DrawInternalPolygonWithNormals(vertices, null);
            // }

            return excess;
        }

        private static List<float> GetInternalAnglesPolygonOnUnitSphere(List<Vector3> vertices)
        {
            if (vertices == null) return null;
            if (vertices.Count == 0) return new List<float>();

            //Generating the intersecting plane normals
            var normals = new List<Vector3>(vertices.Count);
            var directionals = new List<Vector3>(vertices.Count);
            
            for (int i = 0; i < vertices.Count; i++)
            {
                //var t =
                //    new Triangle(vertices[i], Vector3.zero, vertices[(i + 1) % vertices.Count]);
                //normals.Add(t.GetNormal());
                //var dw = t.C - t.A;
                //directionals.Add(dw);
                
                normals.Add(
                    Vector3.Cross(-vertices[i], vertices[(i + 1) % vertices.Count] - vertices[i]).normalized
                );
                
                directionals.Add(vertices[(i + 1) % vertices.Count] - vertices[i]);
            }

            var angles = new List<float>(normals.Count);

            //Calculating dihedral angles
            for (int i = 0; i < normals.Count; i++)
            {
                var n1 = normals[i];
                var n2 = normals[(i + 1) % normals.Count];
                var dw = directionals[(i + 1) % normals.Count];
                var angle = AngleBetweenSurfaceNormals(n1, n2, dw) * Mathf.Deg2Rad;
                
                angles.Add(angle);
                // angles.Add(
                //     AngleBetweenSurfaceNormals(normals[i], normals[(i + 1) % normals.Count]) * Mathf.Deg2Rad
                // );
            }

            // string debug = "Angles: ";
            // foreach (var angle in angles)
            // {
            //     debug += angle + ", ";
            // }

            //DrawInternalPolygonWithNormals(vertices, normals);

            return angles;
        }

        /*
         * Calculates the internal angle between 2 surface
         * (values range from 0 to 360)
         */
        public static float AngleBetweenSurfaceNormals(Vector3 n1, Vector3 n2, Vector3 dw)
        {
            return Vector3.Dot(n1, dw) < 0f ? Vector3.Angle(n1, -n2) : 360f - Vector3.Angle(n1, -n2);
        }


        // /*
        //  * Calculates the internal angle between 2 surface
        //  * (values range from 0 to 360)
        //  */
        // public static float AngleBetweenSurfaceNormals(Vector3 n1, Vector3 n2)
        // {
        //     return Vector3.Dot(n1, n2) > 0f ? Vector3.Angle(n1, -n2) : 180f - Vector3.Angle(n1, -n2);
        // }

        private static void DrawInternalPolygonWithNormals(List<Vector3> vertices, List<Vector3> normals)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                var tri = new Triangle(
                    vertices[i],
                    Vector3.zero,
                    vertices[(i + 1) % vertices.Count]
                );

                //Drawing the triangle
                switch (i)
                {
                    case 0:
                        tri.DebugDraw(Color.green, 300f);
                        break;
                    case 1:
                        tri.DebugDraw(Color.yellow, 300f);
                        break;
                    default:
                        tri.DebugDraw(Color.white, 300f);
                        break;
                }

                //Drawing its normal
                if (normals != null) Debug.DrawRay((vertices[i] + vertices[(i + 1) % vertices.Count]) / 3f, normals[i]);
            }
        }
    }
}