using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour {

	public AudioClip backgroundMusic;
	public AudioClip[] levelWinClips;
	public AudioClip levelLoseClip;

	private static SoundManager instance;

	public static SoundManager Instance { get { return instance; } }

	void Awake() {
		instance = this;
	}

	private AudioSource audioSource {
		get {
			return GetComponent<AudioSource> ();
		}
	}

	public void PlayBackgroundMusic() {
		audioSource.Stop ();
		audioSource.PlayOneShot (backgroundMusic);
	}

	public void PlayLevelWinClip(int levelIndex) {
		AudioClip clip = levelWinClips [levelIndex % levelWinClips.Length];
		audioSource.Stop ();
		audioSource.PlayOneShot (clip);
	}

	public void PlayLevelLoseClip() {
		audioSource.Stop ();
		audioSource.PlayOneShot (levelLoseClip);
	}

}
