LegoLikeBrick 2x2 - Detailed + Bevel + AO/Curvature (Vertex Color)

Files:
- LegoLikeBrick_2x2_Detailed_BevelAO_PivotBottom.obj / .mtl
- LegoLikeBrick_2x2_Detailed_BevelAO_PivotBottom.glb
- LegoLikeBrick_2x2_Detailed_BevelAO_PivotCenter.obj / .mtl
- LegoLikeBrick_2x2_Detailed_BevelAO_PivotCenter.glb
- BrickUnlit_AO_Curv_VertexColor_BuiltIn.shader
- BrickUnlit_AO_Curv_VertexColor_URP.shader

Hard constraints kept:
- Bounding box kept identical to previous Detailed 2x2 model:
  PivotBottom min (-1, 0, -1) max (1, 1.42, 1)
- Axis orientation unchanged (Y up, top surface +Y)
- PivotBottom and PivotCenter variants are provided; use the same one you used before.

Vertex Color packing:
- RGB: AO (0..1). 1 = no occlusion; lower = slightly darker.
- A: Curvature highlight (0..1). Use shader to add subtle edge brightening.

Unity usage:
1) Import the OBJ (for geometry) OR the GLB (if you use a glTF importer).
2) Use one of the provided unlit shaders to preview AO/curvature without realtime lights.
