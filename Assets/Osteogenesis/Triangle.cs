using UnityEngine;

namespace Osteogenesis
{
    public class Triangle
    {
        public Vector3 A { get; }
        public Vector3 B { get; }
        public Vector3 C { get; }

        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            A = a;
            B = b;
            C = c;
        }
        
        public Triangle(FaceIndexTriplet triplet, Mesh mesh)
        {
            A = mesh.vertices[triplet.V1];
            B = mesh.vertices[triplet.V2];
            C = mesh.vertices[triplet.V3];
        }

        public override string ToString()
        {
            return $"{nameof(A)}: {A}, {nameof(B)}: {B}, {nameof(C)}: {C}";
        }

        public Vector3 GetNormal()
        {
            return Vector3.Cross(B - A, C - A).normalized;
        }

        public float GetSurface()
        {
            return Vector3.Cross(B - A, C - A).magnitude * 0.5f;
        }

        public void DebugDraw(Color color, float duration = 0f)
        {
            Debug.DrawLine(A, B, color, duration);
            Debug.DrawLine(B, C, color, duration);
            Debug.DrawLine(C, A, color, duration);
        }
    }
}