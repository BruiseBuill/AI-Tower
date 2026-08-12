using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace BF
{
	[CreateAssetMenu(fileName = "AudioData",menuName ="BF/AudioData")]
	public class AudioData : ScriptableObject
	{
		public enum AudioType
        {
            BGM,
            SFX,
            UI,
			SceneObject
        }

        public AudioType audioType;
        public AudioClip clip;
		public AudioMixerGroup audioMixer;
		
        public float volume = 1;
		public bool isLoop;


        [Header("Optional Parameter")]
        [Range(0,256)]
        [Tooltip("0:Max, 256:Min, 128:Default")]
        public int priority = 128;

        public float volumeMultiple = 1;

        public bool isRandomPitch = false;
        [ShowIf("isRandomPitch")]
        public Vector2 randomPitchRange;

        public bool isDelay = false;
        [ShowIf("isDelay")]
        public float delayTime;
	}
}