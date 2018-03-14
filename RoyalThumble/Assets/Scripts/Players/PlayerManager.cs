using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualControls;
using VirtualControls.VirtualDPad;

[RequireComponent(typeof(CommandInterpreter))]
public class PlayerManager : PlayerMovement {

	public float commandAccelerationSquared = 0.01f;

	public DirectionalAnalogEvent NewInput { get; private set; }
	public DirectionalAnalogEvent PreviousInput { get; private set; }

	private CommandInterpreter cmdIntrp;

	protected override void Awake ()
	{
		base.Awake ();
		NewInput = DirectionalAnalogEvent.Empty();
		PreviousInput = DirectionalAnalogEvent.Empty();

		cmdIntrp = GetComponent<CommandInterpreter> ();
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		DirectionalAnalogEvent newInput = NewInput;
		if (!newInput.IsEmpty) {
			if (PreviousInput.IsEmpty ||  (newInput.Velocity - PreviousInput.Velocity).sqrMagnitude >= commandAccelerationSquared) {
				cmdIntrp.AddToInterpreter (newInput);
				//print (newInput.Region);
			} else {
				Move (newInput.Velocity);
			}
		} else {
			if (!PreviousInput.IsEmpty) {
				print (cmdIntrp.Interpret ());
				cmdIntrp.Flush ();
				BeginRecenter();
			}
		}
		PreviousInput = newInput;
	}

	//Receives new input down for the manager
	void OnInputDown(DirectionalAnalogEvent input) {
		NewInput = input;
	}

	//Receives new input up for the manager
	void OnInputUp() {
		NewInput = DirectionalAnalogEvent.Empty ();
	}
}
