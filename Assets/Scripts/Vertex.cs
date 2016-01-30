using UnityEngine;
using System.Collections;

public class Vertex : MonoBehaviour {

    
    public Vector2 location;

    public void Place() {
        transform.localPosition = location;
    }

    public void SetColliderSize(float colSize) {
        GetComponent<SphereCollider>().radius = colSize;
    }
}
