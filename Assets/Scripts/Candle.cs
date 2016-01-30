using UnityEngine;
using System.Collections;
using System;

public class Candle : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    internal void Lightdelay(float delay) {
        Invoke("LightSingleCandle", delay);
        
    }

     
    private void LightSingleCandle() {
        GetComponent<ParticleSystem>().Play();
        
    }

    public void Stop() {
        GetComponent<ParticleSystem>().Stop();

    }
}
