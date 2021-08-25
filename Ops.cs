using System;
using System.Text;
using System.Threading;

namespace CSASM.Core{
#pragma warning disable IDE1006
	public static class Ops{
		public static CSASMStack stack;
		public static object _reg_a, _reg_1, _reg_2, _reg_3, _reg_4, _reg_5;

		public static string[] args;

		private static byte flags;

		public static bool Carry{
			get => (flags & 0x01) != 0;
			set => flags = (byte)(value ? flags | 0x01 : flags & ~0x01);
		}

		public static bool Comparison{
			get => (flags & 0x02) != 0;
			set => flags = (byte)(value ? flags | 0x02 : flags & ~0x02);
		}

		public static bool Conversion{
			get => (flags & 0x04) != 0;
			set => flags = (byte)(value ? flags | 0x04 : flags & ~0x04);
		}

		public static IntPrimitive SP{
			get => new IntPrimitive(stack.sp);
			set => stack.sp = (int)((IPrimitive)value).Value;
		}

		public static IntPrimitive Head => new IntPrimitive(stack.Head);

		public static DateTimeRef DateTimeNow => new DateTimeRef();

		public static readonly DateTimeRef DateTimeEpoch = new DateTimeRef(DateTime.UnixEpoch);

		private static void CheckVerbose(string instruction, bool beginningOfInstr){
			if(Sandbox.verbose){
				Sandbox.verboseWriter.Flush();

				Sandbox.verboseWriter.WriteLine($"[CSASM] Stack at {(beginningOfInstr ? "beginning" : "end")} of instruction \"{instruction}\":" +
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
					stack.Push(s + (second is string s1 ? s1 : (second is char c1 ? (object)c1 : CSASMStack.FormatObject(second))));
				else if(second is string s2)
					stack.Push((first is string s1 ? s1 : (first is char c1 ? (object)c1 : CSASMStack.FormatObject(first))) + s2);
			}else if(first is ArithmeticSet set && second is ArithmeticSet set2)
				stack.Push(set.Union(set2));
			else if(first is ArithmeticSet set3 && (second is IntPrimitive[] || second is IntPrimitive)){
				if(second is not IntPrimitive[] arr2)
					arr2 = new IntPrimitive[]{ (IntPrimitive)second };

				if(arr2.Length > 0){
					IntPrimitive[] arr = set3.ToArray();

					IntPrimitive[] combined = new IntPrimitive[arr.Length + arr2.Length];
					Array.Copy(arr, 0, combined, 0, arr.Length);
					Array.Copy(arr2, 0, combined, arr.Length, arr2.Length);

					stack.Push(new ArithmeticSet(combined));
				}else
					stack.Push(set3);  //No new items.  Just push the original set back to the stack
			}else
				throw new StackException("add", first, second);
		}

		public static void func_and(){
			CheckVerbose("and", true);

			object second = stack.Pop();
			object first = stack.Pop();

			if(first is IPrimitiveInteger ip && second is IPrimitiveInteger)
				stack.Push(ip.BitwiseAnd(second as IPrimitive));
			else
				throw new StackException("and", first, second);
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

		public static void func_bin(){
			CheckVerbose("bin", true);

			object obj = stack.Pop();

			if(obj is IPrimitiveInteger){
				stack.Push(Utility.IntegerToBinary(obj, leadingZeroes: false));
			}else
				throw new StackException("bin", obj);
		}

		public static void func_binz(){
			CheckVerbose("bin", true);

			object obj = stack.Pop();

			if(obj is IPrimitiveInteger){
				stack.Push(Utility.IntegerToBinary(obj, leadingZeroes: true));
			}else
				throw new StackException("bin", obj);
		}

		public static void func_bit(byte bit){
			CheckVerbose("bit", true);

			object obj = stack.Pop();

			if(obj is IPrimitiveInteger ip)
				stack.Push(ip.GetBit(bit));
			else
				throw new StackException("bit", obj);
		}

		public static void func_bits(){
			CheckVerbose("bits", true);

			object obj = stack.Pop();

			unsafe{
				if(obj is FloatPrimitive fp){
					float f = (float)((IPrimitive)fp).Value;
					int i = *(int*)&f;

					stack.Push(new IntPrimitive(i));
				}else if(obj is DoublePrimitive dp){
					double d = (double)((IPrimitive)dp).Value;
					long l = *(long*)&d;

					stack.Push(new LongPrimitive(l));
				}else
					throw new StackException("bits", obj);
			}
		}

		public static void func_bytes(){
			CheckVerbose("bytes", true);

			object obj = stack.Pop();

			stack.Push(new IntPrimitive(ObjBytes(obj)));
		}

		private static int ObjBytes(object obj){
			return obj switch{
				string s =>          s.Length * 2,
				char _ =>            2,
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
				ArithmeticSet set => set.ToArray().Length * 4,
				CSASMRange _ =>           16,
				CSASMIndexer _ =>         4,
				null => throw new StackException("bytes", obj),
				_ => throw new StackException("bytes", obj)
			};
		}

		private static int ArrayBytes(Array arr){
			if(arr.Length == 0)
				return 0;

			int bytes = 0;
			Type elemType = arr.GetType().GetElementType();
			if(elemType == typeof(string) || elemType == typeof(object)){
				for(int i = 0; i < arr.Length; i++)
					bytes += ObjBytes(arr.GetValue(i)) + (Environment.Is64BitOperatingSystem ? 16 : 8);
			}else{
				//All elements have the same size
				bytes = ObjBytes(arr.GetValue(0)) * arr.Length;
			}

			return bytes;
		}

		public static void func_cls() => Console.Clear();

		public static void func_comp(){
			CheckVerbose("comp", true);

			object obj2 = stack.Pop();
			object obj = stack.Pop();

			if(Sandbox.verbose)
				Sandbox.verboseWriter.WriteLine($"[CSASM] \"comp\" instruction had types \"{(obj == null ? "null" : Utility.GetCSASMType(obj.GetType()))}\"" +
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
			}else if(obj is ArithmeticSet set && obj2 is ArithmeticSet set2){
				if(set == set2)
					Comparison = true;
			}else if(obj is CSASMRange range && obj2 is CSASMRange range2){
				if(range == range2)
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

		public static void func_comp_gte(){
			CheckVerbose("comp.gte", true);

			object obj2 = stack.Pop();
			object obj = stack.Pop();

			if(obj is IPrimitive ip && obj2 is IPrimitive ip2){
				if(ip.CompareTo(ip2) >= 0)
					Comparison = true;
			}else
				throw new StackException("comp.gte", obj, obj2);
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

		public static void func_comp_lte(){
			CheckVerbose("comp.lte", true);

			object obj2 = stack.Pop();
			object obj = stack.Pop();

			if(obj is IPrimitive ip && obj2 is IPrimitive ip2){
				if(ip.CompareTo(ip2) <= 0)
					Comparison = true;
			}else
				throw new StackException("comp.lte", obj, obj2);
		}

		public static void func_conrc() => Console.ResetColor();

		public static void func_conv(string type){
			CheckVerbose("conv", true);

			object obj = stack.Pop();
			if(Utility.GetCSASMType(obj.GetType()) == type){
				stack.Push(obj);
				return;
			}

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
			}else if(type == "~arr" && (obj is ArithmeticSet || obj is CSASMRange || obj is CSASMList)){
				if(obj is ArithmeticSet set)
					stack.Push(set.ToArray());
				else if(obj is CSASMRange range){
					//Ranges with indexers pop an additional value off of the stack to set the indexer
					object source = range.startIndexer != null || range.endIndexer != null ? stack.Pop() : null;

					stack.Push(range.ToArray(source));
				}else if(obj is CSASMList list)
					stack.Push(list.ToArray());
			}else if(type.StartsWith("~arr:"))
				throw new StackException("Array instances cannot be converted using the \"conv\" operator");
			else if(obj is IPrimitive ip){
				object value = ip.Value;
				if(ip is UintPrimitive){
					if(type == "^<u32>")
						stack.Push(new CSASMIndexer((uint)value));
					else
						throw new StackException("conv", obj);
				}else{
					try{
						Type newType = Utility.GetCsharpType(type);
						IPrimitive prim = Utility.CreatePrimitive(newType, value);
						stack.Push(prim);
					}catch(InvalidCastException){
						throw new StackException("conv", obj);
					}catch(ThrowException){
						throw;
					}catch{
						throw new StackException("conv", obj);
					}
				}
			}else if(obj is string s){
				if(type == "char")
					throw new StackException("conv", obj);
				else{
					switch(type){
						case "i8":
							if(sbyte.TryParse(s, out sbyte sb)){
								Conversion = true;
								stack.Push(new SbytePrimitive(sb));
							}else
								stack.Push(new SbytePrimitive(0));
							break;
						case "i16":
							if(short.TryParse(s, out short sh)){
								Conversion = true;
								stack.Push(new ShortPrimitive(sh));
							}else
								stack.Push(new ShortPrimitive(0));
							break;
						case "i32":
							if(int.TryParse(s, out int i)){
								Conversion = true;
								stack.Push(new IntPrimitive(i));
							}else
								stack.Push(new IntPrimitive(0));
							break;
						case "i64":
							if(long.TryParse(s, out long l)){
								Conversion = true;
								stack.Push(new LongPrimitive(l));
							}else
								stack.Push(new LongPrimitive(0));
							break;
						case "u8":
							if(byte.TryParse(s, out byte b)){
								Conversion = true;
								stack.Push(new BytePrimitive(b));
							}else
								stack.Push(new BytePrimitive(0));
							break;
						case "u16":
							if(ushort.TryParse(s, out ushort us)){
								Conversion = true;
								stack.Push(new UshortPrimitive(us));
							}else
								stack.Push(new UshortPrimitive(0));
							break;
						case "u32":
							if(uint.TryParse(s, out uint u)){
								Conversion = true;
								stack.Push(new UintPrimitive(u));
							}else
								stack.Push(new UintPrimitive(0));
							break;
						case "u64":
							if(ulong.TryParse(s, out ulong ul)){
								Conversion = true;
								stack.Push(new UlongPrimitive(ul));
							}else
								stack.Push(new UlongPrimitive(0));
							break;
						case "f32":
							if(float.TryParse(s, out float f)){
								Conversion = true;
								stack.Push(new FloatPrimitive(f));
							}else
								stack.Push(new FloatPrimitive(0));
							break;
						case "f64":
							if(double.TryParse(s, out double d)){
								Conversion = true;
								stack.Push(new DoublePrimitive(d));
							}else
								stack.Push(new DoublePrimitive(0));
							break;
					}
				}
			}else if(obj is char c && type == "i32")
				stack.Push(new IntPrimitive((int)c));
			else
				throw new StackException("conv", obj);

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

		public static void func_disj(){
			CheckVerbose("disj", true);

			object obj2 = stack.Pop();
			object obj = stack.Pop();

			if(obj is ArithmeticSet set && obj2 is ArithmeticSet set2){
				if(set.IsDisjoint(set2))
					Comparison = true;
			}else
				throw new StackException("disj", obj, obj2);
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
			}else if(first is ArithmeticSet set && second is ArithmeticSet set2)
				stack.Push(set.Intersection(set2));
			else
				throw new StackException("div", first, second);
		}

		#region DateTimeRef funcs
		#region Add Time
		public static void func_dtadd_d(){
			CheckVerbose("dtadd.d", true);

			object days = stack.Pop();
			object date = stack.Pop();

			if(date is DateTimeRef dateRef && days is DoublePrimitive dp)
				dateRef.AddDays((double)((IPrimitive)dp).Value);
			else
				throw new StackException("dtadd.d", date, days);
		}

		public static void func_dtadd_h(){
			CheckVerbose("dtadd.h", true);

			object hours = stack.Pop();
			object date = stack.Pop();

			if(date is DateTimeRef dateRef && hours is DoublePrimitive dp)
				dateRef.AddHours((double)((IPrimitive)dp).Value);
			else
				throw new StackException("dtadd.h", date, hours);
		}

		public static void func_dtadd_mi(){
			CheckVerbose("dtadd.mi", true);

			object mins = stack.Pop();
			object date = stack.Pop();

			if(date is DateTimeRef dateRef && mins is DoublePrimitive dp)
				dateRef.AddMinutes((double)((IPrimitive)dp).Value);
			else
				throw new StackException("dtadd.mi", date, mins);
		}

		public static void func_dtadd_ms(){
			CheckVerbose("dtadd.ms", true);

			object ms = stack.Pop();
			object date = stack.Pop();

			if(date is DateTimeRef dateRef && ms is DoublePrimitive dp)
				dateRef.AddMilliseconds((double)((IPrimitive)dp).Value);
			else
				throw new StackException("dtadd.ms", date, ms);
		}

		public static void func_dtadd_mt(){
			CheckVerbose("dtadd.mt", true);

			object months = stack.Pop();
			object date = stack.Pop();

			if(date is DateTimeRef dateRef && months is IntPrimitive ip)
				dateRef.AddMonths((int)((IPrimitive)ip).Value);
			else
				throw new StackException("dtadd.mt", date, months);
		}

		public static void func_dtadd_t(){
			CheckVerbose("dtadd.t", true);

			object ticks = stack.Pop();
			object date = stack.Pop();

			if(date is DateTimeRef dateRef && ticks is LongPrimitive lp)
				dateRef.AddTicks((long)((IPrimitive)lp).Value);
			else
				throw new StackException("dtadd.t", date, ticks);
		}

		public static void func_dtadd_s(){
			CheckVerbose("dtadd.s", true);

			object secs = stack.Pop();
			object date = stack.Pop();

			if(date is DateTimeRef dateRef && secs is DoublePrimitive dp)
				dateRef.AddSeconds((double)((IPrimitive)dp).Value);
			else
				throw new StackException("dtadd.s", date, secs);
		}

		public static void func_dtadd_y(){
			CheckVerbose("dtadd.y", true);

			object years = stack.Pop();
			object date = stack.Pop();

			if(date is DateTimeRef dateRef && years is IntPrimitive ip)
				dateRef.AddYears((int)((IPrimitive)ip).Value);
			else
				throw new StackException("dtadd.y", date, years);
		}
		#endregion

		public static void func_dtfmt(){
			CheckVerbose("dtfmt", true);

			object fmt = stack.Pop();
			object date = stack.Pop();

			if(date is DateTimeRef dateRef && fmt is string s)
				stack.Push(dateRef.Date.ToString(s));
			else
				throw new StackException("dtfmt", date, fmt);
		}

		#region New Instances
		public static void func_dtnew_t(){
			CheckVerbose("dtnew.t", true);

			object ticks = stack.Pop();

			if(ticks is not LongPrimitive lp)
				throw new StackException("dtnew.t", ticks);

			stack.Push(new DateTimeRef((long)((IPrimitive)lp).Value));
		}

		public static void func_dtnew_ymd(){
			CheckVerbose("dtnew.ymd", true);

			object day = stack.Pop();
			object month = stack.Pop();
			object year = stack.Pop();

			if(year is not IntPrimitive ip || month is not IntPrimitive ip2 || day is not IntPrimitive ip3)
				throw new StackException("dtnew.ymd", year, month, day);

			stack.Push(new DateTimeRef((int)((IPrimitive)ip).Value, (int)((IPrimitive)ip2).Value, (int)((IPrimitive)ip3).Value));
		}

		public static void func_dtnew_ymdhms(){
			CheckVerbose("dtnew.ymdhms", true);

			object second = stack.Pop();
			object minute = stack.Pop();
			object hour = stack.Pop();
			object day = stack.Pop();
			object month = stack.Pop();
			object year = stack.Pop();

			if(year is not IntPrimitive ip || month is not IntPrimitive ip2 || day is not IntPrimitive ip3 || hour is not IntPrimitive ip4 || minute is not IntPrimitive ip5 || second is not IntPrimitive ip6)
				throw new StackException("dtnew.ymd", year, month, day);

			stack.Push(new DateTimeRef((int)((IPrimitive)ip).Value, (int)((IPrimitive)ip2).Value, (int)((IPrimitive)ip3).Value, (int)((IPrimitive)ip4).Value, (int)((IPrimitive)ip5).Value, (int)((IPrimitive)ip6).Value));
		}

		public static void func_dtnew_ymdhmsm(){
			CheckVerbose("dtnew.ymdhmsm", true);

			object millisecond = stack.Pop();
			object second = stack.Pop();
			object minute = stack.Pop();
			object hour = stack.Pop();
			object day = stack.Pop();
			object month = stack.Pop();
			object year = stack.Pop();

			if(year is not IntPrimitive ip || month is not IntPrimitive ip2 || day is not IntPrimitive ip3 || hour is not IntPrimitive ip4 || minute is not IntPrimitive ip5 || second is not IntPrimitive ip6 || millisecond is not IntPrimitive ip7)
				throw new StackException("dtnew.ymd", year, month, day);

			stack.Push(new DateTimeRef((int)((IPrimitive)ip).Value, (int)((IPrimitive)ip2).Value, (int)((IPrimitive)ip3).Value, (int)((IPrimitive)ip4).Value, (int)((IPrimitive)ip5).Value, (int)((IPrimitive)ip6).Value, (int)((IPrimitive)ip7).Value));
		}
		#endregion

		#region Value Accessing
		public static void func_dt_day(){
			CheckVerbose("dt.day", true);

			object date = stack.Pop();

			if(date is not DateTimeRef dateRef)
				throw new StackException("dt.day", date);

			stack.Push(new IntPrimitive(dateRef.Date.Day));
		}

		public static void func_dt_hour(){
			CheckVerbose("dt.hour", true);

			object date = stack.Pop();

			if(date is not DateTimeRef dateRef)
				throw new StackException("dt.hour", date);

			stack.Push(new IntPrimitive(dateRef.Date.Hour));
		}

		public static void func_dt_min(){
			CheckVerbose("dt.min", true);

			object date = stack.Pop();

			if(date is not DateTimeRef dateRef)
				throw new StackException("dt.min", date);

			stack.Push(new IntPrimitive(dateRef.Date.Minute));
		}

		public static void func_dt_month(){
			CheckVerbose("dt.month", true);

			object date = stack.Pop();

			if(date is not DateTimeRef dateRef)
				throw new StackException("dt.month", date);

			stack.Push(new IntPrimitive(dateRef.Date.Month));
		}

		public static void func_dt_msec(){
			CheckVerbose("dt.msec", true);

			object date = stack.Pop();

			if(date is not DateTimeRef dateRef)
				throw new StackException("dt.msec", date);

			stack.Push(new IntPrimitive(dateRef.Date.Millisecond));
		}

		public static void func_dt_sec(){
			CheckVerbose("dt.sec", true);

			object date = stack.Pop();

			if(date is not DateTimeRef dateRef)
				throw new StackException("dt.sec", date);

			stack.Push(new IntPrimitive(dateRef.Date.Second));
		}

		public static void func_dt_ticks(){
			CheckVerbose("dt.ticks", true);

			object date = stack.Pop();

			if(date is not DateTimeRef dateRef)
				throw new StackException("dt.ticks", date);

			stack.Push(new LongPrimitive(dateRef.Date.Ticks));
		}

		public static void func_dt_year(){
			CheckVerbose("dt.year", true);

			object date = stack.Pop();

			if(date is not DateTimeRef dateRef)
				throw new StackException("dt.year", date);

			stack.Push(new IntPrimitive(dateRef.Date.Year));
		}
		#endregion
		#endregion

		public static void func_dup(){
			object obj = stack.Peek();
			if(obj is string s)
				stack.Push(s.Clone());
			else if(obj is Array a)
				stack.Push(a.Clone());
			else
				stack.Push(obj);
		}

		public static void func_extern(string func){
			object obj, obj2;
			//Why
			IPrimitive ip, ip2;
			switch(func){
				case "Math.Sqrt":
					obj = stack.Pop();
					if((ip = obj as IPrimitive) != null){
						IPrimitive arg = Utility.CreatePrimitive(typeof(DoublePrimitive), ip.Value);
						stack.Push(new DoublePrimitive(Math.Sqrt((double)arg.Value)));
					}else
						throw new StackException("extern", obj);
					break;
				case "Math.Sin":
					obj = stack.Pop();
					if((ip = obj as IPrimitive) != null){
						IPrimitive arg = Utility.CreatePrimitive(typeof(DoublePrimitive), ip.Value);
						stack.Push(new DoublePrimitive(Math.Sin((double)arg.Value)));
					}else
						throw new StackException("extern", obj);
					break;
				case "Math.Cos":
					obj = stack.Pop();
					if((ip = obj as IPrimitive) != null){
						IPrimitive arg = Utility.CreatePrimitive(typeof(DoublePrimitive), ip.Value);
						stack.Push(new DoublePrimitive(Math.Cos((double)arg.Value)));
					}else
						throw new StackException("extern", obj);
					break;
				case "Math.Tan":
					obj = stack.Pop();
					if((ip = obj as IPrimitive) != null){
						IPrimitive arg = Utility.CreatePrimitive(typeof(DoublePrimitive), ip.Value);
						stack.Push(new DoublePrimitive(Math.Tan((double)arg.Value)));
					}else
						throw new StackException("extern", obj);
					break;
				case "Math.Atan2":
					obj2 = stack.Pop();
					obj = stack.Pop();
					if((ip = obj as IPrimitive) != null && (ip2 = obj2 as IPrimitive) != null){
						IPrimitive arg = Utility.CreatePrimitive(typeof(DoublePrimitive), ip.Value);
						IPrimitive arg2 = Utility.CreatePrimitive(typeof(DoublePrimitive), ip2.Value);
						stack.Push(new DoublePrimitive(Math.Atan2((double)arg.Value, (double)arg2.Value)));
					}else
						throw new StackException("extern", obj);
					break;
				case "Random.Next":
					stack.Push(new IntPrimitive(Sandbox.random.Next()));
					break;
				case "Random.Next(i32)":
					obj = stack.Pop();
					if(obj is IntPrimitive)
						stack.Push(new IntPrimitive(Sandbox.random.Next((int)(obj as IPrimitive).Value)));
					else
						throw new StackException("extern", obj);
					break;
				case "Random.Next(i32,i32)":
					obj2 = stack.Pop();
					obj = stack.Pop();
					if(obj is IntPrimitive && obj2 is IntPrimitive)
						stack.Push(new IntPrimitive(Sandbox.random.Next((int)(obj as IPrimitive).Value, (int)(obj2 as IPrimitive).Value)));
					else
						throw new StackException("extern", obj);
					break;
				default:
					throw new ArithmeticException($"Argument \"{func}\" did not refer to an implemented function redirect");
			}
		}

		public static void func_in(string prompt){
			Console.Write(prompt);

			stack.Push(Console.ReadLine());
		}

		public static void func_inc(){
			object obj = stack.Pop();
			if(obj is IPrimitive ip)
				stack.Push(ip.Increment());
			else
				throw new StackException("inc", obj);
		}

		public static void func_index(){
			CheckVerbose("index", true);

			object find = stack.Pop();
			object source = stack.Pop();

			if(source is string s){
				if(find is string f)
					stack.Push(new IntPrimitive(s.IndexOf(f)));
				else if(find is char c)
					stack.Push(new IntPrimitive(s.IndexOf(c)));
				else
					throw new StackException("index", source, find);
			}else if(source is Array a){
				if(source.GetType().GetElementType() == find.GetType())
					stack.Push(new IntPrimitive(Array.IndexOf(a, find)));
				else
					throw new StackException("index", source, find);
			}else if(source is CSASMList list)
				stack.Push(new IntPrimitive(list.IndexOf(find)));
			else
				throw new StackException("index", source);
		}

		public static void func_ink(string prompt){
			Console.Write(prompt);

			stack.Push(Console.ReadKey().KeyChar);
		}

		public static void func_inki(string prompt){
			Console.Write(prompt);

			stack.Push(Console.ReadKey(intercept: true).KeyChar);
		}

		public static void func_interp(string interp){
			CheckVerbose("interp", true);

			object obj2 = stack.Pop();
			if(obj2 is not object[] args)
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

		public static void func_isarr(string type){
			CheckVerbose("isarr", true);

			object arr = stack.Pop();
			Type arrType = arr.GetType();
			Type checkType = Utility.GetCsharpType(type);

			if(arrType.IsArray && (checkType == typeof(object) || arrType.GetElementType() == checkType))
				Comparison = true;
		}

		public static void func_ldelem(int index){
			CheckVerbose("ldelem", true);
			if(Sandbox.verbose)
				Sandbox.verboseWriter.WriteLine($"[CSASM] Index passed into instruction \"ldelem\": {index}");

			object arr = stack.Pop();

			if(arr is Array array){
				if(index >= array.Length)
					throw new StackException($"Index ({index}) was outside the range of valid entries in the array (0..{array.Length - 1})");

				stack.Push(array.GetValue(index));
			}else if(arr is CSASMList list){
				if(index >= list.Capacity)
					throw new StackException($"Index ({index}) was outside the range of valid entries in the list (0..{list.Capacity - 1})");

				stack.Push(list[index]);
			}else
				throw new StackException("ldelem", arr);
		}

		public static void func_ldelem(CSASMIndexer indexer){
			object arr = stack.Pop();

			if(arr is not Array && arr is not CSASMList)
				throw new StackException("ldelem", arr);

			int offset = indexer.GetIndex(arr);
			stack.Push(arr);

			func_ldelem(offset);
		}

		public static void func_len(){
			object obj = stack.Pop();

			if(obj is Array a)
				stack.Push(new IntPrimitive(a.Length));
			else if(obj is string s)
				stack.Push(new IntPrimitive(s.Length));
			else
				throw new StackException("len", obj);
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

		public static void func_newindex(){
			CheckVerbose("newindex", true);

			object obj = stack.Pop();

			if(obj is UintPrimitive ip)
				stack.Push(new CSASMIndexer((uint)(ip as IPrimitive).Value));
			else
				throw new StackException("newindex", obj);
		}

		public static void func_newlist(){
			CheckVerbose("newlist", true);

			object obj = stack.Pop();

			if(obj is IntPrimitive ip)
				stack.Push(new CSASMList((int)(ip as IPrimitive).Value));
			else
				throw new StackException("newlist", obj);
		}

		public static void func_newlist_z(){
			CheckVerbose("newlist.z", true);

			stack.Push(new CSASMList());
		}

		public static void func_newrange(){
			CheckVerbose("newrange", true);

			object end = stack.Pop();
			object start = stack.Pop();

			if(start is CSASMIndexer && end is IntPrimitive)
				throw new StackException("newrange", start, end);
			else if(start is IntPrimitive ip && end is IntPrimitive ip2)
				stack.Push(new CSASMRange((int)(ip as IPrimitive).Value, (int)(ip2 as IPrimitive).Value));
			else if(start is IntPrimitive ip3 && end is CSASMIndexer idx)
				stack.Push(new CSASMRange((int)(ip3 as IPrimitive).Value, idx));
			else if(start is CSASMIndexer idx2 && end is CSASMIndexer idx3)
				stack.Push(new CSASMRange(idx2, idx3));
			else
				throw new StackException("newrange", start, end);
		}

		public static void func_newset(){
			CheckVerbose("newset", true);

			object arr = stack.Pop();

			if(arr is Array array && typeof(IPrimitive).IsAssignableFrom(array.GetType().GetElementType()))
				stack.Push(new ArithmeticSet(array));
			else if(arr is CSASMRange range)
				stack.Push(new ArithmeticSet(range));
			else
				throw new StackException("newset", arr);
		}

		public static void func_not(){
			CheckVerbose("not", true);

			object obj = stack.Pop();

			if(obj is IPrimitiveInteger ip)
				stack.Push(ip.BitwiseNot());
			else
				throw new StackException("not", obj);
		}

		public static void func_or(){
			CheckVerbose("or", true);

			object second = stack.Pop();
			object first = stack.Pop();

			if(first is IPrimitiveInteger ip && second is IPrimitiveInteger)
				stack.Push(ip.BitwiseOr(second as IPrimitive));
			else
				throw new StackException("or", first, second);
		}

		public static void func_pow(){
			CheckVerbose("pow", true);

			object second = stack.Pop();
			object first = stack.Pop();

			if(first is IPrimitive ip && second is IPrimitive ip2){
				DoublePrimitive dp = (DoublePrimitive)Utility.CreatePrimitive(typeof(DoublePrimitive), ip.Value);
				DoublePrimitive dp2 = (DoublePrimitive)Utility.CreatePrimitive(typeof(DoublePrimitive), ip2.Value);

				stack.Push(Math.Pow((double)(dp as IPrimitive).Value, (double)(dp2 as IPrimitive).Value));
			}else
				throw new StackException("root", first, second);
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

		public static void func_rem(){
			CheckVerbose("rem", true);
			
			object second = stack.Pop();
			object first = stack.Pop();

			if(first is IPrimitive ip && second is IPrimitive ip2)
				stack.Push(ip.Remainder(ip2));
			else if(first is ArithmeticSet set && second is ArithmeticSet set2)
				stack.Push(set.Intersection(set2));
			else
				throw new StackException("rem", first, second);
		}

		public static void func_rend(){
			CheckVerbose("rend", true);

			object obj = stack.Pop();

			if(obj is CSASMRange range){
				if(range.start != null)
					stack.Push(new IntPrimitive(range.start.Value));
				else if(range.startIndexer != null){
					object source = stack.Pop();

					if(source is Array || source is string)
						stack.Push(new IntPrimitive(range.startIndexer.Value.GetIndex(source)));
					else
						throw new StackException("rend", obj, source);
				}else
					throw new FormatException("Range instance on stack had an invalid state");
			}else
				throw new StackException("rend", obj);
		}

		public static void func_rol(){
			CheckVerbose("rol", true);

			object obj = stack.Pop();

			if(obj is IPrimitiveInteger ip)
				stack.Push(ip.ArithmeticRotateLeft(ref flags));
			else
				throw new StackException("rol", obj);
		}

		public static void func_root(){
			CheckVerbose("root", true);

			object second = stack.Pop();
			object first = stack.Pop();

			if(first is IPrimitive ip && second is IPrimitive ip2){
				DoublePrimitive dp = (DoublePrimitive)Utility.CreatePrimitive(typeof(DoublePrimitive), ip.Value);
				DoublePrimitive dp2 = (DoublePrimitive)Utility.CreatePrimitive(typeof(DoublePrimitive), ip2.Value);

				stack.Push(Math.Pow((double)(dp as IPrimitive).Value, 1.0d / (double)(dp2 as IPrimitive).Value));
			}else
				throw new StackException("root", first, second);
		}

		public static void func_ror(){
			CheckVerbose("ror", true);

			object obj = stack.Pop();

			if(obj is IPrimitiveInteger ip)
				stack.Push(ip.ArithmeticRotateRight(ref flags));
			else
				throw new StackException("ror", obj);
		}

		public static void func_rstt(){
			CheckVerbose("rstt", true);

			object obj = stack.Pop();

			if(obj is CSASMRange range){
				if(range.end != null)
					stack.Push(new IntPrimitive(range.end.Value));
				else if(range.endIndexer != null){
					object source = stack.Pop();

					if(source is Array || source is string)
						stack.Push(new IntPrimitive(range.endIndexer.Value.GetIndex(source)));
					else
						throw new StackException("rstt", obj, source);
				}else
					throw new FormatException("Range instance on stack had an invalid state");
			}else
				throw new StackException("rstt", obj);
		}

		public static void func_stelem(int index){
			CheckVerbose("stelem", true);
			if(Sandbox.verbose)
				Sandbox.verboseWriter.WriteLine($"[CSASM] Index passed into instruction \"stelem\": {index}");

			object obj = stack.Pop();
			object arr = stack.Pop();

			Type type = obj.GetType();

			if(arr is Array array){
				Type arrType = arr.GetType();
				Type arrElemType = arrType.GetElementType();

				if(type != arrElemType && !arrElemType.IsAssignableFrom(type))
					throw new StackException($"Instruction \"stelem\" had invalid values on the stack before it ({Utility.GetCSASMType(type)}, {Utility.GetCSASMType(arrType)})");

				if(index >= array.Length)
					throw new StackException($"Index ({index}) was outside the range of valid entries in the array (0..{array.Length - 1})");

				array.SetValue(obj, index);
			}else if(arr is CSASMList list){
				if(index < 0)
					throw new StackException($"Index was negative");

				list[index] = obj;
			}else
				throw new StackException("stelem", arr);
		}

		public static void func_stelem(CSASMIndexer indexer){
			object obj = stack.Pop();
			object arr = stack.Pop();

			Type type = obj.GetType();
			Type arrType = arr.GetType();

			if(arr is not Array && arr is not CSASMList)
				throw new StackException($"Instruction \"stelem\" had invalid values on the stack before it ({Utility.GetCSASMType(type)}, {Utility.GetCSASMType(arrType)})");

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
			}else if(first is ArithmeticSet set && second is ArithmeticSet set2)
				stack.Push(set.Difference(set2));
			else
				throw new StackException("sub", first, second);
		}

		public static void func_substr(){
			CheckVerbose("substr", true);

			object end = stack.Pop();
			object str;

			//If "end" is a range, then the next value on the stack will be a string
			if(end is CSASMRange range){
				str = stack.Pop();

				if(str is string s2){
					range.GetValues(s2, out int posStart, out int posEnd);

					if(posStart > posEnd)
						throw new StackException("Range argument for instruction \"substr\" was invalid");

					stack.Push(s2[posStart..posEnd]);
					return;
				}else
					throw new StackException("substr", str, end);
			}

			object start = stack.Pop();
			str = stack.Pop();

			if(str is string s){
				int i, e;
				if(start is IntPrimitive ip){
					i = (int)((IPrimitive)ip).Value;
					if(end is IntPrimitive ip2)
						e = (int)((IPrimitive)ip2).Value;
					else if(end is CSASMIndexer id2)
						e = id2.GetIndex(s);
					else
						throw new StackException("substr", end);
				}else if(start is CSASMIndexer id){
					i = id.GetIndex(s);
					if(end is IntPrimitive ip2)
						e = (int)((IPrimitive)ip2).Value;
					else if(end is CSASMIndexer id2)
						e = id2.GetIndex(s);
					else
						throw new StackException("substr", end);
				}else
					throw new StackException("substr", start);

				//Check the bounds
				if(i < 0)
					throw new StackException("Start index must be greater than zero (\"substr\")");
				if(i > e)
					throw new StackException("Start index must be before end index (\"substr\")");
				if(e < 0)
					throw new StackException("End index must be greater than zero (\"substr\")");
				if(e > s.Length)
					throw new StackException("End index must be a location in the input string (\"substr\")");

				if(i == e)
					stack.Push(string.Empty);
				else
					stack.Push(s[i..e]);
			}else
				throw new StackException("substr", str);
		}

		public static void func_swap(){
			CheckVerbose("swap", true);

			object second = stack.Pop();
			object first = stack.Pop();

			stack.Push(second);
			stack.Push(first);
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

		public static void func_wait(){
			CheckVerbose("wait", true);

			object ms = stack.Pop();

			if(ms is not IntPrimitive ip)
				throw new StackException("wait", ms);

			Thread.Sleep((int)((IPrimitive)ip).Value);
		}

		public static void func_xor(){
			CheckVerbose("xor", true);

			object second = stack.Pop();
			object first = stack.Pop();

			if(first is IPrimitiveInteger ip && second is IPrimitiveInteger)
				stack.Push(ip.BitwiseXor(second as IPrimitive));
			else
				throw new StackException("xor", first, second);
		}
	}
}
