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
        
        private Vector3 exampleVertex = Vector3.zero;

        public GaussArea(Mesh mesh)
        {
            var faceIndexTriplets = new List<FaceIndexTriplet>(mesh.GetIndices(0).Length / 3);

            //Creating a list of index triplets
            var indices = mesh.GetIndices(0);
            for (int i = 0; i < indices.Length; i += 3)
            {
                faceIndexTriplets.Add(new FaceIndexTriplet(indices[i], indices[i + 1], indices[i + 2]));
            }

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

            foreach (var vertexToTriplets in vertexToIndexTripletMap)
            {
                _vertexGaussArea.Add(vertexToTriplets.Key,
                    ProcessVertex(vertexToTriplets.Key, vertexToTriplets.Value, mesh));
            }
        }

        private float ProcessVertex(Vector3 vertex, List<FaceIndexTriplet> connectedTriangles, Mesh mesh)
        {
            if (connectedTriangles.Count < 2) return 0.0f;

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
                        var angle = Vector3.Angle(tri.B - tri.A, tri.C - tri.A);

                        totalAngle += angle;
                        triangleAngles.Add(angle);
                        triangleNormals.Add(tri.GetNormal());
                        break;
                    }
                }
            }

            // We now have theta_t (total angle at vertex) and theta_i (angle of a given triangle) 
            // Calculating an individual angle's normal contribution and scaling the corresponding normal by that value
            for (int i = 0; i < triangleNormals.Count; i++)
            {
                var normalContribution = triangleAngles[i] / totalAngle;
                triangleNormals[i] = triangleNormals[i] * normalContribution;
            }

            var pivotNormal = Vector3.zero;
            foreach (var normal in triangleNormals)
            {
                pivotNormal += normal;
            }

            pivotNormal = pivotNormal.normalized;

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
                // var projectedNormal = projectedNormals[i];
                // angleNormalPairs.Add(Vector3.SignedAngle(angularOrigin, projectedNormal, pivotNormal),
                //     triangleNormals[i]);
            }

            var sortedNormals = new List<Vector3>(projectedNormals.Count);
            foreach (var angleNormalPair in angleNormalPairs)
            {
                sortedNormals.Add(angleNormalPair.Value);
            }
            
            if (exampleVertex == Vector3.zero)
            {
                var duration = 20f;
                exampleVertex = vertex;
                Debug.DrawRay(vertex, pivotNormal, Color.green, duration);
                
                foreach (var normal in triangleNormals)
                {
                    Debug.DrawRay(vertex, normal, Color.blue,  duration);
                }
                
                foreach (var projected in projectedNormals)
                {
                    Debug.DrawRay(vertex, projected, Color.cyan,  duration);
                }
            }

            return GeometryUtils.AreaPolygonOnUnitSphere(sortedNormals);
        }

        public void DrawPivotNormals()
        {
            foreach (var normalPair in _pivotNormals)
            {
                Debug.DrawLine(normalPair.Key, normalPair.Key + normalPair.Value);
            }
        }
    }
}