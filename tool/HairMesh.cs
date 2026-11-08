using System;
using System.Collections.Generic;

namespace VhrFix
{
	// Skinned hair: BSDynamicTriShape holds float4 positions (w = bitangent X), NiSkinPartition the UV, normal and tangent data.
	public class HairMesh
	{
		public string ShapeName;
		public int NumVerts;
		public int PosOffset;
		public int PartOffset;
		public int Stride;
		public ulong Desc;
		public int NrmOff => (int)((Desc >> 16) & 0xF) * 4;
		public int TanOff => (int)((Desc >> 20) & 0xF) * 4;

		public static List<HairMesh> Find(NifFile n)
		{
			var d = n.Data;
			var result = new List<HairMesh>();
			for (int i = 0; i < n.NumBlocks; i++)
			{
				if (n.TypeOf(i) != "BSDynamicTriShape")
					continue;
				int o = n.BlockOffset(i);
				var m = new HairMesh { ShapeName = n.Str(BitConverter.ToInt32(d, o)) };
				int p = o + 4;
				p += 4 + 4 * (int)BitConverter.ToUInt32(d, p);
				p += 4 + 4 + 12 + 36 + 4 + 4 + 16;  // controller, flags, transform, collision, bounds
				int skinRef = BitConverter.ToInt32(d, p);
				p += 4 + 4 + 4 + 8 + 2;  // skin, shader, alpha, vertex desc, triangle count
				m.NumVerts = BitConverter.ToUInt16(d, p);
				p += 2;
				if (BitConverter.ToUInt32(d, p) != 0)
					throw new Exception(m.ShapeName + ": vertex data stored in the shape is not handled");
				if (BitConverter.ToUInt32(d, p + 4) != 0)
					throw new Exception(m.ShapeName + ": particle data is not handled");
				if (BitConverter.ToUInt32(d, p + 8) != m.NumVerts * 16)
					throw new Exception(m.ShapeName + ": dynamic data size mismatch");
				m.PosOffset = p + 12;
				if (skinRef < 0)
					throw new Exception(m.ShapeName + ": not skinned");
				int partRef = BitConverter.ToInt32(d, n.BlockOffset(skinRef) + 4);
				if (n.TypeOf(partRef) != "NiSkinPartition")
					throw new Exception(m.ShapeName + ": skin partition not found");
				int po = n.BlockOffset(partRef);
				m.Stride = (int)BitConverter.ToUInt32(d, po + 8);
				m.Desc = BitConverter.ToUInt64(d, po + 12);
				m.PartOffset = po + 20;
				if (BitConverter.ToUInt32(d, po + 4) / m.Stride != m.NumVerts)
					throw new Exception(m.ShapeName + ": skin partition vertex count mismatch");
				result.Add(m);
			}
			return result;
		}

		public void Position(byte[] d, int v, out double x, out double y, out double z)
		{
			int b = PosOffset + v * 16;
			x = BitConverter.ToSingle(d, b);
			y = BitConverter.ToSingle(d, b + 4);
			z = BitConverter.ToSingle(d, b + 8);
		}

		public void Normal(byte[] d, int v, out double x, out double y, out double z)
		{
			int b = PartOffset + v * Stride + NrmOff;
			x = d[b] / 255.0 * 2.0 - 1.0;
			y = d[b + 1] / 255.0 * 2.0 - 1.0;
			z = d[b + 2] / 255.0 * 2.0 - 1.0;
		}

		// Pairs of vertices at the same position with opposite normals, the two sides of a hair card.
		public List<int[]> Twins(byte[] d)
		{
			var buckets = new Dictionary<string, List<int>>();
			for (int v = 0; v < NumVerts; v++)
			{
				Position(d, v, out double x, out double y, out double z);
				string k = Math.Round(x, 2) + "," + Math.Round(y, 2) + "," + Math.Round(z, 2);
				if (!buckets.TryGetValue(k, out var l))
					buckets[k] = l = new List<int>();
				l.Add(v);
			}
			var pairs = new List<int[]>();
			foreach (var l in buckets.Values)
			{
				var used = new HashSet<int>();
				for (int a = 0; a < l.Count; a++)
				{
					for (int c = a + 1; c < l.Count; c++)
					{
						if (used.Contains(l[a]) || used.Contains(l[c]))
							continue;
						Normal(d, l[a], out double ax, out double ay, out double az);
						Normal(d, l[c], out double bx, out double by, out double bz);
						if (ax * bx + ay * by + az * bz < -0.5)
						{
							pairs.Add(new[] { l[a], l[c] });
							used.Add(l[a]);
							used.Add(l[c]);
						}
					}
				}
			}
			return pairs;
		}
	}
}
