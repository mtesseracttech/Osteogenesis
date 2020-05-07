using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Osteogenesis
{
    public class TriangulatedMesh
    {
        private VertexSoup _vertices;
        private List<int> _triangulation;

        public TriangulatedMesh(VertexSoup vertices, List<int> triangulation)
        {
            _vertices = vertices;
            _triangulation = triangulation;
        }

        public List<int> GetTriangulation()
        {
            return _triangulation;
        }

        public VertexSoup GetVertices()
        {
            return _vertices;
        }
    }
}