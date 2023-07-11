using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable
{
    public float startingHealth;
    protected float health;
    protected bool alive;

    protected virtual void Start()
    {
        health = startingHealth;
        alive = true;
    }

    public void TakeHit(float damage, Collider hit)
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
        GameObject.Destroy(gameObject);
    }



    
}
