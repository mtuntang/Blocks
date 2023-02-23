using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody playerRigidBody;
    public Vector3 movementVelocity;


    void Start()
    {
        playerRigidBody= GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        playerRigidBody.MovePosition(playerRigidBody.position + movementVelocity);
    }

    public void Move(Vector3 movementDirection)
    {
        movementVelocity = movementDirection;
    }

    public void LookAt(Vector3 lookPoint)
    {
        //Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        // heightCorrectedPoint ensures the player doesn't lean, but the leaning effect looks good though
        transform.LookAt(lookPoint);
    }
}
