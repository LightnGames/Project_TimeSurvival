using System.Linq;
using UnityEngine;

public interface IDamageable
{
    public abstract void Damage(int damageAmount, Transform damageSource);
}

public class DamageableCollider : MonoBehaviour
{
    public enum DamagePartType
    {
        Default,
        Critical
    }

    [SerializeField] private Transform _parent;
    [SerializeField] private DamagePartType _partType;

    public void Damage(int damageAmount, Transform damageSource)
    {
        IDamageable damageable = _parent.GetComponent<IDamageable>();
        damageable.Damage(damageAmount, damageSource);
    }

    public DamagePartType PartType { get { return _partType; } }
}
