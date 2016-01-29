using UnityEngine;
using System.Collections;

public class Vertex : MonoBehaviour {

    
    public Vector2 location;

    public void Place() {
        transform.localPosition = location;
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    
}
