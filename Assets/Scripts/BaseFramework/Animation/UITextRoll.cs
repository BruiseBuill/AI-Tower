using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace BF.Tool
{
    public class UITextRoll:TweenAnimation,IPlayForward
    {
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] int currentValue;
        [SerializeField] int newValue;

        private void Start()
        {
            if (text != null)
            {
                text.text = currentValue.ToString();
            }
        }
        
        public void SetNumber(int newValue)
        {
            if (newValue == currentValue) 
                return; // 如果数值没变，则不执行动画

            if (tween != null && tween.active)
            {
                tween.Kill();
            }
            this.newValue = newValue;
            PlayForward();
        }

        public void PlayForward()
        {
            int startValue = currentValue;
            currentValue = newValue;
            
            tween = DOTween.To(() => startValue, x =>
            {
                startValue = x;
                if (text != null)
                {
                    text.text = startValue.ToString();
                }
            }, newValue, duration).SetDelay(startDelay)
                                   .SetUpdate(ignoreTimeScale)
                                   .SetEase(animationCurve)
                                   .SetAutoKill(false);
            tween.onComplete += () => finishEvent.Invoke();
        }
    }
}
