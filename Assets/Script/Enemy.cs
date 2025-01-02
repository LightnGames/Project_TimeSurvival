using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    private Animator _animator;

    private int _health = 5;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void Damage(int damageAmount)
    {
        if (IsDead())
        {
            return;
        }

        _health = Mathf.Max(_health - damageAmount, 0);
        if (_health == 0)
        {
            _animator.SetTrigger("Dead");
            return;
        }

        _animator.SetTrigger("Damage");
    }

    public bool IsDead()
    {
        return _health == 0;
    }
}
