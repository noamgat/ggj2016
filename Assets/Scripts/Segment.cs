using UnityEngine;
using System.Collections;

public class Segment : MonoBehaviour {
    

    public Vertex VertexA;
    public Vertex VertexB;

    private float[] playerIndexes;
    
    private float _lifetime;

    private float _mixAmmount;
    private int fromColor = 0;
    private int toColor = 0;

    private Color[] _colors;

    private Material mat;

    internal void Init(int numOPlayers, float lifetime) {
        _lifetime = lifetime;
        playerIndexes = new float[numOPlayers];

        _colors = new Color[numOPlayers];
        _colors[0] = Color.red;
        _colors[1] = Color.green;
        _colors[2] = Color.blue;
        _colors[3] = Color.yellow;

        mat = GetComponentInChildren<Renderer>().material;
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
        float maxIntensity = 0;
        for (int i = 0; i < playerIndexes.Length; i++) {
            playerIndexes[i] -= Time.deltaTime / _lifetime;
            maxIntensity = Mathf.Max(maxIntensity, playerIndexes[i]);
        }

        //progress mix. go to next color if needed
        _mixAmmount += Time.deltaTime * 4;
        while (_mixAmmount > 1) {
            _mixAmmount -= 1;
            fromColor = (fromColor + 1) % playerIndexes.Length;
        }

        if (maxIntensity > 0) {
            while (playerIndexes[fromColor] <= 0) {
                fromColor = (fromColor + 1) % playerIndexes.Length;
            }
        }

        toColor = (fromColor + 1) % playerIndexes.Length;
        while (playerIndexes[toColor] <= 0 && toColor != fromColor) {
            toColor = (toColor + 1) % playerIndexes.Length;
        }

        Color targetColor;

        if (toColor != fromColor) {
            targetColor = Color.Lerp(_colors[fromColor], _colors[toColor], _mixAmmount);
        } else {
            targetColor = _colors[fromColor];
        }

        float visIntesity = Mathf.Min(1, maxIntensity * 4);

        targetColor = Color.Lerp(Color.white, targetColor, visIntesity);

        mat.color = targetColor;
    }
}
