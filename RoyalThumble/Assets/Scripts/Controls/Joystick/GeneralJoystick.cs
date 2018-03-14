using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualControls.VirtualDPad;

namespace VirtualControls {

	public class GeneralJoystick : JoystickManager {

		// Use this for initialization
		void Start () {
			
		}
		
		// Update is called once per frame
		void Update () {
			Vector2 vel;
			CommandRegion reg;
			if (GetInput (out vel, out reg)) {
				SendInputDown (vel, reg);
			} else {
				SendInputUp ();
			}
		}
	}
}
