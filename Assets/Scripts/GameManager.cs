using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;


#if UNITY_EDITOR
using UnityEditor;
    #endif

public class GameManager : MonoBehaviour
{
    public GameObject mainUI;
    public GameObject endUI;
    public GameObject GameUI;
    public GameObject player;//플레이어
    private Rigidbody rb;
    public GameObject happyUI;
    public GameObject badUI;
    public Image healthBar;
    public Text waveText;

    public Transform[] spawners; // 두 개 이상의 스포너를 할당할 배열
    public GameObject humanPrefab; // Human 프리팹
    public int totalWaves = 5;
    public float waveInterval = 10f; // 웨이브 간격
    private int baseHumans = 3;
    public int humansPerWave = 2; //증가 수
    public float finalWaveDuration = 15f;
    Vector3 teleportPosition;
    Quaternion teleportRotation;
    
    private int currentWave = 0;
    private bool isGameRunning = false;

    public AudioClip happyEndSound;
    public AudioClip badEndSound;
    SoundEffectPlayer soundEffectPlayer;
    private AudioSource audioSource;
    public AudioSource backGroundMusic;
    public AudioClip clearMusic;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        player.GetComponent<PlayerCtrl>().enabled = false; //조작 멈춤
        mainUI.SetActive(true); //메인화면 활성화
        soundEffectPlayer = player.GetComponent<SoundEffectPlayer>();
        rb = player.GetComponent<Rigidbody>();
        audioSource = soundEffectPlayer.GetComponent<AudioSource>();
        
    }

    public GameObject opening;
    
    public void StartVideo(){
        opening.SetActive(true);
        opening.GetComponent<VideoPlayer>().loopPointReached += StartVideoEnd;
        opening.GetComponent<VideoPlayer>().Play();
    }

    void StartVideoEnd(VideoPlayer vp){
        opening.SetActive(false);
        GameUI.SetActive(true);
        StartGame();
        player.GetComponent<SoundEffectPlayer>().ResetToTnitialTrans();
    }

    public void StartGame(){

        Debug.Log("StartGame 호출됨");
        isGameRunning = true;

        Time.timeScale = 1;

        teleportPosition = new Vector3(48f, 15f, 95f); // 원하는 좌표
        teleportRotation = Quaternion.Euler(0f, 200f, 0f); // 목표 회전 (Euler로 설정)
        TeleportPlayer(teleportPosition, teleportRotation);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        UpdateWaveText();

        StartCoroutine(DelayBeforeGameStarts());

        StartCoroutine(WaveSystem());
    }

    private void UpdateWaveText(){
        if(waveText != null){
            waveText.text = $"{currentWave} Wave";
        }
    }

    private IEnumerator DelayBeforeGameStarts(){
        yield return new WaitForSeconds(0.1f);
        
        player.GetComponent<PlayerCtrl> ().enabled = true;
    }

    private IEnumerator WaveSystem(){

        while(isGameRunning && currentWave < totalWaves){
            currentWave++;
            UpdateWaveText();

            if(currentWave < totalWaves){
                int humansToSpawn = baseHumans + humansPerWave * currentWave;

                for(int i = 0; i < humansToSpawn; i++){
                    SpawnHumans();
                    yield return new WaitForSeconds(1.5f);
                }

                yield return new WaitForSeconds(waveInterval);
            }
            else if(currentWave == totalWaves){
                yield return StartCoroutine(FinalWave());
            }
        }
    }

    private IEnumerator FinalWave(){
        float elapsedTime = 0f;
        while(elapsedTime < finalWaveDuration){
            SpawnHumans();
            yield return new WaitForSeconds(1.5f);
            elapsedTime += 1f;
        }
        while(GameObject.FindGameObjectsWithTag("ENEMY").Length != 0){
            yield return new WaitForSeconds(0.3f);
        }
            EndGame();
    }

    private void SpawnHumans()
    {
        if(spawners.Length == 0 || !isGameRunning) return;

        Transform spawnPoint = spawners[Random.Range(0, spawners.Length)];
        Instantiate(humanPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    public void EndGame(){
        GameUI.SetActive(false);
        isGameRunning = false;
        StopCoroutine(FinalWave());
        StopCoroutine(WaveSystem());

        player.GetComponent<PlayerCtrl> ().enabled = false;
        teleportPosition = new Vector3(30f, 15f, 93f); // 원하는 좌표
        teleportRotation = Quaternion.Euler(0f, 92f, 0f); // 목표 회전 (Euler로 설정)
        TeleportPlayer(teleportPosition, teleportRotation);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameObject[] humans = GameObject.FindGameObjectsWithTag("ENEMY");
        foreach(GameObject human in humans){
            Destroy(human);
        }

        if(healthBar.fillAmount != 0){
            audioSource.volume = Mathf.Clamp(0.5f, 0f, 1f);
            soundEffectPlayer.PlayEffectSound(happyEndSound);

            ChangeBGM(clearMusic);

            ClearVideo();
            //해피
        } else{
            soundEffectPlayer.PlayEffectSound(badEndSound);
            badUI.SetActive(true);
            //배드드
        }

        Invoke(nameof(StopTime), 3f);
    }

    void TeleportPlayer(Vector3 targetPosition, Quaternion targetRotation)
    {
        rb.velocity = Vector3.zero; // 속도 초기화
        rb.angularVelocity = Vector3.zero; // 회전 속도 초기화

        rb.position = targetPosition; // 위치 설정
        rb.rotation = targetRotation; // 회전 설정

        Camera.main.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }

    void StopTime(){
        Time.timeScale = 0;
        audioSource.volume = Mathf.Clamp(1f, 0f, 1f);
        currentWave = 0;
        if(healthBar.fillAmount == 0){
            endUI.SetActive(true);
        }
    }

    public void QuitGame(){
        #if UNITY_EDITOR
    EditorApplication.isPlaying = false; // 에디터에서 게임 종료
    #else
        Application.Quit(); // 빌드 후 게임 종료
    #endif
    }

    public GameObject clear;

    public void ClearVideo(){
        clear.SetActive(true);
        clear.GetComponent<VideoPlayer>().loopPointReached += ClearVideoEnd;
        clear.GetComponent<VideoPlayer>().Play();
    }

    void ClearVideoEnd(VideoPlayer vp){
        endUI.SetActive(true);
        happyUI.SetActive(true);
        clear.SetActive(false);
    }

    public void ChangeBGM(AudioClip newClip){
        backGroundMusic.Stop();
        backGroundMusic.clip = newClip;
        backGroundMusic.Play();
    }
}