using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10.0f;
    //public LayerMask collisionMask;

    void Update()
    {
        float moveDistance = Time.deltaTime * speed;
        //CheckCollisions(moveDistance);
        transform.Translate(Vector3.forward * moveDistance);
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }

    public void OnTriggerEnter(Collider other)
    {
        GameObject.Destroy(gameObject);
    }

   /* Another technique is to use raycasting to see if a ray + move distance will hit the enemy's collision layer. --> alternative to Continuous Dynamic
    * 
    * public void CheckCollisions(float moveDistance)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, moveDistance, collisionMask, QueryTriggerInteraction.Collide)) 
        {
            OnHitObject(hit);
        }
    }

    public void OnHitObject(RaycastHit hit)
    {
        Debug.Log("Hit an enemy: " + hit.collider.gameObject.name);
        GameObject.Destroy(gameObject);
    }*/
}
