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

        //Cleaned up indices, filtered for duplicate vertices
        private List<int> _positionalIndices;
        
        //Standard pivot normal for every vertex, used in all 3 gauss area calculations
        private Dictionary<int, Vector3> _pivotNormals = new Dictionary<int, Vector3>();
        
        private Dictionary<int, UnitSpherePolygon> _vertexNormalPolygons = new Dictionary<int, UnitSpherePolygon>();
        
        /**
         * Gauss area maps
         */
        //Vertex
        private Dictionary<int, float> _vertexGaussArea = new Dictionary<int, float>();
        //Edge
        private Dictionary<EdgeIndices, float> _edgeGaussArea = new Dictionary<EdgeIndices, float>();
        //Face
        private Dictionary<FaceIndexTriplet, float> _faceGaussArea = new Dictionary<FaceIndexTriplet, float>();
        

        
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
         * TODO: THIS PROCESS MAY NEED TO BE REVERSED AT THE END,
         * TODO: TO GET A MAPPING BACK FROM THE SIMPLIFIED VERSION TO THE ORIGINAL
         */
        private List<int> CreatePositionalIndices(Mesh mesh)
        {
            long totalIndices = 0;
            Dictionary<Vector3, int> indexMap = new Dictionary<Vector3, int>();
            List<int> newIndices = new List<int>();
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                totalIndices += mesh.GetIndexCount(i);
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
            
            Debug.Log("Created a new list of " + newIndices.Count + 
                      " position based indices, removing " + (totalIndices - newIndices.Count));

            return newIndices;
        }

        private List<FaceIndexTriplet> CreateFaceIndexTripletList(List<int> indices)
        {
            Debug.Log("Original Indices Count: " + indices.Count);
            
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
            Debug.Log("triangles: " + faceIndexTriplets.Count);

            
            //Contains all edges, linked to the triangles that connect to them.
            var edgeTriangleMap = new Dictionary<EdgeIndices, List<FaceIndexTriplet>>();
            //Make a list of all edge index pairs, there are 3 per triangle
            foreach (var triplet in faceIndexTriplets)
            {
                for (int i = 0; i < 3; i++)
                {
                    var i1 = triplet[i];
                    var i2 = triplet[(i + 1) % 3];
                    
                    var edge = new EdgeIndices(i1, i2);

                    if (!edgeTriangleMap.ContainsKey(edge))
                    {
                        edgeTriangleMap.Add(edge, new List<FaceIndexTriplet>());
                    }
                    edgeTriangleMap[edge].Add(triplet);
                }
            }

            Debug.Log("edges: " + edgeTriangleMap.Count);

            int testDraws = 0;
            foreach (var edge in edgeTriangleMap)
            {
                if (testDraws++ < 10000)
                {
                    var area = ProcessEdge(edge.Key, edge.Value, mesh);
                }
            }
            
            

            //If an edge only has 1 triangle connected to it, it's a mesh edge, and should not have any gaussian area.
        }

        private float ProcessEdge(EdgeIndices edge, List<FaceIndexTriplet> connectedTriangles, Mesh mesh)
        {
            var v1 = mesh.vertices[edge.I1];
            var v2 = mesh.vertices[edge.I2];

            //Debug.DrawLine(v1, v2, Color.yellow, 300f);
                    
            var dv = (v2 - v1).normalized;
            
            //Debug.DrawLine(v1, v1 + dv, Color.magenta, 300f);

            var n1 = _pivotNormals[edge.I1];
            var n2 = _pivotNormals[edge.I2];

            var ne = (n1 + n2).normalized;

            var middle = (v1 + v2) / 2;

            //Debug.DrawLine(middle, middle + ne, Color.green, 300f);

            var side1 = Vector3.Cross(ne, dv).normalized;
            var side2 = -side1;

            //Debug.DrawLine(middle, middle + side1 * 0.01f, Color.cyan, 300f);
            //Debug.DrawLine(middle, middle + side2 * 0.01f, Color.black, 300f);

            //For v1 on side 1:
            //get the triangle edges on side 1 (dot(n1, v1-t1) should be more than 0)
            //where v1 is the vertex and t1 the side of the triangle pointing away from the vertex
            //then check if said triangles are actually connected to v1 (might be faster to do it the other way around, idk)

            //TODO: Use the fact that contribution(v1) = 1 - contribution(v2)
            
            //Side 1
            var nS1V1 = CalculateSidedPivot(edge.I1, connectedTriangles, side1, dv, mesh);
            var nS1V2 = CalculateSidedPivot(edge.I2, connectedTriangles, side1, dv, mesh);
            
            //Debug.DrawLine(v1, v1 + nS1V1 * 0.1f, Color.yellow, 300f);
            //Debug.DrawLine(v2, v2 + nS1V2 * 0.1f, Color.blue, 300f);
            
            //Side 2
            var nS2V1  = CalculateSidedPivot(edge.I1, connectedTriangles, side2, dv, mesh);
            var nS2V2  = CalculateSidedPivot(edge.I2, connectedTriangles, side2, dv, mesh);
            
            //Debug.DrawLine(v1, v1 + nS2V1 * 0.1f, Color.yellow, 300f);
            //Debug.DrawLine(v2, v2 + nS2V2 * 0.1f, Color.blue, 300f);
            
            var sideNormals = new List<Vector3>
            {
                nS1V1,
                nS1V2,
                nS2V1,
                nS2V2
            };
            
            var polygon = new UnitSpherePolygon(sideNormals);
            
            var area = polygon.GetArea();
            
            return area;
        }
        

        private Vector3 CalculateSidedPivot(int vertex, List<FaceIndexTriplet> connectedTriangles, Vector3 normal, Vector3 dv, Mesh mesh)
        {
            Vector3 pivot = Vector3.zero;
            foreach (var connectedTriangle in connectedTriangles)
            {
                //find which index within the connected triangle corresponds to the vertex
                for (int i = 0; i < 3; i++)
                {
                    //If no vertex of the triangle matches the main one, no angular contribution is counted
                    if (connectedTriangle[i] == vertex)
                    {
                        // mod 3 not needed for the first index, since we know it exists within the range
                        // completing the triangle

                        var a = mesh.vertices[connectedTriangle[i]];
                        var b = mesh.vertices[connectedTriangle[(i + 1) % 3]];
                        var c = mesh.vertices[connectedTriangle[(i + 2) % 3]];

                        var triEdge1 = b - a;
                        var triEdge2 = c - a;
                        
                        //Entire triangle on wrong side not counting it
                        if (Vector3.Dot(triEdge1, normal) < 0.0f && Vector3.Dot(triEdge2, normal) < 0.0f)
                        {
                            break;
                        }

                        //Tri edge 1 is on wrong side, but tri edge 2 is not, so we have to cut
                        if (Vector3.Dot(triEdge1, normal) < 0.0f)
                        {
                            //Tri edge 1 is on the left side of v, 
                            if (Vector3.Dot(dv, triEdge1) <= 0.0)
                            {
                                triEdge1 = Vector3.Dot(dv, triEdge1) * dv;
                            }
                            else //Tri edge 1 is on the right side of v
                            {
                                triEdge1 = Vector3.Dot(-dv, triEdge1) * -dv;
                            }
                        }
                        
                        //Tri edge 2 is on wrong side, but tri edge 1 is not, so we have to cut
                        if (Vector3.Dot(triEdge2, normal) < 0.0f)
                        {
                            //Tri edge 2 is on the left side of v, 
                            if (Vector3.Dot(dv, triEdge1) <= 0.0)
                            {
                                triEdge2 = Vector3.Dot(dv, triEdge2) * dv;
                            }
                            else //Tri edge 1 is on the right side of v
                            {
                                triEdge2 = Vector3.Dot(-dv, triEdge2) * -dv;
                            }
                        }
                        
                        
                        Vector3 triNormal = Vector3.Cross(triEdge1, triEdge2).normalized;

                        //Triangle is fine, so we can continue


                        var angle = Vector3.Angle(triEdge1, triEdge2);

                        pivot += triNormal * angle;
                    }
                }
            }

            return pivot.normalized;
        }

        // private Vector3 ClampVectorToHemisphereAlongDirection(Vector3 vertex, Vector3 normal, Vector3 dv)
        // {
        //     //It's in the correct hemisphere, so no issue
        //     if (Vector3.Dot(vertex, normal) >= 0)
        //     {
        //         return vertex;
        //     }
        //     //But what if it's not
        //     //Decide on which side of the vertex it is, using dv
        //     if (Vector3.Dot(dv, vertex) >= 0.0)
        //     {
        //         //It's on the right side, so clamp along dv
        //         return dv * Vector3.Dot(dv, vertex);
        //     }
        //     //It's on the left side, so clamp along -dv
        //     return -dv * Vector3.Dot(-dv, vertex);
        // }
        

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

        private float ProcessVertex(int vertexIndex, List<FaceIndexTriplet> connectedTriangles, Mesh mesh)
        {
            if (connectedTriangles.Count < 1) return 0.0f;

            var normalInfo = CreatePivotNormalForVertex(vertexIndex, connectedTriangles, mesh, true);

            var pivotNormal = normalInfo.Pivot;
            var triangleNormals = normalInfo.TriangleNormals;

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

        private void ProcessFaces(List<FaceIndexTriplet> faceIndices, Mesh mesh)
        {
            foreach (var indexTriplet in faceIndices)
            {
                Vector3 n0 = _pivotNormals[indexTriplet.V1];
                Vector3 n1 = _pivotNormals[indexTriplet.V2];
                Vector3 n2 = _pivotNormals[indexTriplet.V3];
                _faceGaussArea.Add(indexTriplet, ProcessFace(n0, n1, n2));
            }
        }

        /*
         * Calculates the gaussian surface area for a given face element
         */
        private float ProcessFace(Vector3 n0, Vector3 n1, Vector3 n2)
        {
            var sortedVertices = CircularlySortNormals(new List<Vector3>{n0, n1, n2});
            UnitSpherePolygon poly = new UnitSpherePolygon(sortedVertices);
            return poly.GetArea();
        }


        private PivotNormalTriangleNormalSet CreatePivotNormalForVertex(int vertex, List<FaceIndexTriplet> connectedTriangles, Mesh mesh, bool returnTriangleNormals)
        {
            var triangleNormals = new List<Vector3>(connectedTriangles.Count);
            foreach (var connectedTriangle in connectedTriangles)
            {
                //find which index within the connected triangle corresponds to the vertex
                for (int i = 0; i < 3; i++)
                {
                    if (connectedTriangle[i] == vertex)
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
                        
                        //Adding the new triangle times its angular contribution
                        triangleNormals.Add(normal * angle);
                        
                        //Exiting out of the loop as the same vertex cannot really appear twice in the same triangle
                        break;
                    }
                }
            }

            var pivot = CreatePivotNormal(triangleNormals);
            
            if (returnTriangleNormals)
            {
                for (int i = 0; i < triangleNormals.Count; i++)
                {
                    triangleNormals[i] = triangleNormals[i].normalized;
                }
                return new PivotNormalTriangleNormalSet(pivot, triangleNormals);
            }

            return new PivotNormalTriangleNormalSet(pivot);
        }

        private Vector3 CreatePivotNormal(List<Vector3> normals)
        {
            var pivotNormal = Vector3.zero;
            for (int i = 0; i < normals.Count; i++)
            {
                pivotNormal += normals[i];
            }

            //Making sure it is normalized
            return pivotNormal.normalized;
        }

        /**
         * Circularly sorts the input vertices by calculating a pivot
         * projecting the other vertices onto the plane that this pivot defines
         * And then taking an arbitrary vertex as the start, calculates the signed angle around the pivot
         * THIS ALGORITHM DOES PRESUME THEY ARE ALL IN THE SAME HEMISPHERE
         * AND DOES NOT ACCOUNT FOR ANGULAR CONTRIBUTION
         */
        private List<Vector3> CircularlySortNormals(List<Vector3> normals)
        {
            var pivotNormal = CreatePivotNormal(normals);
            
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
            return string.Join(", ", _vertexGaussArea);
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