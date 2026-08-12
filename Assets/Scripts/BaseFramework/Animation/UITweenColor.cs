using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BF.Tool
{
    public class UITweenUIColor : TweenAnimation, IPlayForward, IPlayReverse
    {
        public Color from;
        public Color to;
        [SerializeField] Graphic graphic;
     
        public void PlayForward()
        {
            Play(from, to);
        }
        public void PlayReverse()
        {
            Play(to, from);
        }
        void Play(Color from, Color to)
        {
            if (tween != null && tween.active)
                tween.Kill();

            graphic.color = from;
            tween = graphic.DOColor(to, duration)
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
