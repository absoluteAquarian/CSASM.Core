using System;
using System.Linq;

namespace CSASM.Core{
	public class CSASMStack{
		private readonly object[] stack;
		private int head;
		private readonly int capacity;

		public CSASMStack(){
			capacity = 1000;
			stack = new object[1000];
			head = 0;
		}

		public CSASMStack(int capacity){
			if(capacity <= 0)
				throw new ArgumentException("Size was too small. Expected a value greater than zero");

			this.capacity = capacity;
			stack = new object[capacity];
			head = 0;
		}

		public void Push(object obj){
			if(head >= capacity)
				throw new StackException("Stack overflow detected. Cannot push more objects to the stack.");
			stack[head] = obj;
			head++;

			if(Sandbox.reportStackUsage)
				Console.WriteLine($"[CSASM] Object pushed: {(obj is Array a ? FormatArray(a) : obj?.ToString() ?? "null")}");
		}

		public object Pop(){
			if(head <= 0)
				throw new StackException("Stack underflow detected. Cannot pop more objects from the stack.");
			object obj = stack[head - 1];
			stack[head - 1] = null;
			head--;

			if(Sandbox.reportStackUsage)
				Console.WriteLine($"[CSASM] Object popped: {(obj is Array a ? FormatArray(a) : obj?.ToString() ?? "null")}");

			return obj;
		}

		public object Peek() => head == 0 ? throw new StackException("Stack does not contain any values") : stack[head - 1];

		public static string FormatArrayElement(object elem)
			=> elem is string s
				? $"\"{s}\""
				: (elem is char c
					? $"'{c}'"
					: elem?.ToString() ?? "null");

		public static string FormatArray(Array a){
			if(a.Length == 0)
				return "[ <empty> ]";

			if(a.Length == 1)
				return $"[ {FormatArrayElement(a.GetValue(0))} ]";

			//Arrays of arrays won't be supported for the time being
			string ret = "[ ";

			for(int i = 0; i < a.Length; i++){
				if(i == 5){
					ret += ", ...";
					break;
				}

				var elem = a.GetValue(i);

				if(i > 0)
					ret += ", ";

				ret += FormatArrayElement(elem);
			}
			ret += " ]";

			return ret;
		}

		public override string ToString()
			=> head == 0
				? "[ <empty> ]"
				: "[ " + string.Join(", ", stack.Where((o, i) => i < head).Select(o =>
					o is string s
						? $"\"{s}\""
						: (o is char c
							? $"'{c}'"
							: (o is Array a
								? FormatArray(a)
								: (o?.ToString() ?? "null")))))
					+ " ]";
	}
}
