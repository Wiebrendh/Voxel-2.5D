using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class Movement : MonoBehaviour 
{

	public float speed;
	public float jump;
	public float gravity;
	public float sprintMultiplier;
	private Vector3 moveDirection = Vector3.zero;

	CharacterController controller;

	void Start ()
	{
		controller = GetComponent<CharacterController>();
	}

	void Update () 
	{
		if (controller.isGrounded) 
		{
			moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, 0);
			moveDirection = transform.TransformDirection(moveDirection);
			moveDirection *= speed;

			if (Input.GetKey(KeyCode.LeftShift))
				moveDirection.x *= sprintMultiplier;

			if (Input.GetButton("Jump"))
				moveDirection.y = jump;				
		}

		moveDirection.y -= gravity * Time.deltaTime;
		controller.Move(moveDirection * Time.deltaTime);
	}
	
}
