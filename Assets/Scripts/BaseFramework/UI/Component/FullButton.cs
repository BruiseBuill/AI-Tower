using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BF.UI
{
	public class FullButton : Button,IPointerDownHandler,IPointerUpHandler
	{
        public UnityAction onPointerDown = delegate { };
        public UnityAction onPointerUp = delegate { };

        public AudioData audioData_PointDown;
        public AudioData audioData_PointUp;
        AudioBuilder audioBuilder_PointDown;
        AudioBuilder audioBuilder_PointUp;

        protected override void Awake()
        {
            base.Awake();
            if (audioData_PointDown != null && audioData_PointUp != null) 
            {
                audioBuilder_PointDown = new AudioBuilder(audioData_PointDown);
                audioBuilder_PointUp = new AudioBuilder(audioData_PointUp);
            }            
        }
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            onPointerDown.Invoke();
            AudioManager.Instance().Play(audioBuilder_PointDown);
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            onPointerUp.Invoke();
            AudioManager.Instance().Play(audioBuilder_PointUp);
        }
    }
}