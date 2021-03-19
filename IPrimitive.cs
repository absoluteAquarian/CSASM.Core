using System;

namespace CSASM.Core{
	public interface IPrimitive{
		object Value{ get; }

		int CompareTo(IPrimitive other);
		bool SameType(IPrimitive other);

		object Add(IPrimitive other);
		object Subtract(IPrimitive other);
		object Multiply(IPrimitive other);
		object Divide(IPrimitive other);
		object Negate();
		
		object Abs();
		object Increment();
		object Decrement();

		string CSASMType();
	}
}
