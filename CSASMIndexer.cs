using System;

namespace CSASM.Core{
	public struct CSASMIndexer{
		public uint offset;

		public CSASMIndexer(uint offset){
			this.offset = offset;
		}

		public int GetIndex(object obj){
			if(obj is Array a)
				return a.Length - (int)offset;
			else if(obj is string s)
				return s.Length - (int)offset;

			throw new ArgumentException("Object did not refer to an array or string instance.");
		}

		public override string ToString() => $"^{offset}";
	}
}
