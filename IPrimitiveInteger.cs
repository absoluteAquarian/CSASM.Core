namespace CSASM.Core{
	public interface IPrimitiveInteger{
		object ArithmeticShiftLeft();
		object ArithmeticShiftRight();

		object ArithmeticRotateLeft(ref byte flags);
		object ArithmeticRotateRight(ref byte flags);

		object BitwiseAnd(IPrimitive other);
		object BitwiseNot();
		object BitwiseOr(IPrimitive other);
		object BitwiseXor(IPrimitive other);

		object GetBit(byte bit);
	}
}
