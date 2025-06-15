using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEx : MonoBehaviour
{
    public string targetTag = "ENEMY"; // 감지할 태그
    public string treeTag = "TREE"; // TREE 태그
    public float rotationSpeed = 2f; // 회전 속도
    public float switchDistance = 10f; // 나무에서 멀어지면 타겟 변경 거리

    private GameObject currentTarget; // 현재 바라보고 있는 물체
    private GameObject treeObject; // 나무 오브젝트

    void Start()
    {
        // 트리 오브젝트 찾기 (게임 시작 시 한 번만)
        treeObject = GameObject.FindWithTag(treeTag);
        if (treeObject == null)
        {
            Debug.LogError("TREE 태그를 가진 오브젝트가 존재하지 않습니다.");
        }
    }

    void Update()
    {
        if(currentTarget == null){
            UpdateTargetToClosestToTree();
        }

        if (currentTarget != null)
        {
            SmoothLookAt(currentTarget.transform.position);
        }
    }

    private Vector3 GetTreeCenter(){
        if(treeObject.transform == null){
            return Vector3.zero;
        }

        Renderer[] renderers = treeObject.transform.GetComponentsInChildren<Renderer>();
        if(renderers.Length > 0){
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers) {
                bounds.Encapsulate(renderer.bounds);
            }
            return bounds.center; // 실제 트리모델의 중심
        }

        return treeObject.transform.position;
    }

    /// <summary>
    /// 나무와 가장 가까운 타겟을 선택합니다.
    /// </summary>
    private void UpdateTargetToClosestToTree()
    {
            if (treeObject == null) return;

        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(targetTag);
        if (taggedObjects.Length == 0)
        {
           currentTarget = null;
           return;
        }

        GameObject closestToTree = null; // 여기에서 초기화
        float closestDistance = Mathf.Infinity;

        foreach (GameObject obj in taggedObjects)
        {
            float distance = HorizontalDistance(treeObject.transform.position, obj.transform.position);
           if (distance < closestDistance)
            {
                closestDistance = distance;
               closestToTree = obj; // 가장 가까운 객체 저장
            }
        }

        // 가장 가까운 타겟을 업데이트
        currentTarget = closestToTree;

        // 나무에서 멀어졌을 경우 타겟 초기화
        if (currentTarget != null && HorizontalDistance(treeObject.transform.position, currentTarget.transform.position) > switchDistance)
        {
            currentTarget = null;
        }
    }

    /// <summary>
    /// Y축을 무시하고 두 위치 간의 수평 거리를 계산합니다.
    /// </summary>
    private float HorizontalDistance(Vector3 pos1, Vector3 pos2)
    {
        Vector3 pos1Flat = new Vector3(pos1.x, 0, pos1.z);
        Vector3 pos2Flat = new Vector3(pos2.x, 0, pos2.z);
        return Vector3.Distance(pos1Flat, pos2Flat);
    }

    /// <summary>
    /// 부드럽게 특정 방향을 바라보도록 회전합니다.
    /// </summary>
    private void SmoothLookAt(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
