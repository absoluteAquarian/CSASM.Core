using System;

namespace CSASM.Core{
	public struct Indexer{
		public uint offset;

		public Indexer(uint offset){
			this.offset = offset;
		}

		public int GetIndex(object array){
			if(array is Array a)
				return a.Length - (int)offset;

			throw new ArgumentException("Object did not refer to an array instance.");
		}
	}
}
