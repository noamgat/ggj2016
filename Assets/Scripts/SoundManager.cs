using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour {
    
	public AudioClip levelWinClip;
	public AudioClip levelLoseClip;
    public AudioClip[] levelStartClips;

    private static SoundManager instance;

	public static SoundManager Instance { get { return instance; } }


    private AudioSource _backSrc;
    private AudioSource _effectsSrc;

    void Awake() {
		instance = this;
        _backSrc = GetComponents<AudioSource>()[0];
        _effectsSrc = GetComponents<AudioSource>()[1];
    }
    

	public void PlayBackgroundMusic() {
        _backSrc.Stop ();
        _backSrc.Play();
	}

    public void StopBackgroundMusic() {
        _backSrc.Stop();
    }

    public void PlayLevelWinClip() {
        _effectsSrc.Stop();
        _effectsSrc.PlayOneShot(levelWinClip);
    }

    public void PlayLevelStartClip(int levelIndex) {
		AudioClip clip = levelStartClips[levelIndex % levelStartClips.Length];
        _effectsSrc.Stop ();
        _effectsSrc.PlayOneShot (clip);
	}

	public void PlayLevelLoseClip() {
        _effectsSrc.Stop ();
        _effectsSrc.PlayOneShot (levelLoseClip);
	}

}
