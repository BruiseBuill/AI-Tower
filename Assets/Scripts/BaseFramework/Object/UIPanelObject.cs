using BF.Tool;
using BF.Utility;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace BF.UI
{
 	public class UIPanelObject : MonoBehaviour
	{
        [SerializeField] TweenAnimation[] tweenAnimations;
        [SerializeField] float animationDuration;
        int animationDurationMillsecond;
        
        [SerializeField] bool isAutoCloseWhenStart;
        [HideIf("isAutoCloseWhenStart")]
        [SerializeField] bool isAutoPlayAnimWhenStart;
        public Action onAnimationCloseOver = delegate { };
        public Action onAnimationOpenOver = delegate { };

        bool isDestroy;

        WaitForSeconds wait_Animation;

        protected virtual void Awake()
        {
            tweenAnimations = GetComponents<TweenAnimation>();
            wait_Animation = new WaitForSeconds(animationDuration);
        }
        protected virtual void Start()
        {
            if (isAutoPlayAnimWhenStart && !isAutoCloseWhenStart) 
            {
                PlayForward();
            }
            if (isAutoCloseWhenStart)
            {
                gameObject.SetActive(false);
            }
        }
        public virtual void OnDestroy()
        {
            isDestroy = true;
            Close();
        }

        public virtual void Open()
        {
            gameObject.SetActive(true);
            if (!isAutoPlayAnimWhenStart)
            {
                PlayForward();
            }

        }
        public virtual void Close()
        {
            if (!isDestroy)
            {
                PlayReverse();
                StartCoroutine("PlayingAnimation");
            }
        }
        IEnumerator PlayingAnimation()
        {
            yield return wait_Animation;
            gameObject.SetActive(false);
            
        }
        

        void PlayForward()
        {
            for (int i = 0; i < tweenAnimations.Length; i++)
            {
                tweenAnimations[i].GetComponent<IPlayForward>()?.PlayForward();
            }
        }
        void PlayReverse()
        {
            for (int i = 0; i < tweenAnimations.Length; i++)
            {
                tweenAnimations[i].GetComponent<IPlayReverse>()?.PlayReverse();
            }
        }
    }
}
