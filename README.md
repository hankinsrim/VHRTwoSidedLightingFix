# VHR Two-Sided Lighting Fix

Fixes the dark inner layers of [Vanilla Hair Remake SMP](https://www.nexusmods.com/skyrimspecialedition/mods/63979)
(VHR) hair in Skyrim Special Edition / Anniversary Edition. It is most visible on light hair
with a mostly vanilla lighting setup.

VHR builds its hair from two-sided cards: every strip is a front and a back copy at the same
position, with opposite normals. The copy that faces the scalp gets no light, so wherever you
see it, the inside of a hair layer renders near black. This mod gives the inward copy the
outward copy's normal, tangent and bitangent, so both sides are lit like the outside of the
hair. Positions, triangles, skinning, physics and textures are unchanged.

Built for Vanilla Hair Remake SMP 1.0.3: 140 of its 271 hair meshes have two-sided cards.

This code was written with LLM assistance.

Requirements: [Vanilla Hair Remake SMP](https://www.nexusmods.com/skyrimspecialedition/mods/63979)
1.0.3.

## Installation

Load this mod after Vanilla Hair Remake SMP so its meshes win. It works with
[Weight adjustment for Vanilla Hair Remake SMP](https://www.nexusmods.com/skyrimspecialedition/mods/161949),
which only changes physics settings.

## Scope

* The player and NPCs without baked FaceGen get the fixed hair. Named NPCs use the hair baked
  into their FaceGen meshes (from VHR's NPC files), which this mod doesn't change.
* Which side faces outward is judged from the head: above the head's center, away from it;
  below, horizontally away from the head and neck. About 7% of pairs point too far sideways to
  judge and are left as they are.

## License

The patch tool is released under the MIT License (see `LICENSE`). The meshes in the release
are Vanilla Hair Remake SMP's by jg1, with only their normals and tangents changed.

Source: <https://github.com/hankinsrim/VHRTwoSidedLightingFix>
