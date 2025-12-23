using System.Collections.Generic;
using UnityEngine;

namespace LoopSorting
{
    /// <summary>
    /// Shared utilities for layout bounds, slot generation, and opening alignment.
    /// Used by both runtime and editor to keep visuals consistent.
    /// </summary>
    public static class LayoutUtils
    {
        public static LevelLayout CloneLayout(LevelLayout source)
        {
            if (source == null) return null;

            var clone = ScriptableObject.CreateInstance<LevelLayout>();
            clone.beltCapacity = source.beltCapacity;
            clone.beltSlotSpacing = source.beltSlotSpacing;
            clone.smoothCorners = source.smoothCorners;
            clone.cornerSmoothTension = source.cornerSmoothTension;
            clone.cornerSubdivisions = source.cornerSubdivisions;
            clone.blockSize = source.blockSize;
            clone.overrideLayoutAutoSettings = source.overrideLayoutAutoSettings;
            clone.autoResolveLayoutOverlap = source.autoResolveLayoutOverlap;
            clone.minBoxToBeltGap = source.minBoxToBeltGap;
            clone.preferredBoxToBeltGap = source.preferredBoxToBeltGap;
            clone.overlapResolveIterations = source.overlapResolveIterations;
            clone.cameraMaxOrthoSize = source.cameraMaxOrthoSize;
            clone.minBlockPixelSize = source.minBlockPixelSize;

            if (source.conveyors != null)
            {
                clone.conveyors = new List<ConveyorPath>(source.conveyors.Count);
                for (int i = 0; i < source.conveyors.Count; i++)
                {
                    var c = source.conveyors[i];
                    if (c == null)
                    {
                        clone.conveyors.Add(null);
                        continue;
                    }

                    clone.conveyors.Add(new ConveyorPath
                    {
                        name = c.name,
                        loop = c.loop,
                        width = c.width,
                        points = c.points != null ? new List<Vector2>(c.points) : new List<Vector2>()
                    });
                }
            }

            if (source.boxes != null)
            {
                clone.boxes = new List<BoxSpec>(source.boxes.Count);
                for (int i = 0; i < source.boxes.Count; i++)
                {
                    var b = source.boxes[i];
                    if (b == null)
                    {
                        clone.boxes.Add(null);
                        continue;
                    }

                    var spec = new BoxSpec
                    {
                        name = b.name,
                        position = b.position,
                        size = b.size,
                        color = b.color,
                        columns = b.columns,
                        rows = b.rows,
                        opening = b.opening,
                        autoAlignSlot = b.autoAlignSlot,
                        beltSlotIndex = b.beltSlotIndex,
                        locked = b.locked,
                        unlockColor = b.unlockColor,
                        colorCounts = new List<ColorCount>(),
                        initialBlocks = new List<BlockColor>()
                    };

                    if (b.colorCounts != null)
                    {
                        for (int cc = 0; cc < b.colorCounts.Count; cc++)
                        {
                            var entry = b.colorCounts[cc];
                            if (entry == null) continue;
                            spec.colorCounts.Add(new ColorCount
                            {
                                color = entry.color,
                                count = entry.count,
                                hidden = entry.hidden
                            });
                        }
                    }

                    if (b.initialBlocks != null)
                    {
                        spec.initialBlocks.AddRange(b.initialBlocks);
                    }

                    clone.boxes.Add(spec);
                }
            }

            return clone;
        }

        public sealed class BeltPathCache
        {
            public bool Loop { get; set; }
            public float TotalLength { get; set; }
            public float Offset { get; set; }
            public List<Vector2> EvalPoints { get; set; }
            public List<float> Cumulative { get; set; }
        }

        public static Bounds ComputeLayoutBounds(LevelLayout layout)
        {
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            bool hasData = false;

            if (layout.conveyors != null)
            {
                foreach (var c in layout.conveyors)
                {
                    if (c.points == null) continue;
                    foreach (var p in c.points)
                    {
                        min = Vector2.Min(min, p);
                        max = Vector2.Max(max, p);
                        hasData = true;
                    }
                }
            }

            if (layout.boxes != null)
            {
                foreach (var b in layout.boxes)
                {
                    var boxSize = ComputeBoxSize(b, layout.blockSize);
                    var half = boxSize * 0.5f;
                    var bMin = b.position - half;
                    var bMax = b.position + half;
                    min = Vector2.Min(min, bMin);
                    max = Vector2.Max(max, bMax);
                    hasData = true;
                }
            }

            if (!hasData)
            {
                return new Bounds(Vector3.zero, Vector3.zero);
            }

            var center = (min + max) * 0.5f;
            var boundsSize = max - min;
            return new Bounds(new Vector3(center.x, center.y, 0f), new Vector3(boundsSize.x, boundsSize.y, 0f));
        }

        public static int ResolveBoxBeltOverlap(
            LevelLayout layout,
            float minGap,
            float preferredGap,
            float fallbackSpacing,
            int iterations = 3,
            float sampleSpacingFactor = 0.5f)
        {
            if (layout == null || layout.boxes == null || layout.boxes.Count == 0)
            {
                return 0;
            }

            minGap = Mathf.Max(0f, minGap);
            preferredGap = Mathf.Max(0f, preferredGap);
            if (preferredGap > 0f && preferredGap < minGap)
            {
                preferredGap = minGap;
            }

            if (minGap <= 0f && preferredGap <= 0f)
            {
                return 0;
            }

            ConveyorPath path = null;
            if (layout.conveyors != null)
            {
                for (int i = 0; i < layout.conveyors.Count; i++)
                {
                    var candidate = layout.conveyors[i];
                    if (candidate != null && candidate.points != null && candidate.points.Count >= 2)
                    {
                        path = candidate;
                        break;
                    }
                }
            }
            if (path == null)
            {
                return 0;
            }

            float spacing = layout.beltSlotSpacing > 0f ? layout.beltSlotSpacing : fallbackSpacing;
            float beltWidth = Mathf.Clamp(path.width, spacing * 0.8f, spacing * 1.6f);
            float beltHalf = beltWidth * 0.5f;
            float unit = layout.blockSize > 0 ? layout.blockSize : 0.6f;

            var samplePoints = BuildSamplePoints(path, spacing, layout.smoothCorners, layout.cornerSmoothTension, layout.cornerSubdivisions, sampleSpacingFactor);
            if (samplePoints.Count == 0)
            {
                return 0;
            }

            iterations = Mathf.Clamp(iterations, 1, 8);
            int moved = 0;

            for (int iter = 0; iter < iterations; iter++)
            {
                bool movedThisIter = false;
                for (int i = 0; i < layout.boxes.Count; i++)
                {
                    var spec = layout.boxes[i];
                    if (spec == null) continue;

                    var size = ComputeBoxSize(spec, unit);
                    var half = size * 0.5f;
                    var rectMin = spec.position - half;
                    var rectMax = spec.position + half;

                    float bestDistSq = float.MaxValue;
                    Vector2 bestPoint = Vector2.zero;
                    Vector2 bestOnRect = Vector2.zero;

                    for (int p = 0; p < samplePoints.Count; p++)
                    {
                        var pt = samplePoints[p];
                        var onRect = new Vector2(
                            Mathf.Clamp(pt.x, rectMin.x, rectMax.x),
                            Mathf.Clamp(pt.y, rectMin.y, rectMax.y));
                        var delta = pt - onRect;
                        float dSq = delta.sqrMagnitude;
                        if (dSq < bestDistSq)
                        {
                            bestDistSq = dSq;
                            bestPoint = pt;
                            bestOnRect = onRect;
                        }
                    }

                    float bestDist = Mathf.Sqrt(bestDistSq);
                    float clearance = bestDist - beltHalf;
                    var dir = bestOnRect - bestPoint;
                    if (dir.sqrMagnitude < 0.000001f)
                    {
                        dir = spec.position - bestPoint;
                    }
                    if (dir.sqrMagnitude < 0.000001f)
                    {
                        dir = Vector2.up;
                    }

                    dir.Normalize();
                    if (clearance < minGap)
                    {
                        float push = minGap - clearance;
                        spec.position += dir * push;
                        movedThisIter = true;
                        moved++;
                    }
                    else if (preferredGap > 0f && clearance > preferredGap)
                    {
                        float pull = clearance - preferredGap;
                        spec.position -= dir * pull;
                        movedThisIter = true;
                        moved++;
                    }
                }

                if (!movedThisIter)
                {
                    break;
                }
            }

            return moved;
        }

        public static List<Transform> BuildSlotsFromPath(
            ConveyorPath path,
            float desiredSpacing,
            int explicitSlotCount,
            out float usedSpacing,
            bool smoothCorners = false,
            float smoothTension = 0.2f,
            int smoothSubdivisions = 4)
        {
            return BuildSlotsFromPathInternal(
                path,
                desiredSpacing,
                explicitSlotCount,
                out usedSpacing,
                smoothCorners,
                smoothTension,
                smoothSubdivisions,
                out _);
        }

        public static List<Transform> BuildSlotsFromPath(
            ConveyorPath path,
            float desiredSpacing,
            int explicitSlotCount,
            out float usedSpacing,
            bool smoothCorners,
            float smoothTension,
            int smoothSubdivisions,
            out BeltPathCache cache)
        {
            return BuildSlotsFromPathInternal(
                path,
                desiredSpacing,
                explicitSlotCount,
                out usedSpacing,
                smoothCorners,
                smoothTension,
                smoothSubdivisions,
                out cache);
        }

        private static List<Transform> BuildSlotsFromPathInternal(
            ConveyorPath path,
            float desiredSpacing,
            int explicitSlotCount,
            out float usedSpacing,
            bool smoothCorners,
            float smoothTension,
            int smoothSubdivisions,
            out BeltPathCache cache)
        {
            usedSpacing = desiredSpacing;
            cache = null;
            var slots = new List<Transform>();
            if (path == null || path.points == null || path.points.Count < 2)
            {
                return slots;
            }

            bool loop = path.loop;
            var basePoints = new List<Vector2>(path.points);
            if (loop && basePoints.Count > 1 && (basePoints[0] - basePoints[basePoints.Count - 1]).sqrMagnitude < 0.0001f)
            {
                basePoints.RemoveAt(basePoints.Count - 1);
            }

            var samplePoints = smoothCorners
                ? (loop
                    ? BuildRoundedPathLoop(basePoints, desiredSpacing, smoothTension, smoothSubdivisions)
                    : BuildRoundedPath(basePoints, desiredSpacing, smoothTension, smoothSubdivisions))
                : new List<Vector2>(basePoints);

            if (samplePoints.Count < 2)
            {
                return slots;
            }

            if (loop && samplePoints.Count > 1 &&
                (samplePoints[0] - samplePoints[samplePoints.Count - 1]).sqrMagnitude < 0.0001f)
            {
                samplePoints.RemoveAt(samplePoints.Count - 1);
            }

            var evalPoints = loop
                ? new List<Vector2>(samplePoints) { samplePoints[0] }
                : samplePoints;

            float total = 0f;
            for (int i = 0; i < evalPoints.Count - 1; i++)
            {
                total += Vector2.Distance(evalPoints[i], evalPoints[i + 1]);
            }

            if (total <= 0.0001f)
            {
                return slots;
            }

            int slotCount = explicitSlotCount > 0 ? explicitSlotCount : 50; // default 50 slots
            slotCount = Mathf.Max(1, slotCount);
            float step = total / slotCount;
            usedSpacing = step;

            var cumulative = BuildCumulativeLengths(evalPoints);
            float offset = 0f;

            for (int i = 0; i < slotCount; i++)
            {
                float dist = i * step + offset;
                if (loop)
                {
                    dist = dist % total;
                }
                var pos = PointAtDistance(evalPoints, cumulative, dist);
                var t = new GameObject($"Slot_{i}").transform;
                t.position = pos;
                slots.Add(t);
            }

            cache = new BeltPathCache
            {
                Loop = loop,
                TotalLength = total,
                Offset = offset,
                EvalPoints = evalPoints,
                Cumulative = cumulative
            };

            return slots;
        }

        public static Vector3 PointAtDistance(ConveyorPath path, float dist)
        {
            if (path == null || path.points == null || path.points.Count < 2)
            {
                return Vector3.zero;
            }

            float remaining = dist;
            for (int i = 0; i < path.points.Count - 1; i++)
            {
                var a = path.points[i];
                var b = path.points[i + 1];
                float seg = Vector2.Distance(a, b);
                if (remaining <= seg)
                {
                    float t = seg <= 0.0001f ? 0f : remaining / seg;
                    var p = Vector2.Lerp(a, b, t);
                    return new Vector3(p.x, p.y, 0f);
                }

                remaining -= seg;
            }

            var last = path.points[path.points.Count - 1];
            return new Vector3(last.x, last.y, 0f);
        }

        /// <summary>
        /// Rounds corners minimally: keeps straight segments, replaces each corner with two points offset along incoming/outgoing edges.
        /// Subdivisions controls how many samples between the two offset points (>=2).
        /// </summary>
        private static List<Vector2> BuildRoundedPath(IList<Vector2> pts, float desiredSpacing, float tension, int subdivisions)
        {
            var samples = new List<Vector2>();
            if (pts == null || pts.Count < 2)
            {
                return samples;
            }

            tension = Mathf.Clamp01(tension);
            subdivisions = Mathf.Max(2, subdivisions);

            samples.Add(pts[0]);

            for (int i = 1; i < pts.Count - 1; i++)
            {
                var prev = pts[i - 1];
                var curr = pts[i];
                var next = pts[i + 1];

                var dirIn = curr - prev;
                var dirOut = next - curr;
                float lenIn = dirIn.magnitude;
                float lenOut = dirOut.magnitude;

                if (lenIn < 0.0001f || lenOut < 0.0001f)
                {
                    samples.Add(curr);
                    continue;
                }

                dirIn /= lenIn;
                dirOut /= lenOut;

                float maxRadius = desiredSpacing > 0f ? desiredSpacing * 0.75f : Mathf.Min(lenIn, lenOut);
                float radius = Mathf.Min(lenIn, lenOut, maxRadius) * tension;

                var pIn = curr - dirIn * radius;
                var pOut = curr + dirOut * radius;

                samples.Add(pIn);
                for (int s = 1; s < subdivisions - 1; s++)
                {
                    float t = s / (float)(subdivisions - 1);
                    samples.Add(Vector2.Lerp(pIn, pOut, t));
                }
                samples.Add(pOut);
            }

            samples.Add(pts[pts.Count - 1]);
            return samples;
        }

        private static List<Vector2> BuildRoundedPathLoop(IList<Vector2> pts, float desiredSpacing, float tension, int subdivisions)
        {
            var samples = new List<Vector2>();
            if (pts == null || pts.Count < 3) return samples;

            tension = Mathf.Clamp01(tension);
            subdivisions = Mathf.Max(2, subdivisions);

            int n = pts.Count;
            for (int i = 0; i < n; i++)
            {
                var prev = pts[(i - 1 + n) % n];
                var curr = pts[i];
                var next = pts[(i + 1) % n];

                var dirIn = curr - prev;
                var dirOut = next - curr;
                float lenIn = dirIn.magnitude;
                float lenOut = dirOut.magnitude;
                if (lenIn < 0.0001f || lenOut < 0.0001f)
                {
                    samples.Add(curr);
                    continue;
                }

                dirIn /= lenIn;
                dirOut /= lenOut;

                float maxRadius = desiredSpacing > 0f ? desiredSpacing * 0.75f : Mathf.Min(lenIn, lenOut);
                float radius = Mathf.Min(lenIn, lenOut, maxRadius) * tension;

                var pIn = curr - dirIn * radius;
                var pOut = curr + dirOut * radius;

                samples.Add(pIn);
                for (int s = 1; s < subdivisions - 1; s++)
                {
                    float t = s / (float)(subdivisions - 1);
                    samples.Add(Vector2.Lerp(pIn, pOut, t));
                }
                samples.Add(pOut);
            }

            return samples;
        }

        private static List<Vector2> BuildSamplePoints(
            ConveyorPath path,
            float desiredSpacing,
            bool smoothCorners,
            float smoothTension,
            int smoothSubdivisions,
            float spacingFactor)
        {
            var samples = new List<Vector2>();
            if (path == null || path.points == null || path.points.Count < 2)
            {
                return samples;
            }

            bool loop = path.loop;
            var basePoints = new List<Vector2>(path.points);
            if (loop && basePoints.Count > 1 && (basePoints[0] - basePoints[basePoints.Count - 1]).sqrMagnitude < 0.0001f)
            {
                basePoints.RemoveAt(basePoints.Count - 1);
            }

            var smoothPoints = smoothCorners
                ? (loop
                    ? BuildRoundedPathLoop(basePoints, desiredSpacing, smoothTension, smoothSubdivisions)
                    : BuildRoundedPath(basePoints, desiredSpacing, smoothTension, smoothSubdivisions))
                : new List<Vector2>(basePoints);

            if (smoothPoints.Count < 2)
            {
                return samples;
            }

            if (loop && smoothPoints.Count > 1 &&
                (smoothPoints[0] - smoothPoints[smoothPoints.Count - 1]).sqrMagnitude < 0.0001f)
            {
                smoothPoints.RemoveAt(smoothPoints.Count - 1);
            }

            var evalPoints = loop
                ? new List<Vector2>(smoothPoints) { smoothPoints[0] }
                : smoothPoints;

            float step = Mathf.Max(0.05f, desiredSpacing * Mathf.Clamp(spacingFactor, 0.1f, 1f));
            for (int i = 0; i < evalPoints.Count - 1; i++)
            {
                var a = evalPoints[i];
                var b = evalPoints[i + 1];
                float seg = Vector2.Distance(a, b);
                int steps = Mathf.Max(1, Mathf.CeilToInt(seg / step));
                if (samples.Count == 0)
                {
                    samples.Add(a);
                }
                for (int s = 1; s <= steps; s++)
                {
                    float t = s / (float)steps;
                    samples.Add(Vector2.Lerp(a, b, t));
                }
            }

            return samples;
        }

        private static List<float> BuildCumulativeLengths(List<Vector2> pts)
        {
            var cum = new List<float>(pts.Count);
            float sum = 0f;
            cum.Add(0f);
            for (int i = 1; i < pts.Count; i++)
            {
                sum += Vector2.Distance(pts[i - 1], pts[i]);
                cum.Add(sum);
            }

            return cum;
        }

        public static Vector3 PointAtDistance(IReadOnlyList<Vector2> pts, IReadOnlyList<float> cumulative, float dist)
        {
            if (pts == null || pts.Count == 0 || cumulative == null || cumulative.Count != pts.Count)
            {
                return Vector3.zero;
            }

            float total = cumulative[cumulative.Count - 1];
            if (dist >= total)
            {
                var last = pts[pts.Count - 1];
                return new Vector3(last.x, last.y, 0f);
            }

            for (int i = 0; i < cumulative.Count - 1; i++)
            {
                float start = cumulative[i];
                float end = cumulative[i + 1];
                if (dist <= end)
                {
                    float segLen = end - start;
                    float t = segLen <= 0.0001f ? 0f : (dist - start) / segLen;
                    var p = Vector2.Lerp(pts[i], pts[i + 1], t);
                    return new Vector3(p.x, p.y, 0f);
                }
            }

            var lastPoint = pts[pts.Count - 1];
            return new Vector3(lastPoint.x, lastPoint.y, 0f);
        }

        public static int ResolveBeltSlotIndex(
            BoxSpec spec,
            IList<Transform> slots,
            float blockSize = 0.6f,
            ISet<int> reserved = null,
            ISet<int> avoidForAutoAlign = null)
        {
            if (slots == null || slots.Count == 0)
            {
                return 0;
            }

            // When autoAlignSlot is off, keep the authored beltSlotIndex.
            if (!spec.autoAlignSlot && spec.beltSlotIndex >= 0 && spec.beltSlotIndex < slots.Count)
            {
                return spec.beltSlotIndex;
            }

            var size = ComputeBoxSize(spec, blockSize);
            var mouth = ComputeMouth(spec, size);
            var mouth3 = new Vector3(mouth.x, mouth.y, 0f);

            Vector2 normal2 = Vector2.down;
            switch (spec.opening)
            {
                case OpeningSide.Top: normal2 = Vector2.up; break;
                case OpeningSide.Bottom: normal2 = Vector2.down; break;
                case OpeningSide.Left: normal2 = Vector2.left; break;
                case OpeningSide.Right: normal2 = Vector2.right; break;
            }
            var normal3 = new Vector3(normal2.x, normal2.y, 0f);

            int best = 0;
            float bestDist = float.MaxValue;
            bool foundForward = false;

            // Two-pass search:
            // 1) Prefer slots in front of the opening direction.
            // 2) Fallback to any slot if nothing is in front (or all are reserved).
            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 0; i < slots.Count; i++)
                {
                    if (avoidForAutoAlign != null && avoidForAutoAlign.Contains(i)) continue;
                    if (reserved != null && reserved.Contains(i)) continue;

                    var v = slots[i].position - mouth3;
                    if (pass == 0)
                    {
                        // Require the slot to be roughly "in front" of the mouth.
                        if (Vector3.Dot(v, normal3) <= 0.001f) continue;
                    }

                    float d = v.sqrMagnitude;
                    if (d < bestDist)
                    {
                        bestDist = d;
                        best = i;
                        foundForward = pass == 0;
                    }
                }

                if (bestDist < float.MaxValue)
                {
                    break;
                }
            }

            // If everything is reserved/avoided, just return the nearest slot (even if avoided) to avoid crashes.
            bool bestDisallowed =
                (reserved != null && reserved.Contains(best)) ||
                (avoidForAutoAlign != null && avoidForAutoAlign.Contains(best));
            if (bestDisallowed)
            {
                bestDist = float.MaxValue;
                for (int i = 0; i < slots.Count; i++)
                {
                    var v = slots[i].position - mouth3;
                    float d = v.sqrMagnitude;
                    if (d < bestDist)
                    {
                        bestDist = d;
                        best = i;
                    }
                }
            }

            return best;
        }

        public static Vector2 ComputeBoxSize(BoxSpec spec, float blockSize)
        {
            float unit = blockSize > 0 ? blockSize : 0.6f;
            int cols = Mathf.Max(1, spec.columns);
            int rows = Mathf.Max(1, spec.rows);
            return new Vector2(cols * unit, rows * unit);
        }

        public static Vector2 ComputeMouth(BoxSpec spec, Vector2 size)
        {
            Vector2 normal = Vector2.down;
            switch (spec.opening)
            {
                case OpeningSide.Top: normal = Vector2.up; break;
                case OpeningSide.Bottom: normal = Vector2.down; break;
                case OpeningSide.Left: normal = Vector2.left; break;
                case OpeningSide.Right: normal = Vector2.right; break;
            }

            var half = size * 0.5f;
            float dist = (normal == Vector2.left || normal == Vector2.right) ? half.x : half.y;
            return spec.position + normal * dist;
        }
    }
}
