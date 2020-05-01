using System.Collections.Generic;
using UnityEngine;

namespace Osteogenesis
{
    public class GaussArea
    {
        //A - B and the 2 triangles the Vertex is part of
        private HashSet<(int, int)> _edges;

        //Links a given index to its occurences in other triangles
        //Basically: for a given Vector, it shows all occurences
        private Dictionary<Vector3, List<int>> _vertices;


        public GaussArea(Mesh mesh)
        {
            _vertices = new Dictionary<Vector3, List<int>>();

            var triangles = mesh.GetTriangles(0);
            foreach (var index in triangles)
            {
                Vector3 vtx = mesh.vertices[index];
                if (!_vertices.ContainsKey(vtx))
                {
                    _vertices.Add(vtx, new List<int>());
                }

                _vertices[vtx].Add(index);
            }
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

            angleVectorPairs.Sort();

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
    }
}