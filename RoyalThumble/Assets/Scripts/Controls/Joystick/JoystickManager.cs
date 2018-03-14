using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualControls.VirtualDPad;

namespace VirtualControls {

	public abstract class JoystickManager : MonoBehaviour {


		[Tooltip("The radius in units that nub can be drawn out")]
		public float controllerRadius = 2.0f;
		[Tooltip("The percent of the radius you can overdraw the nub")]
		public float overdraw = 1.5f;
		private float overdrawRadius = 0.0f;
		[Tooltip("The radius of the center region")]
		public float centerRadius = 1.0f;

		public GameObject nubObject;
		private Transform nubTransform;
		private Vector2 nubOrigin;

		public GameObject primaryClient;

		virtual protected void Awake () {
			if (nubObject != null) {
				nubTransform = nubObject.transform;
				nubOrigin = (Vector2)nubTransform.position;
			}
			if (overdraw < 1.0f) {
				overdraw = 1.0f;
			}
			overdrawRadius = overdraw * controllerRadius;
		}

		virtual protected bool GetInput(out Vector2 vel) {
			#if UNITY_EDITOR
			if (Input.GetMouseButton(0)) {
				//Vector3 velocity = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				Vector2 velocity = (Vector2)Input.mousePosition;
				Vector2 dif = (velocity - nubOrigin);
				float magnitude =  dif.magnitude;

				if (magnitude <= controllerRadius) {
					//Valid input
					//Move nub to new position
					nubTransform.position = velocity;

					vel = dif/controllerRadius;
					return true;
				} else if (magnitude <= overdrawRadius) {
					//Valid input
					//Compensate for overdraw
					float angle = Mathf.Atan2(dif.y,dif.x);
					dif.x = controllerRadius * Mathf.Cos(angle);
					dif.y = controllerRadius * Mathf.Sin(angle);
					nubTransform.position = dif + nubOrigin;

					vel = dif/controllerRadius;
					return true;
				} else {
					//Invalid input
					nubTransform.position = nubOrigin;

					vel = Vector2.zero;
					return false;
				}
			} else {
				nubTransform.position = nubOrigin;

				vel = Vector2.zero;
				return false;
			}
			#elif UNITY_IOS
			return false;
			#elif UNITY_ANDROID
			return false;
			#endif
		}

		virtual protected bool GetInput(out Vector2 vel, out CommandRegion reg) {
			bool state = GetInput (out vel);
			if (state) {
				reg = GetRegion (vel);
			} else {
				reg = CommandRegion.None;
			}
			return state;
		}

		virtual protected  CommandRegion GetRegion(Vector2 vel) {
			if (vel.sqrMagnitude <= centerRadius) {
				return CommandRegion.Center;
			} else {
				float angle = Mathf.Atan2 (vel.y, vel.x) * Mathf.Rad2Deg;
				angle = angle > 180.0f ? 360.0f - angle : angle;

				if (112.5 >= angle && angle >= 67.5) {
					return CommandRegion.Up;
				} else if (67.5 > angle && angle > 22.5) {
					return CommandRegion.UpRight;
				} else if (22.5 >= angle && angle >= -22.5) {
					return CommandRegion.Right;
				} else if (-22.5 > angle && angle > -67.5) {
					return CommandRegion.DownRight;
				} else if (-67.5 >= angle && angle >= -112.5) {
					return CommandRegion.Down;
				} else if (-112.5 > angle && angle > -157.5) {
					return CommandRegion.DownLeft;
				} else if (-157.5 >= angle && angle >= 157.5) {
					return CommandRegion.Left;
				} else {
					return CommandRegion.UpLeft;
				}
			}
		}

		virtual protected void SendInputDown(Vector2 velocity) {
			primaryClient.SendMessage ("OnInputDown", velocity, SendMessageOptions.DontRequireReceiver);
		}

		virtual protected void SendInputDown(Vector2 velocity, CommandRegion region) {
			primaryClient.SendMessage ("OnInputDown", new DirectionalAnalogEvent (){ 
				Velocity = velocity, 
				Region = region
				}, SendMessageOptions.DontRequireReceiver);
		}

		virtual protected void SendInputUp(){
			primaryClient.SendMessage ("OnInputUp", SendMessageOptions.DontRequireReceiver);
		}
		
	} 
}
