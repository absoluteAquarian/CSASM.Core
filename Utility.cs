using System;

namespace CSASM.Core{
	public static class Utility{
		public static string GetCSASMType(Type type)
			=> type.Name switch{
				"Sbyte" => "i8",
				"Int16" => "i16",
				"Int32" => "i32",
				"Int64" => "i64",
				"Byte" => "u8",
				"UInt16" => "u16",
				"UInt32" => "u32",
				"UInt64" => "u64",
				"Single" => "f32",
				"Double" => "f64",
				"SbytePrimitive" => "i8",
				"ShortPrimitive" => "i16",
				"IntPrimitive" => "i32",
				"LongPrimitive" => "i64",
				"BytePrimitive" => "u8",
				"UshortPrimitive" => "u16",
				"UintPrimitive" => "u32",
				"UlongPrimitive" => "u64",
				"FloatPrimitive" => "f32",
				"DoublePrimitive" => "f64",
				"Char" => "char",
				"String" => "str",
				"Object" => "obj",
				null => throw new ArgumentNullException("type.Name"),
				_ when type.IsArray => $"~arr:{GetCSASMType(type.GetElementType())}",
				_ => throw new Exception($"Type \"{type.Name}\" does not correspond to a valid CSASM type")
			};

		public static Type GetCsharpType(string asmType)
			=> asmType switch{
				"char" => typeof(char),
				"str" => typeof(string),
				"i8" => typeof(SbytePrimitive),
				"i16" => typeof(ShortPrimitive),
				"i32" => typeof(IntPrimitive),
				"i64" => typeof(LongPrimitive),
				"u8" => typeof(BytePrimitive),
				"u16" => typeof(UshortPrimitive),
				"u32" => typeof(UintPrimitive),
				"u64" => typeof(UlongPrimitive),
				"f32" => typeof(FloatPrimitive),
				"f64" => typeof(DoublePrimitive),
				"obj" => typeof(object),
				null => throw new ArgumentNullException("asmType"),
				_ when asmType.StartsWith("~arr:") => Array.CreateInstance(GetCsharpType(asmType.Substring("~arr:".Length)), 0).GetType(),
				_ => throw new ThrowException($"Type \"{asmType}\" did not correlate to a valid CSASM type")
			};

		public static IPrimitive CreatePrimitive(Type type, object input){
			return type.Name switch{
				"DoublePrimitive" => new DoublePrimitive(Convert.ToDouble(input)),
				"FloatPrimitive" => new FloatPrimitive(Convert.ToSingle(input)),
				"SbytePrimitive" => new SbytePrimitive(Convert.ToInt32(input)),
				"ShortPrimitive" => new ShortPrimitive(Convert.ToInt32(input)),
				"IntPrimitive" => new IntPrimitive(Convert.ToInt32(input)),
				"LongPrimitive" => new LongPrimitive(Convert.ToInt64(input)),
				"BytePrimitive" => new BytePrimitive(Convert.ToInt32(input)),
				"UshortPrimitive" => new UshortPrimitive(Convert.ToInt32(input)),
				"UintPrimitive" => new UintPrimitive(Convert.ToUInt32(input)),
				"UlongPrimitive" => new UlongPrimitive(Convert.ToUInt64(input)),
				_ => throw new InvalidCastException()
			};
		}

		public static byte GetCarry(byte flags) => (byte)(flags & 0x01);
		public static byte SetCarry(ref byte flags, bool set) => flags = (byte)(set ? flags | 0x01 : flags & ~0x01);

		public static byte GetComparison(byte flags) => (byte)(flags & 0x02);
		public static byte SetComparison(ref byte flags, bool set) => flags = (byte)(set ? flags | 0x02 : flags & ~0x02);

		public static byte GetConversion(byte flags) => (byte)(flags & 0x04);
		public static byte SetConversion(ref byte flags, bool set) => flags = (byte)(set ? flags | 0x04 : flags & ~0x04);

		public static bool BrResult(){
			if(Sandbox.verbose){
				Sandbox.verboseWriter.WriteLine($"[CSASM] Stack at beginning of utility \"BrResult\":" +
					$"\n   {Ops.stack}");
			}

			object obj = Ops.stack.Pop();

			if(obj is char c)
				return c != '\0';
			else if(obj is IPrimitive ip)
				return AreEqual(ip, !(ip is IPrimitiveInteger) ? (IPrimitive)new FloatPrimitive(0) : (IPrimitive)new IntPrimitive(0));
			else if(obj is bool b)
				return b;

			//Strings, arrays and objects will end up here
			return obj != null;
		}

		public static bool AreEqual(IPrimitive ip, IPrimitive ip2){
			object obj = ip.Value;
			object obj2 = ip2.Value;

			if(!(ip is IPrimitiveInteger) && !(ip2 is IPrimitiveInteger)){
				double? d = AsFloat(obj);
				double? d2 = AsFloat(obj2);
				return d != null && d2 != null && d == d2;
			}

			//One is a float, but not both
			if(!(ip is IPrimitiveInteger) ^ !(ip2 is IPrimitiveInteger))
				throw new StackException("Cannot compare floating-point types to integer types");

			ulong? u = AsUInteger(obj);
			ulong? u2 = AsUInteger(obj2);
			long? l = AsInteger(obj);
			long? l2 = AsInteger(obj2);

			if(u == null && l == null && u2 == null && l2 == null)
				throw new StackException("Values on stack were not integers (\"comp\")");

			if(u is ulong v){
				if(u2 is ulong v2){
					if(Sandbox.verbose)
						Sandbox.verboseWriter.WriteLine($"[CSASM] AreEqual: unsigned integer ({v}) vs unsigned integer ({v2})");

					return v == v2;
				}else if(l2 is long v3){
					if(Sandbox.verbose)
						Sandbox.verboseWriter.WriteLine($"[CSASM] AreEqual: unsigned integer ({v}) vs signed integer ({v3})");

					return (long)v >= 0 ? (long)v == v3 : false;  //Make comparisons of ulongs that wrap to the negatives always false (if the other one is long)
				}

				throw new StackException("Values on stack were not integers (\"comp\")");
			}
			if(l is long v4){
				if(l2 is long v2){
					if(Sandbox.verbose)
						Sandbox.verboseWriter.WriteLine($"[CSASM] AreEqual: signed integer ({v4}) vs signed integer ({v2})");

					return v4 == v2;
				}else if(u2 is ulong v3){
					if(Sandbox.verbose)
						Sandbox.verboseWriter.WriteLine($"[CSASM] AreEqual: signed integer ({v4}) vs unsigned integer ({v3})");

					return (long)v3 >= 0 ? v4 == (long)v3 : false;  //Make comparisons of ulongs that wrap to the negatives always false (if the other one is long)
				}

				throw new StackException("Values on stack were not integers (\"comp\")");
			}

			throw new StackException("Values on stack were not numbers (\"comp\")");
		}

		public static ulong? AsUInteger(object obj)
			=> obj as ulong? ?? obj as uint? ?? obj as ushort? ?? obj as byte?;

		public static long? AsInteger(object obj)
			=> obj as long? ?? obj as int? ?? obj as short? ?? obj as sbyte?;

		public static double? AsFloat(object obj)
			=> obj as double? ?? obj as float?;
	}
}
