using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : MonoBehaviour
{
    public float speed = 5.0f;
    public Vector3 moveInput;
    public Camera camera;
    public Gun defaultGun;
    public GunController gunController;
    private PlayerController playerController;

    void Start()
    {
        moveInput = new Vector3();
        camera = Camera.main;
        playerController = GetComponent<PlayerController>();
        gunController= GetComponent<GunController>();
    }

    void FixedUpdate()
    {
        moveInput.Set(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveInput = moveInput.normalized * speed * Time.deltaTime;
        playerController.Move(moveInput);

        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
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
