using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Osteogenesis
{
    public class GaussArea
    {
        //A - B and the 2 triangles the Vertex is part of
        private Dictionary<(Vector3, Vector3), List<(int, int)>> _edges = new Dictionary<(Vector3, Vector3), List<(int, int)>>();

        private List<FaceIndexTriplet> _faceIndexTriplets = new List<FaceIndexTriplet>();

        //Links a given index to its occurences in other triangles
        //Basically: for a given Vector, it shows all occurences
        private Dictionary<Vector3, List<int>> _vertices = new Dictionary<Vector3, List<int>>();
        private Dictionary<Vector3, float> _vertexGaussArea = new Dictionary<Vector3, float>();

        //Vertex + Pivot normal pairs
        private Dictionary<Vector3, Vector3> _pivotNormals =  new Dictionary<Vector3, Vector3>();
        
        

        public GaussArea(Mesh mesh)
        {
            var indices = mesh.GetIndices(0);
            //Creating an easier to work with structure of index triplets
            for (int i = 0; i < indices.Length; i+=3)
            {
                var triplet = new FaceIndexTriplet(indices[i], indices[i+1], indices[i+2]);
                _faceIndexTriplets.Add(triplet);
            }

            foreach (var triplet in _faceIndexTriplets)
            {
            }



            // var indices = mesh.GetIndices(0);
            // foreach (var index in indices)
            // {
            //     Vector3 vtx = mesh.vertices[index];
            //     if (!_vertices.ContainsKey(vtx))
            //     {
            //         _vertices.Add(vtx, new List<int>());
            //     }
            //
            //     _vertices[vtx].Add(index);
            // }
            //
            //
            // _vertexGaussArea = new Dictionary<Vector3, float>();
            // foreach (var vertexIndexPair in _vertices)
            // {
            //     _vertexGaussArea.Add(vertexIndexPair.Key, GaussAreaVertex(vertexIndexPair.Key, mesh));
            // }
        }


        private float GaussAreaVertex(Vector3 vertex, Mesh mesh)
        {
            if (!_vertices.ContainsKey(vertex)) return 0.0f;
            //Indices of the triangles that are connected
            var relatedTriangles = _vertices[vertex];

            if (relatedTriangles.Count < 3) return 0.0f;

            //Normal around which the others will be checked
            var pivotNormal = Vector3.zero;
            var relatedNormals = new List<Vector3>(relatedTriangles.Count);

            foreach (var index in relatedTriangles)
            {
                var normal = mesh.normals[index];
                pivotNormal += normal;
                relatedNormals.Add(normal);
            }

            //Now we have a proper pivot to project the rest around
            pivotNormal = pivotNormal.normalized;
            
            _pivotNormals.Add(vertex, pivotNormal);

            var projectedNormals = new List<Vector3>(relatedNormals.Count);
            foreach (var normal in relatedNormals)
            {
                projectedNormals.Add(Vector3.Project(normal, pivotNormal));
            }

            //First vector, should be used as a starting point of the rotation, is effectively arbitrary
            var rotationStart = projectedNormals[0];
            //Now we have a map with rotations, time to sort the original related normals with it

            //Zipping together pairs of angles and corresponding normals, for easy sorting
            var angleVectorPairs = new List<(float, Vector3)>(relatedNormals.Count);
            for (int i = 0; i < relatedNormals.Count; i++)
            {
                angleVectorPairs.Add(
                    (Vector3.SignedAngle(
                        rotationStart,
                        projectedNormals[i],
                        pivotNormal
                    ), relatedNormals[i])
                );
            }

            angleVectorPairs.Sort((t1, t2) =>
            {
                int comp = t1.Item1.CompareTo(t2.Item1);
                if (comp != 0) return comp;
                for (int i = 0; i < 3; i++)
                {
                    comp = Comparer<float>.Default.Compare(t1.Item2[i], t2.Item2[i]);
                    if (comp != 0)
                    {
                        return comp;
                    }
                }
                return comp;
            });

            //Unzipping and moving the sorted normals into a list to be turned into gauss area
            var sortedNormals = new List<Vector3>(angleVectorPairs.Count);
            foreach (var angleVectorPair in angleVectorPairs)
            {
                sortedNormals.Add(angleVectorPair.Item2);
            }

            return GeometryUtils.AreaPolygonOnUnitSphere(sortedNormals);
        }

        private void ProcessEdge((Vector3, Vector3) edge, Mesh mesh)
        {
            
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