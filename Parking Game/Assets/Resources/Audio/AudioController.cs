using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum Mixers {
    Music,
    SFX,
    UI
}

[System.Serializable]
public class SFXConfig {
    public string name;
    public AudioClip audioClip;
    public float volume = 0.5f;
}

public class AudioController : MonoBehaviour {

    public static AudioController main;

    public AudioMixerGroup musicMixer;
    public AudioMixerGroup sfxMixer;
    public AudioMixerGroup uiMixer;

    [Space]
    public SFXConfig[] SFXConfigs;

    private AudioSource activeMusicPlayer;

    private readonly List<AudioSource> sources = new List<AudioSource>();
    private readonly List<AudioSource> fadeSources = new List<AudioSource>();

    public AudioSource ActiveMusicPlayer {
        get => this.activeMusicPlayer;
    }

    void Awake() {
        if (AudioController.main == null) {
            AudioController.main = this;
        }
    }

    void Update() {
        if (this.fadeSources.Count > 0) {
            this.HandleFadeOuts();
        }
    }

    public void PlayMusic(AudioClip main, float volume = 0.5f) {
        if (this.activeMusicPlayer != null) {
            this.activeMusicPlayer.Stop();
        }

        AudioSource mainSource = this.FindFirstAvailableSource(Mixers.Music);
        mainSource.clip = main;
        mainSource.volume = volume;
        mainSource.loop = true;
        mainSource.Play();
        this.activeMusicPlayer = mainSource;
    }

    public void PlayMusic(AudioClip intro, AudioClip main, float volume = 0.5f) {
        if (this.activeMusicPlayer != null) {
            this.activeMusicPlayer.Stop();
        }

        AudioSource introSource = this.FindFirstAvailableSource(Mixers.Music);
        introSource.clip = intro;
        introSource.volume = volume;
        introSource.loop = false;
        introSource.Play();

        AudioSource mainSource = this.FindFirstAvailableSource(Mixers.Music);
        mainSource.clip = main;
        mainSource.volume = volume;
        mainSource.loop = true;

        new TimedTrigger(intro.length, () => {
            introSource.Stop();
            mainSource.Play();
            this.activeMusicPlayer = mainSource;
        });
    }

    public Action InteruptMusic(AudioClip main, float volume = 0.5f) {
        float origionalVolume = this.activeMusicPlayer.volume;
        this.activeMusicPlayer.volume = 0f;
        AudioSource interruptionSource = this.PlayMusicOnNewTrack(main, volume);
        return () => {
            interruptionSource.Stop();
            this.activeMusicPlayer.volume = origionalVolume;
        };
    }

    public AudioSource PlayClip(string clipName, Mixers mixer = Mixers.SFX, bool looping = false) {
        SFXConfig config = this.GetConfigByName(clipName);
        return this.PlayClip(config.audioClip, mixer, config.volume, looping);
    }

    public AudioSource PlayClip(AudioClip clip, Mixers mixer, float volume = 0.5f, bool looping = false) {
        AudioSource source = this.FindFirstAvailableSource(mixer);
        source.clip = clip;
        source.volume = volume;
        source.loop = looping;
        source.Play();
        return source;
    }

    public void FadeOutClip(AudioSource source) {
        this.fadeSources.Add(source);
    }

    public void StopMixer(Mixers mixer) {
        for (int i = 0; i < this.sources.Count; i++) {
            if (this.sources[i].outputAudioMixerGroup == this.MixersToMixerGroup(mixer)) {
                this.sources[i].Stop();
            }
        }
    }

    public SFXConfig GetConfigByName(string name) {
        for (int i = 0; i < this.SFXConfigs.Length; i++) {
            if (this.SFXConfigs[i].name == name) {
                return this.SFXConfigs[i];
            }
        }
        return null;
    }

    private AudioSource FindFirstAvailableSource(Mixers mixer) {
        foreach (AudioSource source in this.sources) {
            Mixers target = this.MixerGroupToMixers(source.outputAudioMixerGroup);
            if (!source.isPlaying && mixer == target) {
                return source;
            }
        }
        return this.AddAudioSource(mixer);
    }

    private AudioSource AddAudioSource(Mixers mixer) {
        var source = this.gameObject.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = this.MixersToMixerGroup(mixer);
        this.sources.Add(source);
        return source;
    }

    private AudioMixerGroup MixersToMixerGroup(Mixers mixer) {
        switch (mixer) {
            case Mixers.Music: return this.musicMixer;
            case Mixers.SFX: return this.sfxMixer;
            case Mixers.UI: return this.uiMixer;
            default: return null;
        }
    }

    private Mixers MixerGroupToMixers(AudioMixerGroup group) {
        if (group == this.musicMixer) { return Mixers.Music; }
        if (group == this.sfxMixer) { return Mixers.SFX; }
        if (group == this.uiMixer) { return Mixers.UI; }
        return Mixers.SFX;
    }

    private AudioSource PlayMusicOnNewTrack(AudioClip main, float volume = 0.5f) {
        AudioSource mainSource = this.FindFirstAvailableSource(Mixers.Music);
        mainSource.clip = main;
        mainSource.volume = volume;
        mainSource.loop = true;
        mainSource.Play();
        return mainSource;
    }

    private void HandleFadeOuts() {
        List<int> toRemove = new List<int>();
        for (int i = 0; i < this.fadeSources.Count; i++) {
            AudioSource source = this.fadeSources[i];
            source.volume -= 0.05f;
            if (source.volume <= 0) {
                source.Stop();
                toRemove.Add(i);
            }
        }
        for (int j = 0; j < toRemove.Count; j++) {
            if (j <= this.fadeSources.Count) {
                this.fadeSources.RemoveAt(toRemove[j]);
            }
        }
    }
}
