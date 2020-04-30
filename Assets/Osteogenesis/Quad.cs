using UnityEngine;

namespace Osteogenesis
{
    public class Quad
    {
        public Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }

        private Vector3 A { get; }
        private Vector3 B { get; }
        private Vector3 C { get; }
        private Vector3 D { get; }
        
        
        public Vector3 GetNormal()
        {
            return Vector3.Cross(B - A, C - A).normalized;
        }

        public float GetSurface()
        {
            return new Triangle(A, B, C).GetSurface() + new Triangle(A, C ,D).GetSurface();
        }

        public void DebugDraw(Color color)
        {
            Debug.DrawLine(A, B, color);
            Debug.DrawLine(B, C, color);
            Debug.DrawLine(C, D, color);
            Debug.DrawLine(D, A, color);
        }
    }
    
    
}