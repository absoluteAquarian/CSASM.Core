using System;
using System.Linq;

namespace CSASM.Core{
	public class CSASMStack{
		private readonly object[] stack;
		public int Head{ get; private set; }
		public int sp;
		private readonly int capacity;

		public CSASMStack(){
			capacity = 1000;
			stack = new object[1000];
			Head = 0;
			sp = 0;
		}

		public CSASMStack(int capacity){
			if(capacity <= 0)
				throw new ArgumentException("Size was too small. Expected a value greater than zero");

			this.capacity = capacity;
			stack = new object[capacity];
			Head = 0;
			sp = 0;
		}

		public void Push(object obj){
			if(Head >= capacity)
				throw new StackException("Stack overflow detected. Cannot push more objects to the stack.");

			if(sp == Head){
				//Locations are in sync.  Just replace the topmost element
				stack[Head] = obj;
				sp++;
				Head++;
			}else if(sp < Head){
				//Locations are not in sync.  Shift everything above $sp and below $head (everything else past $head is just null anyway)
				object[] arr = new object[Head - sp];
				Array.Copy(stack, sp, arr, 0, arr.Length);

				stack[sp] = obj;
				sp++;
				Head++;

				Array.Copy(arr, 0, stack, sp, arr.Length);
			}else
				throw new StackException($"Stack pointer was in an invalid location ($sp: {sp}, $head: {Head})");

			if(Sandbox.reportStackUsage){
				Sandbox.verboseWriter.Flush();

				Sandbox.verboseWriter.WriteLine($"[STACK] Object pushed: {FormatObject(obj)}");
			}
		}

		//Unused for now until I feel like actually implementing it properly
		public void PushIndirect(object obj, int spOffset){
			//Shift "sp", push the thing, then shift "sp" back
			int oldSP = sp;
			sp += spOffset;

			if(sp > Head)
				sp = Head;
			if(sp < 0)
				throw new StackException($"Stack pointer was in an invalid location ($sp: {sp}, $head: {Head})");

			Push(obj);

			if(spOffset < 0)
				sp = oldSP + 1;
		}

		public object Pop(){
			if(Head <= 0)
				throw new StackException("Stack underflow detected. Cannot pop more objects from the stack.");

			object obj;
			if(sp == Head){
				//Locations are in sync.  Get the topmost element and null its index
				obj = stack[Head - 1];
				stack[Head - 1] = null;
				sp--;
				Head--;
			}else if(sp < Head && sp > 0){
				obj = stack[sp - 1];
				sp--;
				Head--;

				object[] arr = new object[Head - sp];
				Array.Copy(stack, sp, arr, 0, arr.Length);

				Array.Copy(arr, 0, stack, sp, arr.Length);

				//Null the extra copy at the end
				stack[Head + 1] = null;
			}else
				throw new StackException($"Stack pointer was in an invalid location ($sp: {sp}, $head: {Head})");

			if(Sandbox.reportStackUsage){
				Sandbox.verboseWriter.Flush();

				Sandbox.verboseWriter.WriteLine($"[STACK] Object popped: {FormatObject(obj)}");
			}

			return obj;
		}

		//Unused for now until I feel like actually implementing it properly
		public object PopIndirect(int spOffset){
			//Shift "sp", pop the thing, then shift "sp" back
			int oldSP = sp;
			sp += spOffset;

			if(sp <= 0)
				throw new StackException($"Stack pointer was in an invalid location ($sp: {sp}, $head: {Head})");
			if(sp > Head)
				sp = Head;

			object obj = Pop();

			if(spOffset < 0)
				sp = oldSP - 1;

			return obj;
		}

		public object Peek() => Head == 0 ? throw new StackException("Stack does not contain any values") : stack[Head - 1];

		public static string FormatArray(Array a){
			if(a.Length == 0)
				return "[ <empty> ]";

			if(a.Length == 1)
				return $"[ {FormatObject(a.GetValue(0))} ]";

			//Arrays of arrays won't be supported for the time being
			string ret = "[ ";

			for(int i = 0; i < a.Length; i++){
				var elem = a.GetValue(i);

				if(i > 0)
					ret += ", ";

				ret += FormatObject(elem);
			}
			ret += " ]";

			return ret;
		}

		public override string ToString()
			=> Head == 0
				? "[ <empty> ]"
				: "[ " + string.Join(", ", stack.Where((o, i) => i < Head).Select(o => FormatObject(o))) + " ]";

		public static string FormatObject(object o)
			=> o is string s
				? $"\"{s}\""
				: (o is char c
					? $"'{c}'"
					: (o is Array a
						? FormatArray(a)
						: (o?.ToString() ?? "null")));
	}
}
