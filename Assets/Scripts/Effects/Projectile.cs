using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f; // 투사체 이동 속도
    public float impactDestroyDelay = 0.5f; // 목표 도달 후 파괴까지의 지연 시간

    private Vector3 targetPosition; // 목표 월드 위치
    private bool hasTarget = false; // 목표가 설정되었는지 여부

    /// <summary>
    /// 투사체를 초기화하고 목표 위치를 설정합니다.
    /// </summary>
    /// <param name="target">투사체가 도달할 월드 위치</param>
    public void Initialize(Vector3 target)
    {
        targetPosition = target;
        hasTarget = true;
    }

    void Update()
    {
        if (!hasTarget)
        {
            return;
        }

        // 목표를 향해 이동
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // 목표에 도달했는지 확인
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f) // 충분히 가까워지면 도달로 간주
        {
            OnImpact();
        }
    }

    /// <summary>
    /// 투사체가 목표에 도달했을 때 호출됩니다.
    /// </summary>
    private void OnImpact()
    {
        hasTarget = false; // 더 이상 이동하지 않도록 설정
        // TODO: 여기에 충돌 효과 (파티클, 사운드 등) 추가
        
        // 잠시 후 투사체 오브젝트 파괴
        Destroy(gameObject, impactDestroyDelay);
    }

    // 투사체의 시작 위치를 설정하는 오버로드 (선택 사항)
    public void SetStartPosition(Vector3 start)
    {
        transform.position = start;
    }
}
