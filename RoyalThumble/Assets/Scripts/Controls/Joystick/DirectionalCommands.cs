using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualControls {

	namespace VirtualDPad {

		public enum CommandRegion
		{
			Center = 0,
			Up = 1,
			UpRight = 2,
			Right = 3,
			DownRight = 4,
			Down = 5,
			DownLeft = 6,
			Left = 7,
			UpLeft = 8,
			None = 9
		}

		public abstract class DirectionalCommands {

			public static bool IsCenterCommand(CommandRegion reg) {
				return (reg == CommandRegion.Center);
			}

			public static bool IsDirectionCommand(CommandRegion reg) {
				return (reg != CommandRegion.Center && reg != CommandRegion.None);
			}

			//Check if the regions are a clockwise sequence
			public static bool ClockwiseSequence(CommandRegion start, CommandRegion end) {
				int dif = (int)end - (int)start;
				return IsDirectionCommand(start) && IsDirectionCommand(end) && (dif == 1 || dif == -7);
			}

			//Chek if the regions are a counter-clockwise sequence
			public static bool CounterClockwiseSequence(CommandRegion start, CommandRegion end) {
				int dif = (int)end - (int)start;
				return IsDirectionCommand (start) && IsDirectionCommand (end) && (dif == -1 || dif == 7);
			}

			public static bool ClockwiseCast (CommandRegion start, CommandRegion end, int count) {
				int dif = (int)end - (int)start;
				return IsDirectionCommand (start) && IsDirectionCommand (end) && (Math.Abs (dif) == count || Math.Abs(dif) == 8 - count); 
			}

			public static bool LineCast (CommandRegion start, CommandRegion middle, CommandRegion end) {
				if (middle != CommandRegion.Center) {
					return false;
				} else {
					return ClockwiseCast (start, end, 4);
				}
			}
		}

		public class DirectionalEvent : EventArgs {
			public CommandRegion Region {
				get;
				set;
			}

			public static DirectionalEvent Empty() {
				return new DirectionalEvent() { Region = CommandRegion.None };
			}
		}

		public class DirectionalAnalogEvent : EventArgs {
			public Vector2 Velocity {
				get;
				set;
			}

			public CommandRegion Region {
				get;
				set;
			}

			public bool IsEmpty {
				get { return Region == CommandRegion.None; }
			}
				
			public static DirectionalAnalogEvent Empty() {
				return new DirectionalAnalogEvent () { Region = CommandRegion.None, Velocity = Vector2.zero };
			}

			public static bool operator== (DirectionalAnalogEvent in1, DirectionalAnalogEvent in2) {
				return (in1.Velocity == in2.Velocity && in1.Region == in2.Region);
			}

			public static bool operator!= (DirectionalAnalogEvent in1, DirectionalAnalogEvent in2) {
				return !(in1 == in2);
			}

			public static bool CloseEnough (DirectionalAnalogEvent in1, DirectionalAnalogEvent in2, float delta) {
				return (Vector2.Distance (in1.Velocity, in2.Velocity) < delta && 
							in1.Region == in2.Region);
			}

		}
	}
}
