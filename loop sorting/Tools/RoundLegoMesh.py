import argparse
import math
from collections import Counter


DEFAULT_FILES = [
    "Assets/Resources/LegoLikeBrick_2x2_Detailed_BevelAO_UnityPack_v4/LegoLikeBrick_2x2_Detailed_BevelAO_PivotCenter_v4.obj",
    "Assets/Resources/LegoLikeBrick_2x2_Detailed_BevelAO_UnityPack_v4/LegoLikeBrick_2x2_Detailed_BevelAO_PivotBottom_v4.obj",
]


def clamp(value, lo, hi):
    return max(lo, min(hi, value))


def parse_obj(path):
    header = []
    verts = []
    extras = []
    faces = []

    with open(path, "r", encoding="utf-8", errors="ignore") as f:
        for line in f:
            if line.startswith("v "):
                parts = line.strip().split()
                if len(parts) >= 4:
                    x, y, z = (float(parts[1]), float(parts[2]), float(parts[3]))
                    verts.append([x, y, z])
                    extras.append(parts[4:])
            elif line.startswith("f "):
                tokens = line.strip().split()[1:]
                if len(tokens) < 3:
                    continue
                idxs = []
                for token in tokens:
                    v_str = token.split("/")[0]
                    if not v_str:
                        continue
                    idx = int(v_str)
                    if idx < 0:
                        idx = len(verts) + 1 + idx
                    idxs.append(idx)
                if len(idxs) < 3:
                    continue
                if len(idxs) == 3:
                    faces.append(idxs)
                else:
                    v0 = idxs[0]
                    for i in range(1, len(idxs) - 1):
                        faces.append([v0, idxs[i], idxs[i + 1]])
            elif line.startswith("vn "):
                continue
            else:
                header.append(line.rstrip("\n"))

    return header, verts, extras, faces


def detect_base_top_y(ys, stud_gap):
    max_y = max(ys)
    candidates = [y for y in ys if y <= max_y - stud_gap]
    if not candidates:
        return max_y
    return max(candidates)


def detect_stud_indices(verts, faces, base_top_y, ratio=0.4):
    max_y = max(v[1] for v in verts)
    stud_height = max_y - base_top_y
    if stud_height <= 0.0:
        return set()

    threshold = base_top_y + stud_height * ratio
    stud_indices = set()
    for face in faces:
        v0 = verts[face[0] - 1]
        v1 = verts[face[1] - 1]
        v2 = verts[face[2] - 1]
        cy = (v0[1] + v1[1] + v2[1]) / 3.0
        if cy >= threshold:
            stud_indices.update(face)
    return stud_indices


def detect_stud_centers(verts, base_top_y, min_band=0.005, peak_ratio=0.7):
    max_y = max(v[1] for v in verts)
    stud_height = max_y - base_top_y
    if stud_height <= 0.0:
        return []

    band = max(min_band, stud_height * 0.2)
    top = [(v[0], v[2]) for v in verts if v[1] >= max_y - band]
    if len(top) < 8:
        return []

    def pick_centers(values):
        counts = Counter(round(v, 3) for v in values)
        if not counts:
            return []
        max_count = max(counts.values())
        threshold = max_count * peak_ratio
        return sorted([val for val, count in counts.items() if count >= threshold])

    xs = pick_centers([x for x, _ in top])
    zs = pick_centers([z for _, z in top])
    if not xs or not zs:
        return []

    return [(x, z) for x in xs for z in zs]


def estimate_stud_radius(verts, centers, base_top_y, min_band=0.005):
    if not centers:
        return 0.0
    max_y = max(v[1] for v in verts)
    stud_height = max_y - base_top_y
    if stud_height <= 0.0:
        return 0.0
    band = max(min_band, stud_height * 0.2)
    top = [(v[0], v[2]) for v in verts if v[1] >= max_y - band]
    if not top:
        return 0.0

    dists = []
    for x, z in top:
        nearest = min((x - cx) * (x - cx) + (z - cz) * (z - cz) for cx, cz in centers)
        if nearest > 1e-8:
            dists.append(math.sqrt(nearest))
    if not dists:
        return 0.0
    dists.sort()
    return dists[len(dists) // 2]


def adjust_studs(
    verts,
    centers,
    base_top_y,
    target_radius=None,
    target_height=None,
    detect_radius=None,
    epsilon=1e-6,
):
    if not centers or (target_radius is None and target_height is None):
        return verts, 0

    max_y = max(v[1] for v in verts)
    current_height = max_y - base_top_y
    if current_height <= 0.0:
        return verts, 0

    height_scale = 1.0
    if target_height is not None:
        height_scale = target_height / current_height
        if height_scale <= 0.0:
            return verts, 0

    if detect_radius is None:
        current_radius = estimate_stud_radius(verts, centers, base_top_y)
        if current_radius <= 0.0:
            return verts, 0
        detect_radius = min(max(current_radius * 1.5, current_radius + 0.1), 0.45)

    detect_sq = detect_radius * detect_radius
    stud_floor = min(0.05, current_height * 0.2)
    moved = 0
    out = []
    for x, y, z in verts:
        if y < base_top_y - stud_floor:
            out.append([x, y, z])
            continue

        cx, cz = min(centers, key=lambda c: (x - c[0]) ** 2 + (z - c[1]) ** 2)
        dx = x - cx
        dz = z - cz
        dist_sq = dx * dx + dz * dz
        if dist_sq > detect_sq:
            out.append([x, y, z])
            continue

        nx, ny, nz = x, y, z
        if target_height is not None:
            ny = base_top_y + (y - base_top_y) * height_scale
            ny = max(base_top_y, min(base_top_y + target_height, ny))
        if target_radius is not None and dist_sq > epsilon * epsilon:
            dist = math.sqrt(dist_sq)
            scale = target_radius / dist
            nx = cx + dx * scale
            nz = cz + dz * scale
        elif target_radius is not None:
            nx = cx
            nz = cz

        if abs(nx - x) > 1e-8 or abs(ny - y) > 1e-8 or abs(nz - z) > 1e-8:
            moved += 1
        out.append([nx, ny, nz])

    return out, moved


def apply_rounding(
    verts,
    radius,
    exclude_studs,
    stud_gap,
    used_indices=None,
    stud_indices=None,
    epsilon=1e-6,
):
    ys = [v[1] for v in verts]
    base_top_y = detect_base_top_y(ys, stud_gap) if exclude_studs else max(ys)

    if used_indices is None:
        affected = [v for v in verts if v[1] <= base_top_y + epsilon]
        used = None
    else:
        used = set(used_indices)
        affected = [verts[i - 1] for i in used if verts[i - 1][1] <= base_top_y + epsilon]
    if not affected:
        return verts, base_top_y, 0

    xs = [v[0] for v in affected]
    ys_aff = [v[1] for v in affected]
    zs = [v[2] for v in affected]

    min_x, max_x = min(xs), max(xs)
    min_y, max_y = min(ys_aff), max(ys_aff)
    min_z, max_z = min(zs), max(zs)

    cx = (min_x + max_x) * 0.5
    cy = (min_y + max_y) * 0.5
    cz = (min_z + max_z) * 0.5
    hx = max((max_x - min_x) * 0.5, epsilon)
    hy = max((max_y - min_y) * 0.5, epsilon)
    hz = max((max_z - min_z) * 0.5, epsilon)

    r = min(radius, hx, hy, hz)
    bx = max(hx - r, 0.0)
    by = max(hy - r, 0.0)
    bz = max(hz - r, 0.0)

    moved = 0
    out = []
    for idx, (x, y, z) in enumerate(verts, start=1):
        if used is not None and idx not in used:
            out.append([x, y, z])
            continue
        if exclude_studs and y > base_top_y + epsilon:
            out.append([x, y, z])
            continue
        if (
            exclude_studs
            and stud_indices
            and idx in stud_indices
            and y >= base_top_y - epsilon
        ):
            out.append([x, y, z])
            continue

        px = x - cx
        py = y - cy
        pz = z - cz

        wx = clamp(px, -bx, bx)
        wy = clamp(py, -by, by)
        wz = clamp(pz, -bz, bz)

        dx = px - wx
        dy = py - wy
        dz = pz - wz

        dlen = math.sqrt(dx * dx + dy * dy + dz * dz)
        if dlen > epsilon:
            scale = r / dlen
            px = wx + dx * scale
            py = wy + dy * scale
            pz = wz + dz * scale

        nx = px + cx
        ny = py + cy
        nz = pz + cz

        if abs(nx - x) > 1e-8 or abs(ny - y) > 1e-8 or abs(nz - z) > 1e-8:
            moved += 1

        out.append([nx, ny, nz])

    return out, base_top_y, moved


def compute_base_bounds(verts, base_top_y, epsilon=1e-6):
    base = [v for v in verts if v[1] <= base_top_y + epsilon]
    if not base:
        base = verts
    xs = [v[0] for v in base]
    ys = [v[1] for v in base]
    zs = [v[2] for v in base]
    return min(xs), max(xs), min(ys), max(ys), min(zs), max(zs)


def scale_base(
    verts,
    base_top_y,
    scale,
    used_indices=None,
    stud_indices=None,
    epsilon=1e-6,
):
    if abs(scale - 1.0) < 1e-6:
        return verts, 0

    min_x, max_x, _, _, min_z, max_z = compute_base_bounds(verts, base_top_y, epsilon=epsilon)
    cx = (min_x + max_x) * 0.5
    cz = (min_z + max_z) * 0.5

    used = set(used_indices) if used_indices is not None else None
    moved = 0
    out = []
    for idx, (x, y, z) in enumerate(verts, start=1):
        if y > base_top_y + epsilon:
            out.append([x, y, z])
            continue
        if used is not None and idx not in used:
            out.append([x, y, z])
            continue
        if (
            y >= base_top_y - epsilon
            and stud_indices
            and idx in stud_indices
        ):
            out.append([x, y, z])
            continue

        nx = cx + (x - cx) * scale
        nz = cz + (z - cz) * scale
        if abs(nx - x) > 1e-8 or abs(nz - z) > 1e-8:
            moved += 1
        out.append([nx, y, nz])

    return out, moved


def filter_faces(verts, faces, base_top_y, shell_margin, top_keep, drop_bottom, bottom_drop):
    min_x, max_x, min_y, _, min_z, max_z = compute_base_bounds(verts, base_top_y)
    keep = []
    outer = []
    removed = 0

    for face in faces:
        vs = [verts[i - 1] for i in face]
        xs = [v[0] for v in vs]
        ys = [v[1] for v in vs]
        zs = [v[2] for v in vs]
        cx = sum(xs) / len(xs)
        cy = sum(ys) / len(ys)
        cz = sum(zs) / len(zs)

        near_outer = (
            max(xs) >= max_x - shell_margin
            or min(xs) <= min_x + shell_margin
            or max(zs) >= max_z - shell_margin
            or min(zs) <= min_z + shell_margin
        )
        near_top = cy >= base_top_y - top_keep
        is_bottom_cap = cy <= min_y + bottom_drop

        if drop_bottom and is_bottom_cap:
            removed += 1
            continue

        if near_top or near_outer:
            keep.append(face)
            if near_outer:
                outer.append(face)
        else:
            removed += 1

    return keep, outer, removed


def should_flip_top_faces(verts, faces, top_offset):
    max_y = max(v[1] for v in verts)
    threshold = max_y - top_offset
    flip = []

    for idx, (a, b, c) in enumerate(faces):
        v0 = verts[a - 1]
        v1 = verts[b - 1]
        v2 = verts[c - 1]
        ux = v1[0] - v0[0]
        uy = v1[1] - v0[1]
        uz = v1[2] - v0[2]
        vx = v2[0] - v0[0]
        vy = v2[1] - v0[1]
        vz = v2[2] - v0[2]
        nx = uy * vz - uz * vy
        ny = uz * vx - ux * vz
        nz = ux * vy - uy * vx
        if nx == 0 and ny == 0 and nz == 0:
            continue

        cy = (v0[1] + v1[1] + v2[1]) / 3.0
        if cy >= threshold and ny < 0.0:
            flip.append(idx)

    return flip, threshold


def compute_normals(verts, faces):
    normals = [[0.0, 0.0, 0.0] for _ in verts]
    for a, b, c in faces:
        v0 = verts[a - 1]
        v1 = verts[b - 1]
        v2 = verts[c - 1]
        ux = v1[0] - v0[0]
        uy = v1[1] - v0[1]
        uz = v1[2] - v0[2]
        vx = v2[0] - v0[0]
        vy = v2[1] - v0[1]
        vz = v2[2] - v0[2]
        nx = uy * vz - uz * vy
        ny = uz * vx - ux * vz
        nz = ux * vy - uy * vx
        normals[a - 1][0] += nx
        normals[a - 1][1] += ny
        normals[a - 1][2] += nz
        normals[b - 1][0] += nx
        normals[b - 1][1] += ny
        normals[b - 1][2] += nz
        normals[c - 1][0] += nx
        normals[c - 1][1] += ny
        normals[c - 1][2] += nz

    for i, n in enumerate(normals):
        length = math.sqrt(n[0] * n[0] + n[1] * n[1] + n[2] * n[2])
        if length > 1e-8:
            normals[i] = [n[0] / length, n[1] / length, n[2] / length]
        else:
            normals[i] = [0.0, 1.0, 0.0]
    return normals


def write_obj(path, header, verts, extras, normals, faces):
    lines = []
    if header:
        lines.extend([h + "\n" for h in header if h.strip() != ""])
    lines.append("# generated by Tools/RoundLegoMesh.py\n")

    for v, extra in zip(verts, extras):
        if extra:
            lines.append(
                "v {:.8f} {:.8f} {:.8f} {}\n".format(v[0], v[1], v[2], " ".join(extra))
            )
        else:
            lines.append("v {:.8f} {:.8f} {:.8f}\n".format(v[0], v[1], v[2]))

    for n in normals:
        lines.append("vn {:.8f} {:.8f} {:.8f}\n".format(n[0], n[1], n[2]))

    for a, b, c in faces:
        lines.append(f"f {a}//{a} {b}//{b} {c}//{c}\n")

    with open(path, "w", encoding="utf-8", errors="ignore") as f:
        f.writelines(lines)


def process(
    path,
    radius,
    exclude_studs,
    stud_gap,
    stud_radius,
    stud_height,
    fix_top,
    top_offset,
    drop_bottom,
    shell_margin,
    top_keep,
    bottom_drop,
    base_scale,
):
    header, verts, extras, faces = parse_obj(path)
    if not verts or not faces:
        print(f"{path}: skipped (no verts/faces)")
        return 0

    base_top_y = (
        detect_base_top_y([v[1] for v in verts], stud_gap)
        if exclude_studs
        else max(v[1] for v in verts)
    )
    stud_indices = set()
    if exclude_studs:
        stud_indices = detect_stud_indices(verts, faces, base_top_y)

    kept_faces, outer_faces, removed = filter_faces(
        verts,
        faces,
        base_top_y,
        shell_margin=shell_margin,
        top_keep=top_keep,
        drop_bottom=drop_bottom,
        bottom_drop=bottom_drop,
    )
    kept_used = set()
    for face in kept_faces:
        kept_used.update(face)
    outer_used = set()
    for face in outer_faces:
        outer_used.update(face)

    verts, scaled = scale_base(
        verts,
        base_top_y,
        base_scale,
        used_indices=outer_used,
        stud_indices=stud_indices if exclude_studs else None,
    )

    round_used = kept_used if not exclude_studs else (outer_used or kept_used)

    rounded, base_top_y, moved = apply_rounding(
        verts,
        radius,
        exclude_studs=exclude_studs,
        stud_gap=stud_gap,
        used_indices=round_used,
        stud_indices=stud_indices if exclude_studs else None,
    )

    stud_moved = 0
    if exclude_studs and (stud_radius is not None or stud_height is not None):
        stud_centers = detect_stud_centers(rounded, base_top_y)
        rounded, stud_moved = adjust_studs(
            rounded,
            stud_centers,
            base_top_y,
            target_radius=stud_radius,
            target_height=stud_height,
        )
    flipped = 0
    if fix_top:
        to_flip, threshold = should_flip_top_faces(rounded, kept_faces, top_offset)
        for idx in to_flip:
            a, b, c = kept_faces[idx]
            kept_faces[idx] = [a, c, b]
        flipped = len(to_flip)
        if flipped:
            print(f"{path}: flipped {flipped} top faces (threshold={threshold:.4f})")

    normals = compute_normals(rounded, kept_faces)
    write_obj(path, header, rounded, extras, normals, kept_faces)
    print(
        f"{path}: moved {moved} verts, base_top_y={base_top_y:.4f}, "
        f"faces kept={len(kept_faces)} removed={removed}, base_scaled={scaled}, "
        f"stud_adjusted={stud_moved}"
    )
    return moved


def main():
    parser = argparse.ArgumentParser(description="Round Lego OBJ edges by adjusting geometry.")
    parser.add_argument("--radius", type=float, default=0.28, help="Round radius in model units.")
    parser.add_argument("--stud-gap", type=float, default=0.12, help="Min Y gap from top to detect stud region.")
    parser.add_argument("--include-studs", action="store_true", help="Also round studs.")
    parser.add_argument("--fix-top", action="store_true", help="Flip inverted top faces.")
    parser.add_argument("--top-offset", type=float, default=0.25, help="Top band thickness in model units.")
    parser.add_argument("--drop-bottom", action="store_true", help="Drop bottom faces (underside not visible).")
    parser.add_argument("--shell-margin", type=float, default=0.12, help="Thickness of the outer shell to keep.")
    parser.add_argument("--top-keep", type=float, default=0.05, help="Keep top faces within this band below base top.")
    parser.add_argument("--bottom-drop", type=float, default=0.04, help="Drop bottom cap faces within this band from min Y.")
    parser.add_argument("--base-scale", type=float, default=1.0, help="Scale base width/depth (studs unaffected).")
    parser.add_argument("--stud-radius", type=float, default=None, help="Target stud radius (omit to keep).")
    parser.add_argument("--stud-height", type=float, default=None, help="Target stud height from base top (omit to keep).")
    parser.add_argument("files", nargs="*", help="OBJ files to process.")
    args = parser.parse_args()

    files = args.files or DEFAULT_FILES
    total = 0
    for path in files:
        total += process(
            path,
            radius=args.radius,
            exclude_studs=not args.include_studs,
            stud_gap=args.stud_gap,
            stud_radius=args.stud_radius,
            stud_height=args.stud_height,
            fix_top=args.fix_top,
            top_offset=args.top_offset,
            drop_bottom=args.drop_bottom,
            shell_margin=args.shell_margin,
            top_keep=args.top_keep,
            bottom_drop=args.bottom_drop,
            base_scale=args.base_scale,
        )
    print(f"total moved verts: {total}")


if __name__ == "__main__":
    main()
