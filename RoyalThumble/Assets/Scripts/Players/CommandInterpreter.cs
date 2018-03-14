using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualControls {
	
	namespace VirtualDPad {

		public enum ActionTypes {
			Tap = 0x0001,
			Counter = 0x0002,
			Jab = 0x0004,
			Pull = 0x0008,
			Swipe = 0x0010,

			QuarterSweep = 0x0020,
			HalfSweep = 0x0040,
			FullSweep = 0x0080,

			QuarterAngle = 0x0100,
			HalfAngle = 0x0200,
			FullAngle = 0x0400,

			QuarterLoop = 0x0800,
			HalfLoop = 0x1000,
			FullLoop = 0x2000,

			FigureL = 0x400,
			FigureZ = 0x800,

			None = 0x0000
		}

		public class ActionInterpretedEvent : EventArgs {
			public ActionTypes Action { get; set; }
			public Vector2 EndVelocity { get; set; }
		}

		public class CommandInterpreter : MonoBehaviour {
			//Keys going from: latest <- earliest
			//n - no input
			//d - directional region
			//c - center region
			//1 thru 7 - 
			public static string TapKey = "nnnnnnnnnd";
			public static string CounterKey = "nnnnnnnnnc";
			public static string JabKey = "nnnnnnnndc";
			public static string PullKey = "nnnnnnnncd";
			public static string SwipeKey = "nnnnnnn4cd";
			public static string QuarterSweepKey = "nnnnnnx11d";
			public static string HalfSweepKey = "nnnxx1111d";
			public static string FullSweepKey = "xx1111111d";
			public static string QuarterAngleKey = "nnnnnx11dc";
			public static string HalfAngleKey = "nnxx1111dc";
			public static string FullAngleKey = "n1111111dc";
			public static string QuarterLoopKey = "nnnnnc11dc";
			public static string HalfLoopKey = "nnnc1111dc";
			public static string FullLoopKey = "c1111111dc";
			public static string FigureLKey = "nnnnnnn2cd";
			public static string FigureZKey = "nnn774c11d";

			public static Dictionary<string,ActionTypes> interpretDict = new Dictionary<string, ActionTypes> (16);

			[Tooltip("The time it takes for an action to time out")]
			public float actionTimeout = 0.2f;
			[Tooltip("A mask that identifies what actions can be interpreted")]
			public int actionMask = 0xFFFF;
			[Tooltip("The time it takes for a hold action to be interpreted")]
			public float holdTime = 0.0f;
		
			private bool heldAction = false;
			private CommandRegion lastDirectionalRegion = CommandRegion.None;
			private char[] commandString = {'n','n','n','n','n','n','n','n','n','n'};
			private int commandSize = 0;
			private int commandDirection = 0; //1 if clockwise and -1 if counter clockwise

			private DirectionalAnalogEvent lastInput;
			public DirectionalAnalogEvent LastInput {
				private set {
					LastUpdate = Time.time;
					lastInput = value;
				}
				get { 
					return lastInput;
				}
			}
				
			public float LastUpdate {
				private set;
				get;
			}

			public bool IsHeld { private set; get; }

			void Awake () {
				lastInput = DirectionalAnalogEvent.Empty ();

				interpretDict.Add (TapKey, ActionTypes.Tap);
				interpretDict.Add (CounterKey, ActionTypes.Counter);
				interpretDict.Add (JabKey, ActionTypes.Jab);
				interpretDict.Add (PullKey, ActionTypes.Pull);
				interpretDict.Add (SwipeKey, ActionTypes.Swipe);
				interpretDict.Add (QuarterSweepKey, ActionTypes.QuarterSweep);
				interpretDict.Add (HalfSweepKey, ActionTypes.HalfSweep);
				interpretDict.Add (FullSweepKey, ActionTypes.FullSweep);
				interpretDict.Add (QuarterAngleKey, ActionTypes.QuarterAngle);
				interpretDict.Add (HalfAngleKey, ActionTypes.HalfAngle);
				interpretDict.Add (FullAngleKey, ActionTypes.FullAngle);
				interpretDict.Add (QuarterLoopKey, ActionTypes.QuarterLoop);
				interpretDict.Add (HalfLoopKey, ActionTypes.HalfLoop);
				interpretDict.Add (FullLoopKey, ActionTypes.FullLoop);
				interpretDict.Add (FigureLKey, ActionTypes.FigureL);
				interpretDict.Add (FigureZKey, ActionTypes.FigureZ);
			}

			void Update () {
				if (Time.time > LastUpdate + actionTimeout) {
					Flush ();
				}
			}

			public void AddToInterpreter(DirectionalAnalogEvent newInput) {
				DirectionalAnalogEvent lastInput = LastInput;
				if (newInput.Region == CommandRegion.None) {
					Flush ();
				} else if (newInput.Region != lastInput.Region) {
					//print (newInput.Region);
					LastInput = newInput;
					InterpretStep (newInput.Region);
				}
			}

			private void InterpretStep(CommandRegion reg) {
				if (commandSize > 9) {
					Flush ();
				}

				if (reg == CommandRegion.Center) {
					commandString [commandSize] = 'c';
				} else if (reg == CommandRegion.None) {
					commandString [commandSize] = 'n';
				} else if (lastDirectionalRegion == CommandRegion.None) {
					commandString [commandSize] = 'd';
					lastDirectionalRegion = reg;
				} else if (commandDirection == 0) {
					int delta = (int)reg - (int)lastDirectionalRegion;
					//commandDirection = delta > 0 || delta == -7 ? 1 : -1;
					//delta = delta == commandDirection * -7 ? 1 : delta;
					//commandString [commandSize] = Math.Abs (delta).ToString () [0];
					//lastDirectionalRegion = reg;


					lastDirectionalRegion = reg;
				} else {
					int delta = commandDirection * ((int)reg - (int)lastDirectionalRegion);
					//delta = delta < 0 ? delta + 8 : delta;
					if (delta == commandDirection * -7) {
						delta = 1;
					} else if (delta < 0) {
						delta += 8;
					}
					commandString [commandSize] = Math.Abs (delta).ToString () [0];
					lastDirectionalRegion = reg;
				}
				commandSize++;
			}

			public void Flush(){
				commandString = new char[] {'n','n','n','n','n','n','n','n','n','n'};
				commandSize = 0;
				commandDirection = 0;
				lastDirectionalRegion = CommandRegion.None;
				LastInput = DirectionalAnalogEvent.Empty ();
			}

			public ActionTypes Interpret() {
				string str = new string (commandString);
				print (str);
				if (interpretDict.ContainsKey (str)) {
					return interpretDict [str];
				} else {
					return ActionTypes.None;
				}
			}

			public ActionInterpretedEvent RichInterpret() {
				return new ActionInterpretedEvent () {
					Action = Interpret (),
					EndVelocity = LastInput.Velocity
				};
			}

//			public ActionTypes Interpret() {
//				ActionTypes action = ActionTypes.None;
//				if (cycleStack.Size > 0) {
//					CommandRegion current = cycleStack.Peek ();
//					if (current == CommandRegion.Center) {
//						action = (ActionTypes)((int)CounterIntrp () & actionMask);
//					} else {
//						action = (ActionTypes)((int)TapIntrp () & actionMask);
//					}
//				} 
//				cycleStack.Flush ();
//				return action;
//			}

//			private ActionTypes TapIntrp() {
//				if (cycleStack.Size > 2) {
//					CommandRegion current = cycleStack.Pop ();
//					CommandRegion before = cycleStack.Pop ();
//					CommandRegion before_before = cycleStack.Peek ();
//
//					if (before == CommandRegion.Center) {
//						if (DirectionalCommands.ClockwiseCast (current, before_before, 4)) {
//							return ActionTypes.Swipe;
//						} else if (DirectionalCommands.ClockwiseCast (current, before_before, 2)) {
//							return ActionTypes.FigureL;
//						} else {
//							return ActionTypes.Pull;
//						}
//					} else if (DirectionalCommands.ClockwiseSequence (before, current) && DirectionalCommands.ClockwiseSequence (before_before, before)) {
//						return QuarterSweepIntrp (true);
//					} else if (DirectionalCommands.CounterClockwiseSequence (before, current) && DirectionalCommands.CounterClockwiseSequence (before_before, before)) {
//						return QuarterSweepIntrp (false);
//					} else {
//						return ActionTypes.Tap;
//					}
//				} else {
//					return ActionTypes.Tap;
//				}
//			}
//
//			private ActionTypes QuarterSweepIntrp(bool dir) {
//				if (cycleStack.Size > 2) {
//					CommandRegion current = cycleStack.Pop ();
//					CommandRegion before = cycleStack.Pop ();
//					CommandRegion before_before = cycleStack.Peek ();
//
//					if ((dir && DirectionalCommands.ClockwiseSequence (before, current) && DirectionalCommands.ClockwiseSequence (before_before, before)) ||
//					    (!dir && DirectionalCommands.CounterClockwiseSequence (before, current) && DirectionalCommands.CounterClockwiseSequence (before_before, before))) {
//						return HalfSweepIntrp (dir);
//					} else if (cycleStack.Size > 4 && DirectionalCommands.LineCast (current, before, before_before)) {
//						current = cycleStack.Pop ();
//						before = cycleStack.Pop ();
//						before_before = cycleStack.Peek ();
//
//						if ((dir && DirectionalCommands.CounterClockwiseSequence (before, current) && DirectionalCommands.CounterClockwiseSequence (before_before, before)) ||
//						    (!dir && DirectionalCommands.ClockwiseSequence (before, current) && DirectionalCommands.ClockwiseSequence (before_before, before))) {
//							return ActionTypes.FigureZ;
//						} 
//					}
//					return ActionTypes.QuarterSweep;
//				} else {
//					return ActionTypes.QuarterSweep;
//				}
//			}
//
//			private ActionTypes HalfSweepIntrp(bool dir) {
//				if (cycleStack.Size > 3) {
//					CommandRegion current = cycleStack.Pop ();
//					CommandRegion before = cycleStack.Pop ();
//					CommandRegion before_before = cycleStack.Pop ();
//					CommandRegion before_before_before = cycleStack.Peek ();
//
//					if ((dir && DirectionalCommands.ClockwiseSequence (before, current) && DirectionalCommands.ClockwiseSequence (before_before, before) && DirectionalCommands.ClockwiseSequence (before_before_before, before_before)) ||
//					    (!dir && DirectionalCommands.CounterClockwiseSequence (before, current) && DirectionalCommands.CounterClockwiseSequence (before_before, before) && DirectionalCommands.CounterClockwiseSequence (before_before_before, before_before))) {
//						return ActionTypes.FullSweep;
//					} else {
//						return ActionTypes.HalfSweep;
//					}
//				} else {
//					return ActionTypes.HalfSweep;
//				}
//			}
//
//			private ActionTypes CounterIntrp() {
//				if (cycleStack.Size > 0) {
//					cycleStack.Pop ();
//					return JabIntrp ();
//				} else {
//					return ActionTypes.Counter;
//				}
//			}
//
//			private ActionTypes JabIntrp() {
//				if (cycleStack.Size > 2) {
//					CommandRegion current = cycleStack.Pop ();
//					CommandRegion before = cycleStack.Pop ();
//					CommandRegion before_before = cycleStack.Peek ();
//
//					if (DirectionalCommands.ClockwiseSequence (before, current) && DirectionalCommands.ClockwiseSequence (before_before, before)) {
//						return QuarterAngleIntrp (true);
//					} else if (DirectionalCommands.CounterClockwiseSequence (before, current) && DirectionalCommands.CounterClockwiseSequence (before_before, before)) {
//						return QuarterAngleIntrp (false);
//					} else {
//						return ActionTypes.Jab;
//					}
//				} else {
//					return ActionTypes.Jab;
//				}
//			}
//				
//			private ActionTypes QuarterAngleIntrp (bool dir) {
//				if (cycleStack.Size > 0) {
//					CommandRegion current = cycleStack.Pop ();
//
//					if (current == CommandRegion.Center) {
//						return ActionTypes.QuarterLoop;
//					} else if (cycleStack.Size > 1) {
//						CommandRegion before = cycleStack.Pop ();
//						CommandRegion before_before = cycleStack.Peek ();
//
//						if ((dir && DirectionalCommands.ClockwiseSequence (before, current) && DirectionalCommands.ClockwiseSequence (before_before, before)) ||
//						    (!dir && DirectionalCommands.CounterClockwiseSequence (before, current) && DirectionalCommands.CounterClockwiseSequence (before_before, before))) {
//							return HalfAngleIntrp (dir);
//						}
//					} 
//					return ActionTypes.QuarterAngle;
//				} else {
//					return ActionTypes.QuarterAngle;
//				}
//			}
//
//			private ActionTypes HalfAngleIntrp (bool dir) {
//				if (cycleStack.Size > 0) {
//					CommandRegion current = cycleStack.Pop ();
//
//					if (current == CommandRegion.Center) {
//						return ActionTypes.HalfLoop;
//					} else if (cycleStack.Size > 1) {
//						CommandRegion before = cycleStack.Pop ();
//						CommandRegion before_before = cycleStack.Pop ();
//						CommandRegion before_before_before = cycleStack.Peek ();
//
//						if ((dir && DirectionalCommands.ClockwiseSequence (before, current) && DirectionalCommands.ClockwiseSequence (before_before, before) && DirectionalCommands.ClockwiseSequence(before_before_before,before_before)) ||
//							(!dir && DirectionalCommands.CounterClockwiseSequence (before, current) && DirectionalCommands.CounterClockwiseSequence (before_before, before) && DirectionalCommands.CounterClockwiseSequence(before_before_before,before_before))) {
//							return FullAngleIntrp (dir);
//						}
//					} 
//					return ActionTypes.HalfAngle;
//				} else {
//					return ActionTypes.HalfAngle;
//				}
//			}
//
//			private ActionTypes FullAngleIntrp (bool dir) {
//				if (cycleStack.Size > 0) {
//					CommandRegion current = cycleStack.Pop ();
//
//					if (current == CommandRegion.Center) {
//						return ActionTypes.FullLoop;
//					}
//				}
//				return ActionTypes.FullAngle;
//			}
		}
	}
}