# Building VHR Two-Sided Lighting Fix

Requirements: Visual Studio 2022 or newer (its C# compiler is used), and an extracted or
installed copy of Vanilla Hair Remake SMP.

## Building the meshes

```
git clone https://github.com/hankinsrim/VHRTwoSidedLightingFix.git
cd VHRTwoSidedLightingFix
powershell -ExecutionPolicy Bypass -File .\build.ps1 -VHR "<Vanilla hair remake SMP folder>"
powershell -ExecutionPolicy Bypass -File .\build.ps1 -VHR "<Vanilla hair remake SMP folder>" -Zip
```

* `build.ps1` compiles `tool\*.cs` into `build\vhrfix.exe` and runs it over every NIF under the
  folder's `meshes\`. Only meshes with two-sided pairs are written to `dist\`.
* For VHR SMP 1.0.3 it reports 140 files, 194,367 pairs patched and 13,747 skipped.
* `-Zip` also writes `build\VHRTwoSidedLightingFix-<version>.zip`, a minimal FOMOD whose
  `fomod\info.xml` carries the name and version for mod managers that read it.

## How it works

* `NifFile`: reads the NIF header, string table and block offsets. Blocks are patched in
  place, so the file size never changes.
* `HairMesh`: finds each `BSDynamicTriShape` with its float4 positions (w holds bitangent X)
  and its `NiSkinPartition` vertex data (normal + bitangent Y, tangent + bitangent Z). Vertices
  at the same position (rounded to 0.01) with opposite normals form a pair.
* `Patch`: the head axis is the xy centroid of the top 35% of vertices, and the head center
  sits 9 units below the crown. Above the center, outward means away from it; below, away from
  the axis horizontally. Pairs whose normal is within 0.30 (dot product) of sideways are
  skipped. The outward vertex's normal, tangent and bitangent are copied onto the inward one.

Rerun after a Vanilla Hair Remake SMP update and update `$vhrVersion` in `build.ps1`.
