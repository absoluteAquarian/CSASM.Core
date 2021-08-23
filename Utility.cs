using System;
using System.IO;
using System.Text;

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
				"Indexer" => "^<u32>",
				"Boolean" => "System.Boolean",
				"ArithmeticSet" => "~set",
				"Range" => "~range",
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
				"^<u32>" => typeof(CSASMIndexer),
				"~set" => typeof(ArithmeticSet),
				"~range" => typeof(CSASMRange),
				null => throw new ArgumentNullException(nameof(asmType)),
				_ when asmType.StartsWith("~arr:") => Array.CreateInstance(GetCsharpType(asmType["~arr:".Length..]), 0).GetType(),
				_ when asmType.StartsWith("^") && uint.TryParse(asmType[1..], out _) => typeof(CSASMIndexer),
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
			bool ret;

			if(obj is char c)
				ret = c != '\0';
			else if(obj is IPrimitive ip)
				ret = !AreEqual(ip, !(ip is IPrimitiveInteger) ? (IPrimitive)new FloatPrimitive(0) : (IPrimitive)new IntPrimitive(0));
			else if(obj is bool b)
				ret = b;
			else if(obj is ArithmeticSet set)
				ret = set != ArithmeticSet.EmptySet;
			else{
				//Strings, arrays, ranges and objects will end up here
				ret = obj != null;
			}

			if(Sandbox.verbose)
				Sandbox.verboseWriter.WriteLine($"[CSASM] Utility \"BrResult\" returned: {ret}");

			return ret;
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

					return (long)v >= 0 && (long)v == v3;  //Make comparisons of ulongs that wrap to the negatives always false (if the other one is long)
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

					return (long)v3 >= 0 && v4 == (long)v3;  //Make comparisons of ulongs that wrap to the negatives always false (if the other one is long)
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

		public static string IntegerToBinary(object obj, bool leadingZeroes = false){
			if(!(obj is IPrimitiveInteger))
				throw new StackException("Value on stack was not an integer");

			object value = (obj as IPrimitive).Value;
			bool foundSetBit = false;

			StringBuilder sb = new StringBuilder(64);
			ulong num;

			if(obj is IUnsignedPrimitiveInteger)
				num = AsUInteger(value).Value;
			else
				num = (ulong)AsInteger(value).Value;

			int place = (obj as IPrimitiveInteger).BitSize();
			while(--place >= 0){
				ulong bit = num & (1uL << place);

				if(bit == 0 && (foundSetBit || (!foundSetBit && leadingZeroes)))
					sb.Append('0');
				else if(bit == 1){
					foundSetBit = true;
					sb.Append('1');
				}
			}

			return sb.ToString();
		}

		public static Array DeepCloneArray(Array array){
			Array ret = Array.CreateInstance(array.GetType().GetElementType(), array.Length);

			for(int i = 0; i < array.Length; i++){
				object orig = array.GetValue(i);
				//Clone clonable types (arrays, strings, etc.), copy everything else
				ret.SetValue((orig as ICloneable)?.Clone() ?? orig, i);
			}
			
			return ret;
		}

		private static void CheckIOActiveHandle(byte id, out IOHandle handle){
			if(id > 7)
				throw new StackException($"I/O handle reference was not valid: {id}");

			handle = Sandbox.ioHandles[id];
			if(handle.file != null)
				throw new InvalidOperationException("Cannot modify I/O handle while it is in use");
		}

		public static void SetIOHandleType(bool write, byte id){
			CheckIOActiveHandle(id, out IOHandle handle);
			handle.write = write;
		}

		public static void SetIOHandleMode(FileMode mode, byte id){
			CheckIOActiveHandle(id, out IOHandle handle);
			handle.mode = mode;
		}

		public static void SetIOHandleFile(string file, byte id){
			if(id > 7)
				throw new StackException($"I/O handle reference was not valid: {id}");

			IOHandle handle = Sandbox.ioHandles[id];

			if(handle.file == null){
				if(file != null){
					handle.file = file;

					if(!handle.isStream){
						if(handle.write)
							handle.handle = new BinaryWriter(File.Open(handle.file, handle.mode));
						else
							handle.handle = new BinaryReader(File.Open(handle.file, handle.mode));
					}else{
						if(handle.write)
							handle.handle = new StreamWriter(File.Open(handle.file, handle.mode));
						else
							handle.handle = new StreamReader(File.Open(handle.file, handle.mode));
					}
				}
			}else if(file == null){
				handle.file = file;

				(handle.handle as IDisposable).Dispose();
			}else
				throw new InvalidOperationException("Cannot modify I/O handle while it is in use");
		}

		public static void SetIOHandleStream(bool isStream, byte id){
			CheckIOActiveHandle(id, out IOHandle handle);
			handle.isStream = isStream;
		}

		public static void SetIOHandleNewline(bool newLine, byte id){
			if(id > 7)
				throw new StackException($"I/O handle reference was not valid: {id}");

			IOHandle handle = Sandbox.ioHandles[id];
			handle.newLine = newLine;
		}

		public static bool GetIOHandleType(byte id){
			if(id > 7)
				throw new StackException($"I/O handle reference was not valid: {id}");

			IOHandle handle = Sandbox.ioHandles[id];
			return handle.write;
		}

		public static FileMode GetIOHandleMode(byte id){
			if(id > 7)
				throw new StackException($"I/O handle reference was not valid: {id}");

			IOHandle handle = Sandbox.ioHandles[id];
			return handle.mode;
		}

		public static string GetIOHandleFile(byte id){
			if(id > 7)
				throw new StackException($"I/O handle reference was not valid: {id}");

			IOHandle handle = Sandbox.ioHandles[id];
			return handle.file;
		}

		public static bool GetIOHandleStream(byte id){
			if(id > 7)
				throw new StackException($"I/O handle reference was not valid: {id}");

			IOHandle handle = Sandbox.ioHandles[id];
			return handle.isStream;
		}

		public static bool GetIOHandleNewline(byte id){
			if(id > 7)
				throw new StackException($"I/O handle reference was not valid: {id}");

			IOHandle handle = Sandbox.ioHandles[id];
			return handle.newLine;
		}

		public static void IOHandleRead(byte id, string type){
			if(id > 7)
				throw new StackException($"I/O handle reference was not valid: {id}");

			IOHandle handle = Sandbox.ioHandles[id];
			if(handle.file == null)
				throw new IOException($"I/O Handle {id} was not initialized");
			if(handle.write)
				throw new IOException($"I/O Handle {id} was set to write-only.  Cannot read using this I/O handle");

			if(handle.isStream){
				StreamReader sReader = handle.handle as StreamReader;
				switch(type){
					case "char":
						Ops.stack.Push((char)sReader.Read());
						break;
					case "str":
						Ops.stack.Push(sReader.ReadLine());
						break;
					default:
						throw new IOException($"I/O handle {id} was set to Stream reading.  The only types supported in this mode are \"char\" and \"str\"");
				}

				return;
			}

			BinaryReader reader = handle.handle as BinaryReader;

			Ops.stack.Push(ReadCSASMObj(type, reader));
		}

		public static void IOHandleWrite(byte id, object value){
			if(id > 7)
				throw new StackException($"I/O handle reference was not valid: {id}");

			IOHandle handle = Sandbox.ioHandles[id];
			if(handle.file == null)
				throw new IOException($"I/O Handle {id} was not initialized");
			if(!handle.write)
				throw new IOException($"I/O Handle {id} was set to read-only.  Cannot write using this I/O handle");

			if(handle.isStream){
				StreamWriter sWriter = handle.handle as StreamWriter;

				if(value is Array a){
					string rep = CSASMStack.FormatArray(a);
					if(handle.newLine)
						sWriter.WriteLine(rep);
					else
						sWriter.Write(rep);
				}else{
					if(handle.newLine)
						sWriter.WriteLine(value);
					else
						sWriter.Write(value);
				}

				return;
			}

			BinaryWriter writer = handle.handle as BinaryWriter;

			WriteCSASMObj(writer, value);
		}

		private static Array ReadArray(BinaryReader reader){
			string subType = reader.ReadString();
			if(subType.StartsWith("~arr:"))
				throw new IOException("CSASM does not support arrays of arrays");

			int length = Read7BitEncodedInt(reader);

			Array array = Array.CreateInstance(GetCsharpType(subType), length);

			for(int i = 0; i < length; i++)
				array.SetValue(ReadCSASMObj(subType, reader), i);

			return array;
		}

		private static CSASMRange ReadRange(BinaryReader reader){
			byte flags = reader.ReadByte();

			int? s = null, e = null;
			CSASMIndexer? si = null, ei = null;

			if((flags & 1) != 0)
				s = reader.ReadInt32();
			if((flags & 2) != 0)
				e = reader.ReadInt32();
			if((flags & 4) != 0)
				si = new CSASMIndexer(reader.ReadUInt32());
			if((flags & 8) != 0)
				ei = new CSASMIndexer(reader.ReadUInt32());

			if(s != null && e != null)
				return new CSASMRange(s.Value, e.Value);
			if(s != null && ei != null)
				return new CSASMRange(s.Value, ei.Value);
			if(si != null && ei != null)
				return new CSASMRange(si.Value, ei.Value);

			throw new IOException("Invalid \"~range\" format");
		}

		private static object ReadCSASMObj(string type, BinaryReader reader)
			=> type switch{
				"i8" => new SbytePrimitive(reader.ReadSByte()),
				"i16" => new ShortPrimitive(reader.ReadUInt16()),
				"i32" => new IntPrimitive(reader.ReadInt32()),
				"i64" => new LongPrimitive(reader.ReadInt64()),
				"u8" => new BytePrimitive(reader.ReadByte()),
				"u16" => new UshortPrimitive(reader.ReadUInt16()),
				"u32" => new UintPrimitive(reader.ReadUInt32()),
				"u64" => new UlongPrimitive(reader.ReadUInt64()),
				"f32" => new FloatPrimitive(reader.ReadSingle()),
				"f64" => new DoublePrimitive(reader.ReadDouble()),
				"char" => reader.ReadChar(),
				"str" => reader.ReadString(),
				"obj" => throw new InvalidOperationException("Type \"obj\" cannot be used as the type argument for file reading"),
				"^<u32>" => new CSASMIndexer(reader.ReadUInt32()),
				"~arr" => ReadArray(reader),
				"~set" => new ArithmeticSet(ReadArray(reader)),
				"~range" => ReadRange(reader),
				_ => throw new IOException($"Unknown type: {type}")
			};

		private static int Read7BitEncodedInt(BinaryReader reader){
			byte b;
			int ret = 0;
			int reads = 5;
			do{
				b = reader.ReadByte();
				if(--reads <= 0 && (b & 0x80) != 0)
					throw new IOException("Value read was not a 7-bit encoded Int32");

				ret |= b << (7 * (reads - 5));
			}while((b & 0x80) != 0);

			return ret;
		}

		private static void WriteCSASMObj(BinaryWriter writer, object obj){
			if(obj is null)
				throw new IOException("Value was null");

			string type = GetCSASMType(obj.GetType());
			IPrimitive ip = obj is IPrimitive ? obj as IPrimitive : null;
			switch(type){
				case "i8":
					writer.Write((sbyte)ip.Value);
					break;
				case "i16":
					writer.Write((short)ip.Value);
					break;
				case "i32":
					writer.Write((int)ip.Value);
					break;
				case "i64":
					writer.Write((long)ip.Value);
					break;
				case "u8":
					writer.Write((byte)ip.Value);
					break;
				case "u16":
					writer.Write((ushort)ip.Value);
					break;
				case "u32":
					writer.Write((uint)ip.Value);
					break;
				case "u64":
					writer.Write((ulong)ip.Value);
					break;
				case "char":
					writer.Write((char)obj);
					break;
				case "str":
					writer.Write(obj as string);
					break;
				case "^<u32>":
					writer.Write(((CSASMIndexer)obj).offset);
					break;
				case "~set":
					WriteCSASMObj(writer, ((ArithmeticSet)obj).ToArray());
					break;
				case "~range":
					WriteRange(writer, (CSASMRange)obj);
					break;
				default:
					if(type.StartsWith("~arr:"))
						WriteArray(writer, obj);
					else
						throw new IOException($"Unknown type: {type}");
					break;
			}
		}

		private static void WriteArray(BinaryWriter writer, object obj){
			string subType = GetCSASMType(obj.GetType().GetElementType());
			if(subType.StartsWith("~arr:"))
				throw new IOException("CSASM does not support arrays of arrays");

			Array arr = obj as Array;

			writer.Write(subType);
			Write7BitEncodedInt(writer, arr.Length);

			for(int i = 0; i < arr.Length; i++)
				WriteCSASMObj(writer, arr.GetValue(i));
		}

		private static void Write7BitEncodedInt(BinaryWriter writer, int value){
			do{
				writer.Write((byte)(value > 0x7f ? (value & 0x7f) | 0x80 : value));
				value >>= 7;
			}while(value > 0);
		}

		private static void WriteRange(BinaryWriter writer, CSASMRange range){
			byte b = 0;
			if(range.start != null)
				b |= 1;
			if(range.end != null)
				b |= 2;
			if(range.startIndexer != null)
				b |= 4;
			if(range.endIndexer != null)
				b |= 8;

			writer.Write(b);
			if((b & 1) != 0)
				writer.Write(range.start.Value);
			if((b & 2) != 0)
				writer.Write(range.end.Value);
			if((b & 4) != 0)
				writer.Write(range.startIndexer.Value.offset);
			if((b & 8) != 0)
				writer.Write(range.endIndexer.Value.offset);
		}
	}
}
