using System.Linq;
using UnityEngine;

public interface IDamageable
{
    public abstract void Damage(int damageAmount);
}

public class DamageableCollider : MonoBehaviour
{
    [SerializeField] private Transform _parent;

    public void Damage(int damageAmount)
    {
        IDamageable damageable = _parent.GetComponent<IDamageable>();
        damageable.Damage(damageAmount);
    }
}
