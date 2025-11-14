using UnityEngine;
using UnityEngine.AI;
using Valve.VR.InteractionSystem;

public class EnemyDetect : MonoBehaviour
{
    EnemyDataManager dataManager; 
    [SerializeField] EnemyData _enemyData; // detectRange를 포함한 _enemyData
    [SerializeField] Transform _player; // Inspector 주입 권장

    public Transform Target{ get; private set; } // 읽기 전용 노출. set은 private으로 두어 수정 접근 제한.


    void Start()
    {
        dataManager = GetComponent<EnemyDataManager>();
        _enemyData = dataManager._enemyData;
        
        if (_player == null)
        {
            var go = GameObject.FindWithTag("Player");
            Target = go.transform;
        }
        else
        {
            Target = GameObject.FindWithTag("Player").transform;
        }
    }
    

    public bool IsPlayerInDetectRange()
{
    float chaseRange = _enemyData.DetectRange; // 더 넓은 탐지 범위
    return Vector3.Distance(Target.position, transform.position) < chaseRange;
}

    public bool IsPlayerInAttackRange()
{
    float attackRange = _enemyData.AttackRange; // 근접 공격 거리
    return Vector3.Distance(Target.position, transform.position) < attackRange;
}


}