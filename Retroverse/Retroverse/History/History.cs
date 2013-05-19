using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public class History
    {
        private static Queue<History> histories = new Queue<History>();
        public static History LastHistory { get; private set; }
        public static GameState LastState { get; private set; }

        // retroport values
        public static readonly float RETROPORT_BASE_SECS = 2.5f;
        public static float secsSinceLastRetroPort = 0;

        // history frame application values
        public static int retroportFrames = 0;
        public static readonly float FRAME_VELOCITY_MIN = 10;
        public static readonly float FRAME_VELOCITY_MAX = 45;
        public static float currentFrame = 0;
        public static float secsInRetroPort = 0;

        //This field keeps track of the current set of reversible objects in the game
        private static List<IReversible> registeredReversibles = new List<IReversible>();
        //Each individual History frame has a list of reversibles and their memento for that frame
        private Dictionary<IReversible, IMemento> mementos = new Dictionary<IReversible, IMemento>();
        //Explicitly store mementos from static classes... no better solution for this for now
        private IMemento retroGameMemento;
        private IMemento riotGuardWallMemento;
        private IMemento soundManagerMemento;

        // distortion effect values
        public const float DISTORTION_WAVE_FREQUENCY = 0.3f;
        public const float DISTORTION_WAVE_GRANULARITY = 0.5f;
        public const float DISTORTION_WAVE_OFFSET = -0.5f;
        public static readonly float DISTORTION_WAVE_AMPLITUDE_BASE = 0.1f;
        public static float waveAmplitude = 0f;
        public static float phaseOffset = 0f;

        private History() { }

        public static void UpdateForward(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();
            secsSinceLastRetroPort += seconds;

            History history = new History();
            foreach (IReversible reversible in registeredReversibles)
            {
                history.mementos[reversible] = reversible.GenerateMementoFromCurrentFrame();
            }
            history.retroGameMemento = RetroGame.GenerateMementoFromCurrentFrame();
            history.riotGuardWallMemento = RiotGuardWall.GenerateMementoFromCurrentFrame();
            history.soundManagerMemento = SoundManager.GenerateMementoFromCurrentFrame();
            histories.Enqueue(history);

            if (secsSinceLastRetroPort >= RETROPORT_BASE_SECS)
            {
                LastHistory = histories.Dequeue();
            }
            else
            {
                LastHistory = null;
            }
        }

        public static void UpdateReverse(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();
            secsSinceLastRetroPort = 0;

            bool isNewFrame = false;
            int RETROPORT_FRAMES = histories.Count;
            float framePerc = currentFrame / RETROPORT_FRAMES;
            float frameVelocity = getFrameVelocity(framePerc);
            float oldFrame = currentFrame;
            currentFrame += frameVelocity * seconds;
            if ((int)currentFrame != (int)oldFrame)
                isNewFrame = true;

            int frameIndex = RETROPORT_FRAMES - (int)currentFrame - 1;
            float interpolation = currentFrame - (int)currentFrame;
            History currentHistory = null;
            History nextHistory = null;
            if (frameIndex >= 0)
            {
                currentHistory = histories.ElementAt(frameIndex);
            }
            else
            {
                CancelRevert();
                return;
            }

            if (frameIndex > 0)
            {
                nextHistory = histories.ElementAt(frameIndex - 1);
            }

            foreach (IReversible reversible in currentHistory.mementos.Keys)
            {
                IMemento nextFrame = null;
                if (nextHistory != null && nextHistory.mementos.ContainsKey(reversible))
                    nextFrame = nextHistory.mementos[reversible];
                currentHistory.mementos[reversible].Apply(interpolation, isNewFrame, nextFrame);
            }
            currentHistory.retroGameMemento.Apply(interpolation, isNewFrame, (nextHistory != null) ? nextHistory.retroGameMemento : null);
            currentHistory.riotGuardWallMemento.Apply(interpolation, isNewFrame, (nextHistory != null) ? nextHistory.riotGuardWallMemento : null);
            currentHistory.soundManagerMemento.Apply(interpolation, isNewFrame, (nextHistory != null) ? nextHistory.soundManagerMemento : null);

            LevelManagerScreen topScreen = RetroGame.TopLevelManagerScreen;
            topScreen.currentEffect = Effects.RewindDistortion;
            topScreen.currentEffect.CurrentTechnique = topScreen.currentEffect.Techniques["DistortRight"];
            topScreen.currentEffect.Parameters["waveFrequency"].SetValue(DISTORTION_WAVE_FREQUENCY);
            topScreen.currentEffect.Parameters["granularity"].SetValue(DISTORTION_WAVE_GRANULARITY);
            topScreen.currentEffect.Parameters["waveOffset"].SetValue(DISTORTION_WAVE_OFFSET);
            waveAmplitude = 2 * frameVelocity / FRAME_VELOCITY_MAX;
            topScreen.currentEffect.Parameters["waveAmplitude"].SetValue((DISTORTION_WAVE_AMPLITUDE_BASE * waveAmplitude) - DISTORTION_WAVE_OFFSET);
            phaseOffset = framePerc * 2;
            topScreen.currentEffect.Parameters["phaseOffset"].SetValue(phaseOffset);
            topScreen.drawEffects = true;
        }

        private static float getFrameVelocity(float framePerc)
        {
            float frameVelocity;
            float perc2 = 2 * framePerc;

            if (perc2 < 1)
            {
                frameVelocity = FRAME_VELOCITY_MIN * (1 - perc2) + FRAME_VELOCITY_MAX * perc2;
            }
            else
            {
                perc2 -= 1;
                frameVelocity = FRAME_VELOCITY_MIN * perc2 + FRAME_VELOCITY_MAX * (1 - perc2);
            }

            return frameVelocity;
        }

        public void AttemptDrawHero(SpriteBatch spriteBatch, Hero hero)
        {
            if (registeredReversibles.Contains(hero))
                hero.DrawHistorical(spriteBatch, mementos[hero]);
        }

        public static void Clear()
        {
            histories.Clear();
            LastHistory = null;
            currentFrame = 0;
            secsInRetroPort = 0;
            secsSinceLastRetroPort = 0;
        }

        public static void ResetReversibles()
        {
            Clear();
            registeredReversibles.Clear();
        }

        public static bool CanRevert()
        {
            return LastHistory != null;
        }

        public static void ActivateRevert(Hero controllingHero, InputAction cancelAction)
        {
            LastState = RetroGame.State;
            RetroGame.AddScreen(new RetroPortScreen(controllingHero, cancelAction), true);
            SoundManager.SetMusicReverse(true);
            SoundManager.SetLoopingSoundsReverse(true);
        }

        public static void CancelRevert()
        {
            Clear();
            if (RetroGame.TopScreen is RetroPortScreen)
                RetroGame.PopScreen(true);
            SoundManager.SetMusicReverse(false);
            SoundManager.SetLoopingSoundsReverse(false);
        }

        public static bool IsRegistered(IReversible reversible)
        {
            return registeredReversibles.Contains(reversible);
        }

        public static void RegisterReversible(IReversible reversible)
        {
            registeredReversibles.Add(reversible);
        }

        public static void UnRegisterReversible(IReversible reversible)
        {
            registeredReversibles.Remove(reversible);
        }
    }
}
