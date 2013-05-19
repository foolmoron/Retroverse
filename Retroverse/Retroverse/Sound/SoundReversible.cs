using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Retroverse
{
    public class SoundReversible
    {
        /*Header info*/
        public readonly int chunkID;
        public readonly int fileSize;
        public readonly int riffType;
        public readonly int fmtID;
        public readonly int fmtSize;
        public readonly int fmtCode;
        public readonly int channels;
        public readonly int sampleRate;
        public readonly int fmtAvgBPS;
        public readonly int fmtBlockAlign;
        public readonly int bitDepth;
        public readonly int fmtExtraSize;
        public readonly int dataID;
        public readonly int dataSize;

        public byte[] audioBytes;

        private TimeSpan timeSpan;
        public TimeSpan Length { get { return timeSpan; } }

        public SoundReversible(string soundPath)
        {
            Stream waveFileStream = TitleContainer.OpenStream(soundPath);
            BinaryReader reader = new BinaryReader(waveFileStream);
            chunkID = reader.ReadInt32();
            fileSize = reader.ReadInt32();
            riffType = reader.ReadInt32();
            fmtID = reader.ReadInt32();
            fmtSize = reader.ReadInt32();
            fmtCode = reader.ReadInt16();
            channels = reader.ReadInt16();
            sampleRate = reader.ReadInt32();
            fmtAvgBPS = reader.ReadInt32();
            fmtBlockAlign = reader.ReadInt16();
            bitDepth = reader.ReadInt16();
            if (fmtSize == 18)
            {
                // Read any extra values
                fmtExtraSize = reader.ReadInt16();
                reader.ReadBytes(fmtExtraSize);
            }
            dataID = reader.ReadInt32();
            dataSize = reader.ReadInt32();

            audioBytes = reader.ReadBytes(dataSize);

            long millis = SoundInfo.GetSoundLength(RetroGame.EXECUTABLE_ROOT_DIRECTORY + "\\" + soundPath);
            long ticks = millis * 10000;
            timeSpan = new TimeSpan(ticks);
        }

        public SoundReversibleInstance CreateInstance(bool inReverse = false)
        {
            return CreateInstance(SoundManager.DEFAULT_VOLUME, 0, 0, inReverse);
        }

        public SoundReversibleInstance CreateInstance(float volume, float pitch, float pan, bool inReverse = false)
        {
            SoundReversibleInstance newInstance = new SoundReversibleInstance(this, audioBytes, sampleRate, (AudioChannels)channels, inReverse);
            newInstance.Volume = volume;
            newInstance.Pitch = pitch;
            newInstance.Pan = pan;
            newInstance.IsReversed = inReverse;
            return newInstance;
        }

        public void Play(bool inReverse = false)
        {
            Play(SoundManager.DEFAULT_VOLUME, 0, 0, inReverse);
        }

        public void Play(float volume, float pitch, float pan, bool inReverse = false)
        {
            SoundReversibleInstance instance = CreateInstance(volume, pitch, pan, inReverse);
            instance.Play();
        }        
    }
}
