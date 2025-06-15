using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UIElements;

public class ParticleMover : MonoBehaviour
{
    private Vector3 moveDirection;
    private float speed;
    private float lifetime;
    private float timer;

    private bool isCollided = false;
    private Transform target;
    public LayerMask groundLayer;

    public AudioClip magic;
    public AudioClip hitSound;
    private AudioSource audioSource;

    public void Start(){
        audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(magic);
    }

    public void SetDirection(Vector3 direction, float moveSpeed, float time){
        moveDirection = direction.normalized;
        speed = moveSpeed;
        lifetime = time;
    }

    private void Update(){
        if(!isCollided){
            transform.position += - moveDirection * speed * Time.deltaTime;
        }else if(target != null){
            target.position += Vector3.up * 0.5f * Time.deltaTime;
            transform.position += Vector3.up * 0.5f * Time.deltaTime;

            FadeOut(target.gameObject);
            FadeOut(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision){
        if(collision.gameObject.CompareTag("ENEMY")){
            isCollided = true;
            target = collision.transform;
            audioSource.PlayOneShot(hitSound);

            Destroy(gameObject,1f);
            Destroy(collision.gameObject,1f);
        }else{
            Destroy(gameObject);
        }
    }

    private void FadeOut(GameObject obj){
        Renderer renderer = GetComponent<Renderer>();
        if(renderer != null){
            Material mat = renderer.material;
            Color color = mat.color;
            color.a -= Time.deltaTime * 0.5f;
            mat.color = color;

            if(color.a <= 0){
                Destroy(obj);
            }
        }
    }
}
