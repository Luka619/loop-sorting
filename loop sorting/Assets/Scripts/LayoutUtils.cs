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

        public static List<Transform> BuildSlotsFromPath(
            ConveyorPath path,
            float desiredSpacing,
            int explicitSlotCount,
            out float usedSpacing,
            bool smoothCorners = false,
            float smoothTension = 0.2f,
            int smoothSubdivisions = 4)
        {
            usedSpacing = desiredSpacing;
            var slots = new List<Transform>();
            if (path == null || path.points == null || path.points.Count < 2)
            {
                return slots;
            }

            var samplePoints = smoothCorners
                ? BuildRoundedPath(path.points, desiredSpacing, smoothTension, smoothSubdivisions)
                : new List<Vector2>(path.points);

            float total = 0f;
            for (int i = 0; i < samplePoints.Count - 1; i++)
            {
                total += Vector2.Distance(samplePoints[i], samplePoints[i + 1]);
            }

            if (total <= 0.0001f)
            {
                return slots;
            }

            int slotCount = explicitSlotCount > 0 ? explicitSlotCount : 50; // default 50 slots
            slotCount = Mathf.Max(1, slotCount);
            float step = total / slotCount;
            usedSpacing = step;

            var cumulative = BuildCumulativeLengths(samplePoints);
            for (int i = 0; i < slotCount; i++)
            {
                float dist = i * step;
                var pos = PointAtDistance(samplePoints, cumulative, dist);
                var t = new GameObject($"Slot_{i}").transform;
                t.position = pos;
                slots.Add(t);
            }

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

        private static Vector3 PointAtDistance(List<Vector2> pts, List<float> cumulative, float dist)
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

        public static int ResolveBeltSlotIndex(BoxSpec spec, IList<Transform> slots, float blockSize = 0.6f)
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

            int best = 0;
            float bestDist = float.MaxValue;
            for (int i = 0; i < slots.Count; i++)
            {
                float d = Vector3.SqrMagnitude(slots[i].position - mouth3);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = i;
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
