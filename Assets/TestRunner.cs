using System;
using System.Collections;
using System.Collections.Generic;
using Osteogenesis;
using UnityEngine;

public class TestRunner : MonoBehaviour
{
    public Vector3 A = Vector3.right;
    public Vector3 B = Vector3.forward;
    public Vector3 C = Vector3.up;

    // Start is called before the first frame update
    void Start()
    {
        // //Triangle Tests
        // float result = GeometryUtils.AreaTriangleOnUnitSphere(Vector3.right, Vector3.forward, Vector3.up);
        // Debug.Log("Triangle on 1/8th of unit sphere: " + result);
        // Debug.Assert(Math.Abs(result - Mathf.PI / 2f) < 0.0001f);
        // float result2 =
        //     GeometryUtils.AreaTriangleOnUnitSphere(new Vector3(1, 0, 1).normalized, Vector3.forward, Vector3.up);
        // Debug.Log("Triangle on 1/16th of unit sphere: " + result2);
        // Debug.Assert(Math.Abs(result2 - Mathf.PI / 4f) < 0.0001f);
        //
        // //Quad Tests
        // float result3 = GeometryUtils.AreaQuadOnUnitSphere(Vector3.right, Vector3.forward, Vector3.up, Vector3.back);
        // Debug.Log("Quad on 1/4th of unit sphere: " + result3);
        // Debug.Assert(Math.Abs(result3 - Mathf.PI) < 0.0001f);
        //
        // //polygon test
        //
        // //3/8th poly 
        //
        // var threeEightsPoly = new List<Vector3>
        // {
        //     Vector3.right,
        //     Vector3.forward,
        //     Vector3.up,
        //     (Vector3.left + Vector3.forward).normalized,
        //     Vector3.left,
        //     Vector3.back,
        // };
        //
        // float result4 = GeometryUtils.AreaPolygonOnUnitSphere(threeEightsPoly);
        // Debug.Log("Quad on 7/16th of unit sphere: " + result4);
        // Debug.Assert(Math.Abs(result4 - Mathf.PI * 7/4f) < 0.0001f);
        //
        //
        // var halfPiPoly = new List<Vector3>
        // {
        //     Vector3.right, 
        //     Vector3.forward, 
        //     Vector3.up
        // };
        //
        // float halfPi = GeometryUtils.AreaPolygonOnUnitSphere(halfPiPoly);
        //
        
    }

    // Update is called once per frame
    void Update()
    {
        var threeEightsPoly = new List<Vector3>
        {
            Vector3.right,
            Vector3.forward,
            Vector3.up,
            (Vector3.left + Vector3.forward).normalized,
            Vector3.left,
            Vector3.back,
        };

        //float result4 = GeometryUtils.AreaPolygonOnUnitSphere(threeEightsPoly);
        //Debug.Log("Quad on 3/8th of unit sphere: " + result4);

        //float result = GeometryUtils.AreaTriangleOnUnitSphere(Vector3.right, Vector3.forward, new Vector3(Mathf.Sin(Time.time * .3f), Mathf.Sin(Time.time * .5f), Mathf.Sin(Time.time * .7f)).normalized);
        //float result = GeometryUtils.AreaQuadOnUnitSphere(Vector3.right, Vector3.forward, new Vector3(Mathf.Sin(Time.time * .3f), Mathf.Sin(Time.time * .5f), Mathf.Sin(Time.time * .7f)).normalized,  Vector3.back);
        // float result = GeometryUtils.AreaTriangleOnUnitSphere(A.normalized, B.normalized, C.normalized);
        // float normalResult = new Triangle(A.normalized, B.normalized, C.normalized).GetSurface();
        // Debug.Log("Triangle on 1/8th of unit sphere: " + result + ", for normal triangle area: " + normalResult);
    }
}