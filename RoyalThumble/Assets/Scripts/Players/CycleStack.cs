using System;
using System.Collections;
using System.Collections.Generic;

namespace VirtualControls {
	
	namespace VirtualDPad {

		public class CycleStack<T> where T : IConvertible {

			private static int DEFAULT = 16;

			private T[] buffer;
			private int top, size, max;
			public int Size {
				get {
					return size;
				}
			}

			public CycleStack() {
				buffer = new T[DEFAULT];
				max = DEFAULT;
			}

			public CycleStack (int _size) {
				buffer = new T[_size];
				max = _size;
			}

			public T Peek () {
				if (size <= 0) {
					throw new IndexOutOfRangeException();
				}

				if (top <= 0) {
					return buffer [max];
				} else {
					return buffer [top - 1];
				}

			}

			public T Pop () {
				if (size <= 0) {
					throw new IndexOutOfRangeException();
				} 

				size--;
				if (top <= 0) {
					top = max;
					return buffer [top];
				} else {
					return buffer [--top];
				}
			}

			public void Push (T item) {
				buffer [top++] = item;
				if (top >= max) {
					top = 0;
				}
				if (size < max) {
					size++;
				}
			}

			public void Flush () {
				size = 0;
				top = 0;
			}
		}
	}
}