LegoLikeBrick 2x2 BevelAO v4 (Fix Stud Top Face Winding)

Change from v3:
- Fixed the 4 studs top surfaces being backfacing (triangle winding was inverted).
  In Unity with default backface culling, the stud top looked like it had no cap.
  v4 flips the winding of the 96 top-cap triangles (24 per stud), without changing
  bounding box / axis orientation / pivot.

Files:
- *_PivotBottom_v4.*  : pivot at bottom-center (recommended for placing on slots)
- *_PivotCenter_v4.*  : pivot at model center

Notes:
- OBJ contains vertex colors appended on v lines (non-standard). Unity may ignore
  vertex colors on OBJ. If you need AO/curvature via vertex color, prefer GLB.
- Geometry size/orientation/pivot: unchanged from v3.
