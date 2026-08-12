using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using static UnityEditor.ObjectChangeEventStream;

namespace BF
{
    public class AudioManager : Single<AudioManager>
    {
        [Header("Global AudioSource Setting")]
        [SerializeField] AudioSource UISource;
        [SerializeField] AudioSource SFXSource;
        [SerializeField] AudioSource BGMSource;

        [Header("Scene AudioSource Setting")]
        [SerializeField] GameObject audioPrefab;        

        #region Play
        public void Play(AudioBuilder builder)
        {
            if (builder == null) 
            {
#if UNITY_EDITOR
                Debug.Log("AudioBuilder is null");
#endif
                return;
            }
            switch (builder.audioData.audioType)
            {
                case AudioData.AudioType.BGM:
                    PlayBGMAudio(builder);
                    break;
                case AudioData.AudioType.SFX:
                    PlaySFXAudio(builder);
                    break;
                case AudioData.AudioType.UI:
                    PlayUIAudio(builder);
                    break;
                case AudioData.AudioType.SceneObject:
                    PlaySceneAudio(builder);
                    break;
            }
        }  
        void PlayBGMAudio(AudioBuilder builder)
        {
            builder.Play(BGMSource);
        }
        void PlayUIAudio(AudioBuilder builder)
        {
            builder.Play(UISource);
        }
        void PlaySFXAudio(AudioBuilder builder)
        {
            builder.Play(SFXSource);
        }
        void PlaySceneAudio(AudioBuilder builder)
        {
            var go = PoolManager.Instance().Release(audioPrefab.name);
            builder.Play(go.GetComponent<AudioSource>());
            if (!builder.audioData.isLoop)
            {
                StartCoroutine("RecyclingAudioSource", builder);
            }
        }
        IEnumerator RecyclingAudioSource(AudioBuilder builder)
        {
            var source = builder.LastAudioSource;
            yield return new WaitForSeconds(builder.audioData.clip.length);
            PoolManager.Instance().Recycle(source.gameObject);
        }
        #endregion
        public void Pause(AudioBuilder builder)
        {
            if (builder.LastAudioSource && builder.LastAudioSource.isPlaying)
                builder.LastAudioSource.Pause();
        }
        public void UnPause(AudioBuilder builder)
        {
            if (builder.LastAudioSource && builder.LastAudioSource.isPlaying)
                builder.LastAudioSource.UnPause();
        }
        public void Stop(AudioBuilder builder)
        {
            if (builder.LastAudioSource)
            {
                if (!builder.audioData.isLoop)
                {
                    StopCoroutine("RecyclingAudioSource");
                }
                PoolManager.Instance().Recycle(builder.LastAudioSource.gameObject);
            }
        }
    }

    public class AudioBuilder
    {
        //Necessary
        public AudioData audioData;
        public Vector3 pos;
        public Transform parent;

        AudioSource lastAudioSource;
        public AudioSource LastAudioSource => lastAudioSource;

        public AudioBuilder(AudioData audioData)
        {
            this.audioData = audioData;
        }
        public AudioBuilder(AudioData audioData, Vector3 pos)
        {
            this.audioData = audioData;
            this.pos = pos;
            parent = null;
        }
        public AudioBuilder(AudioData audioData, Transform parent)
        {
            this.audioData = audioData;
            this.parent = parent;
        }
        public void Play(AudioSource audioSource)
        {
            lastAudioSource = audioSource;

            if (audioData.audioType == AudioData.AudioType.SceneObject) 
            {
                if (parent != null)
                {
                    lastAudioSource.transform.parent = parent;
                }
                else
                {
                    lastAudioSource.transform.parent = PoolManager.Instance().transform;
                    lastAudioSource.transform.position = pos;
                }
            }
            
            lastAudioSource.clip = audioData.clip;
            lastAudioSource.outputAudioMixerGroup = audioData.audioMixer;
            lastAudioSource.loop = audioData.isLoop;

            if (audioData.isRandomPitch)
                lastAudioSource.pitch = Random.Range(audioData.randomPitchRange.x, audioData.randomPitchRange.y);
            else
                lastAudioSource.pitch = 1f;

            if (audioData.isDelay)
                lastAudioSource.PlayDelayed(audioData.delayTime);
            else
                lastAudioSource.Play();
        }
    }
}

