using System;

namespace CSASM.Core{
	/// <summary>
	/// Represents a list of values
	/// </summary>
	public class CSASMList{
		private object[] arr;

		public CSASMList(){
			arr = Array.Empty<object>();
		}

		public CSASMList(int capacity){
			arr = capacity > 0 ? new object[capacity] : Array.Empty<object>();
		}

		public object this[int index]{
			get{
				if(index < 0 || index >= arr.Length)
					throw new IndexOutOfRangeException("List index must be a non-negative integer and be less than the collection's length");

				return arr[index];
			}
			set{
				if(index < 0)
					throw new IndexOutOfRangeException("List index must be a non-negative integer");

				//Passing in a larger index into the setter will resize the list to fit that index
				if(index >= arr.Length)
					Capacity = index + 1;

				arr[index] = value;
			}
		}

		public int Capacity{
			get => arr.Length;
			set{
				if(value < 0)
					throw new ArgumentException("Capacity must be a non-negative integer");

				if(arr.Length != value){
					if(value > 0)
						Array.Resize(ref arr, value);
					else
						arr = Array.Empty<object>();
				}
			}
		}

		public int IndexOf(object value)
			=> Array.IndexOf(arr, value);

		public bool Contains(object value)
			=> IndexOf(value) >= 0;

		public object[] ToArray() => arr;

		public override string ToString()
			=> CSASMStack.FormatArray(arr);
	}
}
