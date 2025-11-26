using UnityEngine;

public class GuardObjectHealth : MonoBehaviour, IDamageable
{
    [SerializeField] float maxHP = 200f;
    [SerializeField] HPBar hpBar;

    float _current;
    bool _destroyed;

    void Awake()
    {
        _current = maxHP;
        if (hpBar == null)
            hpBar = GetComponentInChildren<HPBar>();

        if (hpBar != null)
        {
            hpBar.SetMaxHP(maxHP);
            hpBar.SetHP(maxHP, _current);
        }
    }

    public void TakeDamage(float amount)
    {
        if (_destroyed)
            return;

        _current = Mathf.Max(0f, _current - amount);
        if (hpBar != null)
            hpBar.SetHP(maxHP, _current);

        if (_current <= 0f)
            OnDestroyed();
    }

    void OnDestroyed()
    {
        _destroyed = true;
        Destroy(gameObject);
    }
}
