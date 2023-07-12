using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable
{
    public float startingHealth;
    protected float health;
    protected bool alive;

    public event System.Action OnDeath;

    protected virtual void Start()
    {
        health = startingHealth;
        alive = true;
    }

    public void TakeHit(float damage, Collision hit)
    {
        TakeDamage(damage);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0 && alive)
        {
            Die();
        }
    }

    protected void Die()
    {
        alive = false;
        health = 0;
        if (OnDeath!= null)
        {
            OnDeath();
        }
        GameObject.Destroy(gameObject);
    }



    
}
