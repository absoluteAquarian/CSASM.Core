namespace CSASM.Core{
	public struct FloatPrimitive : IPrimitive{
		readonly float value;

		public FloatPrimitive(float value) => this.value = value;

		public override string ToString() => value.ToString();

		object IPrimitive.Value => value;

		object IPrimitive.Abs()
			=> new FloatPrimitive(value >= 0 ? value : -value);

		object IPrimitive.Add(IPrimitive other){
			if(other is FloatPrimitive p)
				return new FloatPrimitive(value + p.value);
			throw new ArithmeticException("add", this, other);
		}

		int IPrimitive.CompareTo(IPrimitive other){
			if(other is FloatPrimitive p)
				return value.CompareTo(p.value);
			throw new ArithmeticException("compare", this, other);
		}

		string IPrimitive.CSASMType() => "f32";

		object IPrimitive.Decrement()
			=> new FloatPrimitive(value - 1);

		object IPrimitive.Divide(IPrimitive other){
			if(other is FloatPrimitive p)
				return new FloatPrimitive(value / p.value);
			throw new ArithmeticException("divide", this, other);
		}

		object IPrimitive.Increment()
			=> new FloatPrimitive(value + 1);

		object IPrimitive.Multiply(IPrimitive other){
			if(other is FloatPrimitive p)
				return new FloatPrimitive(value * p.value);
			throw new ArithmeticException("multiply", this, other);
		}

		object IPrimitive.Negate()
			=> new FloatPrimitive(-value);

		bool IPrimitive.SameType(IPrimitive other)
			=> other is FloatPrimitive;

		object IPrimitive.Subtract(IPrimitive other){
			if(other is FloatPrimitive p)
				return new FloatPrimitive(value - p.value);
			throw new ArithmeticException("subtraction", this, other);
		}
	}

	public struct DoublePrimitive : IPrimitive{
		readonly double value;

		public override string ToString() => value.ToString();

		public DoublePrimitive(double value) => this.value = value;

		object IPrimitive.Value => value;

		object IPrimitive.Abs()
			=> new DoublePrimitive(value >= 0 ? value : -value);

		object IPrimitive.Add(IPrimitive other){
			if(other is DoublePrimitive p)
				return new DoublePrimitive(value + p.value);
			throw new ArithmeticException("add", this, other);
		}

		int IPrimitive.CompareTo(IPrimitive other){
			if(other is DoublePrimitive p)
				return value.CompareTo(p.value);
			throw new ArithmeticException("compare", this, other);
		}

		string IPrimitive.CSASMType() => "f64";

		object IPrimitive.Decrement()
			=> new DoublePrimitive(value - 1);

		object IPrimitive.Divide(IPrimitive other){
			if(other is DoublePrimitive p)
				return new DoublePrimitive(value / p.value);
			throw new ArithmeticException("divide", this, other);
		}

		object IPrimitive.Increment()
			=> new DoublePrimitive(value + 1);

		object IPrimitive.Multiply(IPrimitive other){
			if(other is DoublePrimitive p)
				return new DoublePrimitive(value * p.value);
			throw new ArithmeticException("multiply", this, other);
		}

		object IPrimitive.Negate()
			=> new DoublePrimitive(-value);

		bool IPrimitive.SameType(IPrimitive other)
			=> other is DoublePrimitive;

		object IPrimitive.Subtract(IPrimitive other){
			if(other is DoublePrimitive p)
				return new DoublePrimitive(value - p.value);
			throw new ArithmeticException("subtraction", this, other);
		}
	}
}
