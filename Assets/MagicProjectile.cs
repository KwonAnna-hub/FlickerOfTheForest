using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicProjectile : MonoBehaviour
{
    public ParticleSystem magicParticle;
    public Transform target;

    private ParticleSystem.ShapeModule shape;

    // Start is called before the first frame update
    void Start()
    {
        shape = magicParticle.shape;
    }

    void Update(){
        if(target != null){
            shape.position = target.position;
        }
    }

    void OnParticleCollision(GameObject other){
        if(other.CompareTag("ENEMY")){
            EnemyFade enemy = other.GetComponent<EnemyFade>();
            if(enemy != null){
                enemy.ApplyMagicEffect();
            }
        }
        Destroy(this);
    }
}
