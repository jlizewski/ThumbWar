using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameGenerics {

	public abstract class MovingObject : MonoBehaviour {

		[Tooltip("The maximum speed the object can move in units per second")]
		public float maxSpeed = 2.0f;

		virtual protected void Move(Vector2 control) {
			Vector2 start = transform.position;
			Vector2 end = start + (maxSpeed * Time.deltaTime * control);

			Move (start, end);
		}

		virtual protected void Move(Vector2 start, Vector2 end) {
			RaycastHit2D hit = Physics2D.Linecast (start, end);

			if (hit.collider != null) {
				end = hit.point;
			}

			transform.position = end;
		}

	}

}
