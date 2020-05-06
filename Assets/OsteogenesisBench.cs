using System.Collections;
using System.Collections.Generic;
using Osteogenesis;
using UnityEngine;

public class OsteogenesisBench : MonoBehaviour
{
    public GameObject testObject;

    private GaussArea gaussArea;

    //private List<(Vector3, Vector3)> _transformedPivotNormals = new List<(Vector3, Vector3)>();
    //private float normalizationFactor = 1.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        if(testObject == null) Debug.Log("No Test Object was bound");
        
        if(!testObject.GetComponent<MeshFilter>()) Debug.Log("No Meshfilter on the Test Object");

        var mesh = testObject.GetComponent<MeshFilter>().sharedMesh;
        
        gaussArea = new GaussArea(mesh);
        
        //gaussArea.DrawPivotNormals(testObject.transform);
        //gaussArea.DrawVertexDebugInfo(testObject.transform);
        //gaussArea.DrawSurfaceInfo();
        Debug.Log(gaussArea.ToString());
    }
    
    // Update is called once per frame
    void Update()
    {
    }
}
