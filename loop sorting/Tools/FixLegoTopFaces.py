import argparse
import math


DEFAULT_FILES = [
    "Assets/Resources/LegoLikeBrick_2x2_Detailed_BevelAO_UnityPack_v4/LegoLikeBrick_2x2_Detailed_BevelAO_PivotCenter_v4.obj",
    "Assets/Resources/LegoLikeBrick_2x2_Detailed_BevelAO_UnityPack_v4/LegoLikeBrick_2x2_Detailed_BevelAO_PivotBottom_v4.obj",
    "Assets/Resources/LegoLikeBrick_2x2_Detailed_BevelAO_UnityPack_v3/LegoLikeBrick_2x2_Detailed_BevelAO_PivotCenter.obj",
    "Assets/Resources/LegoLikeBrick_2x2_Detailed_BevelAO_UnityPack_v3/LegoLikeBrick_2x2_Detailed_BevelAO_PivotBottom.obj",
    "Assets/Resources/LegoLikeBrick_2x2_Detailed_UnityPack/LegoLikeBrick_2x2_Detailed_PivotCenter.obj",
    "Assets/Resources/LegoLikeBrick_2x2_Detailed_UnityPack/LegoLikeBrick_2x2_Detailed_PivotBottom.obj",
]


def parse_vertices(lines):
    verts = []
    for line in lines:
        if line.startswith("v "):
            parts = line.strip().split()
            if len(parts) >= 4:
                verts.append(tuple(float(x) for x in parts[1:4]))
    return verts


def vertex_index_from_token(token, vertex_count):
    v_str = token.split("/")[0]
    if not v_str:
        return None
    idx = int(v_str)
    if idx < 0:
        idx = vertex_count + 1 + idx
    return idx


def face_normal_and_center(face_indices, verts):
    if len(face_indices) < 3:
        return None, None
    a = verts[face_indices[0] - 1]
    b = verts[face_indices[1] - 1]
    c = verts[face_indices[2] - 1]
    ux, uy, uz = b[0] - a[0], b[1] - a[1], b[2] - a[2]
    vx, vy, vz = c[0] - a[0], c[1] - a[1], c[2] - a[2]
    nx = uy * vz - uz * vy
    ny = uz * vx - ux * vz
    nz = ux * vy - uy * vx
    length = math.sqrt(nx * nx + ny * ny + nz * nz)
    if length <= 1e-8:
        return None, None
    nx /= length
    ny /= length
    nz /= length

    xs = [verts[i - 1][0] for i in face_indices]
    ys = [verts[i - 1][1] for i in face_indices]
    zs = [verts[i - 1][2] for i in face_indices]
    cx = sum(xs) / len(xs)
    cy = sum(ys) / len(ys)
    cz = sum(zs) / len(zs)
    return (nx, ny, nz), (cx, cy, cz)


def fix_file(path, offset_from_top, dry_run=False):
    with open(path, "r", encoding="utf-8", errors="ignore") as f:
        lines = f.readlines()

    verts = parse_vertices(lines)
    if not verts:
        return 0

    max_y = max(v[1] for v in verts)
    threshold = max_y - offset_from_top
    flipped = 0
    updated = []

    for line in lines:
        if not line.startswith("f "):
            updated.append(line)
            continue

        parts = line.strip().split()
        tokens = parts[1:]
        face_indices = []
        for token in tokens:
            idx = vertex_index_from_token(token, len(verts))
            if idx is None:
                face_indices = []
                break
            face_indices.append(idx)
        if not face_indices:
            updated.append(line)
            continue

        normal, center = face_normal_and_center(face_indices, verts)
        if normal is None or center is None:
            updated.append(line)
            continue

        _, ny, _ = normal
        _, cy, _ = center
        if cy >= threshold and ny < 0.0:
            tokens = list(reversed(tokens))
            flipped += 1
            updated.append("f " + " ".join(tokens) + "\n")
        else:
            updated.append(line)

    if not dry_run:
        with open(path, "w", encoding="utf-8", errors="ignore") as f:
            f.writelines(updated)

    return flipped


def main():
    parser = argparse.ArgumentParser(description="Flip inverted top faces on Lego-like OBJ meshes.")
    parser.add_argument("--offset", type=float, default=0.25, help="Top band thickness in model units.")
    parser.add_argument("--dry-run", action="store_true", help="Only report face flips.")
    parser.add_argument("files", nargs="*", help="OBJ files to process.")
    args = parser.parse_args()

    files = args.files or DEFAULT_FILES
    total = 0
    for path in files:
        flipped = fix_file(path, args.offset, dry_run=args.dry_run)
        total += flipped
        print(f"{path}: flipped {flipped} faces")
    print(f"total flipped: {total}")


if __name__ == "__main__":
    main()
