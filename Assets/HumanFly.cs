using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanFly : MonoBehaviour
{
    public float knockbackForce = 5f; // 뒤로 날아가는 힘
    public float knockbackDuration = 3f; // 뒤로 날아가는 시간
    public float fallDelay = 3f; // 아래로 떨어지는 시간 지연

    private EnemyAgents enemyAgents;
    public Transform rotate;
    private Rigidbody rb; // Rigidbody 참조

    public AudioClip audioClip;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        enemyAgents = GetComponent<EnemyAgents>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody가 이 오브젝트에 없습니다. 추가해 주세요!");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Rock과 충돌 시 처리
        if (collision.gameObject.CompareTag("Rock"))
        {
            Debug.Log("Human이 Rock과 충돌!");

            enemyAgents.PlayEffectSound(audioClip);

            StartCoroutine(HandleKnockback(collision));
        }

        if (collision.gameObject.CompareTag("Magic")){
            enemyAgents.enabled = false;
            rb.useGravity = false;
        }
    }

    private IEnumerator HandleKnockback(Collision collision)
    {

        enemyAgents.enabled = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = false;
        // 충돌 지점에서 방향 계산
        Vector3 knockbackDirection = -rotate.forward.normalized;

        Vector3 upwardForce = Vector3.up * 0.1f;

        // Knockback 이동 거리
        float knockbackDistance = 10f;

        // 목표 위치 계산 (x, z만 이동)
        Vector3 targetPosition = transform.position + (knockbackDirection * knockbackDistance);
        targetPosition.y = transform.position.y; // y좌표는 그대로 유지

        // 이동 시간 설정
        float moveDuration = 2f;
        float elapsedTime = 0f;

        // 부드러운 이동 (x, z만 처리)
        while (elapsedTime < moveDuration)
        {
            Vector3 currentPosition = transform.position;

            // y 좌표 유지, x와 z는 Lerp로 부드럽게 이동
            Vector3 newPosition = Vector3.Lerp(currentPosition, targetPosition, elapsedTime / moveDuration);
            newPosition.y = currentPosition.y; // y 좌표는 그대로

            transform.position = newPosition;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 최종적으로 정확한 목표 위치로 설정 (x, z)
        transform.position = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);

        // 중력 다시 활성화
        rb.useGravity = true;

        // Knockback 지속 시간 대기
        yield return new WaitForSeconds(fallDelay);

        // 오브젝트 삭제
        Destroy(gameObject);
    }
}

