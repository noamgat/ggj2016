using UnityEngine;
using System.Collections;

public class Vertex : MonoBehaviour {

    
    public Vector2 location;

    public void Place() {
        transform.localPosition = (Vector3)location + new Vector3(0, 0, -0.01f);
    }

    public void SetColliderSize(float colSize) {
        GetComponent<SphereCollider>().radius = colSize;
    }
}
