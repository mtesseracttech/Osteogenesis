using System.Collections.Generic;
using UnityEngine;

namespace Osteogenesis
{
    public class AnimatedMeshSequence
    {
        /*
         * Simple mesh, containing the vertices and the triangulation indices
         */
        private TriangulatedMesh _mesh;
        
        /*
         * Contains the positional data for every vertex in the mesh, at all time steps
         * Every index in the list contains a next frame of the animation
         */
        private List<VertexSoup> _positionalData;
    }
}