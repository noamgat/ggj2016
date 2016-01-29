using UnityEngine;
using System.Collections;

public class Segment : MonoBehaviour {

    public enum States {Idle, Player, OtherPlayer, Both };
    

    public Vertex VertexA;
    public Vertex VertexB;

    private States _state = States.Idle;

    public void Place() {
        transform.localPosition = VertexA.location;
        transform.localRotation = Quaternion.LookRotation(VertexB.location - VertexA.location, Vector3.back);
        transform.localScale = new Vector3(1, 1, (VertexB.location - VertexA.location).magnitude);
    }

    public States State{
        get{
            return _state;
        }
        set{
            _state = value;

            Material mat = GetComponentInChildren<Renderer>().material;

            switch (value) {
                case States.Idle: mat.color = Color.white; break;
                case States.Player: mat.color = Color.green; break;
                default : mat.color = Color.black; break;
            }
        }
    }


    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
