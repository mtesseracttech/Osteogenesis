using UnityEngine;

namespace Osteogenesis
{
    public class VectorMaths
    {
        public static Vector3 ProjectAlongVectorOntoPlane(Vector3 point, Vector3 normal, Vector3 direction)
        {
            var distance = Vector3.Dot(-point, normal)/Vector3.Dot(normal, direction);
            return point + direction * distance;
        }
    }
}