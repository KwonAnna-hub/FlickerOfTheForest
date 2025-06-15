using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.VisualScripting;


public class EnemyAg2 : Agent
{
    public Transform treeTrans; // 나무 객체
    public GameObject player; // 플레이어 객체
    public Camera assignedCamera; //카메라라
    private Transform playerTrans;
    private Transform agentTrans;
    private Rigidbody agent_Rigidbody;
    Vector3 treeInitPos;

    public float RotationSpeed = 5f;
    private float lastDistanceToTree;

    public float moveSpeed = 5f;

    public float gravity = -9.18f;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundLayer;
    Vector3 velocity;
    bool isGrounded;

    private bool isCollidingWithTree = false;

    public override void Initialize(){//스타트함수와 거의 유사
        Debug.Log("Agent Initialized");
        agentTrans = gameObject.transform;
        playerTrans = player.transform;
        treeInitPos = GetTreeCenter();
        Academy.Instance.AgentPreStep += WaitTimeInference;

        agent_Rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("Episode Started");
        // 에이전트의 초기화
        transform.position = GetRandomPositionAroundTree();
        transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        lastDistanceToTree = CalculateFlatDistance(transform.position, GetTreeCenter());
    }

    private Vector3 GetTreeCenter(){
        if(treeTrans == null){
            return Vector3.zero;
        }

        Renderer[] renderers = treeTrans.GetComponentsInChildren<Renderer>();
        if(renderers.Length > 0){
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers) {
                bounds.Encapsulate(renderer.bounds);
            }
            return bounds.center; // 실제 트리모델의 중심
        }

        return treeTrans.position;
    }

    //평면 거리 계산 메서드
    private float CalculateFlatDistance(Vector3 pos1, Vector3 pos2){
        Vector3 flatPos1 = new Vector3(pos1.x, 0, pos1.z);
        Vector3 flatPos2 = new Vector3(pos2.x, 0, pos2.z);
        return Vector3.Distance(flatPos1, flatPos2);
    }

    private Vector3 GetRandomPositionAroundTree(){
        float minDistance = 15f;
        float maxDistance = 17f;

        //구 표면에서 랜덤 방향 선택
        Vector3 randomDirection = Random.onUnitSphere;
        randomDirection.y = 0; //평면에서만 랜덤 방향 생성

        //랜덤 거리를 생성 (minDistance ~ maxDistance 범위)
        float randomDistance = Random.Range(minDistance, maxDistance);

        //나무의 위치에서 랜덤 방향과 거리를 적용
        Vector3 randomPosition = GetTreeCenter() + randomDirection * randomDistance;

        RaycastHit hit;
        if(Physics.Raycast(randomPosition + Vector3.up * 20f, Vector3.down, out hit, Mathf.Infinity, groundLayer)){
            randomPosition.y = hit.point.y + 0.2f;
        }else{
            //높이를 나무와 동일하게 맞춤
            randomPosition.y = GetTreeCenter().y - 10f;
        }

        return randomPosition;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 나무 중심의 상대 위치
        Vector3 relativePosition = GetTreeCenter() - transform.position;
        sensor.AddObservation(relativePosition / 20f); // 나무 중심과의 거리 정규화

        // 에이전트의 현재 위치
        sensor.AddObservation(transform.position / 20f);

        // 에이전트의 전방 방향
        Vector3 agentForward = transform.forward.normalized;
        sensor.AddObservation(agentForward); // 에이전트의 전방 벡터

        // 플레이어 시선과 에이전트 위치의 관계
        Vector3 playerToAgent = (transform.position - playerTrans.position).normalized;
        Vector3 cameraForward = assignedCamera.transform.forward.normalized;
        float dotProduct = Vector3.Dot(playerToAgent, cameraForward);
        sensor.AddObservation(dotProduct); // 시선 내에서의 상대적 위치 (float)

        // 나무와 에이전트의 거리
        float distanceToTree = CalculateFlatDistance(transform.position, GetTreeCenter());
        sensor.AddObservation(distanceToTree / 20f); // 나무와의 거리 정규화

        // 5. 충돌 여부
        sensor.AddObservation(isCollidingWithTree ? 1.0f : 0.0f); // 충돌 여부 (float: 1이면 충돌, 0이면 미충돌)
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
            // 중력 적용 및 이동 처리
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;

        var actions = actionBuffers.ContinuousActions;
        float moveForward = Mathf.Clamp(actions[0], -1f, 1f);
        float moveRight = Mathf.Clamp(actions[1], -1f, 1f);

        Vector3 moveDirection = transform.forward * moveForward + transform.right * moveRight;
        agent_Rigidbody.MovePosition(transform.position + moveDirection * moveSpeed * Time.deltaTime);

        // 1. 목표 위치(나무 중심)와의 거리 계산
         float distanceToTree = CalculateFlatDistance(transform.position, GetTreeCenter());
        float treeReward = Mathf.Clamp01(1.0f - distanceToTree / 20f); // 0~20 거리 기준 정규화
        AddReward(treeReward * 0.5f); // 나무에 가까워질수록 보상

        // 2. 플레이어 시선에서 멀어지는 정도 계산
        Vector3 playerToAgent = (transform.position - playerTrans.position).normalized;
        Vector3 cameraForward = assignedCamera.transform.forward.normalized; // 플레이어 카메라의 시선
        float dotProduct = Vector3.Dot(playerToAgent, cameraForward);
        float visibilityReward = Mathf.Clamp01(-dotProduct); // 플레이어 시선 밖에 있을수록 보상

        float cosThetaThreshold = Mathf.Cos(5 * Mathf.Deg2Rad); // 15도를 라디안으로 변환

        AddReward(visibilityReward * 0.3f); // 시선에서 멀어질수록 보상

        // 에피소드 종료 조건
        if (distanceToTree > 20f) // 나무에서 너무 멀거나 시선 안에 있을 때
        {
            SetReward(-1f); // 패널티
            EndEpisode();
        }

        if(dotProduct > cosThetaThreshold){
            AddReward(-0.5f);
            EndEpisode();
        }

        if (distanceToTree < 3.8f) // 나무에 도달했을 때
        {
            SetReward(1f); // 성공 보상
            EndEpisode();
        }
    }

    private void OnCollisionEnter(Collision collision) {
        foreach (Collider childCollider in treeTrans.GetComponentsInChildren<Collider>()){
            if(collision.collider == childCollider){
                isCollidingWithTree = true;
                SetReward(1f);
                EndEpisode();
                break;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        foreach (Collider childCollider in treeTrans.GetComponentsInChildren<Collider>())
        {
            if (collision.collider == childCollider)
            {
                isCollidingWithTree = false; // 충돌 상태 해제
                break;
            }
        }
    }

    //플레이어 시선 위치 계산
    private Vector3 GetPlayerLineOfSight(){
        RaycastHit hit;
        Ray ray = new Ray(playerTrans.position, playerTrans.forward);//플레이어의 시선 방향으로 레이 캐스트
        if(Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer)) //groundLayer에 닿는 위치를 찾음
        {
            return hit.point;
        }

        return playerTrans.position + playerTrans.forward * 10f;
    }

    public void RotateAgentMovement(Vector3 moveDirection){
        if(moveDirection != Vector3.zero){
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut){//사람이 직접 조종하는 함수, in -> 참조형식으로 전달
        var continuousActionsOut = actionsOut.ContinuousActions;

        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }

private void OnDrawGizmos()
{
    if (assignedCamera != null)
    {
        Vector3 cameraForward = assignedCamera.transform.forward.normalized;
        //Vector3 playerPosition = playerTrans.position;

        // 시선 방향 표시
        Gizmos.color = Color.green;
        //Gizmos.DrawLine(playerPosition, playerPosition + cameraForward * 10f);

        // 에이전트와 시선 충돌 여부 확인
        Vector3 agentPosition = transform.position;
        Gizmos.color = Color.red;
        //Gizmos.DrawLine(playerPosition, agentPosition);
    }
}

    public float DecisionWaitingTime = 0.01f;
    float m_currentTime = 0f;

    public void WaitTimeInference(int action){
        if(Academy.Instance.IsCommunicatorOn){
            RequestDecision();
        }else{
            if(m_currentTime < DecisionWaitingTime){
                m_currentTime = 0f;
                RequestDecision();
            }else{
                m_currentTime += Time.fixedDeltaTime;
            }
        }
    }
}
