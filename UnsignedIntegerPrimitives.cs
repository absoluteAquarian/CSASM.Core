namespace CSASM.Core{
	public struct UintPrimitive : IPrimitive, IPrimitiveInteger, IUnsignedPrimitiveInteger{
		readonly uint value;

		public UintPrimitive(uint value) => this.value = value;

		public override string ToString() => value.ToString();

		object IPrimitive.Value => value;

		object IPrimitive.Abs()
			=> new UintPrimitive(value);

		object IPrimitive.Add(IPrimitive other){
			if(other is UintPrimitive p)
				return new UintPrimitive(value + p.value);
			throw new ArithmeticException("add", this, other);
		}

		object IPrimitiveInteger.BitwiseNot(){
			return new UintPrimitive(~value);
		}

		int IPrimitive.CompareTo(IPrimitive other){
			if(other is UintPrimitive p)
				return value.CompareTo(p.value);
			throw new ArithmeticException("compare", this, other);
		}

		string IPrimitive.CSASMType()
			=> "u32";

		object IPrimitive.Decrement()
			=> new UintPrimitive(value - 1);

		object IPrimitive.Divide(IPrimitive other){
			if(other is UintPrimitive p)
				return new UintPrimitive(value / p.value);
			throw new ArithmeticException("divide", this, other);
		}

		object IPrimitive.Increment()
			=> new UintPrimitive(value + 1);

		object IPrimitive.Multiply(IPrimitive other){
			if(other is UintPrimitive p)
				return new UintPrimitive(value * p.value);
			throw new ArithmeticException("multiply", this, other);
		}

		object IPrimitive.Negate()
			=> throw new ArithmeticException("Operation \"negate\" cannot be used on unsigned integers");

		object IPrimitiveInteger.ArithmeticShiftLeft()
			=> new UintPrimitive(value << 1);

		object IPrimitiveInteger.ArithmeticShiftRight()
			=> new UintPrimitive(value >> 1);

		object IPrimitiveInteger.ArithmeticRotateLeft(ref byte flags){
			var ret = new UintPrimitive((value << 1) | Utility.GetCarry(flags));
			Utility.SetCarry(ref flags, (value & (1 << 31)) == 1);
			return ret;
		}

		object IPrimitiveInteger.ArithmeticRotateRight(ref byte flags){
			var ret = new UintPrimitive((value >> 1) | ((uint)Utility.GetCarry(flags) << 31));
			Utility.SetCarry(ref flags, (value & 1) == 1);
			return ret;
		}

		object IPrimitive.Remainder(IPrimitive other){
			if(other is UintPrimitive p)
				return new UintPrimitive(value % p.value);
			throw new ArithmeticException("remainder", this, other);
		}

		bool IPrimitive.SameType(IPrimitive other)
			=> other is UintPrimitive;

		object IPrimitive.Subtract(IPrimitive other){
			if(other is UintPrimitive p)
				return new UintPrimitive(value - p.value);
			throw new ArithmeticException("subtraction", this, other);
		}
	}

	public struct UshortPrimitive : IPrimitive, IPrimitiveInteger, IUnsignedPrimitiveInteger{
		readonly ushort value;

		public UshortPrimitive(ushort value) => this.value = value;

		public UshortPrimitive(int value) => this.value = (ushort)value;

		public override string ToString() => value.ToString();

		object IPrimitive.Value => throw new System.NotImplementedException();

		object IPrimitive.Abs()
			=> new UshortPrimitive(value);

		object IPrimitive.Add(IPrimitive other){
			if(other is UshortPrimitive p)
				return new UshortPrimitive(value + p.value);
			throw new ArithmeticException("add", this, other);
		}

		object IPrimitiveInteger.BitwiseNot(){
			return new UshortPrimitive(~value);
		}

		int IPrimitive.CompareTo(IPrimitive other){
			if(other is UshortPrimitive p)
				return value.CompareTo(p.value);
			throw new ArithmeticException("compare", this, other);
		}

		string IPrimitive.CSASMType() => "u16";

		object IPrimitive.Decrement()
			=> new UshortPrimitive(value - 1);

		object IPrimitive.Divide(IPrimitive other){
			if(other is UshortPrimitive p)
				return new UshortPrimitive(value / p.value);
			throw new ArithmeticException("divide", this, other);
		}

		object IPrimitive.Increment()
			=> new UshortPrimitive(value + 1);

		object IPrimitive.Multiply(IPrimitive other){
			if(other is UshortPrimitive p)
				return new UshortPrimitive(value * p.value);
			throw new ArithmeticException("multiply", this, other);
		}

		object IPrimitive.Negate()
			=> throw new ArithmeticException("Operation \"negate\" cannot be used on unsigned integers");

		object IPrimitiveInteger.ArithmeticShiftLeft()
			=> new UshortPrimitive(value << 1);

		object IPrimitiveInteger.ArithmeticShiftRight()
			=> new UshortPrimitive(value >> 1);

		object IPrimitiveInteger.ArithmeticRotateLeft(ref byte flags){
			var ret = new UshortPrimitive((value << 1) | Utility.GetCarry(flags));
			Utility.SetCarry(ref flags, (value & (1 << 15)) == 1);
			return ret;
		}

		object IPrimitiveInteger.ArithmeticRotateRight(ref byte flags){
			var ret = new UshortPrimitive((value >> 1) | (Utility.GetCarry(flags) << 15));
			Utility.SetCarry(ref flags, (value & 1) == 1);
			return ret;
		}

		object IPrimitive.Remainder(IPrimitive other){
			if(other is UshortPrimitive p)
				return new UshortPrimitive(value % p.value);
			throw new ArithmeticException("remainder", this, other);
		}

		bool IPrimitive.SameType(IPrimitive other)
			=> other is UshortPrimitive;

		object IPrimitive.Subtract(IPrimitive other){
			if(other is UshortPrimitive p)
				return new UshortPrimitive(value - p.value);
			throw new ArithmeticException("subtraction", this, other);
		}
	}

	public struct BytePrimitive : IPrimitive, IPrimitiveInteger, IUnsignedPrimitiveInteger{
		readonly byte value;

		public BytePrimitive(byte value) => this.value = value;

		public BytePrimitive(int value) => this.value = (byte)value;

		public override string ToString() => value.ToString();

		object IPrimitive.Value => value;

		object IPrimitive.Abs()
			=> new BytePrimitive(value);

		object IPrimitive.Add(IPrimitive other){
			if(other is BytePrimitive p)
				return new BytePrimitive(value + p.value);
			throw new ArithmeticException("add", this, other);
		}

		object IPrimitiveInteger.BitwiseNot(){
			return new BytePrimitive(~value);
		}

		int IPrimitive.CompareTo(IPrimitive other){
			if(other is BytePrimitive p)
				return value.CompareTo(p.value);
			throw new ArithmeticException("compare", this, other);
		}

		string IPrimitive.CSASMType() => "i8";

		object IPrimitive.Decrement()
			=> new BytePrimitive(value - 1);

		object IPrimitive.Divide(IPrimitive other){
			if(other is BytePrimitive p)
				return new BytePrimitive(value / p.value);
			throw new ArithmeticException("divide", this, other);
		}

		object IPrimitive.Increment()
			=> new BytePrimitive(value + 1);

		object IPrimitive.Multiply(IPrimitive other){
			if(other is BytePrimitive p)
				return new BytePrimitive(value * p.value);
			throw new ArithmeticException("multiply", this, other);
		}

		object IPrimitive.Negate()
			=> throw new ArithmeticException("Operation \"negate\" cannot be used on unsigned integers");

		object IPrimitiveInteger.ArithmeticShiftLeft()
			=> new BytePrimitive(value << 1);

		object IPrimitiveInteger.ArithmeticShiftRight()
			=> new BytePrimitive(value >> 1);

		object IPrimitiveInteger.ArithmeticRotateLeft(ref byte flags){
			var ret = new BytePrimitive((value << 1) | Utility.GetCarry(flags));
			Utility.SetCarry(ref flags, (value & (1 << 7)) == 1);
			return ret;
		}

		object IPrimitiveInteger.ArithmeticRotateRight(ref byte flags){
			var ret = new BytePrimitive((value >> 1) | (Utility.GetCarry(flags) << 7));
			Utility.SetCarry(ref flags, (value & 1) == 1);
			return ret;
		}

		object IPrimitive.Remainder(IPrimitive other){
			if(other is BytePrimitive p)
				return new BytePrimitive(value % p.value);
			throw new ArithmeticException("remainder", this, other);
		}

		bool IPrimitive.SameType(IPrimitive other)
			=> other is BytePrimitive;

		object IPrimitive.Subtract(IPrimitive other){
			if(other is BytePrimitive p)
				return new BytePrimitive(value - p.value);
			throw new ArithmeticException("subtraction", this, other);
		}
	}

	public struct UlongPrimitive : IPrimitive, IPrimitiveInteger, IUnsignedPrimitiveInteger{
		readonly ulong value;

		public UlongPrimitive(ulong value) => this.value = value;

		public override string ToString() => value.ToString();

		object IPrimitive.Value => value;

		object IPrimitive.Abs()
			=> new UlongPrimitive(value);

		object IPrimitive.Add(IPrimitive other){
			if(other is UlongPrimitive p)
				return new UlongPrimitive(value + p.value);
			throw new ArithmeticException("add", this, other);
		}

		object IPrimitiveInteger.BitwiseNot(){
			return new UlongPrimitive(~value);
		}

		int IPrimitive.CompareTo(IPrimitive other){
			if(other is UlongPrimitive p)
				return value.CompareTo(p.value);
			throw new ArithmeticException("compare", this, other);
		}

		string IPrimitive.CSASMType() => "u64";

		object IPrimitive.Decrement()
			=> new UlongPrimitive(value - 1);

		object IPrimitive.Divide(IPrimitive other){
			if(other is UlongPrimitive p)
				return new UlongPrimitive(value / p.value);
			throw new ArithmeticException("divide", this, other);
		}

		object IPrimitive.Increment()
			=> new UlongPrimitive(value + 1);

		object IPrimitive.Multiply(IPrimitive other){
			if(other is UlongPrimitive p)
				return new UlongPrimitive(value * p.value);
			throw new ArithmeticException("multiply", this, other);
		}

		object IPrimitive.Negate()
			=> throw new ArithmeticException("Operation \"negate\" cannot be used on unsigned integers");

		object IPrimitiveInteger.ArithmeticShiftLeft()
			=> new UlongPrimitive(value << 1);

		object IPrimitiveInteger.ArithmeticShiftRight()
			=> new UlongPrimitive(value >> 1);

		object IPrimitiveInteger.ArithmeticRotateLeft(ref byte flags){
			unchecked{
				var ret = new UlongPrimitive((value << 1) | Utility.GetCarry(flags));
				Utility.SetCarry(ref flags, (value & ((ulong)1 << 63)) == 1);
				return ret;
			}
		}

		object IPrimitiveInteger.ArithmeticRotateRight(ref byte flags){
			unchecked{
				var ret = new UlongPrimitive((value >> 1) | ((ulong)Utility.GetCarry(flags) << 63));
				Utility.SetCarry(ref flags, (value & 1) == 1);
				return ret;
			}
		}

		object IPrimitive.Remainder(IPrimitive other){
			if(other is UlongPrimitive p)
				return new UlongPrimitive(value % p.value);
			throw new ArithmeticException("remainder", this, other);
		}

		bool IPrimitive.SameType(IPrimitive other)
			=> other is UlongPrimitive;

		object IPrimitive.Subtract(IPrimitive other){
			if(other is UlongPrimitive p)
				return new UlongPrimitive(value - p.value);
			throw new ArithmeticException("subtraction", this, other);
		}
	}
}
