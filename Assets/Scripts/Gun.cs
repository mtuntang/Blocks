using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform muzzleLocation;
    public Projectile bullet;
    public float shotInterval;
    public float muzzleVelocity;
    private float nextShotTime;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Shoot()
    {
        if (Time.time > nextShotTime) 
        {
            nextShotTime = Time.time + shotInterval / 1000;
            Projectile newProjectile = Instantiate(bullet, muzzleLocation.transform.position, muzzleLocation.transform.rotation);
            newProjectile.SetSpeed(muzzleVelocity);
        }
        

    }
}
