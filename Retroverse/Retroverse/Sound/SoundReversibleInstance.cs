using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace Retroverse
{
    public class SoundReversibleInstance
    {
        public const int BUFFER_CHUNK_SIZE = 100; //millis

        public SoundReversible sound;
        private DynamicSoundEffectInstance dynamicSound;
        /// <summary>Pans the sound left (-1.0) or right (1.0).</summary>
        public float Pan { get { return dynamicSound.Pan; } set { dynamicSound.Pan = value; } }

        /// <summary>Sets the frequency of the sound. Between 0.0 and 1.0 is slowed down, greater than 1.0 is sped up.</summary>
        public float Pitch { get { return dynamicSound.Pitch; } set { dynamicSound.Pitch = value; } }

        /// <summary>Sets the volume of the sound relative to the master volume, from 0.0 to 1.0.</summary>
        public float Volume { get { return dynamicSound.Volume; } set { dynamicSound.Volume = value; } }

        public SoundState State { get { return dynamicSound.State; } }
        private bool reversed = false;
        public bool IsReversed { get { return reversed; } set { reversed = value; } }
        private bool looped = false;
        public bool IsLooped { get { return looped; } set { looped = value; } }
        public bool IsDisposed { get { return dynamicSound.IsDisposed; } private set { } }

        private readonly byte[] baseAudioBytes;
        private readonly int sampleRate;
        private readonly AudioChannels channels;
        private readonly int count;
        private bool playedSomething = false;
        private int position = 0;

        public SoundReversibleInstance(SoundReversible sound, byte[] audioBytes, int sampleRate, AudioChannels channels, bool inReverse)
        {
            this.sound = sound;
            this.sampleRate = sampleRate;
            this.channels = channels;
            reversed = inReverse;
            baseAudioBytes = audioBytes;
            dynamicSound = NewDynamicSoundEffectInstance();
            count = dynamicSound.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(BUFFER_CHUNK_SIZE));
        }

        private DynamicSoundEffectInstance NewDynamicSoundEffectInstance()
        {
            if (dynamicSound != null && !dynamicSound.IsDisposed)
            {
                Stop();
            }
            playedSomething = false;
            SoundManager.RegisterSoundInstance(this);
            return new DynamicSoundEffectInstance(sampleRate, channels);
        }

        //private void DynamicSound_BufferNeeded(object sender, EventArgs e)
        private void SubmitBuffers()
        {
            //Console.WriteLine("SRI #" + GetHashCode() + " bufferneeded pos=" + position + " count=" + count + " bytelen=" + baseAudioBytes.Length);
            bool shouldLoop = !playedSomething || looped;
            if (!shouldLoop && (position < 0 || position >= baseAudioBytes.Length))
            {
                //Console.WriteLine("SRI #" + GetHashCode() + " STOPPING");
                Stop();
                return;
            }
            playedSomething = true;

            byte[] audioBytes = baseAudioBytes;
            //int sum = 0;
            //int maxi = 0;

            byte[] bufferToSubmit = new byte[count];
            for (int i = 0; i < count; i++)
            {
                if (position >= audioBytes.Length)
                {
                    if (shouldLoop)
                        position = 0;
                    else
                        break;
                }
                else if (position < 0)
                {
                    if (shouldLoop)
                        position = audioBytes.Length - 1;
                    else
                        break;
                }

                if (reversed)
                {
                    bufferToSubmit[i] = audioBytes[position];
                    position--;
                    //maxi = i;
                    //sum += bufferToSubmit[i];
                }
                else
                {
                    bufferToSubmit[i] = audioBytes[position];
                    position++;
                    //maxi = i;
                    //sum += bufferToSubmit[i];
                }
            }

            //Console.WriteLine("             sum=" + sum + " maxi=" + maxi);
            dynamicSound.SubmitBuffer(bufferToSubmit, 0, count);
        }

        public void Play()
        {
            if (dynamicSound.IsDisposed)
            {
                dynamicSound = NewDynamicSoundEffectInstance();
            }
            dynamicSound.Play();
        }

        public void Pause()
        {
            dynamicSound.Pause();
        }

        public void Resume()
        {
            dynamicSound.Resume();
        }

        public void Stop()
        {
            position = 0;
            dynamicSound.Stop();
            dynamicSound.Dispose();
            SoundManager.UnregisterSoundInstance(this);
        }

        public void Update()
        {
            if (dynamicSound.PendingBufferCount <= 2)
                SubmitBuffers();
        }
    }
}
