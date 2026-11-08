using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VhrFix
{
	// Minimal reader for Skyrim SE NIFs (20.2.0.7, user 12, BS 100): header, string table and block offsets.
	public class NifFile
	{
		public byte[] Data;
		public uint NumBlocks;
		readonly List<string> blockTypes = new List<string>();
		readonly List<string> strings = new List<string>();
		ushort[] blockTypeIndex;
		long[] blockOffsets;

		public static NifFile Load(string path)
		{
			var n = new NifFile { Data = File.ReadAllBytes(path) };
			var d = n.Data;
			int p = Array.IndexOf(d, (byte)0x0A) + 1;
			p += 4 + 1 + 4;
			n.NumBlocks = BitConverter.ToUInt32(d, p);
			p += 4 + 4;
			for (int i = 0; i < 3; i++)
				p += 1 + d[p];
			ushort numTypes = BitConverter.ToUInt16(d, p);
			p += 2;
			for (int i = 0; i < numTypes; i++)
			{
				int l = BitConverter.ToInt32(d, p);
				n.blockTypes.Add(Encoding.ASCII.GetString(d, p + 4, l));
				p += 4 + l;
			}
			n.blockTypeIndex = new ushort[n.NumBlocks];
			for (int i = 0; i < n.NumBlocks; i++, p += 2)
				n.blockTypeIndex[i] = BitConverter.ToUInt16(d, p);
			var sizes = new uint[n.NumBlocks];
			for (int i = 0; i < n.NumBlocks; i++, p += 4)
				sizes[i] = BitConverter.ToUInt32(d, p);
			uint numStrings = BitConverter.ToUInt32(d, p);
			p += 4 + 4;
			for (int i = 0; i < numStrings; i++)
			{
				int l = BitConverter.ToInt32(d, p);
				n.strings.Add(Encoding.ASCII.GetString(d, p + 4, l));
				p += 4 + l;
			}
			uint numGroups = BitConverter.ToUInt32(d, p);
			p += 4 + 4 * (int)numGroups;
			n.blockOffsets = new long[n.NumBlocks];
			long o = p;
			for (int i = 0; i < n.NumBlocks; i++)
			{
				n.blockOffsets[i] = o;
				o += sizes[i];
			}
			return n;
		}

		public int BlockOffset(int i) => (int)blockOffsets[i];
		public string TypeOf(int i) => blockTypes[blockTypeIndex[i]];
		public string Str(int i) => i >= 0 && i < strings.Count ? strings[i] : "(none)";
	}
}
