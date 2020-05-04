using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Osteogenesis
{
    public class GaussArea
    {
        private Dictionary<Vector3, float> _vertexGaussArea = new Dictionary<Vector3, float>();

        private Dictionary<Vector3, Vector3> _pivotNormals = new Dictionary<Vector3, Vector3>();
        
        public GaussArea(Mesh mesh)
        {
            var faceIndexTriplets = new List<FaceIndexTriplet>(mesh.GetIndices(0).Length / 3);

            //Creating a list of index triplets
            var indices = mesh.GetIndices(0);
            for (int i = 0; i < indices.Length; i += 3)
            {
                faceIndexTriplets.Add(new FaceIndexTriplet(indices[i], indices[i + 1], indices[i + 2]));
            }
            
            Debug.Log("Creating " + faceIndexTriplets.Count + " face index triplets done");

            //Creating vertex -> connecting triangles dictionary
            //Deriving gauss area from vertices

            var vertices = mesh.vertices;

            //Every vertex can have multiple triangles (face triplets) connected
            var vertexToIndexTripletMap = new Dictionary<Vector3, List<FaceIndexTriplet>>();

            //This should generate a mapping of every vertex to its connecting FaceIndexTriplets
            foreach (var indexTriplet in faceIndexTriplets)
            {
                //Iterate through index triplets, if triplet connects to vertex, add it to given vertex
                //Else if's are fine, because only 1 vertex of a triangle should theoretically be able to match
                for (int i = 0; i < 3; i++)
                {
                    var triangleVertex = vertices[indexTriplet[i]];

                    if (!vertexToIndexTripletMap.ContainsKey(triangleVertex))
                    {
                        vertexToIndexTripletMap.Add(triangleVertex, new List<FaceIndexTriplet>());
                    }

                    vertexToIndexTripletMap[triangleVertex].Add(indexTriplet);
                }
            }
            
            Debug.Log("Mapped all " + vertexToIndexTripletMap.Count + " vertices to their triangles");


            foreach (var vertexToTriplets in vertexToIndexTripletMap)
            {
                _vertexGaussArea.Add(vertexToTriplets.Key,
                    ProcessVertex(vertexToTriplets.Key, vertexToTriplets.Value, mesh));
            }
        }

        private float ProcessVertex(Vector3 vertex, List<FaceIndexTriplet> connectedTriangles, Mesh mesh)
        {
            if (connectedTriangles.Count < 1) return 0.0f;

            var triangleAngles = new List<float>();
            var triangleNormals = new List<Vector3>();
            var totalAngle = 0.0f;

            foreach (var connectedTriangle in connectedTriangles)
            {
                //find which index within the triangle corresponds to the vertex
                for (int i = 0; i < 3; i++)
                {
                    if (mesh.vertices[connectedTriangle[i]] == vertex)
                    {
                        // mod 3 not needed for the first index, since we know it exists within the range
                        var tri = new Triangle(
                            mesh.vertices[connectedTriangle[i]],
                            mesh.vertices[connectedTriangle[(i + 1) % 3]],
                            mesh.vertices[connectedTriangle[(i + 2) % 3]]);

                        //get the angle between b - a and c - a
                        var angle = Vector3.Angle(tri.B - tri.A, tri.C - tri.A) * Mathf.Deg2Rad;

                        totalAngle += angle;
                        triangleAngles.Add(angle);
                        triangleNormals.Add(tri.GetNormal());
                        break;
                    }
                }
            }

            // We now have theta_t (total angle at vertex) and theta_i (angle of a given triangle) 
            // Calculating an individual angle's normal contribution and scaling the corresponding normal by that value
            var pivotNormal = Vector3.zero;
            for (int i = 0; i < triangleNormals.Count; i++)
            {
                var normalContribution = triangleAngles[i] / totalAngle;
                pivotNormal += triangleNormals[i] * normalContribution;
            }

            //Making sure the magnitude is 1
            pivotNormal = pivotNormal.normalized;
            //Adding it to the list of debug info
            _pivotNormals.Add(vertex, pivotNormal);

            //We have our pivot normal now, so it's time to project the other vertices around this normal
            //as they only occupy the half of the unit sphere where dot(n, p) > 0
            //This allows us to project them down onto the plane and order them

            var projectedNormals = new List<Vector3>(triangleNormals.Count);
            foreach (var triangleNormal in triangleNormals)
            {
                projectedNormals.Add(Vector3.ProjectOnPlane(triangleNormal, pivotNormal));
            }

            //Now we have a plane with the normal vectors projected onto it, now we can use their angular difference
            //from an arbitrary vector within the set, to order them

            var angularOrigin = projectedNormals[0];

            var angleNormalPairs = new SortedDictionary<float, Vector3>();
            for (var i = 0; i < projectedNormals.Count; i++)
            {
                var projectedNormal = projectedNormals[i];
                var angle = Vector3.SignedAngle(angularOrigin, projectedNormal, pivotNormal);
                if (!angleNormalPairs.ContainsKey(angle))
                {
                    angleNormalPairs.Add(angle, triangleNormals[i]);
                }
            }

            var sortedNormals = new List<Vector3>(projectedNormals.Count);
            foreach (var angleNormalPair in angleNormalPairs)
            {
                sortedNormals.Add(angleNormalPair.Value);
            }

            var area = GeometryUtils.AreaPolygonOnUnitSphere(sortedNormals);

            // if (area > 2.0 * Mathf.PI)
            // {
            //     string sortedNormalsInfo = "Sorted Normals of Vertex " + vertex.ToString("F4") + ": ";
            //     foreach (var normal in sortedNormals)
            //     {
            //         sortedNormalsInfo += normal.ToString("F4") + ", ";
            //     }
            //     
            //     Debug.Log(sortedNormalsInfo);
            //
            //     Debug.DrawRay(vertex, pivotNormal, Color.magenta, 300f);
            //
            //     for (var i = 0; i < sortedNormals.Count; i++)
            //     {
            //         var sortedNormal = sortedNormals[i];
            //
            //         var color = Color.black;
            //         switch (i)
            //         {
            //             case 0:
            //                 color = Color.green;
            //                 break;
            //             case 1:
            //                 color = Color.yellow;
            //                 break;
            //             case 2:
            //                 color = Color.blue;
            //                 break;
            //         }
            //
            //         Debug.DrawRay(vertex, sortedNormal, color, 300f);
            //     }
            // }

            return area;
        }

        public void DrawPivotNormals()
        {
            // foreach (var normalPair in _pivotNormals)
            // {
            //     Debug.DrawLine(normalPair.Key, normalPair.Key + normalPair.Value);
            // }
        }

        public void DrawSurfaceInfo()
        {
            foreach (var vertexGaussArea in _vertexGaussArea)
            {
                //Max area that a gauss area in this context could have is 2 pi, as that is half the area of the unit circle
                var scale = 1.0f;// / (Mathf.PI * 2);

                Debug.DrawLine(vertexGaussArea.Key,
                    vertexGaussArea.Key + _pivotNormals[vertexGaussArea.Key] * vertexGaussArea.Value * scale,
                    Color.red);
            }
        }

        public override string ToString()
        {
            string gaussInfo = "";
            foreach (var gausArea in _vertexGaussArea)
            {
                gaussInfo += gausArea.Value + ", ";
            }

            return gaussInfo;
        }
    }
}