using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Holoville.HOTween;

public class GameManager : MonoBehaviour {

    public GameObject MainMenuUI;
    public GameObject IngameUI;
    
    // Current game state
    private enum States { Menu, Connecting, Ingame, Loss, Win }
    private States _state;

    // Use this for initialization
    void Start () {
        // 
        IngameUI.SetActive(false);
        MainMenuUI.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Start game - Look for a new game and start
    public void InitiateConnection ()
    {
        // Display a connecting messaGE
        //Text statusText = GameObject.Find("StatusText").GetComponent<Text>();
        //statusText.text = "Forming a pact...";

        // Start the game 
        IngameUI.SetActive(true);
        MainMenuUI.SetActive(false);
        
    }
    

}
