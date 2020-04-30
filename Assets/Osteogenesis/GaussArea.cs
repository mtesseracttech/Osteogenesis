using System.Collections.Generic;
using UnityEngine;

namespace Osteogenesis
{
    public class GaussArea
    {
        //A - B and the 2 triangles the Vertex is part of
        private HashSet<(int, int)> _edges;
        
        //A - 
        
        
        public GaussArea(Mesh mesh)
        {
            foreach (var triangle in mesh.GetTriangles(0))
            {
                
            }
        }


        private void ProcessVertex(Mesh mesh, int)
        {
            
        }
    }
}