using UnityEngine;
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

    public GameObject Floor;
    private Material FloorMat;

    public float _InitialWave = 0;
    private Tweener _waveTweenr;

    private Color _offColor = new Color(0.3f, 0.3f, 0.3f);
    internal bool IsWorking;

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

        FloorMat = Floor.GetComponent<Renderer>().material;

        IsWorking = false;
    }

    public void Place() {
        transform.localPosition = VertexA.location;
        transform.localPosition += new Vector3(0, 0, -0.02f);
        transform.localRotation = Quaternion.LookRotation(VertexB.location - VertexA.location, Vector3.back);
        transform.localScale = new Vector3(1, 1, (VertexB.location - VertexA.location).magnitude);
    }

    public void AddPlayer(int playerIndex){
        playerIndexes[playerIndex] = 1;

        if (_waveTweenr != null) _waveTweenr.Kill();
        _InitialWave = 0;
        _waveTweenr =  HOTween.To(this, 0.2f, new TweenParms().Prop("_InitialWave", 1).Loops(2, LoopType.Yoyo));
    }

    public void Splash(float timing) {
        if (_waveTweenr != null) _waveTweenr.Kill();
        _waveTweenr = HOTween.To(this, 0.4f, new TweenParms().Prop("_InitialWave", 1).Loops(2, LoopType.Yoyo).Delay(0.1f + timing * 0.45f).Ease(EaseType.EaseOutSine));
//        print(timing);
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
        _mixAmmount += Time.deltaTime * 2;
        while (_mixAmmount > 1) {
            _mixAmmount -= 1;
            fromColor = (fromColor + 1) % playerIndexes.Length;
        }

        IsWorking = maxIntensity > 0;
        
        Color targetColor;

        if (IsWorking) {
            while (playerIndexes[fromColor] <= 0) {
                fromColor = (fromColor + 1) % playerIndexes.Length;
            }

            toColor = (fromColor + 1) % playerIndexes.Length;
            while (playerIndexes[toColor] <= 0 && toColor != fromColor) {
                toColor = (toColor + 1) % playerIndexes.Length;
            }

            if (toColor != fromColor) {
                targetColor = Color.Lerp(_colors[fromColor], _colors[toColor], _mixAmmount);
            } else {
                targetColor = _colors[fromColor];
            }

        } else {
            targetColor = Color.white;
        }
        

        float visIntesity = Mathf.Min(1, maxIntensity * 4 + _InitialWave);
        
        targetColor.a = visIntesity;
        WallMat[0].color = targetColor;
        WallMat[1].color = targetColor;

        float height = 0.3f * _InitialWave + 0.2f * visIntesity;

        Walls[0].transform.localScale = new Vector3(1, height, 1);
        Walls[0].transform.localPosition = new Vector3(0, height / 2, 0.5f);
        Walls[1].transform.localScale = new Vector3(1, height, 1);
        Walls[1].transform.localPosition = new Vector3(0, height / 2, 0.5f);

        FloorMat.color = Color.Lerp(Color.Lerp(_offColor, Color.white, visIntesity), targetColor, 0.3f * visIntesity);

        
    }
}
