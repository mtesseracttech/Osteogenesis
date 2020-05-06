using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Osteogenesis
{
    public class GaussArea
    {
        private Mesh _mesh;

        private List<int> _positionalIndices;
        
        private Dictionary<int, Vector3> _pivotNormals = new Dictionary<int, Vector3>();
        
        private Dictionary<int, UnitSpherePolygon> _vertexNormalPolygons = new Dictionary<int, UnitSpherePolygon>();
        
        private Dictionary<int, float> _vertexGaussArea = new Dictionary<int, float>();

        private Dictionary<FaceIndexTriplet, float> _faceGaussArea = new Dictionary<FaceIndexTriplet, float>();
        
        private Dictionary<EdgeIndices, float> _edgeGaussArea = new Dictionary<EdgeIndices, float>();

        
        public GaussArea(Mesh mesh)
        {
            _mesh = mesh;

            _positionalIndices = CreatePositionalIndices(mesh);

            var faceIndexTriplets = CreateFaceIndexTripletList(_positionalIndices);
            
            ProcessVertices(faceIndexTriplets, mesh);

            ProcessFaces(faceIndexTriplets, mesh);

            ProcessEdges(faceIndexTriplets, mesh);
        }

        /**
         * Creates a new, unified index list that effectively unifies all vertices sharing the same position
         */
        private List<int> CreatePositionalIndices(Mesh mesh)
        {
            Dictionary<Vector3, int> indexMap = new Dictionary<Vector3, int>();
            List<int> newIndices = new List<int>();
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                foreach (var index in mesh.GetIndices(i))
                {
                    var vertex = mesh.vertices[index];
                    if (indexMap.ContainsKey(vertex))
                    {
                        newIndices.Add(indexMap[vertex]);
                    }
                    else
                    {
                        indexMap.Add(vertex, index);
                        newIndices.Add(index);
                    }
                }
            }
            
            Debug.Log("Created a new list of " + newIndices.Count + " position based indices");

            return newIndices;
        }

        private List<FaceIndexTriplet> CreateFaceIndexTripletList(List<int> indices)
        {
            Debug.Log(string.Join(", ", indices));
            
            var faceIndexTriplets = new List<FaceIndexTriplet>(indices.Count/3);
            for (var i = 0; i < indices.Count; i += 3)
            {
                faceIndexTriplets.Add(new FaceIndexTriplet(
                    indices[i], 
                    indices[i + 1], 
                    indices[i + 2])
                );
            }
            
            Debug.Log("Creating " + faceIndexTriplets.Count + " position based face index triplets done");

            return faceIndexTriplets;
        }
        
        
        private void ProcessEdges(List<FaceIndexTriplet> faceIndexTriplets, Mesh mesh)
        {
            //Make a list of all edge index pairs, there are 3 per triangle, and remove duplicates
            foreach (var triplet in faceIndexTriplets)
            {
                
            }
        }
        
        private void ProcessVertices(List<FaceIndexTriplet> faceIndexTriplets, Mesh mesh)
        {
            var vertexToIndexTripletMap = MapVerticesToIndexTriplets(faceIndexTriplets, mesh);
            
            Debug.Log("Vertex To Index Triplet Map Count:" + vertexToIndexTripletMap.Count);
            
            foreach (var vertexToTriplets in vertexToIndexTripletMap)
            {
                var gaussArea = ProcessVertex(vertexToTriplets.Key, vertexToTriplets.Value, mesh);
                //Debug.Log("GaussArea for " + vertexToTriplets.Key + ": " + gaussArea);
                _vertexGaussArea.Add(vertexToTriplets.Key, gaussArea);
            }
        }


        /**
         * Creating vertex -> connecting triangles dictionary
         * Deriving gauss area from vertices
         */
        private Dictionary<int, List<FaceIndexTriplet>> MapVerticesToIndexTriplets(List<FaceIndexTriplet> faceIndexTriplets, Mesh mesh)
        {
            Debug.Log("Index Triplets: " + faceIndexTriplets.Count);
            
            //Every vertex can have multiple triangles (face triplets) connected
            var vertexToIndexTripletMap = new Dictionary<int, List<FaceIndexTriplet>>();

            foreach (var triplet in faceIndexTriplets)
            {
                for (int i = 0; i < 3; i++)
                {
                    var index = triplet[i];
                    if (!vertexToIndexTripletMap.ContainsKey(index))
                    {
                        vertexToIndexTripletMap.Add(index, new List<FaceIndexTriplet>());
                    }
                    vertexToIndexTripletMap[index].Add(triplet);
                }
            }

            Debug.Log("Mapped all " + vertexToIndexTripletMap.Count + " vertices to their triplets");
            
            return vertexToIndexTripletMap;
        }


        private void ProcessFaces(
            List<FaceIndexTriplet> faceIndices,
            Mesh mesh)
        {
            foreach (var indexTriplet in faceIndices)
            {
                Debug.Log(indexTriplet);
                
                Vector3 n0 = _pivotNormals[indexTriplet.V1];
                Vector3 n1 = _pivotNormals[indexTriplet.V2];
                Vector3 n2 = _pivotNormals[indexTriplet.V3];
                _faceGaussArea.Add(indexTriplet, ProcessFace(n0, n1, n2));
            }
        }

        private float ProcessFace(Vector3 n0, Vector3 n1, Vector3 n2)
        {
            var sortedVertices = circularlySortNormals(new List<Vector3>{n0, n1, n2});
            UnitSpherePolygon poly = new UnitSpherePolygon(sortedVertices);
            return poly.GetArea();
        }


        /**
         * Circularly sorts the input vertices by calculating a pivot
         * projecting the other vertices onto the plane that this pivot defines
         * And then taking an arbitrary vertex as the start, calculates the signed angle around the pivot
         * THIS ALGORITHM DOES PRESUME THEY ARE ALL IN THE SAME HEMISPHERE
         * AND DOES NOT ACCOUNT FOR ANGULAR CONTRIBUTION
         */
        private List<Vector3> circularlySortNormals(List<Vector3> normals)
        {
            var pivotNormal = Vector3.zero;
            for (int i = 0; i < normals.Count; i++)
            {
                pivotNormal += normals[i];
            }

            //Making sure the magnitude is 1
            pivotNormal = pivotNormal.normalized;
            
            //We have our pivot normal now, so it's time to project the other vertices around this normal
            //as they only occupy the half of the unit sphere where dot(n, p) > 0
            //This allows us to project them down onto the plane and order them

            var projectedNormals = new List<Vector3>(normals.Count);
            foreach (var triangleNormal in normals)
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
                    angleNormalPairs.Add(angle, normals[i]);
                }
            }

            var sortedNormals = new List<Vector3>(projectedNormals.Count);
            foreach (var angleNormalPair in angleNormalPairs)
            {
                sortedNormals.Add(angleNormalPair.Value);
            }

            return sortedNormals;
        }

        private float ProcessVertex(int vertexIndex, List<FaceIndexTriplet> connectedTriangles, Mesh mesh)
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
                    if (connectedTriangle[i] == vertexIndex)
                    {
                        // mod 3 not needed for the first index, since we know it exists within the range
                        // completing the triangle

                        var a = mesh.vertices[connectedTriangle[i]];
                        var b = mesh.vertices[connectedTriangle[(i + 1) % 3]];
                        var c = mesh.vertices[connectedTriangle[(i + 2) % 3]];

                        var edge1 = b - a;
                        var edge2 = c - a;
                        
                        var angle = Vector3.Angle(edge1, edge2);
                        var normal = Vector3.Cross(edge1, edge2).normalized;
                        
                        totalAngle += angle;
                        triangleAngles.Add(angle);
                        triangleNormals.Add(normal);
                        //Exiting out of the loop as the same vertex cannot really appear twice in the same triangle
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
            _pivotNormals.Add(vertexIndex, pivotNormal);

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
            
            Debug.Log("Sorted normal count: " + angleNormalPairs.Count);

            
            var sortedNormals = new List<Vector3>(projectedNormals.Count);
            foreach (var angleNormalPair in angleNormalPairs)
            {
                sortedNormals.Add(angleNormalPair.Value);
            }

            
            var polygon = new UnitSpherePolygon(sortedNormals);
            _vertexNormalPolygons.Add(vertexIndex, polygon);
            
            var area = polygon.GetArea();
            
            return area;
        }

        public Dictionary<int, Vector3> GetPivotNormals()
        {
            return _pivotNormals;
        }

        public Dictionary<int, UnitSpherePolygon> GetVertexNormalPolygons()
        {
            return _vertexNormalPolygons;
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

        public void DrawVertexDebugInfo(Transform transform)
        {
            Debug.Log("Drawing debug info for gauss area of vertices");

            foreach (var vertexNormalPolygon in GetVertexNormalPolygons())
            {
                var worldPos = transform.TransformPoint(_mesh.vertices[vertexNormalPolygon.Key]);
                var poly = vertexNormalPolygon.Value;

                //Indication that something went wrong

                var area = poly.GetArea();
                if (true/*Mathf.Abs(area) > 0.5f*/)
                {
                    //Draw poly from position in model
                    float scalingFactor = Mathf.Abs(area);
                    var vertices = poly.GetVertices();
                    for (var i = 0; i < vertices.Length; i++)
                    {
                        var rotation = transform.rotation;
                        var v0 = rotation * vertices[i] * scalingFactor;
                        var v1 = rotation * vertices[(i + 1) % vertices.Length] * scalingFactor;
                        Triangle tri = new Triangle(worldPos + v0, worldPos, worldPos + v1);
                        
                        if (area > 0)
                        {
                            tri.DebugDraw(Color.white, 300f);
                        }
                        if (area < 0)
                        {
                            Color color = Color.white;
                            switch (i)
                            {
                                case 0:
                                    color = Color.green;
                                    break;
                                case 1:
                                    color = Color.yellow;
                                    break;
                            }
                            tri.DebugDraw(color, 300f);
                        }
                    }
                }
            }
        }

        public void DrawPivotNormals(Transform transform)
        {
            var normalizationFactor = 1.0f;
            foreach (var pivotNormal in GetPivotNormals())
            {
                var worldPos = transform.TransformPoint(_mesh.vertices[pivotNormal.Key]);
                var directedNormal = transform.rotation * pivotNormal.Value * normalizationFactor;
                var relativeNormalSize = 0.01f;

                Debug.DrawLine(worldPos, worldPos + directedNormal);
            }
        }
        
        public void DrawSurfaceInfo()
        {
            foreach (var vertexGaussArea in _vertexGaussArea)
            {
                //Max area that a gauss area in this context could have is 2 pi, as that is half the area of the unit circle
                var scale = 1.0f;// / (Mathf.PI * 2);

                Debug.DrawLine(_mesh.vertices[vertexGaussArea.Key],
                    _mesh.vertices[vertexGaussArea.Key] + _pivotNormals[vertexGaussArea.Key] * vertexGaussArea.Value * scale,
                    Color.red, 300f);
            }
        }
    }
}