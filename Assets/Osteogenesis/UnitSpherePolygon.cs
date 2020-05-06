using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Osteogenesis
{
    public class UnitSpherePolygon
    {
        private Vector3[] _vertices;

        public UnitSpherePolygon(List<Vector3> vertices)
        {
            var uniqueVertices = new List<Vector3>();
            foreach (var vertex in vertices)
            {
                bool alreadyExists = false;
                foreach (var uniqueVertex in uniqueVertices)
                {
                    if (approximateEqualVector3(vertex, uniqueVertex))
                    {
                        alreadyExists = true;
                        break;
                    }
                }

                if (!alreadyExists)
                {
                    uniqueVertices.Add(vertex);
                }
            }

            _vertices = uniqueVertices.ToArray();
        }

        private bool approximateEqualVector3(Vector3 a, Vector3 b)
        {
            for(int i = 0; i < 3; i++)
            {
                
                if (!fastApproximatelyEquals(a[i], b[i])) return false;
            }

            return true;
        }

        private bool fastApproximatelyEquals(float a, float b)
        {
            return Mathf.Abs(b - a) < 1E-05f;
        }

        /**
         * Returns the surface area of the represented polygon
         */
        public float GetArea()
        {
            if (_vertices == null || _vertices.Length < 3) return 0.0f;
            var internalAngles = GetInternalAngles();
            var excess = internalAngles.Sum() - (internalAngles.Length - 2) * Mathf.PI;
            return excess;
        }

        public override string ToString()
        {
            string info = $"{nameof(_vertices)}: {_vertices}";
            foreach (var vertex in _vertices)
            {
                info += vertex.ToString("F4") + ", ";
            }
            return info;
        }

        public Vector3[] GetVertices()
        {
            return _vertices;
        }
        
        /**
         * Gets a list of internal triangles to debug with
         */
        public List<Triangle> GetInternalTriangles()
        {
            if (_vertices == null || _vertices.Length < 3) return new List<Triangle>();
            
            int count = _vertices.Length;

            var tris = new List<Triangle>(count);

            for (int i = 0; i < count; i++)
            {
                var v0 = _vertices[i];
                var v1 = _vertices[(i + 1) % count];
                var triangle = new Triangle(v0, Vector3.zero, v1);
                
                tris.Add(triangle);
            }

            return tris;
        }

        public float[] GetInternalAngles()
        {
            //No internal angles exist for no input or an input of 2 or fewer vertices
            if (_vertices == null || _vertices.Length < 3) return new float[0];

            int count = _vertices.Length;
            
            //Terminology:
            //v0 = current vertex
            //v1 = next vertex
            //n0 = normal between v0 and v1
            //dv = direction from v0 to v1 (v2 - v1)

            //Construct a list of triangles to represent planes and
            //Calculate their normals and their n -> n+1 directional vectors
            var internalNormals = new Vector3[count];
            var directionalVectors = new Vector3[count];
            
            
            for (int i = 0; i < count; i++)
            {
                var v0 = _vertices[i];
                var v1 = _vertices[(i + 1) % count];
                
                var triangle = new Triangle(v0, Vector3.zero, v1);
                
                internalNormals[i] = triangle.GetNormal();
                directionalVectors[i] = v0 - v1;
            }
            
            //the needed dv for vertex v0 is dv = v2 - v1
            //so it is directionalVectors[(i + 1) % count]
            //When dot(n0, dv) >= 0 , return theta(n0, -n1) otherwise return 360 - theta(n0, -n1)
            var internalAngles = new float[count];
            for (int i = 0; i < count; i++)
            {
                var iNext = (i + 1) % count;
                var n0 = internalNormals[i];
                var n1 = internalNormals[iNext];
                var dv = directionalVectors[iNext];
                internalAngles[i] = GetInternalAngle2Surfaces(n0, n1, dv) * Mathf.Deg2Rad;
            }

            return internalAngles;
        }
        
        /**
         * Returns the internal angle of 2 given surface normals and the directional vector of the second surface
         * dv should be the difference between the 2 vertices of edge n1, so (v2 - v1)
         */
        private float GetInternalAngle2Surfaces(Vector3 n0, Vector3 n1, Vector3 dv)
        {
            return Vector3.Dot(n0, dv) >= 0 ? Vector3.Angle(n0, -n1) : 360 - Vector3.Angle(n0, -n1);
        }
    }
}