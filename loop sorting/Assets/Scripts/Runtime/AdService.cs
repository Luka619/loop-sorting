using System;
using UnityEngine;
using WeChatWASM;

namespace LoopSorting
{
    internal readonly struct AdResult
    {
        public bool Success { get; }
        public string FailureReason { get; }

        public AdResult(bool success, string failureReason = null)
        {
            Success = success;
            FailureReason = failureReason;
        }
    }

    internal interface IAdService
    {
        void ShowReviveAd(Action<AdResult> onResult);
        void ShowBoosterAd(Action<AdResult> onResult);
    }

    internal static class AdServiceResolver
    {
        public static IAdService Create(MonoBehaviour owner, string reviveAdUnitId, string boosterAdUnitId)
        {
            if (owner == null || Application.isEditor)
            {
                return new MockAdService();
            }
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                return new MockAdService();
            }

#if WEIXINMINIGAME || PLATFORM_WEIXINMINIGAME || (UNITY_WEBGL && !UNITY_EDITOR)
            return new WXAdService(reviveAdUnitId, boosterAdUnitId);
#else
            return new MockAdService();
#endif
        }
    }

    internal sealed class MockAdService : IAdService
    {
        public void ShowReviveAd(Action<AdResult> onResult)
        {
            InvokeSuccess(onResult);
        }

        public void ShowBoosterAd(Action<AdResult> onResult)
        {
            InvokeSuccess(onResult);
        }

        private static void InvokeSuccess(Action<AdResult> onResult)
        {
            onResult?.Invoke(new AdResult(true));
        }
    }

    internal sealed class WXAdService : IAdService
    {
        private readonly string _reviveAdUnitId;
        private readonly string _boosterAdUnitId;
        private const string IncompleteWatchFailureReason = "广告未完整观看";

        public WXAdService(string reviveAdUnitId, string boosterAdUnitId)
        {
            _reviveAdUnitId = reviveAdUnitId;
            _boosterAdUnitId = boosterAdUnitId;
        }

        public void ShowReviveAd(Action<AdResult> onResult)
        {
            ShowRewardedVideo(_reviveAdUnitId, onResult);
        }

        public void ShowBoosterAd(Action<AdResult> onResult)
        {
            ShowRewardedVideo(_boosterAdUnitId, onResult);
        }

        private void ShowRewardedVideo(string adUnitId, Action<AdResult> onResult)
        {
            if (string.IsNullOrWhiteSpace(adUnitId))
            {
                Debug.LogWarning("[AdService] Rewarded video ad unit ID is empty. Continuing without ad.");
                onResult?.Invoke(new AdResult(true));
                return;
            }

            WXRewardedVideoAd ad = null;
            var finished = false;
            string adUnitIdOrEmpty = adUnitId?.Trim();

            Action<WXRewardedVideoAdOnCloseResponse> onClose = null;
            Action<WXADErrorResponse> onLoadFail = null;
            Action<WXTextResponse> onShowFailed = null;
            Action<WXTextResponse> onLoaded = null;
            Action<WXADErrorResponse> onError = null;

            void Finish(bool success, string failureReason = null)
            {
                if (finished) return;

                finished = true;

                if (ad != null)
                {
                    if (onClose != null)
                    {
                        ad.OffClose(onClose);
                    }

                    if (onError != null)
                    {
                        ad.OffError(onError);
                    }

                    ad.Destroy();
                }

                onResult?.Invoke(new AdResult(success, failureReason));
            }

            try
            {
                ad = WX.CreateRewardedVideoAd(new WXCreateRewardedVideoAdParam
                {
                    adUnitId = adUnitIdOrEmpty
                });

                onClose = response =>
                {
                    bool watched = response != null && response.isEnded;
                    Finish(watched, watched ? null : IncompleteWatchFailureReason);
                };

                onError = err =>
                {
                    string reason = NormalizeReason(err?.errMsg, "广告播放报错");
                    Debug.LogWarning($"[AdService] Rewarded video error: {reason}");
                    Finish(false, reason);
                };

                onShowFailed = response =>
                {
                    string reason = NormalizeReason(response?.errMsg, "广告展示失败");
                    Debug.LogWarning("[AdService] Rewarded video failed to show.");
                    Finish(false, reason);
                };

                onLoadFail = response =>
                {
                    string reason = NormalizeReason(response?.errMsg, "广告加载失败");
                    Debug.LogWarning("[AdService] Rewarded video failed to load.");
                    Finish(false, reason);
                };

                onLoaded = _ =>
                {
                    if (finished) return;
                    try
                    {
                        ad.Show(_ => { }, onShowFailed);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[AdService] Rewarded video show threw: {e.Message}");
                        Finish(false, NormalizeReason(e.Message, "播放异常"));
                    }
                };

                ad.OnClose(onClose);
                ad.OnError(onError);
                ad.Load(onLoaded, onLoadFail);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AdService] Rewarded video failed to initialize: {e.Message}");
                Finish(false, NormalizeReason(e.Message, "初始化失败"));
            }
        }

        private static string NormalizeReason(string source, string fallback)
        {
            if (!string.IsNullOrWhiteSpace(source))
            {
                return source.Trim();
            }

            return fallback;
        }
    }
}
