using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFade : MonoBehaviour
{
    private Renderer enemyRenderer;
    private Material enemyMaterial;
    private float fadeDuration = 2f;

    // Start is called before the first frame update
    void Start()
    {
        enemyRenderer = GetComponent<Renderer>();
        if(enemyRenderer != null){
            enemyMaterial = enemyRenderer.material;
        }
    }

    public void ApplyMagicEffect(){
        if(enemyMaterial != null){
            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator FadeOut(){
        float elapsedTime = 0f;
        Color initialColor = enemyMaterial.color;

        while(elapsedTime < fadeDuration){
            float alpha = Mathf.Lerp(initialColor.a, 0f, elapsedTime / fadeDuration);
            enemyMaterial.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        enemyMaterial.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);
        Destroy(gameObject);
    }
}
