using System;

namespace CSASM.Core{
	public struct Range{
		public readonly int? start, end;
		public readonly Indexer? startIndexer, endIndexer;

		public Range(int start, int end){
			if(end < start)
				throw new ArithmeticException("Range end value cannot be less than range start value");

			this.start = start;
			startIndexer = null;
			this.end = end;
			endIndexer = null;
		}

		public Range(int start, Indexer end){
			if(start < 0)
				throw new ArithmeticException("Range start value cannot be less than zero when using an end indexer");

			this.start = start;
			startIndexer = null;
			this.end = null;
			endIndexer = end;
		}

		public Range(Indexer start, Indexer end){
			if(start.offset < end.offset)
				throw new ArithmeticException("Range end indexer offset cannot be less than range start indexer offset");

			this.start = null;
			startIndexer = start;
			this.end = null;
			endIndexer = end;
		}

		public IntPrimitive[] ToArray(object indexerSource = null){
			GetValues(indexerSource, out int indexStart, out int indexEnd);

			if(indexStart > indexEnd)
				throw new InvalidOperationException("Range start index must be less than end index");
			
			IntPrimitive[] ret = new IntPrimitive[indexEnd - indexStart + 1];

			for(int i = indexStart; i <= indexEnd; i++)
				ret[i - indexStart] = new IntPrimitive(i);
			
			return ret;
		}

		public void GetValues(object source, out int start, out int end){
			start = this.start ?? startIndexer?.GetIndex(source) ?? throw new ArithmeticException("Range object did not have a start location");
			end = this.end ?? endIndexer?.GetIndex(source) ?? throw new ArithmeticException("Range object did not have an end location");
		}

		public static bool operator ==(Range first, Range second) => first.start == second.start && first.end == second.end;

		public static bool operator !=(Range first, Range second) => first.start != second.start || first.end == second.end;

		public override bool Equals(object obj) => obj is Range range && this == range;

		public override int GetHashCode() => base.GetHashCode();

		public override string ToString() => $"[{start}..{end}]";
	}
}
