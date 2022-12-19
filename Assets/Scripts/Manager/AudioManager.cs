using UnityEngine;
using UnityEngine.Audio;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioMixerGroup _musicMixerGroup;
    [SerializeField] private AudioMixerGroup _SFXMixerGroup;
    [SerializeField] private Sound[] _sounds;

    private void Awake()
    {
        //Singleton Design Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }


        //Assign atributes to the sound
        foreach (Sound s in _sounds)
        {
            s.Source = gameObject.AddComponent<AudioSource>();
            s.Source.clip = s.Clip;

            s.Source.volume = s.Volume;
            s.Source.pitch = s.Pitch;
            s.Source.loop = s.Loop;

            switch(s.AudioType)
            {
                case (Sound.AudioTypes.SFX):
                    s.Source.outputAudioMixerGroup = _SFXMixerGroup;
                    break;
                case (Sound.AudioTypes.Music):
                    s.Source.outputAudioMixerGroup = _musicMixerGroup;
                    break;
            }
        }
    }

    private void Start()
    {
        Play("Light Snow");
    }

    public static AudioManager GetInstance()
    {
        return Instance;
    }


    //Play/Stop Audio Clip
    //ex use: AudioManager.GetInstance().Play("name");
    public void Play(string name)
    {
        Sound s = FindSound(name);  
        if(s == null) return;
        s.Source.Play();
    }

    public void Stop(string name)
    {
        Sound s = FindSound(name);
        if(s == null) return;
        s.Source.Stop();
    }

    public void Pause(string name)
    {
        Sound s = FindSound(name);
        if(s == null) return;
        s.Source.Pause();
    }

    public bool IsPlaying(string name)
    {
        Sound s = FindSound(name);
        if (s == null) return false;
        return s.Source.isPlaying;
    }

    private Sound FindSound(string name) {
        Sound s = Array.Find(_sounds, sound => sound.Name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found.");
            return null;
        }
        return s;
    }
}
