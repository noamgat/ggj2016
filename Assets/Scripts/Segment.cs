using UnityEngine;
using System.Collections;

public class Segment : MonoBehaviour {
    

    public Vertex VertexA;
    public Vertex VertexB;

    private float[] playerIndexes;

    private int _numOPlayers;
    private float _lifetime;

    internal void Init(int numOPlayers, float lifetime) {
        _numOPlayers = numOPlayers;
        _lifetime = lifetime;
        playerIndexes = new float[numOPlayers];
    }

    public void Place() {
        transform.localPosition = VertexA.location;
        transform.localPosition += new Vector3(0, 0, -0.02f);
        transform.localRotation = Quaternion.LookRotation(VertexB.location - VertexA.location, Vector3.back);
        transform.localScale = new Vector3(1, 1, (VertexB.location - VertexA.location).magnitude);
    }

    public void AddPlayer(int playerIndex){
        playerIndexes[playerIndex] = 1;
        /*get{
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
        }*/
    }


    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
        for (int i = 0; i < playerIndexes.Length; i++) {
            playerIndexes[i] -= Time.deltaTime / _lifetime;
        }
	}
}
