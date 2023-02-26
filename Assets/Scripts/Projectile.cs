using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10.0f;



    void Start()
    {
        speed = this.speed;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * speed *Time.deltaTime);
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }
}
