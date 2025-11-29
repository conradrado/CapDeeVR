using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{

    public string Name;

    [TextArea]
    public string Description;
    
    public float MaxHP;

    public float Damage;

    public float PatrolSpeed;
    public float ChaseSpeed = 2f;
    
    public float DetectRange = 10f;   // 플레이어/오브젝트 탐지 범위
    public float AttackRange = 2f;

    public float AttackSpeed = 4f;

    // If true, this enemy should never use melee and should rely on ranged attacks.
    public bool IsRangedOnly = false;

    [Header("Rewards")]
    public int GoldReward = 10;

    public enum TargetPriority
    {
        PlayerFirst,
        ObjectFirst
    }

    [Header("Targeting")]
    public TargetPriority Priority = TargetPriority.PlayerFirst;
    [Tooltip("Optional override: if > 0, use this range to detect defendable objects. Otherwise DetectRange is used.")]
    public float ObjectDetectRange = -1f;
    
}
