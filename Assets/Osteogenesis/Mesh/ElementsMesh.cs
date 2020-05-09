using System.Collections.Generic;
using UnityEngine;

namespace Osteogenesis
{
    // public class ElementsMesh
    // {
    //     private Vector3[] _vertices = null;
    //     private int[] _indices = null;
    //     private EdgeIndices[] _edges = null;
    //     
    //
    //     public ElementsMesh(Mesh mesh)
    //     {
    //         _vertices = mesh.vertices;
    //         _indices = CreatePositionalIndices(mesh, _vertices);
    //
    //         var vertexTriangleMap = VertexToTrianglesMap();
    //
    //
    //     }
    //
    //     private Dictionary<int, HashSet<FaceIndexTriplet>> VertexToTriangles()
    //     {
    //         var vertexTriangleMap = new Dictionary<int, HashSet<FaceIndexTriplet>>();
    //         foreach (var triplet in faceIndexTriplets)
    //         {
    //             for (int i = 0; i < 3; i++)
    //             {
    //                 var vertex = triplet[i];
    //                 if (!vertexTriangleMap.ContainsKey(vertex))
    //                 {
    //                     vertexTriangleMap.Add(vertex, new HashSet<FaceIndexTriplet>());
    //                 }
    //                 vertexTriangleMap[vertex].Add(triplet);
    //             }
    //         }
    //     }
    //     
    //     
    //
    //     private EdgeIndices[] CreateEdges()
    //     {
    //
    //         
    //         
    //         //Contains all edges, linked to the triangles that connect to them.
    //         var edgeTriangleMap = new Dictionary<EdgeIndices, HashSet<FaceIndexTriplet>>();
    //         //Make a list of all edge index pairs, there are 3 per triangle
    //         foreach (var triplet in faceIndexTriplets)
    //         {
    //             for (int i = 0; i < 3; i++)
    //             {
    //                 var i1 = triplet[i];
    //                 var i2 = triplet[(i + 1) % 3];
    //                 
    //                 var edge = new EdgeIndices(i1, i2);
    //
    //                 if (!edgeTriangleMap.ContainsKey(edge))
    //                 {
    //                     edgeTriangleMap.Add(edge, new HashSet<FaceIndexTriplet>());
    //                 }
    //                 edgeTriangleMap[edge].UnionWith(vertexTriangleMap[i1]);
    //                 edgeTriangleMap[edge].UnionWith(vertexTriangleMap[i2]);
    //             }
    //         }
    //     }
    //
    //     /**
    //      * Creates a new, unified index list that effectively unifies all vertices sharing the same position
    //      */
    //     private int[] CreatePositionalIndices(Mesh mesh, Vector3[] vertices)
    //     {
    //         long totalIndices = 0;
    //         var indexMap = new Dictionary<Vector3, int>();
    //         var newIndices = new List<int>();
    //         for (int i = 0; i < mesh.subMeshCount; i++)
    //         {
    //             totalIndices += mesh.GetIndexCount(i);
    //             var submeshIndices = mesh.GetIndices(i);
    //
    //             foreach (var index in submeshIndices)
    //             {
    //                 var vertex = vertices[index];
    //                 if (indexMap.ContainsKey(vertex))
    //                 {
    //                     newIndices.Add(indexMap[vertex]);
    //                 }
    //                 else
    //                 {
    //                     indexMap.Add(vertex, index);
    //                     newIndices.Add(index);
    //                 }
    //             }
    //         }
    //         Debug.Log("Created a new list of " + newIndices.Count + 
    //                   " position based indices, removing " + (totalIndices - indexMap.Count));
    //
    //         return newIndices.ToArray();
    //     }
    // }
}