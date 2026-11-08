using System;
using System.IO;

namespace VhrFix
{
	public static class Program
	{
		public static int Main(string[] args)
		{
			if (args.Length != 2)
			{
				Console.WriteLine("usage: vhrfix <Vanilla Hair Remake SMP folder> <output folder>");
				return 1;
			}
			string src = Path.GetFullPath(args[0]), dst = Path.GetFullPath(args[1]);
			string meshes = Path.Combine(src, "meshes");
			if (!Directory.Exists(meshes))
			{
				Console.WriteLine("no meshes folder in " + src);
				return 1;
			}
			int files = 0, patched = 0, skipped = 0;
			foreach (var f in Directory.GetFiles(meshes, "*.nif", SearchOption.AllDirectories))
			{
				var n = NifFile.Load(f);
				Patch.Run(n, out int p, out int s);
				skipped += s;
				if (p == 0)
					continue;
				string o = Path.Combine(dst, f.Substring(src.Length + 1));
				Directory.CreateDirectory(Path.GetDirectoryName(o));
				File.WriteAllBytes(o, n.Data);
				files++;
				patched += p;
			}
			Console.WriteLine("{0} files, {1} pairs patched, {2} skipped", files, patched, skipped);
			return 0;
		}
	}
}
