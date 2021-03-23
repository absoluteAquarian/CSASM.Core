using System;

namespace CSASM.Core{
	public class AccumulatorException : Exception{
		public AccumulatorException(string message) : base(message){ }
	}

	public class ArithmeticException : Exception{
		public ArithmeticException(string message) : base(message){ }

		public ArithmeticException(string operation, Type first, Type second) : base($"Could not perform operation \"{operation}\" on types \"{Utility.GetCSASMType(first)}\" and \"{Utility.GetCSASMType(second)}\""){ }

		public ArithmeticException(string operation, IPrimitive p1, IPrimitive p2) : this(operation, p1.Value.GetType(), p2.Value.GetType()){ }
	}

	public class StackException : Exception{
		public StackException(string message) : base(message){ }

		public StackException(string instruction, object value) : base($"Stack value had an invalid type for the \"{instruction}\" instruction: " +
			$"{(value is null ? "null reference" : Utility.GetCSASMType(value.GetType()))}"){ }

		public StackException(string instruction, object value, object value2) : base($"Stack values had invalid types for the \"{instruction}\" instruction: " +
			$"{(value is null ? "null reference" : Utility.GetCSASMType(value.GetType()))}," +
			$"{(value is null ? "null reference" : Utility.GetCSASMType(value2.GetType()))}"){ }
	}

	public class ThrowException : Exception{
		public ThrowException(string message) : base(message){ }

		public ThrowException(string message, Exception innerException) : base(message, innerException){ }
	}
}
