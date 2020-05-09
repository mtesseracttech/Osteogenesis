using UnityEngine;

namespace Osteogenesis
{
    public class PlanePlaneIntersection
    {
        public static PlanePlaneIntersectionResult PlanePlaneIntersect(Plane plane0, Plane plane1)
        {
            PlanePlaneIntersectionResult result = new PlanePlaneIntersectionResult();

            float dot = Vector3.Dot(plane0.normal, plane1.normal);
            if (Mathf.Abs(dot) >= 1f)
            {
                // The planes are parallel.  Check if they are coplanar.
                float cDiff;
                if (dot >= 0)
                {
                    // Normals are in same direction, need to look at c0-c1.
                    cDiff = plane0.distance - plane1.distance;
                }
                else
                {
                    // Normals are in opposite directions, need to look at
                    // c0+c1.
                    cDiff = plane0.distance + plane1.distance;
                }
        
                if (Mathf.Abs(cDiff) == 0f)
                {
                    // The planes are coplanar.
                    result.Intersect = true;
                    result.IsLine = false;
                    result.Plane = plane0;
                    return result;
                }
        
                // The planes are parallel but distinct.
                result.Intersect = false;
                return result;
            }
            
            float invDet = 1f / (1f - dot * dot);
            float c0 = (plane0.distance - dot * plane1.distance) * invDet;
            float c1 = (plane1.distance - dot * plane0.distance) * invDet;
            result.Intersect = true;
            result.IsLine = true;
            result.Line.Origin = c0 * plane0.normal + c1 * plane1.normal;
            result.Line.Direction = Vector3.Normalize(Vector3.Cross(plane0.normal, plane1.normal));
            return result;
        }
        
        public struct PlanePlaneIntersectionResult
        {
            public bool Intersect;

            // If 'intersect' is true, the intersection is either a line or
            // the planes are the same.  When a line, 'line' is valid.  When
            // the same plane, 'plane' is set to one of the planes.
            public bool IsLine;
            public Line Line;
            public Plane Plane;
        }
    };
}