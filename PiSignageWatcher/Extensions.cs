using System.Text.RegularExpressions;

namespace PiSignageWatcher
{
	public static class Extensions
	{
		public static string ReplaceInvalidChars(this string filename)
		{
			return Regex.Replace(filename.Trim(), "[^A-Za-z0-9_. ]+", "");
		}
	}
}
