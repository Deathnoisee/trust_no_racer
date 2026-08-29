using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace SmallHedge.SoundManager
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private SoundsSO SO;
        [SerializeField] private AudioSource ambianceSource; // separate source — assign in Inspector

        private static SoundManager instance = null;
        private AudioSource audioSource;
        private SoundType? currentMusic = null;
        private AudioClip lastPlayedClip = null;

        public bool isMainMenu = false;

        private void Awake()
        {
            if (!instance)
            {
                instance = this;
                audioSource = GetComponent<AudioSource>();
            }
        }

        private void Start()
        {
            if (!isMainMenu)
            {
                // StartCoroutine(StartMusic());
                StartAmbiance(SoundType.Ambient, 0.5f);
                StartCoroutine(StartMusic());
            }
        }

        IEnumerator StartMusic()
        {
            yield return new WaitForSeconds(0.1f);
            PlayMusicInternal(SoundType.Jazz, audioSource, 0.2f);
        }

        private void Update()
        {
            if (currentMusic != null && !audioSource.isPlaying)
            {
                PlayMusicInternal(currentMusic.Value, audioSource);
            }
        }
        public static void PlayMusic(SoundType sound, float volume = 1f)
        {
            if (instance == null) return;
            instance.PlayMusicInternal(sound, instance.audioSource, volume);
        }

        private void PlayMusicInternal(SoundType sound, AudioSource source, float volume = 1)
        {
            currentMusic = sound;
            SoundList soundList = SO.sounds[(int)sound];
            AudioClip[] clips = soundList.sounds;

            AudioClip randomClip = GetRandomClipExcludingLast(clips);
            lastPlayedClip = randomClip;

            source.outputAudioMixerGroup = soundList.mixer;
            source.clip = randomClip;
            source.volume = volume * soundList.volume;
            source.loop = false;
            source.Play();
        }
        private AudioClip GetRandomClipExcludingLast(AudioClip[] clips)
        {
            if (clips.Length <= 1)
            {
                return clips[0];
            }

            AudioClip chosen;
            do
            {
                chosen = clips[UnityEngine.Random.Range(0, clips.Length)];
            }
            while (chosen == lastPlayedClip);

            return chosen;
        }

        public static void StopMusic()
        {
            if (instance != null && instance.audioSource != null && instance.audioSource.isPlaying)
            {
                instance.audioSource.Stop();
                instance.currentMusic = null;
            }
        }

        public static void PlaySound(SoundType sound, AudioSource source = null, float volume = 1)
        {
            SoundList soundList = instance.SO.sounds[(int)sound];
            AudioClip[] clips = soundList.sounds;
            AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

            if (source)
            {
                source.outputAudioMixerGroup = soundList.mixer;
                source.clip = randomClip;
                source.volume = volume * soundList.volume;
                source.Play();
            }
            else
            {
                instance.audioSource.outputAudioMixerGroup = soundList.mixer;
                instance.audioSource.PlayOneShot(randomClip, volume * soundList.volume);
            }
        }

        // ---- Ambiance (works like music, but loops and runs independently) ----

        public static void StartAmbiance(SoundType sound, float volume = 1)
        {
            if (instance == null || instance.ambianceSource == null) return;

            SoundList soundList = instance.SO.sounds[(int)sound];
            AudioClip[] clips = soundList.sounds;
            AudioClip chosenClip = clips[UnityEngine.Random.Range(0, clips.Length)];

            instance.ambianceSource.outputAudioMixerGroup = soundList.mixer;
            instance.ambianceSource.clip = chosenClip;
            instance.ambianceSource.volume = volume * soundList.volume;
            instance.ambianceSource.loop = true; // ambiance loops continuously, unlike music
            instance.ambianceSource.Play();
        }

        public static void StopAmbiance()
        {
            if (instance != null && instance.ambianceSource != null && instance.ambianceSource.isPlaying)
            {
                instance.ambianceSource.Stop();
            }
        }

        public static bool IsAmbiancePlaying()
        {
            return instance != null && instance.ambianceSource != null && instance.ambianceSource.isPlaying;
        }
    }

    [Serializable]
    public struct SoundList
    {
        [HideInInspector] public string name;
        [Range(0, 1)] public float volume;
        public AudioMixerGroup mixer;
        public AudioClip[] sounds;
    }
}