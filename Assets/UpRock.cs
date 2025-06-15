using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class UpRock : MonoBehaviour
{
    public float targetHeight = 1f;
    public float duration = 0.3f;
    public float lifetime = 3f;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    public AudioClip upSound;
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition + new Vector3(0, targetHeight, 0);
        audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(upSound);

        StartCoroutine(RiseUp());    

        Invoke(nameof(DestroyRock), lifetime);
    }

    private IEnumerator RiseUp(){
        float elapsedTime = 0f;



        while(elapsedTime < duration){
            transform.position = Vector3.Lerp (startPosition, targetPosition, elapsedTime/duration);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        transform.position = targetPosition;
    }

    private void OnCollisionEnter(Collision collision){
        if(collision.gameObject.CompareTag("ENEMY")){
            DestroyRock();
        }
    }

    private void DestroyRock(){
        Destroy(gameObject);
    }
}
