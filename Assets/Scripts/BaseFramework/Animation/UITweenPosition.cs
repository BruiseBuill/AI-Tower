using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace BF.Tool
{
    public class UITweenPosition : TweenAnimation, IPlayForward, IPlayReverse
    {
        public Vector3 from;
        public Vector3 to;
        [SerializeField] RectTransform rectTransform;

        public void PlayForward()
        {
            Play(from, to);
        }
        public void PlayReverse()
        {
            Play(to, from);
        }
        void Play(Vector3 from, Vector3 to)
        {
            if (tween != null && tween.active)
                tween.Kill();

            rectTransform.anchoredPosition = from;
            tween = rectTransform.DOAnchorPos(to, duration)
                                  .SetDelay(startDelay)
                                   .SetUpdate(ignoreTimeScale)
                                   .SetEase(animationCurve)
                                   .SetAutoKill(false);

            switch (playStyle)
            {
                case PlayStyle.Once:
                    break;
                case PlayStyle.Repeat:
                    tween.SetLoops(-1, LoopType.Restart);
                    break;
                case PlayStyle.PingPong:
                    tween.SetLoops(-1, LoopType.Yoyo);
                    break;
            }
            tween.onComplete += () => finishEvent.Invoke();
        }
    }
}
