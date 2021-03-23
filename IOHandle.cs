using System.IO;

namespace CSASM.Core{
	public class IOHandle{
		public bool write;
		public FileMode mode;
		public string file;
		public object handle;
		public bool isStream;
		public bool newLine;
	}
}
