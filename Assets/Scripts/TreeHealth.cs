using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreeHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public GameObject gameManager;

    public Image healthBar;
    public GameObject endUI;

    private bool gameOver = false;

    Transform flamesEffect;


    // Start is called before the first frame update
    void Start()
    {
        flamesEffect = GetComponent<Transform>().Find("FlamesParticleEffect");
        currentHealth = maxHealth;

        UpdateHealthBar();    
    }
    
    public void TakeDamage(int damage){
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthBar();

        if(currentHealth <= 0 && !gameOver){
            GameOver();
        }
    }

    private void UpdateHealthBar(){
        if (healthBar != null){
            healthBar.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    private void GameOver(){
        gameOver = true;
        flamesEffect.gameObject.SetActive(true);
        flamesEffect.gameObject.GetComponent<ParticleSystem>().Play();
        
        gameManager.GetComponent<GameManager>().EndGame();

        gameOver = false;
    }

    public void reStart(){
        currentHealth = maxHealth;
        flamesEffect.gameObject.SetActive(false);
        UpdateHealthBar();
    }
}
