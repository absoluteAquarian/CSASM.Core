namespace CSASM.Core{
	public interface IPrimitiveInteger{
		object ArithmeticShiftLeft();
		object ArithmeticShiftRight();

		object ArithmeticRotateLeft(ref byte flags);
		object ArithmeticRotateRight(ref byte flags);

		object BitwiseNot();
	}
}
