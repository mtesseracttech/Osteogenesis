using System;
using System.Collections.Generic;

namespace Osteogenesis
{
    public class FaceIndexTriplet : IComparable<FaceIndexTriplet>
    {
        public int V1 { get; }
        public int V2 { get; }
        public int V3 { get; }

        public FaceIndexTriplet(int v1, int v2, int v3)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
        }

        protected bool Equals(FaceIndexTriplet other)
        {
            return V1 == other.V1 && V2 == other.V2 && V3 == other.V3;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FaceIndexTriplet) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = V1;
                hashCode = (hashCode * 397) ^ V2;
                hashCode = (hashCode * 397) ^ V3;
                return hashCode;
            }
        }

        public int CompareTo(FaceIndexTriplet other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var v1Comparison = V1.CompareTo(other.V1);
            if (v1Comparison != 0) return v1Comparison;
            var v2Comparison = V2.CompareTo(other.V2);
            if (v2Comparison != 0) return v2Comparison;
            return V3.CompareTo(other.V3);
        }
        
        public override string ToString()
        {
            return $"{nameof(V1)}: {V1}, {nameof(V2)}: {V2}, {nameof(V3)}: {V3}";
        }

        public int this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return V1;
                    case 1:
                        return V2;
                    case 2:
                        return V3;
                    default:
                        throw new NotImplementedException(
                            "Face index triplet assumes triangels, indices below 0 and over 2 are invalid.");
                }
            }
        }
    }
}