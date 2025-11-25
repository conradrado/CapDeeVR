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
    
    public float DetectRange = 10f;   // 플레이어를 “탐지”하는 범위
    public float AttackRange = 2f;

    public float AttackSpeed = 4f;

    // If true, this enemy should never use melee and should rely on ranged attacks.
    public bool IsRangedOnly = false;
    
}
