using UnityEngine;

public interface IDamageable
{
    void TakeHit(float damage, Collision collision);
    void TakeDamage(float damage);
}
