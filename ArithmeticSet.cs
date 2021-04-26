using System;
using System.Collections.Generic;
using System.Text;

namespace CSASM.Core{
	public struct ArithmeticSet{
		private readonly IntPrimitive[] set;

		public static readonly ArithmeticSet EmptySet = new ArithmeticSet(new IntPrimitive[0]);

		public ArithmeticSet(Array set){
			Type type;
			string typeName;
			if(set is IntPrimitive[] ip){
				this.set = null;
				this.set = ResolveSet(ip);
			}else if(typeof(IPrimitive).IsAssignableFrom(type = set.GetType().GetElementType()) && (typeName = Utility.GetCSASMType(type)) != "f32" && typeName != "f64"){
				this.set = new IntPrimitive[set.Length];

				for(int i = 0; i < set.Length; i++)
					this.set[i] = new IntPrimitive(Convert.ToInt32((set.GetValue(i) as IPrimitive).Value));

				this.set = ResolveSet(this.set);
			}else
				throw new ArithmeticException("Sets can only be initialized via an array of integers");
		}

		public ArithmeticSet(Range range){
			if(range.startIndexer != null || range.endIndexer != null)
				throw new ArgumentException("Sets cannot be initialized with ranges that use indexer bounds");

			set = null;
			set = ResolveSet(range.ToArray());
		}

		public IntPrimitive[] ToArray() => (IntPrimitive[])set.Clone();

		public ArithmeticSet Union(ArithmeticSet other){
			if(set.Length == 0)
				return other;

			if(other.set.Length == 0)
				return this;

			IntPrimitive[] newSet = new IntPrimitive[set.Length + other.set.Length];
			Array.Copy(set, 0, newSet, 0, set.Length);
			Array.Copy(other.set, 0, newSet, set.Length, other.set.Length);

			newSet = ResolveSet(newSet);

			return new ArithmeticSet(newSet);
		}

		public ArithmeticSet Intersection(ArithmeticSet other){
			List<IntPrimitive> list = new List<IntPrimitive>(set);
			List<IntPrimitive> otherList = new List<IntPrimitive>(other.set);

			List<IntPrimitive> ret = new List<IntPrimitive>(this.Union(other).set);
			for(int i = 0; i < ret.Count; i++){
				var item = ret[i];
				if(!list.Contains(item) || !otherList.Contains(item)){
					ret.RemoveAt(i);
					i--;
				}
			}

			return new ArithmeticSet(ret.ToArray());
		}

		public ArithmeticSet Difference(ArithmeticSet other){
			List<IntPrimitive> list = new List<IntPrimitive>(set);
			List<IntPrimitive> otherList = new List<IntPrimitive>(other.set);

			for(int i = 0; i < list.Count; i++){
				if(otherList.Contains(list[i])){
					list.RemoveAt(i);
					i--;
				}
			}

			return new ArithmeticSet(list.ToArray());
		}

		public bool IsDisjoint(ArithmeticSet other) => this.Intersection(other).set.Length == EmptySet.set.Length;

		private IntPrimitive[] ResolveSet(IntPrimitive[] collection){
			if(collection.Length < 2)
				return (IntPrimitive[])collection.Clone();

			IntPrimitive[] sorted = new IntPrimitive[collection.Length];
			collection.CopyTo(sorted, 0);

			SortAlgorithms.TimSort(sorted);

			List<IntPrimitive> list = new List<IntPrimitive>(sorted);

			for(int i = 0; i < list.Count - 1; i++){
				if((list[i] as IComparable<IntPrimitive>).CompareTo(list[i + 1]) == 0){
					list.RemoveAt(i);
					i--;
				}
			}

			return list.ToArray();
		}

		public static bool operator ==(ArithmeticSet first, ArithmeticSet second){
			if(first.set.Length != second.set.Length)
				return false;

			var intersection = first.Intersection(second);

			return intersection.set.Length == first.set.Length && intersection.set.Length == second.set.Length;
		}

		public static bool operator !=(ArithmeticSet first, ArithmeticSet second) => !(first == second);

		public override bool Equals(object obj) => obj is ArithmeticSet other && this == other;

		public override int GetHashCode() => base.GetHashCode();

		public override string ToString(){
			if(set.Length == 0)
				return "{ empty set }";
			
			if(set.Length == 1)
				return $"{{ {set[0]} }}";

			StringBuilder sb = new StringBuilder();
			sb.Append($"{{ {set[0]}");

			for(int i = 1; i < set.Length; i++)
				sb.Append($", {set[i]}");

			sb.Append(" }");

			return sb.ToString();
		}
	}
}
