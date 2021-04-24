namespace CSASM.Core{
	public struct IntPrimitive : IPrimitive, IPrimitiveInteger{
		readonly int value;

		public IntPrimitive(int value) => this.value = value;

		public override string ToString() => value.ToString();

		public override int GetHashCode() => value.GetHashCode();

		object IPrimitive.Value => value;

		object IPrimitive.Abs()
			=> new IntPrimitive(value >= 0 ? value : -value);

		object IPrimitive.Add(IPrimitive other){
			if(other is IntPrimitive p)
				return new IntPrimitive(value + p.value);
			throw new ArithmeticException("add", this, other);
		}

		object IPrimitiveInteger.BitwiseNot(){
			return new IntPrimitive(~value);
		}

		int IPrimitive.CompareTo(IPrimitive other){
			if(other is IntPrimitive p)
				return value.CompareTo(p.value);
			throw new ArithmeticException("compare", this, other);
		}

		string IPrimitive.CSASMType()
			=> "i32";

		object IPrimitive.Decrement()
			=> new IntPrimitive(value - 1);

		object IPrimitive.Divide(IPrimitive other){
			if(other is IntPrimitive p)
				return new IntPrimitive(value / p.value);
			throw new ArithmeticException("divide", this, other);
		}

		object IPrimitive.Increment()
			=> new IntPrimitive(value + 1);

		object IPrimitive.Multiply(IPrimitive other){
			if(other is IntPrimitive p)
				return new IntPrimitive(value * p.value);
			throw new ArithmeticException("multiply", this, other);
		}

		object IPrimitive.Negate()
			=> new IntPrimitive(-value);

		object IPrimitiveInteger.ArithmeticShiftLeft()
			=> new IntPrimitive(value << 1);

		object IPrimitiveInteger.ArithmeticShiftRight()
			=> new IntPrimitive(value >> 1);

		object IPrimitiveInteger.ArithmeticRotateLeft(ref byte flags){
			var ret = new IntPrimitive((value << 1) | Utility.GetCarry(flags));
			Utility.SetCarry(ref flags, (value & (1 << 31)) == 1);
			return ret;
		}

		object IPrimitiveInteger.ArithmeticRotateRight(ref byte flags){
			var ret = new IntPrimitive((value >> 1) | (Utility.GetCarry(flags) << 31));
			Utility.SetCarry(ref flags, (value & 1) == 1);
			return ret;
		}

		object IPrimitive.Remainder(IPrimitive other){
			if(other is IntPrimitive p)
				return new IntPrimitive(value % p.value);
			throw new ArithmeticException("remainder", this, other);
		}

		bool IPrimitive.SameType(IPrimitive other)
			=> other is IntPrimitive;

		object IPrimitive.Subtract(IPrimitive other){
			if(other is IntPrimitive p)
				return new IntPrimitive(value - p.value);
			throw new ArithmeticException("subtraction", this, other);
		}

		object IPrimitiveInteger.BitwiseAnd(IPrimitive other){
			if(other is IntPrimitive p)
				return new IntPrimitive(value & p.value);
			throw new ArithmeticException("and", this, other);
		}

		object IPrimitiveInteger.BitwiseOr(IPrimitive other){
			if(other is IntPrimitive p)
				return new IntPrimitive(value | p.value);
			throw new ArithmeticException("or", this, other);
		}

		object IPrimitiveInteger.BitwiseXor(IPrimitive other){
			if(other is IntPrimitive p)
				return new IntPrimitive(value ^ p.value);
			throw new ArithmeticException("xor", this, other);
		}

		object IPrimitiveInteger.GetBit(byte bit)
			=> bit < 32 ? value & (1 << bit) : 0;

		int IPrimitiveInteger.BitSize() => 32;
	}

	public struct ShortPrimitive : IPrimitive, IPrimitiveInteger{
		readonly short value;

		public ShortPrimitive(short value) => this.value = value;

		public ShortPrimitive(int value) => this.value = (short)value;

		public override string ToString() => value.ToString();

		object IPrimitive.Value => throw new System.NotImplementedException();

		object IPrimitive.Abs()
			=> new ShortPrimitive(value >= 0 ? value : -value);

		object IPrimitive.Add(IPrimitive other){
			if(other is ShortPrimitive p)
				return new ShortPrimitive(value + p.value);
			throw new ArithmeticException("add", this, other);
		}

		object IPrimitiveInteger.BitwiseNot(){
			return new ShortPrimitive(~value);
		}

		int IPrimitive.CompareTo(IPrimitive other){
			if(other is ShortPrimitive p)
				return value.CompareTo(p.value);
			throw new ArithmeticException("compare", this, other);
		}

		string IPrimitive.CSASMType() => "i16";

		object IPrimitive.Decrement()
			=> new ShortPrimitive(value - 1);

		object IPrimitive.Divide(IPrimitive other){
			if(other is ShortPrimitive p)
				return new ShortPrimitive(value / p.value);
			throw new ArithmeticException("divide", this, other);
		}

		object IPrimitive.Increment()
			=> new ShortPrimitive(value + 1);

		object IPrimitive.Multiply(IPrimitive other){
			if(other is ShortPrimitive p)
				return new ShortPrimitive(value * p.value);
			throw new ArithmeticException("multiply", this, other);
		}

		object IPrimitive.Negate()
			=> new ShortPrimitive(-value);

		object IPrimitiveInteger.ArithmeticShiftLeft()
			=> new ShortPrimitive(value << 1);

		object IPrimitiveInteger.ArithmeticShiftRight()
			=> new ShortPrimitive(value >> 1);

		object IPrimitiveInteger.ArithmeticRotateLeft(ref byte flags){
			var ret = new ShortPrimitive((value << 1) | Utility.GetCarry(flags));
			Utility.SetCarry(ref flags, (value & (1 << 15)) == 1);
			return ret;
		}

		object IPrimitiveInteger.ArithmeticRotateRight(ref byte flags){
			var ret = new ShortPrimitive((value >> 1) | (Utility.GetCarry(flags) << 15));
			Utility.SetCarry(ref flags, (value & 1) == 1);
			return ret;
		}

		object IPrimitive.Remainder(IPrimitive other){
			if(other is ShortPrimitive p)
				return new ShortPrimitive(value % p.value);
			throw new ArithmeticException("remainder", this, other);
		}

		bool IPrimitive.SameType(IPrimitive other)
			=> other is ShortPrimitive;

		object IPrimitive.Subtract(IPrimitive other){
			if(other is ShortPrimitive p)
				return new ShortPrimitive(value - p.value);
			throw new ArithmeticException("subtraction", this, other);
		}

		object IPrimitiveInteger.BitwiseAnd(IPrimitive other){
			if(other is ShortPrimitive p)
				return new ShortPrimitive(value & p.value);
			throw new ArithmeticException("and", this, other);
		}

		object IPrimitiveInteger.BitwiseOr(IPrimitive other){
			if(other is ShortPrimitive p)
				return new ShortPrimitive(value | p.value);
			throw new ArithmeticException("or", this, other);
		}

		object IPrimitiveInteger.BitwiseXor(IPrimitive other){
			if(other is ShortPrimitive p)
				return new ShortPrimitive(value ^ p.value);
			throw new ArithmeticException("xor", this, other);
		}

		object IPrimitiveInteger.GetBit(byte bit)
			=> bit < 16 ? value & (1 << bit) : 0;

		int IPrimitiveInteger.BitSize() => 16;
	}

	public struct SbytePrimitive : IPrimitive, IPrimitiveInteger{
		readonly sbyte value;

		public SbytePrimitive(sbyte value) => this.value = value;

		public SbytePrimitive(int value) => this.value = (sbyte)value;

		public override string ToString() => value.ToString();

		object IPrimitive.Value => value;

		object IPrimitive.Abs()
			=> new SbytePrimitive(value >= 0 ? value : -value);

		object IPrimitive.Add(IPrimitive other){
			if(other is SbytePrimitive p)
				return new SbytePrimitive(value + p.value);
			throw new ArithmeticException("add", this, other);
		}

		object IPrimitiveInteger.BitwiseNot(){
			return new SbytePrimitive(~value);
		}

		int IPrimitive.CompareTo(IPrimitive other){
			if(other is SbytePrimitive p)
				return value.CompareTo(p.value);
			throw new ArithmeticException("compare", this, other);
		}

		string IPrimitive.CSASMType() => "u8";

		object IPrimitive.Decrement()
			=> new SbytePrimitive(value - 1);

		object IPrimitive.Divide(IPrimitive other){
			if(other is SbytePrimitive p)
				return new SbytePrimitive(value / p.value);
			throw new ArithmeticException("divide", this, other);
		}

		object IPrimitive.Increment()
			=> new SbytePrimitive(value + 1);

		object IPrimitive.Multiply(IPrimitive other){
			if(other is SbytePrimitive p)
				return new SbytePrimitive(value * p.value);
			throw new ArithmeticException("multiply", this, other);
		}

		object IPrimitive.Negate()
			=> new SbytePrimitive(-value);

		object IPrimitiveInteger.ArithmeticShiftLeft()
			=> new SbytePrimitive(value << 1);

		object IPrimitiveInteger.ArithmeticShiftRight()
			=> new SbytePrimitive(value >> 1);

		object IPrimitiveInteger.ArithmeticRotateLeft(ref byte flags){
			var ret = new SbytePrimitive((value << 1) | Utility.GetCarry(flags));
			Utility.SetCarry(ref flags, (value & (1 << 7)) == 1);
			return ret;
		}

		object IPrimitiveInteger.ArithmeticRotateRight(ref byte flags){
			var ret = new SbytePrimitive((value >> 1) | (Utility.GetCarry(flags) << 7));
			Utility.SetCarry(ref flags, (value & 1) == 1);
			return ret;
		}

		object IPrimitive.Remainder(IPrimitive other){
			if(other is SbytePrimitive p)
				return new SbytePrimitive(value % p.value);
			throw new ArithmeticException("remainder", this, other);
		}

		bool IPrimitive.SameType(IPrimitive other)
			=> other is SbytePrimitive;

		object IPrimitive.Subtract(IPrimitive other){
			if(other is SbytePrimitive p)
				return new SbytePrimitive(value - p.value);
			throw new ArithmeticException("subtraction", this, other);
		}

		object IPrimitiveInteger.BitwiseAnd(IPrimitive other){
			if(other is SbytePrimitive p)
				return new SbytePrimitive(value & p.value);
			throw new ArithmeticException("and", this, other);
		}

		object IPrimitiveInteger.BitwiseOr(IPrimitive other){
			if(other is SbytePrimitive p)
				return new SbytePrimitive(value | p.value);
			throw new ArithmeticException("or", this, other);
		}

		object IPrimitiveInteger.BitwiseXor(IPrimitive other){
			if(other is SbytePrimitive p)
				return new SbytePrimitive(value ^ p.value);
			throw new ArithmeticException("xor", this, other);
		}

		object IPrimitiveInteger.GetBit(byte bit)
			=> bit < 8 ? value & (1 << bit) : 0;

		int IPrimitiveInteger.BitSize() => 8;
	}

	public struct LongPrimitive : IPrimitive, IPrimitiveInteger{
		readonly long value;

		public LongPrimitive(long value) => this.value = value;

		public override string ToString() => value.ToString();

		object IPrimitive.Value => value;

		object IPrimitive.Abs()
			=> new LongPrimitive(value >= 0 ? value : -value);

		object IPrimitive.Add(IPrimitive other){
			if(other is LongPrimitive p)
				return new LongPrimitive(value + p.value);
			throw new ArithmeticException("add", this, other);
		}

		object IPrimitiveInteger.BitwiseNot(){
			return new LongPrimitive(~value);
		}

		int IPrimitive.CompareTo(IPrimitive other){
			if(other is LongPrimitive p)
				return value.CompareTo(p.value);
			throw new ArithmeticException("compare", this, other);
		}

		string IPrimitive.CSASMType() => "i64";

		object IPrimitive.Decrement()
			=> new LongPrimitive(value - 1);

		object IPrimitive.Divide(IPrimitive other){
			if(other is LongPrimitive p)
				return new LongPrimitive(value / p.value);
			throw new ArithmeticException("divide", this, other);
		}

		object IPrimitive.Increment()
			=> new LongPrimitive(value + 1);

		object IPrimitive.Multiply(IPrimitive other){
			if(other is LongPrimitive p)
				return new LongPrimitive(value * p.value);
			throw new ArithmeticException("multiply", this, other);
		}

		object IPrimitive.Negate()
			=> new LongPrimitive(-value);

		object IPrimitiveInteger.ArithmeticShiftLeft()
			=> new LongPrimitive(value << 1);

		object IPrimitiveInteger.ArithmeticShiftRight()
			=> new LongPrimitive(value >> 1);

		object IPrimitiveInteger.ArithmeticRotateLeft(ref byte flags){
			var ret = new LongPrimitive((value << 1) | Utility.GetCarry(flags));
			Utility.SetCarry(ref flags, (value & ((long)1 << 63)) == 1);
			return ret;
		}

		object IPrimitiveInteger.ArithmeticRotateRight(ref byte flags){
			unchecked{
				var ret = new LongPrimitive((value >> 1) | ((long)Utility.GetCarry(flags) << 63));
				Utility.SetCarry(ref flags, (value & 1) == 1);
				return ret;
			}
		}

		object IPrimitive.Remainder(IPrimitive other){
			if(other is LongPrimitive p)
				return new LongPrimitive(value % p.value);
			throw new ArithmeticException("remainder", this, other);
		}

		bool IPrimitive.SameType(IPrimitive other)
			=> other is LongPrimitive;

		object IPrimitive.Subtract(IPrimitive other){
			if(other is LongPrimitive p)
				return new LongPrimitive(value - p.value);
			throw new ArithmeticException("subtraction", this, other);
		}

		object IPrimitiveInteger.BitwiseAnd(IPrimitive other){
			if(other is LongPrimitive p)
				return new LongPrimitive(value & p.value);
			throw new ArithmeticException("and", this, other);
		}

		object IPrimitiveInteger.BitwiseOr(IPrimitive other){
			if(other is LongPrimitive p)
				return new LongPrimitive(value | p.value);
			throw new ArithmeticException("or", this, other);
		}

		object IPrimitiveInteger.BitwiseXor(IPrimitive other){
			if(other is LongPrimitive p)
				return new LongPrimitive(value ^ p.value);
			throw new ArithmeticException("xor", this, other);
		}

		object IPrimitiveInteger.GetBit(byte bit)
			=> bit < 64 ? value & (1L << bit) : 0;

		int IPrimitiveInteger.BitSize() => 64;
	}
}
