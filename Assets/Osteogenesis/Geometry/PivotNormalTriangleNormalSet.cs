using System.Collections.Generic;
using UnityEngine;

namespace Osteogenesis
{
    public class PivotNormalTriangleNormalSet
    {
        public Vector3 Pivot;
        public List<Vector3> TriangleNormals = new List<Vector3>();
        
        public PivotNormalTriangleNormalSet(Vector3 pivot, List<Vector3> triangleNormals = null)
        {
            Pivot = pivot;
            if (triangleNormals != null)
            {
                TriangleNormals = triangleNormals;
            }
        }

        protected bool Equals(PivotNormalTriangleNormalSet other)
        {
            return Pivot.Equals(other.Pivot) && Equals(TriangleNormals, other.TriangleNormals);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PivotNormalTriangleNormalSet) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Pivot.GetHashCode() * 397) ^ (TriangleNormals != null ? TriangleNormals.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return $"{nameof(Pivot)}: {Pivot}, {nameof(TriangleNormals)}: {TriangleNormals}";
        }
    }
}