using System.Collections;
using System.Collections.Generic;
//using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCtrl : MonoBehaviour
{
    public float moveSpeed = 7f; // 이동 속도
    public float verticalSpeed = 3f; // 위/아래 이동 속도
    private Rigidbody rb;

   public float mouseSensitivity = 70f; // 마우스 민감도
    public Transform playerBody; // 플레이어의 몸체 (카메라 기준 회전할 오브젝트)
    public GameObject magicParticle; // 발사할 물체 프리팹
    public GameObject spawnablePrefab; // 생성할 물체 프리팹
    public float magicSpeed = 20f; // 발사할 힘
    public Camera playerCamera; // 플레이어의 카메라
    public LayerMask groundLayer; // 땅 레이어 지정

    private float xRotation = 0f;

    public bool isOpening = false;

    SoundEffectPlayer soundEffectPlayer;

    private bool canUseSpawnSkill = true;
    public float spawnCooldown = 3f;
    private bool canUseMagicSkill = true;
    public float magicCooldown = 1f;
    public Image spawnCooldownImage;
    public Image magicCooldownImage;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        soundEffectPlayer = GetComponent<SoundEffectPlayer>();
        Cursor.lockState = CursorLockMode.Locked; // 마우스 커서 고정

    }

    void FixedUpdate()
    {
        MovePlayer();
        HandleMouseLook(); // 마우스 시야 조작
        HandleMouseActions(); // 마우스 버튼 동작 처리
    }

    void MovePlayer(){
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        float moveY = 0f;
        if(Input.GetKey(KeyCode.Space)){
            moveY = verticalSpeed;
        }else if(Input.GetKey(KeyCode.LeftShift)){
            moveY = -verticalSpeed;
        }

        // 앞뒤/양옆 속도 조절
        Vector3 movement = transform.right * moveX + transform.forward * moveZ;
        Vector3 horizontalMovement = movement.normalized * moveSpeed;

        // Rigidbody 속도 설정 (XZ와 Y축 분리)
        rb.velocity = new Vector3(horizontalMovement.x, moveY, horizontalMovement.z);
    }

    void HandleMouseLook()
    {
        // 마우스 움직임 입력
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 카메라 상하 회전 제한
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // 카메라와 몸체 회전
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        Quaternion newRotation = Quaternion.Euler(0f, playerBody.eulerAngles.y + mouseX, 0f);
        rb.MoveRotation(newRotation); // Rigidbody를 통해 회전 적용
    }

    void HandleMouseActions()
    {
        if (Input.GetMouseButtonDown(0)) // 좌클릭
        {
            if(canUseMagicSkill){
                ShootProjectile();
                StartCoroutine(MagicCooldown());
            }
        }
        if (Input.GetMouseButtonDown(1)) // 우클릭
        {
            if(canUseSpawnSkill){
                SpawnObjectOnGround();
                StartCoroutine(SpawnCooldown());
            }
        }
    }

    void ShootProjectile()
    {
        Vector3 spawnPosition = playerCamera.transform.position + playerCamera.transform.forward * 2f;
        Vector3 forwardDirection = -playerCamera.transform.forward;

        GameObject particleInstance = Instantiate(magicParticle, spawnPosition, Quaternion.identity);

        if(particleInstance == null){
            Debug.Log("왜 없음??");
        }

        particleInstance.transform.rotation = Quaternion.LookRotation(forwardDirection);

        ParticleMover mover = particleInstance.AddComponent<ParticleMover>();
        mover.SetDirection(forwardDirection, magicSpeed, 5f);
    }

    IEnumerator UpdateParticlePosition(Rigidbody rb, Transform particleTransform){
        while(rb != null){
            particleTransform.position = rb.position;
            particleTransform.rotation = rb.rotation;

            yield return null;
        }
    }

    IEnumerator SpawnCooldown(){
        canUseSpawnSkill = false;
        float delayTime = 0f;

        while (delayTime < spawnCooldown){
            delayTime += Time.deltaTime;
            if(spawnCooldownImage != null){
                spawnCooldownImage.fillAmount = 1 - (delayTime/spawnCooldown);
            }
            yield return null;
        }
        if(spawnCooldownImage != null){
            spawnCooldownImage.fillAmount = 0f;
        }
        canUseSpawnSkill = true;
    }

    IEnumerator MagicCooldown(){
        canUseMagicSkill = false;
        float delayTime = 0f;

        while (delayTime < magicCooldown){
            delayTime += Time.deltaTime;
            if(magicCooldownImage != null){
                magicCooldownImage.fillAmount = 1 - (delayTime/magicCooldown);
            }
            yield return null;
        }
        if(magicCooldownImage != null){
            magicCooldownImage.fillAmount = 0f;
        }
        canUseMagicSkill = true;
    }

    void SpawnObjectOnGround()
    {
        // 마우스가 가리키는 땅의 위치를 찾음
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            Vector3 spawnPosition = new Vector3(hit.point.x, hit.point.y - 3f, hit.point.z);
            
            Quaternion playerRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

            // 물체 생성
            Instantiate(spawnablePrefab, spawnPosition, playerRotation);
        }
    }
}
