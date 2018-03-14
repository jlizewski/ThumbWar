using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameGenerics;

public class PlayerMovement : MovingObject {
	//For single stick controlled 2D player object with grace radius and angle

	[Tooltip("The camera for this object")]
	public Camera playerCamera;
	[Tooltip("Maximum angular movement speed in ")]
	public float maxAngularSpeed = 10.0f;
	[Tooltip("Movement radius that won't move the camera")]
	public float graceRadius = 1.0f;
	[Tooltip("Rotation angle that won't move the camera")]
	public float graceAngle = 30.0f;
	[Tooltip("The inverse time it takes the camera to recenter")]
	public float inverseRecenteringTime = 2.0f;

	private int colliderMask = 0;
	private Transform parentTransform;
	private Quaternion rotator;

	public bool IsRecentering { get; private set; }

	virtual protected void Awake() {
		colliderMask = 1 << LayerMask.NameToLayer ("Physical");
	}

	//Movement that considers collisions and has a grace radius where the camera won't move
	protected override void Move (Vector2 control)
	{		
		//If currently recentering, stop
		StopRecenter ();

		float rotation = -(maxAngularSpeed * Time.deltaTime * control.x);
		transform.rotation = transform.rotation * Quaternion.Euler (0.0f, 0.0f, rotation);

		//Find starting and ending points
		Vector2 start = transform.position;
		Vector2 end = start + (maxSpeed * Time.deltaTime * (Vector2)transform.TransformDirection(control));

		//Check for collition
		RaycastHit2D hit = Physics2D.Linecast (start, end, colliderMask);
		if (hit.collider != null) {
			end = hit.point;
		}
		//Change position
		transform.position = new Vector3(end.x,end.y,transform.position.z);

		//Keep camera within a distance of the player 
		float distance = Vector2.Distance ((Vector2)playerCamera.transform.position, end);
		if (distance > graceRadius) {
			//Move camera towards player
			Vector2 correction = Vector2.MoveTowards (playerCamera.transform.position, end, 
				(distance - graceRadius) / distance);
			playerCamera.transform.position = new Vector3 (correction.x, correction.y, playerCamera.transform.position.z);

		}

		//Keep camera within a set of degrees of the player
		float angle = Quaternion.Angle (playerCamera.transform.rotation, transform.rotation);
		if (angle > graceAngle) {
			//Move camera so the player is only 
			playerCamera.transform.rotation = Quaternion.Slerp (playerCamera.transform.rotation, transform.rotation, (angle - graceAngle) / angle);
		}
	}

	public void BeginRecenter() {
		if (!IsRecentering) {
			IsRecentering = true;
			StartCoroutine ("RecenterRoutine", (object)playerCamera);
		}
	}

	public void StopRecenter() {
		IsRecentering = false;
	}

	private IEnumerator RecenterRoutine(object obj) {
		Camera cam = (Camera)obj;

		if (cam != null) {
			float distance = ((Vector2)cam.transform.position - (Vector2)transform.position).sqrMagnitude;
			float angleRate = Quaternion.Angle (cam.transform.rotation, transform.rotation) / distance;

			while (IsRecentering && distance >= Mathf.Epsilon) {
				float step = inverseRecenteringTime * Time.deltaTime;
				Vector2 move = Vector2.MoveTowards (cam.transform.position, transform.position, step);

				cam.transform.position = new Vector3 (move.x, move.y, cam.transform.position.z);
				cam.transform.rotation = Quaternion.RotateTowards (cam.transform.rotation, transform.rotation, step * angleRate);

				distance = ((Vector2)cam.transform.position - (Vector2)transform.position).sqrMagnitude;
				yield return null;
			}

			IsRecentering = false;
			//cam.transform.position = new Vector3 (transform.position.x,transform.position.y,cam.transform.position.z);
		}
	}
}
