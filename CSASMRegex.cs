using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSASM.Core{
	/// <summary>
	/// A wrapper class around a <seealso cref="Regex"/> for better interfacing with the stack
	/// </summary>
	public class CSASMRegex{
		public readonly string pattern;
		private readonly Regex regex;

		//Optimize searching through the same string multiple times
		private string lastPatternCheck;
		internal MatchCollection lastMatches;

		public CSASMRegex(string pattern){
			this.pattern = pattern;
			regex = new Regex(pattern, RegexOptions.Compiled);
		}

		public void AttemptToMatch(string str){
			if(str is null)
				return;

			if(str == lastPatternCheck)
				return;

			lastPatternCheck = str;

			//Perform the match
			lastMatches = regex.Matches(str);

			//Accessing the Count property will force the collection to update
			if(lastMatches.Count > 0)
				Ops.RegexSuccess = true;
		}

		public string GetMatchString(int index){
			//null == invalid state or no matches found
			if(lastPatternCheck is null || lastMatches is null || lastMatches.Count == 0)
				return null;

			return lastMatches[index].Value;
		}

		public string ReplaceString(string original, string replace){
			if(lastPatternCheck is null || lastMatches is null || lastMatches.Count == 0)
				return original;

			return regex.Replace(original, replace);
		}

		public override string ToString() => $"Regex: \"{pattern}\"";
	}
}
