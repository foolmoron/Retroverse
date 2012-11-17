using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;

namespace Retroverse
{
    public static class Powerups
    {
        public static readonly int TILE_SNAP_DISTANCE = 10;
        public static readonly int COLLECTABLES_FOR_BOOST = 3;
        public static readonly int COLLECTABLES_FOR_GUN = 3;
        public static readonly int SAND_FOR_RETRO = 3;

        public static readonly int BACKGROUND_ANIMATION_TIMESTEP = 100; //ms
        public static readonly float BACKGROUND_DRAWSCALE = 0.6f;
        public static readonly Color DEFAULT_YELLOW = new Color(255, 255, 50);
        public static readonly float ICON_DRAWSCALE = 0.45f;

        private static List<Powerup> powerups = new List<Powerup>();
        private static List<Powerup> powerupsToRemove = new List<Powerup>();
        private static List<Powerup> powerupsToAdd = new List<Powerup>();
        private static Powerup radarPowerup;
        private static Powerup currentPowerup;

        private static Powerup[] debugPowerups = new Powerup[] 
        {
            new Powerup(0, 0, TextureManager.Get("boosticon"), DEFAULT_YELLOW, PowerupType.BoostInitial, true),
            new Powerup(0, 0, TextureManager.Get("boosticon"), DEFAULT_YELLOW, PowerupType.BoostConstant, true),
            new Powerup(0, 0, TextureManager.Get("boosticon"), DEFAULT_YELLOW, PowerupType.BoostBurst, true),
            new Powerup(0, 0, TextureManager.Get("gunicon"), DEFAULT_YELLOW, PowerupType.GunInitial, true),
            new Powerup(0, 0, TextureManager.Get("forwardshoticon1"), DEFAULT_YELLOW, PowerupType.GunStraight, true),
            new Powerup(0, 0, TextureManager.Get("sideshoticon1"), DEFAULT_YELLOW, PowerupType.GunSide, true),
            new Powerup(0, 0, TextureManager.Get("chargeshoticon1"), DEFAULT_YELLOW, PowerupType.GunCharge, true),
            new Powerup(0, 0, TextureManager.Get("retroicon"), DEFAULT_YELLOW, PowerupType.RetroInitial, true),
            new Powerup(0, 0, TextureManager.Get("retroicon1"), DEFAULT_YELLOW, PowerupType.RetroPort, true),
            new Powerup(0, 0, TextureManager.Get("retroicon2"), DEFAULT_YELLOW, PowerupType.RetroStasis, true),
            new Powerup(0, 0, TextureManager.Get("drillicon"), DEFAULT_YELLOW, PowerupType.DrillInitial, true),
            new Powerup(0, 0, TextureManager.Get("drillicon1"), DEFAULT_YELLOW, PowerupType.DrillSingle, true),
            new Powerup(0, 0, TextureManager.Get("drillicon2"), DEFAULT_YELLOW, PowerupType.DrillTriple, true),
            new Powerup(0, 0, TextureManager.Get("radaricon"), DEFAULT_YELLOW, PowerupType.Radar, true),
        };

        public static void Initialize()
        {
            radarPowerup = new Powerup(20, 17, TextureManager.Get("radaricon"), Color.HotPink, PowerupType.Radar);
            radarPowerup.ableToBeCollected = true;
            currentPowerup = new Powerup(15, 29, TextureManager.Get("boosticon"), DEFAULT_YELLOW, PowerupType.BoostInitial, true);
            currentPowerup.progressNeededToAppear = COLLECTABLES_FOR_BOOST;
            powerups.Add(currentPowerup);
            powerupsToRemove.Clear();
            powerupsToAdd.Clear();
        }

        public static void Update(GameTime gameTime)
        {
            foreach (Powerup p in powerupsToRemove)
                powerups.Remove(p);
            powerupsToRemove.Clear();
            foreach (Powerup p in powerupsToAdd)
                powerups.Add(p);
            powerupsToAdd.Clear();
            foreach (Powerup p in powerups)
                p.Update(gameTime);
            radarPowerup.Update(gameTime);

            foreach (Powerup p in debugPowerups)
                p.Update(gameTime);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (Powerup p in powerups)
                p.Draw(spriteBatch);
            radarPowerup.Draw(spriteBatch);
        }

        public static void DrawDebug(SpriteBatch spriteBatch, Vector2 position)
        {
            Vector2 offset = Vector2.Zero;
            offset.X += 30;
            foreach (Powerup p in debugPowerups)
            {
                p.Draw(spriteBatch, position + offset);
                offset.X += 40;
            }
        }

        public static void addToProgress(Collectable c)
        {
            currentPowerup.addToProgress(c);
        }

        private enum PowerupType
        {
            BoostInitial,
            BoostConstant,
            BoostBurst,
            GunInitial,
            GunStraight,
            GunSide,
            GunCharge,
            RetroInitial,
            RetroPort,
            RetroStasis,
            DrillInitial,
            DrillSingle,
            DrillTriple,
            Radar
        }

        private class Powerup: Collectable
        {
            public static readonly Vector2 UP = new Vector2(0, -1);
            public static readonly Vector2 DOWN = new Vector2(0, 1);
            public static readonly Vector2 LEFT = new Vector2(-1, 0);
            public static readonly Vector2 RIGHT = new Vector2(1, 0);
            public static GameTime currentGameTime;
            public static float seconds;

            public AnimatedTexture background;
            public Texture2D icon;
            public Action perFrameAction;
            public Action collectedAction;
            public Color tint;
            public byte iconAlpha = 255;
            public Emitter progressEmitter;
            public bool hasProgress;
            public int progressNeededToAppear = 0;
            private int progress = 0;
            private bool progressBySand = false;

            // sequencing fields
            public float moveSpeed = 400f;
            public int sequenceIndex = 0;

            public Powerup(int _tileX, int _tileY, Texture2D icon, Color tint, PowerupType type, bool hasProgress = false) :
                base(LevelManager.STARTING_LEVEL.X * Level.TEX_SIZE + _tileX * Level.TILE_SIZE + Level.TILE_SIZE / 2,
                LevelManager.STARTING_LEVEL.Y * Level.TEX_SIZE + _tileY * Level.TILE_SIZE + Level.TILE_SIZE / 2,
                LevelManager.STARTING_LEVEL.X, LevelManager.STARTING_LEVEL.Y, _tileX, _tileY)
            {
                background = new AnimatedTexture("fairyglow", 4, BACKGROUND_ANIMATION_TIMESTEP);
                this.icon = icon;
                this.tint = tint;
                iconAlpha = 0;
                texture = null;
                this.tileX = _tileX;
                this.tileY = _tileY;
                emitter = Emitter.getPrebuiltEmitter(PrebuiltEmitter.PrisonerSparks);
                emitter.startColor = new Color(tint.R, tint.G, tint.B, 255);
                emitter.endColor = new Color(tint.R, tint.G, tint.B, 0);
                progressEmitter = Emitter.getPrebuiltEmitter(PrebuiltEmitter.CollectedSparks);
                byte maxColor = Math.Max(Math.Max(tint.R, tint.G), tint.B);
                progressEmitter.startColor = new Color(tint.R, tint.G, tint.B, 255);
                progressEmitter.endColor = new Color(tint.R, tint.G, tint.B, 0);
                progressEmitter.active = false;
                baseScore = 0;
                this.hasProgress = hasProgress;
                ableToBeCollected = false;
                #region Specific Powerup object declarations and sequencing
                switch (type)
                {
                    case PowerupType.BoostInitial:
                        perFrameAction = delegate() { };
                        collectedAction = delegate()
                        {
                            Powerup constantBoost = new Powerup(15, 28, TextureManager.Get("boosticon"), new Color(255, 100, 100), PowerupType.BoostConstant);
                            Powerup burstBoost = new Powerup(15, 28, TextureManager.Get("boosticon"), new Color(100, 100, 255), PowerupType.BoostBurst);
                            powerupsToAdd.Add(constantBoost);
                            powerupsToAdd.Add(burstBoost);
                        };
                        break;
                    case PowerupType.BoostConstant:
                        perFrameAction = delegate()
                        {
                            switch (sequenceIndex)
                            {
                                case 0:
                                    Console.WriteLine("tiles=" + tileX + "," + tileY);
                                    if (!(tileX == 13 && tileY == 28))
                                    {
                                        position += LEFT * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(13, 28);
                                        sequenceIndex++;
                                    }
                                    break;
                                case 1: 
                                    if (!(tileX == 13 && tileY == 1))
                                    {
                                        position += UP * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(13, 1);
                                        ableToBeCollected = true;
                                        sequenceIndex++;
                                    }
                                    break;
                                case 2:
                                    break;
                            }
                        };
                        collectedAction = delegate()
                        {
                            foreach (Powerup p in powerups)
                                if (p != this)
                                    powerupsToRemove.Add(p);
                            currentPowerup = new Powerup(29, 15, TextureManager.Get("gunicon"), DEFAULT_YELLOW, PowerupType.GunInitial, true);
                            currentPowerup.progressNeededToAppear = COLLECTABLES_FOR_GUN;
                            powerupsToAdd.Add(currentPowerup);
                            Hero.instance.powerUp1 = 2;
                        };
                        break;
                    case PowerupType.BoostBurst:
                        perFrameAction = delegate()
                        {
                            switch (sequenceIndex)
                            {
                                case 0:
                                    if (!(tileX == 17 && tileY == 28))
                                    {
                                        position += RIGHT * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(17, 28);
                                        sequenceIndex++;
                                    }
                                    break;
                                case 1:
                                    if (!(tileX == 17 && tileY == 1))
                                    {
                                        position += UP * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(17, 1);
                                        ableToBeCollected = true;
                                        sequenceIndex++;
                                    }
                                    break;
                                case 2:
                                    break;
                            }
                        };
                        collectedAction = delegate()
                        {
                            foreach (Powerup p in powerups)
                                if (p != this)
                                    powerupsToRemove.Add(p);
                            currentPowerup = new Powerup(29, 15, TextureManager.Get("gunicon"), DEFAULT_YELLOW, PowerupType.GunInitial, true);
                            currentPowerup.progressNeededToAppear = COLLECTABLES_FOR_GUN;
                            powerupsToAdd.Add(currentPowerup);
                            Hero.instance.powerUp1 = 1;
                        };
                        break;
                    case PowerupType.GunInitial:
                        perFrameAction = delegate()
                        {
                        };
                        collectedAction = delegate()
                        {
                            Powerup gunStraight = new Powerup(28, 15, TextureManager.Get("forwardshoticon1"), new Color(255, 50, 50), PowerupType.GunStraight);
                            Powerup gunSide = new Powerup(28, 14, TextureManager.Get("sideshoticon1"), new Color(100, 255, 100), PowerupType.GunSide);
                            Powerup gunCharge = new Powerup(28, 16, TextureManager.Get("chargeshoticon1"), new Color(255, 180, 80), PowerupType.GunCharge);
                            powerupsToAdd.Add(gunStraight);
                            powerupsToAdd.Add(gunSide);
                            powerupsToAdd.Add(gunCharge);
                        };
                        break;
                    case PowerupType.GunStraight:
                        perFrameAction = delegate()
                        {
                            switch (sequenceIndex)
                            {
                                case 0:
                                    if (!(tileX == 1 && tileY == 15))
                                    {
                                        position += LEFT * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(1, 15);
                                        ableToBeCollected = true;
                                        sequenceIndex++;
                                    }
                                    break;
                                case 1:
                                    break;
                            }
                        };
                        collectedAction = delegate()
                        {
                            foreach (Powerup p in powerups)
                                if (p != this)
                                    powerupsToRemove.Add(p);
                            currentPowerup = new Powerup(15, 1, TextureManager.Get("retroicon"), DEFAULT_YELLOW, PowerupType.RetroInitial, true);
                            currentPowerup.progressNeededToAppear = SAND_FOR_RETRO;
                            currentPowerup.progressBySand = true;
                            powerupsToAdd.Add(currentPowerup);
                            Hero.instance.powerUp3 = 1;
                        };
                        break;
                    case PowerupType.GunSide:
                        perFrameAction = delegate()
                        {
                            switch (sequenceIndex)
                            {
                                case 0:
                                    if (!(tileX == 28 && tileY == 10))
                                    {
                                        position += UP * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(28, 10);
                                        sequenceIndex++;
                                    }
                                    break;
                                case 1:
                                    if (!(tileX == 21 && tileY == 10))
                                    {
                                        position += LEFT * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(21, 10);
                                        sequenceIndex++;
                                    }
                                    break;
                                case 2:
                                    if (!(tileX == 21 && tileY == 1))
                                    {
                                        position += UP * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(21, 1);
                                        ableToBeCollected = true;
                                        sequenceIndex++;
                                    }
                                    break;
                                case 3:
                                    break;
                            }
                        };
                        collectedAction = delegate()
                        {
                            foreach (Powerup p in powerups)
                                if (p != this)
                                    powerupsToRemove.Add(p);
                            currentPowerup = new Powerup(15, 1, TextureManager.Get("retroicon"), DEFAULT_YELLOW, PowerupType.RetroInitial, true);
                            currentPowerup.progressNeededToAppear = SAND_FOR_RETRO;
                            currentPowerup.progressBySand = true;
                            powerupsToAdd.Add(currentPowerup);
                            Hero.instance.powerUp3 = 2;
                        };
                        break;
                    case PowerupType.GunCharge:
                        perFrameAction = delegate()
                        {
                            switch (sequenceIndex)
                            {
                                case 0:
                                    if (!(tileX == 28 && tileY == 22))
                                    {
                                        position += DOWN * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(28, 22);
                                        sequenceIndex++;
                                    }
                                    break;
                                case 1:
                                    if (!(tileX == 23 && tileY == 22))
                                    {
                                        position += LEFT * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(23, 22);
                                        sequenceIndex++;
                                    }
                                    break;
                                case 2:
                                    if (!(tileX == 23 && tileY == 19))
                                    {
                                        position += UP * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(23, 19);
                                        sequenceIndex++;
                                    }
                                    break;
                                case 3:
                                    if (!(tileX == 19 && tileY == 19))
                                    {
                                        position += LEFT * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(19, 19);
                                        sequenceIndex++;
                                    }
                                    break;
                                case 4:
                                    if (!(tileX == 19 && tileY == 29))
                                    {
                                        position += DOWN * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(19, 29);
                                        ableToBeCollected = true;
                                        sequenceIndex++;
                                    }
                                    break;
                                case 5:
                                    break;
                            }
                        };
                        collectedAction = delegate()
                        {
                            foreach (Powerup p in powerups)
                                if (p != this)
                                    powerupsToRemove.Add(p);
                            currentPowerup = new Powerup(15, 1, TextureManager.Get("retroicon"), DEFAULT_YELLOW, PowerupType.RetroInitial, true);
                            currentPowerup.progressNeededToAppear = SAND_FOR_RETRO;
                            currentPowerup.progressBySand = true;
                            powerupsToAdd.Add(currentPowerup);
                            Hero.instance.powerUp3 = 3;
                        };
                        break;
                    case PowerupType.RetroInitial:
                        perFrameAction = delegate()
                        {
                        };
                        collectedAction = delegate()
                        {
                            Powerup retroPort = new Powerup(15, 2, TextureManager.Get("retroicon1"), new Color(130, 130, 130), PowerupType.RetroPort);
                            Powerup retroStasis = new Powerup(15, 2, TextureManager.Get("retroicon2"), new Color(200, 200, 200), PowerupType.RetroStasis);
                            powerupsToAdd.Add(retroPort);
                            powerupsToAdd.Add(retroStasis);
                        };
                        break;
                    case PowerupType.RetroPort:
                        perFrameAction = delegate()
                        {
                            switch (sequenceIndex)
                            {
                                case 0:
                                    if (!(tileX == 13 && tileY == 2))
                                    {
                                        position += LEFT * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(13, 2);
                                        sequenceIndex++;
                                    }
                                    break;
                                case 1:
                                    if (!(tileX == 13 && tileY == 28))
                                    {
                                        position += DOWN * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(13, 28);
                                        ableToBeCollected = true;
                                        sequenceIndex++;
                                    }
                                    break;
                                case 2:
                                    break;
                            }
                        };
                        collectedAction = delegate()
                        {
                            foreach (Powerup p in powerups)
                                if (p != this)
                                    powerupsToRemove.Add(p);
                            currentPowerup = new Powerup(1, 15, TextureManager.Get("drillicon"), DEFAULT_YELLOW, PowerupType.DrillInitial);
                            currentPowerup.ableToBeCollected = true;
                            powerupsToAdd.Add(currentPowerup);
                            Hero.instance.powerUp4 = 1;
                        };
                        break;
                    case PowerupType.RetroStasis:
                        perFrameAction = delegate()
                        {
                            switch (sequenceIndex)
                            {
                                case 0:
                                    if (!(tileX == 17 && tileY == 2))
                                    {
                                        position += RIGHT * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(17, 2);
                                        sequenceIndex++;
                                    }
                                    break;
                                case 1:
                                    if (!(tileX == 17 && tileY == 28))
                                    {
                                        position += DOWN * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(17, 28);
                                        ableToBeCollected = true;
                                        sequenceIndex++;
                                    }
                                    break;
                                case 2:
                                    break;
                            }
                        };
                        collectedAction = delegate()
                        {
                            foreach (Powerup p in powerups)
                                if (p != this)
                                    powerupsToRemove.Add(p);
                            currentPowerup = new Powerup(1, 15, TextureManager.Get("drillicon"), DEFAULT_YELLOW, PowerupType.DrillInitial);
                            currentPowerup.ableToBeCollected = true;
                            powerupsToAdd.Add(currentPowerup);
                            Hero.instance.powerUp4 = 2;
                        };
                        break;
                    case PowerupType.DrillInitial:                        
                        perFrameAction = delegate()
                        {
                        };
                        collectedAction = delegate()
                        {
                            Powerup singleDrill = new Powerup(2, 15, TextureManager.Get("drillicon2"), new Color(100, 100, 255), PowerupType.DrillSingle);
                            Powerup tripleDrill = new Powerup(2, 15, TextureManager.Get("drillicon1"), new Color(100, 255, 100), PowerupType.DrillTriple);
                            powerupsToAdd.Add(singleDrill);
                            powerupsToAdd.Add(tripleDrill);
                        };
                        break;
                    case PowerupType.DrillSingle:
                        perFrameAction = delegate()
                        {
                            switch (sequenceIndex)
                            {
                                case 0:
                                    if (!(tileX == 2 && tileY == 22))
                                    {
                                        position += DOWN * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(2, 22);
                                        sequenceIndex++;
                                    }
                                    break;
                                case 1:
                                    if (!(tileX == 17 && tileY == 22))
                                    {
                                        position += RIGHT * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(17, 22);
                                        ableToBeCollected = true;
                                        sequenceIndex++;
                                    }
                                    break;
                                case 2:
                                    break;
                            }
                        };
                        collectedAction = delegate()
                        {
                            foreach (Powerup p in powerups)
                                if (p != this)
                                    powerupsToRemove.Add(p);
                            Hero.instance.powerUp2 = 1;
                        };
                        break;
                    case PowerupType.DrillTriple:
                        perFrameAction = delegate()
                        {
                            switch (sequenceIndex)
                            {
                                case 0:
                                    if (!(tileX == 2 && tileY == 8))
                                    {
                                        position += UP * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(2, 8);
                                        sequenceIndex++;
                                    }
                                    break;
                                case 1:
                                    if (!(tileX == 17 && tileY == 8))
                                    {
                                        position += RIGHT * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(17, 8);
                                        ableToBeCollected = true;
                                        sequenceIndex++;
                                    }
                                    break;
                                case 2:
                                    break;
                            }
                        };
                        collectedAction = delegate()
                        {
                            foreach (Powerup p in powerups)
                                if (p != this)
                                    powerupsToRemove.Add(p);
                            Hero.instance.powerUp2 = 2;
                        };
                        break;
                    case PowerupType.Radar:
                        perFrameAction = delegate() { };
                        collectedAction = delegate()
                        {
                            Hero.instance.powerUp5 = 1;
                            Game1.state = GameState.Escape;
                        };
                        break;
                }
                #endregion
            }

            public void addToProgress(Collectable c)
            {
                if (hasProgress && c != null && progress < progressNeededToAppear)
                {
                    if (!progressBySand || (progressBySand && c is Collectable))
                    {
                        progress++;
                        progressEmitter.particlesEmitted = 0;
                        progressEmitter.active = true;
                    }
                }
            }

            public void setPositionByTile(int tileX, int tileY)
            {
                position.X = LevelManager.STARTING_LEVEL.X * Level.TEX_SIZE + tileX * Level.TILE_SIZE + Level.TILE_SIZE / 2;
                position.Y = LevelManager.STARTING_LEVEL.Y * Level.TEX_SIZE + tileY * Level.TILE_SIZE + Level.TILE_SIZE / 2;
            }

            public override void Update(GameTime gameTime)
            {
                currentGameTime = gameTime;
                seconds = gameTime.getSeconds();
                background.Update(gameTime);
                if (hasProgress)
                {
                    progressEmitter.position = position;
                    progressEmitter.Update(gameTime);
                }
                Vector2 centerPos = position;
                tileX = ((int)centerPos.X % Level.TEX_SIZE) / Level.TILE_SIZE;
                tileY = ((int)centerPos.Y % Level.TEX_SIZE) / Level.TILE_SIZE;
                if (perFrameAction != null)
                    perFrameAction();
                if (hasProgress)
                {
                    iconAlpha = (byte)(((float)progress / progressNeededToAppear) * 255 * 0.5f + 50);
                    if (progress == progressNeededToAppear)
                    {
                        iconAlpha = 255;
                        ableToBeCollected = true;
                    }
                }
                else 
                    iconAlpha = 255;
                base.Update(gameTime);
                if (collectedThisFrame)
                {
                    if (collectedAction != null)
                        collectedAction();
                } 
                if (dying)
                {
                    if (emitter.isFinished())
                    {
                        powerupsToRemove.Add(this);
                    }
                }
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                if (!dying)
                {
                    Color backgroundColor = Color.Lerp(DEFAULT_YELLOW, Color.White, 0.25f);
                    backgroundColor.R = (byte)(backgroundColor.R * iconAlpha / 255f);
                    backgroundColor.G = (byte)(backgroundColor.G * iconAlpha / 255f);
                    backgroundColor.B = (byte)(backgroundColor.B * iconAlpha / 255f);
                    backgroundColor.A = iconAlpha;
                    Color iconColor = Color.White;
                    iconColor.R = (byte)(iconColor.R * iconAlpha / 255f);
                    iconColor.G = (byte)(iconColor.G * iconAlpha / 255f);
                    iconColor.B = (byte)(iconColor.B * iconAlpha / 255f);
                    iconColor.A = iconAlpha;
                    spriteBatch.Draw(background.getTexture(), position, null, backgroundColor, rotation, new Vector2(background.getTexture().Width / 2, background.getTexture().Height / 2), BACKGROUND_DRAWSCALE, SpriteEffects.None, 1);
                    spriteBatch.Draw(icon, position, null, iconColor, rotation, new Vector2(icon.Width / 2, icon.Height / 2), ICON_DRAWSCALE, SpriteEffects.None, 1);
                }
                progressEmitter.Draw(spriteBatch);
                base.Draw(spriteBatch);
            }

            public void Draw(SpriteBatch spriteBatch, Vector2 position)
            {
                Color backgroundColor = Color.Lerp(DEFAULT_YELLOW, Color.White, 0.25f);
                backgroundColor.R = (byte)(backgroundColor.R * iconAlpha / 255f);
                backgroundColor.G = (byte)(backgroundColor.G * iconAlpha / 255f);
                backgroundColor.B = (byte)(backgroundColor.B * iconAlpha / 255f);
                backgroundColor.A = iconAlpha;
                Color iconColor = Color.White;
                iconColor.R = (byte)(iconColor.R * iconAlpha / 255f);
                iconColor.G = (byte)(iconColor.G * iconAlpha / 255f);
                iconColor.B = (byte)(iconColor.B * iconAlpha / 255f);
                iconColor.A = iconAlpha;
                spriteBatch.Draw(background.getTexture(), position, null, backgroundColor, rotation, new Vector2(background.getTexture().Width / 2, background.getTexture().Height / 2), BACKGROUND_DRAWSCALE, SpriteEffects.None, 1);
                spriteBatch.Draw(icon, position, null, iconColor, rotation, new Vector2(icon.Width / 2, icon.Height / 2), ICON_DRAWSCALE, SpriteEffects.None, 1);
            }
        }
    }
}
