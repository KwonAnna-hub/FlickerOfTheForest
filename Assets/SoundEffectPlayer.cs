using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectPlayer : MonoBehaviour
{
    public AudioClip effectSound;
    private AudioSource audioSource;
    private Transform initialTrans;

    // Start is called before the first frame update
    void Start()
    {
        initialTrans = transform;
        audioSource = GetComponent<AudioSource>();
    }

    public void ResetToTnitialTrans(){
        transform.position = initialTrans.position;
        transform.rotation = initialTrans.rotation;
    }

    public void PlayUISound(){
        PlayEffectSound(effectSound);
    }

    // Update is called once per frame
    public void PlayEffectSound(AudioClip audio){
        if(audio != null && audioSource != null){
            audioSource.PlayOneShot(audio);
        }
    }
}
