using System.IO;

namespace PiSignageWatcher
{
	public static class Extensions
	{
		public static string ReplaceInvalidChars(this string filename) => string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
	}
}
