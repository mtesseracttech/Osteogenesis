using System.Collections;
using System.Collections.Generic;
using Osteogenesis;
using UnityEngine;

public class IntersectionTester : MonoBehaviour
{
    public GameObject inPlane1;
    public GameObject inPlane2;

    private Plane plane1;
    private Plane plane2;
    
    // Start is called before the first frame update
    void Start()
    {
        plane1 = new Plane();
        plane2 = new Plane();
    }

    // Update is called once per frame
    void Update()
    {
        Transform p1t = inPlane1.transform;
        Transform p2t = inPlane2.transform;
        
        plane1 = new Plane(p1t.position, p1t.position + p1t.right, p1t.position + p1t.up);
        plane2 = new Plane(p2t.position, p2t.position + p2t.right, p2t.position + p2t.up);
        
        Debug.DrawRay(plane1.normal * plane1.distance, plane1.normal, Color.green);
        Debug.DrawRay(plane2.normal * plane2.distance, plane2.normal, Color.blue);
        
        var intersection = PlanePlaneIntersection.PlanePlaneIntersect(plane1, plane2);
        
        if (intersection.Intersect)
        {
            if (intersection.IsLine)
            {
                var start = intersection.Line.Origin;
                Debug.DrawLine(start - intersection.Line.Direction * 100, start + intersection.Line.Direction * 100, Color.magenta);
            }
        }
        
        DebugDrawPlanes();
    }

    private void DebugDrawPlanes()
    {
        
        // Debug.DrawRay(inPlane1.transform.position, inPlane1.transform.right, Color.red);
        // Debug.DrawRay(inPlane1.transform.position, inPlane1.transform.up, Color.green);
        // Debug.DrawRay(inPlane1.transform.position, inPlane1.transform.forward, Color.blue);
        //
        // Debug.DrawRay(inPlane2.transform.position, inPlane2.transform.right, Color.red);
        // Debug.DrawRay(inPlane2.transform.position, inPlane2.transform.up, Color.green);
        // Debug.DrawRay(inPlane2.transform.position, inPlane2.transform.forward, Color.blue);
    }
}
