using System;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LoopSorting.Editor
{
    public sealed class PythonUtf8BuildWorkaround : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
#if UNITY_EDITOR_WIN
        private static readonly Dictionary<string, string> Prev = new Dictionary<string, string>();
        private static bool _applied;
#endif

        public int callbackOrder => int.MinValue;

        public void OnPreprocessBuild(BuildReport report)
        {
#if UNITY_EDITOR_WIN
            if (report == null) return;

            // Applies only to WebGL/WeixinMiniGame builds where the WebGL toolchain spawns Python (Emscripten).
            var platformName = report.summary.platform.ToString();
            if (platformName != "WebGL" && platformName != "WeixinMiniGame")
            {
                return;
            }

            try
            {
                ApplyEnv("PYTHONUTF8", "1");
                ApplyEnv("PYTHONIOENCODING", "utf-8");
                ApplyEnv("LANG", "en_US.UTF-8");
                ApplyEnv("LC_ALL", "en_US.UTF-8");
                _applied = true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"PythonUtf8BuildWorkaround: failed to set env vars ({e.GetType().Name}).");
            }
#endif
        }

        public void OnPostprocessBuild(BuildReport report)
        {
#if UNITY_EDITOR_WIN
            if (!_applied) return;
            _applied = false;
            try
            {
                RestoreEnv("PYTHONUTF8");
                RestoreEnv("PYTHONIOENCODING");
                RestoreEnv("LANG");
                RestoreEnv("LC_ALL");
            }
            catch
            {
                // Best-effort restore only.
            }
#endif
        }

#if UNITY_EDITOR_WIN
        private static void ApplyEnv(string key, string value)
        {
            if (!Prev.ContainsKey(key))
            {
                Prev[key] = Environment.GetEnvironmentVariable(key);
            }
            Environment.SetEnvironmentVariable(key, value);
        }

        private static void RestoreEnv(string key)
        {
            if (!Prev.TryGetValue(key, out var prev)) return;
            Environment.SetEnvironmentVariable(key, prev);
            Prev.Remove(key);
        }
#endif
    }
}

