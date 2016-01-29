using UnityEngine;
using System.Collections;

public class Segment : MonoBehaviour {
    
    
    public Vertex VertexA;
    public Vertex VertexB;
    

    public void Place() {
        transform.localPosition = VertexA.location;
        transform.localRotation = Quaternion.LookRotation(VertexB.location - VertexA.location, Vector3.back);
        transform.localScale = new Vector3(1, 1, (VertexB.location - VertexA.location).magnitude);
    }

    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
