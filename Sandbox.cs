using System;
using System.Linq;
using System.Reflection;

namespace CSASM.Core{
	public static class Sandbox{
		public static bool verbose;
		public static bool reportStackUsage;

		public static int Main(MethodInfo main, int stackSize, string[] args){
			Ops.stack = new CSASMStack(stackSize);

			if(args.Length > 0){
				foreach(string arg in args){
					if(arg == "-verbose")
						verbose = true;
					else if(arg == "-reportstack")
						reportStackUsage = true;
				}
			}

			try{
				main.Invoke(null, null);
				return 0;
			}catch(AccumulatorException aex){
				Console.WriteLine($"AccumulatorException thrown: {aex.Message}");
			}catch(ThrowException tex){
				Console.WriteLine($"ThrowException thrown: {tex.Message}");
			}catch(StackException stex){
				Console.WriteLine($"StackException thrown: {stex.Message}");
			}catch(Exception ex){
				if(ex.InnerException != null)
					ex = ex.InnerException;

				string message = ex.ToString();
				//Remove the part saying the name since it's said below
				message = message.Substring(message.IndexOf(":") + 2);
				Console.WriteLine($"{ex.GetType().Name} thrown in compiled code:\n   {message}");
			}
			return -1;
		}
	}
}
