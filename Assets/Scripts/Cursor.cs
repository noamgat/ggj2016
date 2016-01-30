using UnityEngine;
using System.Collections;

public class Cursor : MonoBehaviour {

    public Plane groundPlane;

    public PatternProxy PatternProxyInst;

    private bool _isMouseDown = false;
    private bool _isTouchDown = false;

    // Use this for initialization
    void Start () {
        groundPlane = new Plane(new Vector3(), new Vector3(0, 1, 0),new  Vector3(1, 1, 0));
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetMouseButtonDown(0)) _isMouseDown = true;
        if (Input.GetMouseButtonUp(0)) _isMouseDown = false;

        _isTouchDown = Input.touchCount > 0;


        if (_isMouseDown || _isTouchDown) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float rayDistance;
            if (groundPlane.Raycast(ray, out rayDistance))
                transform.position = ray.GetPoint(rayDistance);
        }
        
        
    }


    void OnTriggerEnter(Collider collider) {
        PatternProxyInst.CheckCollision(collider);
    }

}
