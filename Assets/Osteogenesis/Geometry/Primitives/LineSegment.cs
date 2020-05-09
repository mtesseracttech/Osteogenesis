using UnityEngine;

namespace Osteogenesis
{
    public class LineSegment
    {
        public Vector3 P1 { get; }
        public Vector3 P2 { get; }

        public LineSegment(Vector3 p1, Vector3 p2)
        {
            P1 = p1;
            P2 = p2;
        }

        protected bool Equals(LineSegment other)
        {
            return P1.Equals(other.P1) && P2.Equals(other.P2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LineSegment) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (P1.GetHashCode() * 397) ^ P2.GetHashCode();
            }
        }
    }
}