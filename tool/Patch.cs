using System;

namespace VhrFix
{
	// Copies the outward side's lighting frame (normal, tangent, bitangent) onto the inward side of each two-sided pair.
	public static class Patch
	{
		public const double Threshold = 0.30;

		public static void Run(NifFile n, out int patched, out int skipped)
		{
			patched = 0;
			skipped = 0;
			var d = n.Data;
			foreach (var m in HairMesh.Find(n))
			{
				HeadCenter(d, m, out double hx, out double hy, out double hz);
				foreach (var pair in m.Twins(d))
				{
					int a = pair[0], b = pair[1];
					m.Position(d, a, out double px, out double py, out double pz);
					m.Normal(d, a, out double nx, out double ny, out double nz);
					double dot = OutwardDot(px, py, pz, nx, ny, nz, hx, hy, hz);
					if (Math.Abs(dot) < Threshold)
					{
						skipped++;
						continue;
					}
					int outV = dot > 0 ? a : b, inV = dot > 0 ? b : a;
					int so = m.PartOffset + outV * m.Stride, si = m.PartOffset + inV * m.Stride;
					Array.Copy(d, so + m.NrmOff, d, si + m.NrmOff, 4);
					Array.Copy(d, so + m.TanOff, d, si + m.TanOff, 4);
					Array.Copy(d, m.PosOffset + outV * 16 + 12, d, m.PosOffset + inV * 16 + 12, 4);
					patched++;
				}
			}
		}

		// Head axis from the xy centroid of the top 35% of vertices; the center sits 9 units (about a head radius) below the crown.
		static void HeadCenter(byte[] d, HairMesh m, out double hx, out double hy, out double hz)
		{
			double maxz = -1e9, minz = 1e9;
			for (int v = 0; v < m.NumVerts; v++)
			{
				m.Position(d, v, out _, out _, out double z);
				maxz = Math.Max(maxz, z);
				minz = Math.Min(minz, z);
			}
			double cut = maxz - 0.35 * (maxz - minz), sx = 0, sy = 0;
			int c = 0;
			for (int v = 0; v < m.NumVerts; v++)
			{
				m.Position(d, v, out double x, out double y, out double z);
				if (z >= cut)
				{
					sx += x;
					sy += y;
					c++;
				}
			}
			hx = c > 0 ? sx / c : 0;
			hy = c > 0 ? sy / c : 0;
			hz = maxz - 9.0;
		}

		// Above the head center, outward is away from the center; below it, horizontally away from the head and neck axis.
		static double OutwardDot(double px, double py, double pz, double nx, double ny, double nz, double hx, double hy, double hz)
		{
			double rx = px - hx, ry = py - hy, rz = pz >= hz ? pz - hz : 0.0;
			double rl = Math.Sqrt(rx * rx + ry * ry + rz * rz);
			return rl < 1e-6 ? 0 : (nx * rx + ny * ry + nz * rz) / rl;
		}
	}
}
