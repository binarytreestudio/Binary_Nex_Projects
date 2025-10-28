using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    public enum SFXAudioType
    {
        Hit,
        Miss,
    }
    public enum MusicAudioType
    {

    }

    [Serializable]
    private class SFXAudioMapping
    {
        public SFXAudioType audioType;
        public AudioSource audioSource;
    }
    [Serializable]
    private class MusicAudioMapping
    {
        public MusicAudioType audioType;
        public AudioSource audioSource;
    }

    [SerializeField] private List<SFXAudioMapping> sFXAudioClips = new();
    [SerializeField] private List<MusicAudioMapping> musicAudioClips = new();

    public void PlayAudio(SFXAudioType audioType)
    {
        sFXAudioClips.Find(clip => clip.audioType == audioType)?.audioSource.Play();
    }
    public void PlayAudio(MusicAudioType audioType)
    {
        musicAudioClips.Find(clip => clip.audioType == audioType)?.audioSource.Play();
    }

    public void StopAudio(SFXAudioType audioType)
    {
        sFXAudioClips.Find(clip => clip.audioType == audioType)?.audioSource.Stop();
    }
    public void StopAudio(MusicAudioType audioType)
    {
        musicAudioClips.Find(clip => clip.audioType == audioType)?.audioSource.Stop();
    }

    public void StopAllAudio()
    {
        sFXAudioClips.ForEach(clip => clip.audioSource.Stop());
        musicAudioClips.ForEach(clip => clip.audioSource.Stop());
    }
}
