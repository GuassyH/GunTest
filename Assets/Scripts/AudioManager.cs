using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public GameObject AudioSoundPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

        
    public void PlayAudio(AudioClip audioClip, Vector3 spawnPos){
        GameObject tempAudioSource = Instantiate(AudioSoundPrefab, spawnPos, Quaternion.identity);
        tempAudioSource.GetComponent<AudioSource>().clip = audioClip;
        tempAudioSource.GetComponent<AudioSource>().Play();
        tempAudioSource.AddComponent<DestroyAfterTime>().Lifetime = audioClip.length + 1;
    }
}
