using System.Collections;
using System.Collections.Generic;
using Osteogenesis;
using UnityEngine;

public class OsteogenesisBench : MonoBehaviour
{
    public GameObject testObject;

    private GaussArea gaussArea;

    // Start is called before the first frame update
    void Start()
    {
        if(testObject == null) Debug.Log("No Test Object was bound");
        
        if(!testObject.GetComponent<MeshFilter>()) Debug.Log("No Meshfilter on the Test Object");
        
        gaussArea = new GaussArea(testObject.GetComponent<MeshFilter>().sharedMesh);
    }

    // Update is called once per frame
    void Update()
    {
        gaussArea.DrawPivotNormals();
    }
}
