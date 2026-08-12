using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BF.Tool
{
	public abstract class TweenAnimation : MonoBehaviour
	{
		public enum PlayStyle { Once,Repeat,PingPong};

        [FoldoutGroup("Parameter")]
        [EnumToggleButtons]
        public PlayStyle playStyle;

        [FoldoutGroup("Parameter")]
        public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [FoldoutGroup("Parameter")]
        public float duration = 1f;

        [FoldoutGroup("Parameter")]
        public float startDelay = 0f;

        [FoldoutGroup("Parameter")]
        public bool ignoreTimeScale = true;

        [FoldoutGroup("OnFinished")]
        public UnityEvent finishEvent;

        protected Tween tween;
	}
    interface IPlayForward
    {
        void PlayForward();
    }
    interface IPlayReverse
    {
        void PlayReverse();
    }
}