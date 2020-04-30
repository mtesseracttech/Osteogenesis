using UnityEngine;

namespace Osteogenesis
{
    public class Triangle
    {
        private Vector3 A { get; }
        private Vector3 B { get; }
        private Vector3 C { get; }
        
        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            A = a;
            B = b;
            C = c;
        }

        public Vector3 GetNormal()
        {
            return Vector3.Cross(B - A, C - A).normalized;
        }

        public float GetSurface()
        {
            return Vector3.Cross(B - A, C - A).magnitude * 0.5f;
        }

        public void DebugDraw(Color color)
        {
            Debug.DrawLine(A, B, color);
            Debug.DrawLine(B, C, color);
            Debug.DrawLine(C, A, color);
        }
    }
}