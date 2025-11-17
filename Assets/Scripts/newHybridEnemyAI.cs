using UnityEngine;
using UnityEngine.AI;

// Hybrid AI: Shoots at range, melee when close
public class newHybridEnemyAI : MonoBehaviour
{
    public Transform player;
    public NavMeshAgent agent;

    [Header("NPC 애니메이터")]
    [SerializeField] private Animator anim;

    [Header("인지 범위 속성")]
    [SerializeField] private float detectRange;
    [SerializeField] private float meleeRange;
    [SerializeField] private float shootRange;

    void Start(){
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    private void Update(){
        float distance = Vector3.Distance(transform.position, player.position);

        if(distance <= detectRange){
            agent.isStopped = false;
            Debug.Log("플레이어 거리 내 존재.");
            agent.SetDestination(player.position);   
        }
        
        if(distance > detectRange){
            agent.isStopped = true;
            Debug.Log("플레이어 추격 종료");
        }

        
    }
}

