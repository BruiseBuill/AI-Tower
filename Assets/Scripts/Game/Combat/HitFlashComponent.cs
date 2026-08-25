using System;
using System.Collections;
using BF;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Data 对外暴露的伤害事件契约。表现组件只依赖该接口，不依赖具体 Control。
    /// </summary>
    public interface IDamagePresentation
    {
        event Action<float> Damaged;
    }

    /// <summary>
    /// 通用受击闪白：受到伤害时短暂改变主体颜色，随后恢复原色。
    /// 可挂在任意 Data 实现了 IDamagePresentation 的对象上（士兵 / 防御塔 / 大本营）。
    /// </summary>
    public class HitFlashComponent : BaseComponent
    {
        [SerializeField] SpriteRenderer bodyRenderer;
        [SerializeField] Color flashColor = Color.white;
        [SerializeField, Range(0.02f, 0.5f)] float flashDuration = 0.08f;

        IDamagePresentation damageSource;
        Color originalColor = Color.white;
        Coroutine flashRoutine;

        protected override void Awake()
        {
            base.Awake();
            damageSource = data as IDamagePresentation;
            if (damageSource == null)
            {
                Debug.LogError($"{GetType().Name} 要求 Data 实现 {nameof(IDamagePresentation)}。", this);
                enabled = false;
            }
        }

        protected override void OnOpen()
        {
            if (bodyRenderer != null)
            {
                originalColor = bodyRenderer.color;
            }
            if (damageSource != null)
            {
                damageSource.Damaged += HandleDamaged;
            }
        }

        protected override void OnClose()
        {
            if (damageSource != null)
            {
                damageSource.Damaged -= HandleDamaged;
            }
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                flashRoutine = null;
            }
            if (bodyRenderer != null)
            {
                bodyRenderer.color = originalColor;
            }
        }

        void HandleDamaged(float amount)
        {
            if (bodyRenderer == null)
            {
                return;
            }
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }
            flashRoutine = StartCoroutine(Flash());
        }

        IEnumerator Flash()
        {
            bodyRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            bodyRenderer.color = originalColor;
            flashRoutine = null;
        }
    }
}
