using System;

namespace Osteogenesis
{
    public class EdgeIndices : IComparable<EdgeIndices>
    {
        public int I1 { get; }

        public int I2 { get; }
        public EdgeIndices(int i1, int i2)
        {
            I1 = i1;
            I2 = i2;
        }

        protected bool Equals(EdgeIndices other)
        {
            //Symmetric comparison, to make sure that A-B == B-A
            return I1 == other.I1 && I2 == other.I2 || I1 == other.I2 && I2 == other.I1;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EdgeIndices) obj);
        }

        public override int GetHashCode()
        {
            //Symmetric hash, to make sure that A-B == B-A
            return I1 ^ I2;
        }

        public int CompareTo(EdgeIndices other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var i1Comparison = I1.CompareTo(other.I1);
            if (i1Comparison != 0) return i1Comparison;
            return I2.CompareTo(other.I2);
        }

        public override string ToString()
        {
            return $"Edge {nameof(I1)}: {I1}, {nameof(I2)}: {I2}";
        }
    }
}