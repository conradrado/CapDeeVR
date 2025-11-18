using UnityEngine;


public abstract class StatBehaviour : MonoBehaviour {
    
    public float MaxHP = 0f;
    public float CurrentHP = 0f;


    public abstract void Die();

    public abstract void TakeDamage(float damage);

    public abstract void HealDamage(float healAmount);


}