using System;
using System.Text;

namespace CSASM.Core{
#pragma warning disable IDE1006
	public static class Ops{
		public static CSASMStack stack;
		public static object _reg_a, _reg_1, _reg_2, _reg_3, _reg_4, _reg_5;

		private static byte flags;

		public static bool Carry{
			get => (flags & 0x01) != 0;
			set => flags = (byte)(value ? flags | 0x01 : flags & ~0x01);
		}

		public static bool Comparison{
			get => (flags & 0x02) != 0;
			set => flags = (byte)(value ? flags | 0x02 : flags & ~0x02);
		}

		private static void CheckVerbose(string instruction, bool beginningOfInstr){
			if(Sandbox.verbose){
				Console.WriteLine($"[CSASM] Stack at {(beginningOfInstr ? "beginning" : "end")} of instruction \"{instruction}\":" +
					$"\n   {stack}");
			}
		}

		public static void func_abs(){
			CheckVerbose("abs", true);

			object o = stack.Pop();

			if(o is IPrimitive ip){
				stack.Push(ip.Abs());
				return;
			}

			throw new StackException("abs", o);
		}

		public static void func_add(){
			CheckVerbose("add", true);
			
			object second = stack.Pop();
			object first = stack.Pop();

			if(first is null || second is null)
				throw new StackException("add", first, second);

			if(first is IPrimitive ip && second is IPrimitive ip2)
				stack.Push(ip.Add(ip2));
			else if(first is string || second is string){
				//Strings will concat with the other argument
				if(first is string s)
					stack.Push(s + second.ToString());
				else if(second is string s2)
					stack.Push(first.ToString() + s2);
			}else
				throw new StackException("add", first, second);
		}

		public static void func_asl(){
			CheckVerbose("asl", true);

			object obj = stack.Pop();
			if(obj is IPrimitiveInteger ip)
				stack.Push(ip.ArithmeticShiftLeft());
			else
				throw new StackException("asl", obj);
		}

		public static void func_asr(){
			CheckVerbose("asr", true);

			object obj = stack.Pop();
			if(obj is IPrimitiveInteger ip)
				stack.Push(ip.ArithmeticShiftRight());
			else
				throw new StackException("asr", obj);
		}

		public static void func_bytes(){
			CheckVerbose("bytes", true);

			object obj = stack.Pop();

			stack.Push(new IntPrimitive(obj switch{
				string s =>          s.Length,
				char _ =>            1,
				BytePrimitive _ =>   1,
				SbytePrimitive _ =>  1,
				UshortPrimitive _ => 2,
				ShortPrimitive _ =>  2,
				UintPrimitive _ =>   4,
				IntPrimitive _ =>    4,
				UlongPrimitive _ =>  8,
				LongPrimitive _ =>   8,
				FloatPrimitive _ =>  4,
				DoublePrimitive _ => 8,
				Array arr =>         ArrayBytes(arr),
				null => throw new StackException("bytes", obj),
				_ => throw new StackException("bytes", obj)
			}));
		}

		private static int ArrayBytes(Array arr){
			if(arr.Length == 0)
				return 0;

			int bytes = 0;
			Type elemType = arr.GetType().GetElementType();
			if(elemType == typeof(string) || elemType == typeof(object)){
				for(int i = 0; i < arr.Length; i++){
					stack.Push(arr.GetValue(i));
					func_bytes();
					bytes += (int)((IPrimitive)stack.Pop()).Value;
				}
			}else{
				//All elements have the same size
				stack.Push(arr.GetValue(0));
				func_bytes();
				bytes = (int)((IPrimitive)stack.Pop()).Value * arr.Length;
			}

			return bytes;
		}

		public static void func_comp(){
			CheckVerbose("comp", true);

			object obj2 = stack.Pop();
			object obj = stack.Pop();

			if(Sandbox.verbose)
				Console.WriteLine($"[CSASM] \"comp\" instruction had types \"{(obj == null ? "null" : Utility.GetCSASMType(obj.GetType()))}\"" +
					$" and \"{(obj2 == null ? "null" : Utility.GetCSASMType(obj2.GetType()))}\"");

			if(obj == null || obj2 == null){
				//Null checks
				if(obj == null && obj2 == null)
					Comparison = true;
			}else if(obj is IPrimitive ip && obj2 is IPrimitive ip2){
				//Primitives comparison
				if(Utility.AreEqual(ip, ip2))
					Comparison = true;
			}else if(obj is char c && obj2 is char c2){
				//Char comparison
				if(c == c2)
					Comparison = true;
			}else if(obj is string s && obj2 is string s2){
				//String comparison
				if(s == s2)
					Comparison = true;
			}else{
				Type t = obj.GetType();
				Type t2 = obj2.GetType();

				if(t.IsArray && t2.IsArray && t.GetElementType() == t2.GetElementType()){
					Array first = obj as Array;
					Array second = obj as Array;
					if(first.Length == second.Length){
						bool allSame = true;

						for(int i = 0; i < first.Length; i++)
							if(first.GetValue(i) != second.GetValue(i))
								allSame = false;

						if(allSame)
							Comparison = true;
					}
				}else
					throw new StackException("comp", obj, obj2);
			}
		}

		public static void func_comp_gt(){
			CheckVerbose("comp.gt", true);

			object obj2 = stack.Pop();
			object obj = stack.Pop();

			if(obj is IPrimitive ip && obj2 is IPrimitive ip2){
				if(ip.CompareTo(ip2) > 0)
					Comparison = true;
			}else
				throw new StackException("comp.gt", obj, obj2);
		}

		public static void func_comp_lt(){
			CheckVerbose("comp.lt", true);

			object obj2 = stack.Pop();
			object obj = stack.Pop();

			if(obj is IPrimitive ip && obj2 is IPrimitive ip2){
				if(ip.CompareTo(ip2) < 0)
					Comparison = true;
			}else
				throw new StackException("comp.lt", obj, obj2);
		}

		public static void func_conv(string type){
			CheckVerbose("conv", true);

			object obj = stack.Pop();
			if(type == "char"){
				//Only conversions from i32 -> char are allowed for the time being
				if(obj is IntPrimitive intprim)
					stack.Push((char)(int)((IPrimitive)intprim).Value);
				else
					throw new StackException("conv", obj);
			}else if(type == "str"){
				//Strings can convert directly to and from char arrays only
				//Other types will just call their ToString() methods
				if(obj is char[] letters)
					stack.Push(new string(letters));
				else if(obj is Array arr)
					stack.Push(CSASMStack.FormatArray(arr));
				else
					stack.Push(obj.ToString());
			}else if(type == "~arr:char"){
				if(obj is string s)
					stack.Push(s.ToCharArray());
				else
					throw new StackException("conv", obj);
			}else if(type.StartsWith("~arr:") && obj.GetType().IsArray)
				throw new StackException("Array instances cannot be converted using the \"conv\" operator");
			else if(obj is IPrimitiveInteger){
				IPrimitive ip = obj as IPrimitive;
				object value = ip.Value;
				try{
					Type newType = Utility.GetCsharpType(type);
					IPrimitive prim = Utility.CreatePrimitive(newType, value);
					stack.Push(prim);
				}catch(InvalidCastException){
					throw new StackException("conv", obj);
				}catch(ThrowException tex){
					throw tex;
				}catch{
					throw new StackException("conv", obj);
				}
			}

			CheckVerbose("conv", false);
		}

		public static void func_conv_a(string type){
			stack.Push(_reg_a);
			try{
				func_conv(type);
			}catch(Exception ex){
				if(ex.Message.Contains("\"conv\""))
					ex = new ThrowException(ex.Message.Replace("\"conv\"", "\"conv.a\""), ex.InnerException);
				throw ex;
			}
			_reg_a = stack.Pop();
		}

		public static void func_dec(){
			object obj = stack.Pop();
			if(obj is IPrimitive ip)
				stack.Push(ip.Decrement());
			else
				throw new StackException("dec", obj);
		}

		public static void func_div(){
			CheckVerbose("div", true);
			
			object second = stack.Pop();
			object first = stack.Pop();

			if(first is IPrimitive ip && second is IPrimitive ip2)
				stack.Push(ip.Divide(ip2));
			else if(first is string s){
				if(second is string s2)
					stack.Push(s.Split(new string[]{ s2 }, StringSplitOptions.RemoveEmptyEntries));
				else if(second is char c)
					stack.Push(s.Split(new char[]{ c }, StringSplitOptions.None));
				else
					throw new StackException("div", first, second);
			}else
				throw new StackException("div", first, second);
		}

		public static void func_dup(){
			object obj = stack.Peek();
			if(obj is string s)
				stack.Push(s.Clone());
			else if(obj is Array a)
				stack.Push(a.Clone());
			else
				stack.Push(obj);
		}

		public static void func_inc(){
			object obj = stack.Pop();
			if(obj is IPrimitive ip)
				stack.Push(ip.Increment());
			else
				throw new StackException("inc", obj);
		}

		public static void func_interp(string interp){
			CheckVerbose("interp", true);

			object obj2 = stack.Pop();
			if(!(obj2 is object[] args))
				throw new StackException("interp", obj2);

			//Perform the interpolation
			stack.Push(string.Format(interp, args));
		}

		public static void func_is(string type){
			CheckVerbose("is", true);

			object obj = stack.Pop();
			Type checkType = Utility.GetCsharpType(type);

			if(obj.GetType() == checkType)
				Comparison = true;
		}

		public static void func_is_a(string type){
			Type checkType = Utility.GetCsharpType(type);

			if(_reg_a.GetType() == checkType)
				Comparison = true;
		}

		public static void func_ldelem(int index){
			CheckVerbose("ldelem", true);
			if(Sandbox.verbose)
				Console.WriteLine($"[CSASM] Index passed into instruction \"ldelem\": {index}");

			object arr = stack.Pop();

			if(!arr.GetType().IsArray)
				throw new StackException("ldelem", arr);

			Array array = arr as Array;

			if(index >= array.Length)
				throw new StackException($"Index ({index}) was outside the range of valid entries in the array (0..{array.Length - 1})");

			stack.Push(array.GetValue(index));
		}

		public static void func_ldelem(Indexer indexer){
			object arr = stack.Pop();

			if(!arr.GetType().IsArray)
				throw new StackException("ldelem", arr);

			int offset = indexer.GetIndex(arr);
			stack.Push(arr);

			func_ldelem(offset);
		}

		public static void func_len(){
			object arr = stack.Pop();

			if(!arr.GetType().IsArray)
				throw new StackException("len", arr);

			stack.Push(new IntPrimitive((arr as Array).Length));
		}

		public static void func_mul(){
			CheckVerbose("mul", true);
			
			object second = stack.Pop();
			object first = stack.Pop();

			ulong? count = null;
			long? sCount = null;
			if(first is IPrimitive ip && second is IPrimitive ip2)
				stack.Push(ip.Multiply(ip2));
			else if(first is string s && ((second is IUnsignedPrimitiveInteger && (count = Utility.AsUInteger((second as IPrimitive).Value)) != null) || (second is IPrimitiveInteger && (sCount = Utility.AsInteger((second as IPrimitive).Value)) != null))){
				//If a string and and unsigned integer are on the stack, duplicate the string that many times
				//Multiplying by zero will convert it to an empty string
				int repetitions = 0;
				if(sCount is long l){
					if(l < 0)
						throw new StackException("Instruction \"mul\" must be given a positive integer when multiplying strings.");
					else if(l >= int.MaxValue)
						throw new StackException($"Instruction \"mul\" must be given a value between 0 and {int.MaxValue - 1}, inclusive, when multiplying strings.");
					else
						repetitions = (int)l;
				}else if(count is ulong u){
					if(u >= int.MaxValue)
						throw new StackException($"Instruction \"mul\" must be given a value between 0 and {int.MaxValue - 1}, inclusive, when multiplying strings.");
					else
						repetitions = (int)u;
				}

				if((long)s.Length * repetitions >= int.MaxValue)
					throw new StackException("Could not allocate enough memory for the \"mul\" instruction.");

				if(repetitions == 0)
					stack.Push(string.Empty);
				else if(repetitions == 1)
					stack.Push(s);
				else{
					StringBuilder sb = new StringBuilder(s.Length * repetitions);
					for(int i = 0; i < repetitions; i++)
						sb.Append(s);
					stack.Push(sb.ToString());
				}
			}else
				throw new StackException("mul", first, second);
		}

		public static void func_neg(){
			CheckVerbose("neg", true);

			object obj = stack.Pop();

			if(obj is IPrimitive ip)
				stack.Push(ip.Negate());
			else
				throw new StackException("neg", obj);
		}

		public static void func_not(){
			CheckVerbose("not", true);

			object obj = stack.Pop();

			if(obj is IPrimitiveInteger ip)
				stack.Push(ip.BitwiseNot());
			else
				throw new StackException("not", obj);
		}

		public static void func_print(){
			CheckVerbose("print", true);

			object obj = stack.Pop();
			if(obj is Array a)
				Console.Write(CSASMStack.FormatArray(a));
			else
				Console.Write(obj?.ToString() ?? "null");
		}

		public static void func_print_n(){
			CheckVerbose("print.n", true);

			object obj = stack.Pop();
			if(obj is Array a)
				Console.WriteLine(CSASMStack.FormatArray(a));
			else
				Console.WriteLine(obj?.ToString() ?? "null");
		}

		public static void func_rol(){
			CheckVerbose("rol", true);

			object obj = stack.Pop();

			if(obj is IPrimitiveInteger ip)
				stack.Push(ip.ArithmeticRotateLeft(ref flags));
			else
				throw new StackException("rol", obj);
		}

		public static void func_ror(){
			CheckVerbose("ror", true);

			object obj = stack.Pop();

			if(obj is IPrimitiveInteger ip)
				stack.Push(ip.ArithmeticRotateRight(ref flags));
			else
				throw new StackException("ror", obj);
		}

		public static void func_stelem(int index){
			CheckVerbose("stelem", true);
			if(Sandbox.verbose)
				Console.WriteLine($"[CSASM] Index passed into instruction \"stelem\": {index}");

			object obj = stack.Pop();
			object arr = stack.Pop();

			Type type = obj.GetType();
			Type arrType = arr.GetType();

			if(!arrType.IsArray)
				throw new StackException("stelem", arr);
			
			Type arrElemType = arrType.GetElementType();

			if(type != arrElemType && !arrElemType.IsAssignableFrom(type))
				throw new StackException($"Instruction \"stelem\" had invalid values on the stack before it ({type.Name}, {arrType.Name})");

			Array array = arr as Array;

			if(index >= array.Length)
				throw new StackException($"Index ({index}) was outside the range of valid entries in the array (0..{array.Length - 1})");

			array.SetValue(obj, index);
		}

		public static void func_stelem(Indexer indexer){
			object obj = stack.Pop();
			object arr = stack.Pop();

			Type type = obj.GetType();
			Type arrType = arr.GetType();

			if(!arrType.IsArray)
				throw new StackException("stelem", arr);
			
			Type arrElemType = arrType.GetElementType();

			if(type != arrElemType && !arrElemType.IsAssignableFrom(type))
				throw new StackException($"Instruction \"stelem\" had invalid values on the stack before it ({type.Name}, {arrType.Name})");

			int offset = indexer.GetIndex(arr);
			stack.Push(arr);
			stack.Push(obj);

			func_stelem(offset);
		}

		public static void func_sub(){
			CheckVerbose("sub", true);
			
			object second = stack.Pop();
			object first = stack.Pop();

			if(first is IPrimitive ip && second is IPrimitive ip2)
				stack.Push(ip.Subtract(ip2));
			else if(first is string s){
				if(second is string s2){
					//Using "sub" with two strings will remove all instances of the second from the first
					stack.Push(s.Replace(s2, string.Empty));
				}else if(second is char c)
					stack.Push(s.Replace(c.ToString(), string.Empty));
				else
					throw new StackException("Instruction \"sub\" must have a <str> or <char> value as the second argument when manipulating strings.");
			}else
				throw new StackException("sub", first, second);
		}

		public static void func_throw(string message){
			throw new ThrowException(message);
		}

		public static void func_type(){
			CheckVerbose("type", true);

			object obj = stack.Pop();
			if(obj == null)
				throw new StackException("type", obj);

			stack.Push(Utility.GetCSASMType(obj.GetType()));
		}
	}
}
