using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using Microsoft.Xna.Framework;

namespace Retroverse
{
    public static class SoundManager
    {
        public const string AUDIO_ROOT = @"\Audio\Waves\";
        public const float DEFAULT_VOLUME = 1f;

        private static HashSet<SoundReversibleInstance> registeredSounds = new HashSet<SoundReversibleInstance>();
        private static List<SoundReversibleInstance> soundsToUnregister = new List<SoundReversibleInstance>();

        private static Dictionary<string, SoundReversible> SoundMap = new Dictionary<string, SoundReversible>();
        private static Dictionary<string, SoundReversibleInstance> SoundLoops = new Dictionary<string, SoundReversibleInstance>();
        private static Dictionary<string, bool> HasPlayedMap = new Dictionary<string, bool>();
        private static SoundReversibleInstance currentMusicInstance = null;
        private static string currentMusicName = null;
        public static string CurrentMusicName { get { return currentMusicName; } }

        private static GameTime latestGameTime;
        private static List<Tuple<SoundReversible, TimeSpan>> soundsToSaveForReversePlayback = new List<Tuple<SoundReversible, TimeSpan>>();
        private static List<SoundReversible> reversibleSoundsEndedThisFrame = new List<SoundReversible>();
        private static List<string> reversibleSoundLoopsStartedThisFrame = new List<string>();
        private static List<string> reversibleSoundLoopsEndedThisFrame = new List<string>();
        public const float PERCENTAGE_SOUND_REVERSE_DELAY = 0.25f; //make sounds in reverse start playing earlier than they actually ended, to better make it seem like the sound coincides with the action

        private static Random rand = new Random();

        private static float targetVolume = DEFAULT_VOLUME;
        private static float currentVolume = targetVolume;
        private const float DEFAULT_VOLUME_SPEED = 1f; //mute to full in 1 second
        private static float volumeSpeed = DEFAULT_VOLUME_SPEED;
        public static float TargetVolume { get { return targetVolume; } }
        public static float CurrentVolume { get { return currentVolume; } }

        private static float masterVolume = DEFAULT_VOLUME;
        private static float musicMasterVolume = DEFAULT_VOLUME;
        private static float soundMasterVolume = DEFAULT_VOLUME;
        public static float MasterVolume { get { return masterVolume; } }
        public static float MusicMasterVolume { get { return musicMasterVolume; } }
        public static float SoundMasterVolume { get { return soundMasterVolume; } }

        public static void Initialize()
        {
            registeredSounds = new HashSet<SoundReversibleInstance>();
            soundsToUnregister = new List<SoundReversibleInstance>();
            HasPlayedMap = new Dictionary<string, bool>();
            currentMusicInstance = null;
            currentMusicName = null;
            targetVolume = DEFAULT_VOLUME;
            currentVolume = targetVolume;
        }

        public static void RegisterSoundInstance(SoundReversibleInstance instance)
        {
            registeredSounds.Add(instance);
        }

        public static void UnregisterSoundInstance(SoundReversibleInstance instance)
        {
            soundsToUnregister.Add(instance);
        }

        public static void PlaySoundOnce(string soundName, PlayMode mode = PlayMode.Forward, bool playInReverseDuringReverse = false)
        {
            if (soundName == null)
                return;
            if (SoundMap.ContainsKey(soundName))
            {
                PlaySoundOnce(SoundMap[soundName], mode, playInReverseDuringReverse);
            }
        }

        public static void PlaySoundOnce(SoundReversible sound, PlayMode mode, bool playInReverseDuringReverse)
        {
            if (mode == PlayMode.Forward)
            {
                sound.Play(soundMasterVolume, 0, 0);
                if (playInReverseDuringReverse)
                    soundsToSaveForReversePlayback.Add(new Tuple<SoundReversible, TimeSpan>(sound, latestGameTime.TotalGameTime + sound.Length - new TimeSpan((long)(sound.Length.Ticks * (1 - PERCENTAGE_SOUND_REVERSE_DELAY)))));
            }
            else
            {
                sound.Play(soundMasterVolume, 0, 0, true);
            }
        }

        public static void PlaySoundOnLoop(string soundName, PlayMode mode = PlayMode.Forward)
        {
            if (soundName == null)
                return;
            if (SoundLoops.ContainsKey(soundName))
            {
            }
            else if (SoundMap.ContainsKey(soundName))
            {
                SoundLoops[soundName] = SoundMap[soundName].CreateInstance();
                SoundLoops[soundName].IsLooped = true;
                SoundLoops[soundName].Volume = soundMasterVolume;
                SoundLoops[soundName].Play();
                SoundLoops[soundName].IsReversed = mode != PlayMode.Forward;
                reversibleSoundLoopsStartedThisFrame.Add(soundName);
            }
        }

        public static void SetSoundOnLoopReverse(string soundName, bool reverse)
        {
            if (soundName == null)
                return;
            if (SoundLoops.ContainsKey(soundName))
            {
                SoundLoops[soundName].IsReversed = reverse;
            }
        }

        public static void StopLoopingSound(string soundName)
        {
            if (soundName == null)
                return;
            if (SoundLoops.ContainsKey(soundName))
            {
                SoundLoops[soundName].Stop();
                SoundLoops.Remove(soundName);
                reversibleSoundLoopsEndedThisFrame.Add(soundName);
            }
        }

        public static void StopLoopingSounds()
        {
            foreach (KeyValuePair<string, SoundReversibleInstance> pair in SoundLoops)
            {
                pair.Value.Stop();
                reversibleSoundLoopsEndedThisFrame.Add(pair.Key);
            }
            SoundLoops.Clear();
        }

        public static void SetLoopingSoundsReverse(bool reverse)
        {
            foreach (KeyValuePair<string, SoundReversibleInstance> pair in SoundLoops)
            {
                pair.Value.IsReversed = reverse;
            }
        }

        public static void PauseLoopingSounds()
        {
            foreach (KeyValuePair<string, SoundReversibleInstance> pair in SoundLoops)
            {
                pair.Value.Pause();
            }
        }

        public static void ResumeLoopingSounds()
        {
            foreach (KeyValuePair<string, SoundReversibleInstance> pair in SoundLoops)
            {
                pair.Value.Resume();
            }
        }

        public static void PlaySoundAsMusic(string soundName)
        {
            if (soundName == currentMusicName)
                return;
            else if (soundName == null)
            {
                currentMusicInstance.Stop();
                currentMusicInstance = null;
                return;
            }
            if (SoundMap.ContainsKey(soundName))
            {
                if (currentMusicInstance != null)
                {
                    if (soundName == currentMusicName)
                    {
                        if (currentMusicInstance.IsDisposed || currentMusicInstance.State == SoundState.Stopped)
                            currentMusicInstance.Play();
                    }
                    else
                    {
                        currentMusicInstance.Stop();
                        SoundReversibleInstance instance = SoundMap[soundName].CreateInstance();
                        instance.IsLooped = true;
                        instance.Volume = musicMasterVolume * currentVolume;
                        instance.Play();
                        currentMusicInstance = instance;
                        currentMusicName = soundName;
                        HasPlayedMap[soundName] = true;
                    }
                }
                else
                {
                    SoundReversibleInstance instance = SoundMap[soundName].CreateInstance();
                    instance.IsLooped = true;
                    instance.Volume = musicMasterVolume * currentVolume;
                    instance.Play();
                    currentMusicInstance = instance;
                    currentMusicName = soundName;
                    HasPlayedMap[soundName] = true;
                }
            }
        }

        public static void SetMusicReverse(bool reverse)
        {
            if (currentMusicInstance != null)
            {
                currentMusicInstance.IsReversed = reverse;
            }
        }

        public static void SetMusicVolumeSmooth(float volume, float volumeChangePerSecond = DEFAULT_VOLUME_SPEED)
        {
            targetVolume = volume;
            volumeSpeed = volumeChangePerSecond;
        }

        public static void SetMusicVolume(float volume)
        {
            targetVolume = volume;
            currentVolume = volume;
            if (currentMusicInstance != null)
                currentMusicInstance.Volume = musicMasterVolume * currentVolume;
        }

        public static void SetMusicPitch(float pitch)
        {
            if (currentMusicInstance != null)
            {
                currentMusicInstance.Pitch = pitch;
            }
        }

        public static void SetMusicPan(float pan)
        {
            if (currentMusicInstance != null)
            {
                currentMusicInstance.Pan = pan;
            }
        }

        public static void SetMasterVolume(float volume)
        {
            masterVolume = MathHelper.Clamp(volume, 0, 1);
            SoundEffect.MasterVolume = masterVolume;
        }

        public static void SetMusicMasterVolume(float volume)
        {
            musicMasterVolume = MathHelper.Clamp(volume, 0, 1);
            if(currentMusicInstance != null)
                currentMusicInstance.Volume = musicMasterVolume * currentVolume;
        }

        public static void SetSoundMasterVolume(float volume)
        {
            soundMasterVolume = MathHelper.Clamp(volume, 0, 1);
            foreach (KeyValuePair<string, SoundReversibleInstance> pair in SoundLoops)
            {
                if (!pair.Value.IsDisposed)
                    pair.Value.Volume = soundMasterVolume;
            }
            foreach (SoundReversibleInstance sound in registeredSounds)
            {
                if (!sound.IsDisposed && sound != currentMusicInstance)
                    sound.Volume = soundMasterVolume;
            }
        }

        public static void Update(GameTime gameTime)
        {
            latestGameTime = gameTime;
            float seconds = gameTime.getSeconds();

            foreach (SoundReversibleInstance sound in soundsToUnregister)
            {
                registeredSounds.Remove(sound);
            }
            soundsToUnregister.Clear();
            foreach (SoundReversibleInstance sound in registeredSounds)
            {
                sound.Update();
            }

            if (currentMusicInstance != null)
            {
                if (currentVolume == targetVolume) { }
                else
                {
                    if (currentVolume > targetVolume)
                    {
                        currentVolume -= volumeSpeed * seconds;
                        currentVolume = MathHelper.Clamp(currentVolume, targetVolume, 1f);
                    }
                    else if (currentVolume < targetVolume)
                    {
                        currentVolume += volumeSpeed * seconds;
                        currentVolume = MathHelper.Clamp(currentVolume, 0f, targetVolume);
                    }
                    currentMusicInstance.Volume = musicMasterVolume * currentVolume;
                }
            }

            reversibleSoundLoopsStartedThisFrame.Clear();
            reversibleSoundLoopsEndedThisFrame.Clear();
            reversibleSoundsEndedThisFrame.Clear();
            for(int i = 0; i < soundsToSaveForReversePlayback.Count; i++)
            {
                Tuple<SoundReversible, TimeSpan> pair = soundsToSaveForReversePlayback[i];
                if (gameTime.TotalGameTime >= pair.Item2)
                {
                    soundsToSaveForReversePlayback.RemoveAt(i);
                    reversibleSoundsEndedThisFrame.Add(pair.Item1);
                    i--;
                }
            }
        }

        public static SoundState GetSoundState(string soundName)
        {
            if (currentMusicInstance != null && currentMusicName == soundName)
                return currentMusicInstance.State;
            else if (SoundLoops.Keys.Contains(soundName))
                return SoundLoops[soundName].State;
            return SoundState.Stopped;
        }

        public static void PauseMusic()
        {
            if (currentMusicInstance != null)
            {
                currentMusicInstance.Pause();
            }
        }

        public static void ResumeMusic()
        {
            if (currentMusicInstance != null)
            {
                currentMusicInstance.Resume();
            }
        }

        public static void StopMusic()
        {
            if (currentMusicInstance != null)
            {
                currentMusicInstance.Stop();
                currentMusicInstance = null;
                currentMusicName = null;
            }
        }

        public static void LoadContent(ContentManager Content)
        {
            //clean out accidental xnb files
            FileInfo[] filePaths = new DirectoryInfo(Content.RootDirectory + AUDIO_ROOT).GetFiles("*.xnb");
            foreach (FileInfo file in filePaths)
            {
                file.Delete();
            }
            filePaths = new DirectoryInfo(Content.RootDirectory + AUDIO_ROOT).GetFiles("*.wav");
            foreach (FileInfo file in filePaths)
            {
                string soundName = file.Name.Split('.')[0];
                SoundReversible sound = new SoundReversible(Content.RootDirectory + AUDIO_ROOT + soundName + ".wav");
                if (SoundMap.ContainsKey(soundName))
                    SoundMap.Remove(soundName);
                SoundMap.Add(soundName, sound);
            }
        }

        public static IMemento GenerateMementoFromCurrentFrame()
        {
            return new SoundManagerMemento();
        }

        private class SoundManagerMemento : IMemento
        {
            //add necessary fields to save information here
            public Object Target { get; set; }
            List<SoundReversible> reversibleSoundsEndedThisFrame;
            List<string> reversibleSoundLoopsStartedThisFrame;
            List<string> reversibleSoundLoopsEndedThisFrame;

            public SoundManagerMemento()
            {
                reversibleSoundsEndedThisFrame = new List<SoundReversible>(SoundManager.reversibleSoundsEndedThisFrame);
                reversibleSoundLoopsStartedThisFrame = new List<string>(SoundManager.reversibleSoundLoopsStartedThisFrame);
                reversibleSoundLoopsEndedThisFrame = new List<string>(SoundManager.reversibleSoundLoopsEndedThisFrame);
            }

            public void Apply(float interpolationFactor, bool isNewFrame, IMemento nextFrame)
            {
                if (isNewFrame)
                {
                    foreach(SoundReversible sound in reversibleSoundsEndedThisFrame)
                    {
                        PlaySoundOnce(sound, PlayMode.Reverse, false);
                    }
                    foreach (string soundName in reversibleSoundLoopsEndedThisFrame)
                    {
                        PlaySoundOnLoop(soundName, PlayMode.Reverse);
                    }
                    foreach (string soundName in reversibleSoundLoopsStartedThisFrame)
                    {
                        StopLoopingSound(soundName);
                    }
                }
            }
        }
    }
}
