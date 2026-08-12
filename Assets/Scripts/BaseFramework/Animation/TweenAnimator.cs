using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BF.Tool
{
    public abstract class TweenAnimator : MonoBehaviour 
    {
        [SerializeField] protected List<TweenAnimation> animationList = new List<TweenAnimation>();
    }
    interface ITweenAnimatorPlay
    {
        void Play();
    }
    interface ITweenAnimatorRedoPlay
    {
        void RedoPlay();
    }
}
