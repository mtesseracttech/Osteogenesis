using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Osteogenesis
{
    public class GaussArea
    {
        //private Mesh _mesh;

        private Vector3[] _vertices;

        //Cleaned up indices, filtered for duplicate vertices
        private List<int> _positionalIndices;
        
        //Standard pivot normal for every vertex, used in all 3 gauss area calculations
        private Dictionary<int, Vector3> _pivotNormals = new Dictionary<int, Vector3>();
        
        //private Dictionary<int, UnitSpherePolygon> _vertexNormalPolygons = new Dictionary<int, UnitSpherePolygon>();
        
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
            _vertices = mesh.vertices;

            var positionalIndices = CreatePositionalIndices(mesh, _vertices);

            var faceIndexTriplets = CreateFaceIndexTripletList(positionalIndices);
 
            ProcessVertices(faceIndexTriplets, _vertices);

            ProcessEdges(faceIndexTriplets, _vertices);
            
            ProcessFaces(faceIndexTriplets, _vertices);

        }
        
        /**
         * Creates a new, unified index list that effectively unifies all vertices sharing the same position
         */
        private int[] CreatePositionalIndices(Mesh mesh, Vector3[] vertices)
        {
            long totalIndices = 0;
            var indexMap = new Dictionary<Vector3, int>();
            var newIndices = new List<int>();
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                totalIndices += mesh.GetIndexCount(i);
                var submeshIndices = mesh.GetIndices(i);
        
                foreach (var index in submeshIndices)
                {
                    var vertex = vertices[index];
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
                      " position based indices, removing " + (totalIndices - indexMap.Count));
        
            return newIndices.ToArray();
        }
        
        private FaceIndexTriplet[] CreateFaceIndexTripletList(int[] indices)
        {
            Debug.Log("Original Indices Count: " + indices.Length);
            
            var faceIndexTriplets = new List<FaceIndexTriplet>(indices.Length/3);
            for (var i = 0; i < indices.Length; i += 3)
            {
                faceIndexTriplets.Add(new FaceIndexTriplet(
                    indices[i], 
                    indices[i + 1], 
                    indices[i + 2])
                );
            }
            
            Debug.Log("Creating " + faceIndexTriplets.Count + " position based face index triplets done");

            return faceIndexTriplets.ToArray();
        }
        
        
        private void ProcessEdges(FaceIndexTriplet[] faceIndexTriplets, Vector3[] vertices)
        {
            var vertexTriangleMap = new Dictionary<int, HashSet<FaceIndexTriplet>>();
            foreach (var triplet in faceIndexTriplets)
            {
                for (int i = 0; i < 3; i++)
                {
                    var vertex = triplet[i];
                    if (!vertexTriangleMap.ContainsKey(vertex))
                    {
                        vertexTriangleMap.Add(vertex, new HashSet<FaceIndexTriplet>());
                    }
                    vertexTriangleMap[vertex].Add(triplet);
                }
            }
            
            
            //Contains all edges, linked to the triangles that connect to them.
            var edgeTriangleMap = new Dictionary<EdgeIndices, HashSet<FaceIndexTriplet>>();
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
                        edgeTriangleMap.Add(edge, new HashSet<FaceIndexTriplet>());
                    }
                    edgeTriangleMap[edge].UnionWith(vertexTriangleMap[i1]);
                    edgeTriangleMap[edge].UnionWith(vertexTriangleMap[i2]);
                }
            }
            
            foreach (var edge in edgeTriangleMap)
            {
                _edgeGaussArea.Add(edge.Key, ProcessEdge(edge.Key, edge.Value.ToArray(), vertices));
            }
            //If an edge only has 1 triangle connected to it, it's a mesh edge, and should not have any gaussian area.
        }

        private float ProcessEdge(EdgeIndices edge, FaceIndexTriplet[] connectedTriangles, Vector3[] vertices)
        {
            var v1 = vertices[edge.I1];
            var v2 = vertices[edge.I2];
            
            var dv = (v2 - v1).normalized;
            
            var n1 = _pivotNormals[edge.I1];
            var n2 = _pivotNormals[edge.I2];

            var ne = (n1 + n2).normalized;
            
            var side1 = Vector3.Cross(ne, dv).normalized;
            var side2 = -side1;
                
            //For v1 on side 1:
            //get the triangle edges on side 1 (dot(n1, v1-t1) should be more than 0)
            //where v1 is the vertex and t1 the side of the triangle pointing away from the vertex
            //then check if said triangles are actually connected to v1 (might be faster to do it the other way around, idk)

            //Side 1
            var nS1V1 = CalculateSidedPivot(edge.I1, connectedTriangles, side1, ne, dv, vertices);
            var nS1V2 = CalculateSidedPivot(edge.I2, connectedTriangles, side1, ne, dv, vertices);

            //Side 2
            var nS2V1  = CalculateSidedPivot(edge.I1, connectedTriangles, side2, ne, dv, vertices);
            var nS2V2  = CalculateSidedPivot(edge.I2, connectedTriangles, side2, ne, dv, vertices);

            var polygon = new UnitSpherePolygon(CircularlySortNormals(new List<Vector3>
            {
                nS1V1,
                nS1V2,
                nS2V1,
                nS2V2
            }));

            return polygon.GetArea();
        }
        

        private Vector3 CalculateSidedPivot(int vertex, FaceIndexTriplet[] connectedTriangles, Vector3 sideNormal, Vector3 edgeNormal, Vector3 dv, Vector3[] vertices)
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
                        //var v = vertices[vertex];
                        //Debug.DrawLine(v, v + sideNormal * 0.1f, Color.yellow, 300f);

                        // mod 3 not needed for the first index, since we know it exists within the range
                        // completing the triangle

                        var a = vertices[connectedTriangle[i]];
                        var b = vertices[connectedTriangle[(i + 1) % 3]];
                        var c = vertices[connectedTriangle[(i + 2) % 3]];

                        //new Triangle(a, b,c).DebugDraw(Color.white, 300f);
                        
                        var triEdge1 = b - a;
                        var triEdge2 = c - a;

                        //Entire triangle on wrong side, so not counting it
                        if (Vector3.Dot(triEdge1, sideNormal) < 0.0f && Vector3.Dot(triEdge2, sideNormal) < 0.0f)
                        {
                            break; //Break because no vertex in this tri can connect
                        }

                        if (Vector3.Dot(triEdge1, sideNormal) < 0.0f)
                        {
                            //First edge of the triangle is on the wrong side of the plane
                            //Centering it all on zero
                            var normalPlane = new Plane(sideNormal, Vector3.zero);
                            var triPlane = new Plane(Vector3.zero, triEdge1, triEdge2);
                            var intersection = PlanePlaneIntersection.PlanePlaneIntersect(normalPlane, triPlane);
                            if (intersection.Intersect && intersection.IsLine)
                            {
                                triEdge1 = intersection.Line.Direction;
                            }

                        } else if (Vector3.Dot(triEdge2, sideNormal) < 0.0f) 
                        {
                            //Second edge of the triangle is on the wrong side of the plane
                            //Centering it all on zero
                            var normalPlane = new Plane(sideNormal, Vector3.zero);
                            var triPlane = new Plane(Vector3.zero, triEdge2, triEdge1);
                            var intersection = PlanePlaneIntersection.PlanePlaneIntersect(normalPlane, triPlane);
                            if (intersection.Intersect && intersection.IsLine)
                            {
                                triEdge2 = intersection.Line.Direction;
                            }
                        }

                        var triNormal = Vector3.Cross(triEdge1, triEdge2);
                        
                        var angle = Vector3.Angle(triEdge1, triEdge2);

                        pivot += triNormal * angle;
                    }
                }
            }

            return pivot.normalized;
        }


        /**
         * Creating vertex -> connecting triangles dictionary
         * Deriving gauss area from vertices
         */
        private Dictionary<int, List<FaceIndexTriplet>> MapVerticesToIndexTriplets(FaceIndexTriplet[] faceIndexTriplets, Vector3[] vertices)
        {
            Debug.Log("Index Triplets: " + faceIndexTriplets.Length);
            
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
        
        
        private void ProcessVertices(FaceIndexTriplet[] faceIndexTriplets, Vector3[] vertices)
        {
            var vertexToIndexTripletMap = MapVerticesToIndexTriplets(faceIndexTriplets, vertices);
            
            Debug.Log("Vertex To Index Triplet Map Count:" + vertexToIndexTripletMap.Count);

            // Parallel.ForEach(vertexToIndexTripletMap, (vertexToTriplets) =>
            // {
            //     _vertexGaussArea.Add(vertexToTriplets.Key,
            //         ProcessVertex(vertexToTriplets.Key, vertexToTriplets.Value.ToArray(), vertices));
            // });

            foreach (var vertexToTriplets in vertexToIndexTripletMap)
            {
                _vertexGaussArea.Add(vertexToTriplets.Key, ProcessVertex(vertexToTriplets.Key, vertexToTriplets.Value.ToArray(), vertices));
            }
        }

        private float ProcessVertex(int vertexIndex, FaceIndexTriplet[] connectedTriangles, Vector3[] vertices)
        {
            if (connectedTriangles.Length < 1) return 0.0f;

            var normalInfo = CreatePivotNormalForVertex(vertexIndex, connectedTriangles, vertices, true);

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
            //_vertexNormalPolygons.Add(vertexIndex, polygon);
            
            var area = polygon.GetArea();
            
            return area;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessFaces(FaceIndexTriplet[] faceIndices, Vector3[] vertices)
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float ProcessFace(Vector3 n0, Vector3 n1, Vector3 n2)
        {
            var sortedVertices = CircularlySortNormals(new List<Vector3>{n0, n1, n2});
            UnitSpherePolygon poly = new UnitSpherePolygon(sortedVertices);
            return poly.GetArea();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PivotNormalTriangleNormalSet CreatePivotNormalForVertex(int vertex, FaceIndexTriplet[] connectedTriangles, Vector3[] vertices, bool returnTriangleNormals)
        {
            var triangleNormals = new List<Vector3>(connectedTriangles.Length);
            foreach (var connectedTriangle in connectedTriangles)
            {
                //find which index within the connected triangle corresponds to the vertex
                for (int i = 0; i < 3; i++)
                {
                    if (connectedTriangle[i] == vertex)
                    {
                        // mod 3 not needed for the first index, since we know it exists within the range
                        // completing the triangle

                        var a = vertices[connectedTriangle[i]];
                        var b = vertices[connectedTriangle[(i + 1) % 3]];
                        var c = vertices[connectedTriangle[(i + 2) % 3]];

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        // public Dictionary<int, UnitSpherePolygon> GetVertexNormalPolygons()
        // {
        //     return _vertexNormalPolygons;
        // }

        public override string ToString()
        {
            return string.Join(", ", _vertexGaussArea);
        }

        public void DrawVertexDebugInfo(Transform transform)
        {
            // Debug.Log("Drawing debug info for gauss area of vertices");
            //
            // foreach (var vertexNormalPolygon in GetVertexNormalPolygons())
            // {
            //     var worldPos = transform.TransformPoint(_vertices[vertexNormalPolygon.Key]);
            //     var poly = vertexNormalPolygon.Value;
            //
            //     //Indication that something went wrong
            //
            //     var area = poly.GetArea();
            //     if (true/*Mathf.Abs(area) > 0.5f*/)
            //     {
            //         //Draw poly from position in model
            //         float scalingFactor = Mathf.Abs(area);
            //         var vertices = poly.GetVertices();
            //         for (var i = 0; i < vertices.Length; i++)
            //         {
            //             var rotation = transform.rotation;
            //             var v0 = rotation * vertices[i] * scalingFactor;
            //             var v1 = rotation * vertices[(i + 1) % vertices.Length] * scalingFactor;
            //             Triangle tri = new Triangle(worldPos + v0, worldPos, worldPos + v1);
            //             
            //             if (area > 0)
            //             {
            //                 tri.DebugDraw(Color.white, 300f);
            //             }
            //             if (area < 0)
            //             {
            //                 Color color = Color.white;
            //                 switch (i)
            //                 {
            //                     case 0:
            //                         color = Color.green;
            //                         break;
            //                     case 1:
            //                         color = Color.yellow;
            //                         break;
            //                 }
            //                 tri.DebugDraw(color, 300f);
            //             }
            //         }
            //     }
            // }
        }

        public void DrawPivotNormals(Transform transform)
        {
            var normalizationFactor = 1.0f;
            foreach (var pivotNormal in GetPivotNormals())
            {
                var worldPos = transform.TransformPoint(_vertices[pivotNormal.Key]);
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

                Debug.DrawLine(_vertices[vertexGaussArea.Key],
                    _vertices[vertexGaussArea.Key] + _pivotNormals[vertexGaussArea.Key] * vertexGaussArea.Value * scale,
                    Color.red, 300f);
            }
        }
    }
}