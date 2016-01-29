﻿using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class Segment : MonoBehaviour {


    internal Vertex VertexA;
    internal Vertex VertexB;

    private float[] playerIndexes;
    
    private float _lifetime;

    private float _mixAmmount;
    private int fromColor = 0;
    private int toColor = 0;

    public Texture[] FireTextures;

    private Color[] _colors;
    private Color _transColor = new Color(0, 0, 0, 0);

    private Material[] WallMat = new Material[2];

    public GameObject[] Walls;

    internal void Init(int numOPlayers, float lifetime) {
        _lifetime = lifetime;
        playerIndexes = new float[numOPlayers];

        _colors = new Color[numOPlayers];
        _colors[0] = Color.red;
        _colors[1] = Color.green;
        _colors[2] = Color.blue;
        _colors[3] = Color.yellow;

        WallMat[0] = Walls[0].GetComponent<Renderer>().material;
        WallMat[1] = Walls[1].GetComponent<Renderer>().material;
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

        WallMat[0].mainTexture = FireTextures[UnityEngine.Random.Range(0, FireTextures.Length)];
        WallMat[1].mainTexture = FireTextures[UnityEngine.Random.Range(0, FireTextures.Length)];

        HOTween.To(WallMat[0], Random.Range(0.8f, 1.2f), new TweenParms().Prop("mainTextureOffset", new Vector2(0.8f, 0)).Ease(EaseType.Linear).Loops(-1, LoopType.Restart));
        HOTween.To(WallMat[1], Random.Range(0.8f, 1.2f), new TweenParms().Prop("mainTextureOffset", new Vector2(0.8f, 0)).Ease(EaseType.Linear).Loops(-1, LoopType.Restart));

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

        //targetColor = Color.Lerp(_transColor, targetColor, visIntesity);
        targetColor.a = visIntesity;
        WallMat[0].color = targetColor;
        WallMat[1].color = targetColor;
        
    }
}
