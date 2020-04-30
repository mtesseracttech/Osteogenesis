using System.Collections.Generic;
using UnityEngine;

namespace Osteogenesis
{
    public class VertexSoup
    {
        /*
         * Ordered, but bare vertices, without indices
         */
        private List<Vector3> _vertices;

        public VertexSoup(List<Vector3> vertices)
        {
            _vertices = vertices;
        }

        public Vector3 this[int i] => _vertices[i];
    }
}