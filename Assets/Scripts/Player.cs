using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{
    public float speed = 5.0f;
    public Vector3 moveInput;
    public Camera viewCamera;
    public Gun defaultGun;
    public GunController gunController;
    private PlayerController playerController;

    protected override void Start()
    {
        base.Start();
        moveInput = new Vector3();
        viewCamera = Camera.main;
        playerController = GetComponent<PlayerController>();
        gunController= GetComponent<GunController>();
    }

    void Update()
    {
        moveInput.Set(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveInput = moveInput.normalized * speed * Time.deltaTime;
        playerController.Move(moveInput);

        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            Debug.DrawLine(ray.origin, point, Color.red);
            //Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
            playerController.LookAt(point);
        }

        if (Input.GetMouseButtonDown(0)) 
        {
            gunController.Shoot();
        }
    }

   
}
