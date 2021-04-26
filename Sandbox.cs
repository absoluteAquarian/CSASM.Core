using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CSASM.Core{
	public static class Sandbox{
		public static bool verbose;
		public static bool reportStackUsage;

		public static StreamWriter verboseWriter;

		public static Random random;

		internal static IOHandle[] ioHandles;

		public static int Main(MethodInfo main, int stackSize, string[] args){
			Ops.stack = new CSASMStack(stackSize);

			random = new Random();

			const int HANDLES = 8;
			ioHandles = new IOHandle[HANDLES];
			for(int i = 0; i < HANDLES; i++)
				ioHandles[i] = new IOHandle();

			List<string> cmdArgs = new List<string>(args.Length);
			bool flagArg = false;
			if(args.Length > 0){
				foreach(string arg in args){
					if(arg == "-verbose"){
						verbose = true;
						flagArg = true;
					}else if(arg == "-reportstack"){
						reportStackUsage = true;
						flagArg = true;
					}else{
						if(flagArg)
							throw new Exception("Debug flags must be specified after all program arguments");

						cmdArgs.Add(arg);
					}
				}
			}

			Ops.args = cmdArgs.ToArray();

			//Create the verbose output
			if(verbose || reportStackUsage)
				verboseWriter = new StreamWriter(File.Open("verbose.txt", FileMode.Create));

			//Try to print any unhandled exceptions to the console
			AppDomain.CurrentDomain.UnhandledException += (obj, args) => {
				Console.Out.Flush();
				Console.WriteLine("Unhandled exception\n" + args.ExceptionObject.ToString());
			};

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
			}finally{
				//No matter what, the file handles need to be disposed of
				for(int i = 0; i < HANDLES; i++)
					if(ioHandles[i].file != null)
						(ioHandles[i].handle as IDisposable).Dispose();

				if(verbose)
					verboseWriter.Close();
			}
			return -1;
		}
	}
}
