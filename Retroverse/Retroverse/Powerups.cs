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
        public static readonly int COLLECTABLES_FOR_GUN = 5;
        public static readonly int SAND_FOR_RETRO = 3;
        public static readonly int COLLECTABLES_FOR_DRILL = 5;

        public static readonly int BACKGROUND_ANIMATION_TIMESTEP = 100; //ms
        public static readonly float BACKGROUND_DRAWSCALE = 0.6f;
        public static readonly Color DEFAULT_YELLOW = new Color(255, 255, 50);
        public static readonly float ICON_DRAWSCALE = 0.45f;

        public static List<Powerup> powerups = new List<Powerup>();
        private static List<Powerup> powerupsToRemove = new List<Powerup>();
        private static List<Powerup> powerupsToAdd = new List<Powerup>();
        private static Powerup radarPowerup;
        private static Powerup currentPowerup;

        private static int previousEnemyDifficulty = 0;
        public static int currentEnemyDifficulty = 1;
        private static readonly float ENEMY_SPAWN_INTERVAL = 3f;
        private static float enemyTimer = 0;
        private static readonly int[] ENEMY_LIMITS_SAME_TIME = { 0, 3, 5, 15, 40 };
        public static readonly int[] ENEMY_LIMITS_TOTAL_SPAWNED = { 0, 3, 10, 30, 40 };
        public static int enemiesSpawnedThisPhase = 0;
        private static readonly int MIN_ENEMIES_ON_SCREEN_AT_ALL_TIMES_HARDEST_MODE = 10;
        private static readonly int MIN_SPAWN_DISTANCE_FROM_HERO = 10;
        private static readonly int[][] enemySpawnLocationsEasy = new int[][]
        {
            new int[]{1,1},
            new int[]{4,10},
            new int[]{10,29},
            new int[]{15,17},
            new int[]{24,29},
            new int[]{29,9},
            new int[]{21,1},
            new int[]{13,1},
        };
        private static readonly int[][] enemySpawnLocationsMedium = new int[][]
        {
            new int[]{2,8},
            new int[]{3,15},
            new int[]{5,13},
            new int[]{7,15},
            new int[]{7,28},
            new int[]{1,19},
            new int[]{2,17},
            new int[]{6,6},
        };
        private static readonly int[][] enemySpawnLocationsHard = new int[][]
        {
            new int[]{8,19},
            new int[]{7,22},
            new int[]{7,8},
            new int[]{1,2},
            new int[]{2,12},
            new int[]{3,24},
            new int[]{8,17},
            new int[]{2,9},
        };

        public static bool enabled = false;

        private static Powerup[] debugPowerups = new Powerup[] 
        {
            new Powerup(0, 0, TextureManager.Get("boosticon"), DEFAULT_YELLOW, PowerupType.BoostInitial, true),
            new Powerup(0, 0, TextureManager.Get("boosticon1"), DEFAULT_YELLOW, PowerupType.BoostBurst, true),
            new Powerup(0, 0, TextureManager.Get("boosticon2"), DEFAULT_YELLOW, PowerupType.BoostConstant, true),
            new Powerup(0, 0, TextureManager.Get("gunicon"), DEFAULT_YELLOW, PowerupType.GunInitial, true),
            new Powerup(0, 0, TextureManager.Get("forwardshoticon1"), DEFAULT_YELLOW, PowerupType.GunStraight, true),
            new Powerup(0, 0, TextureManager.Get("sideshoticon2"), DEFAULT_YELLOW, PowerupType.GunSide, true),
            new Powerup(0, 0, TextureManager.Get("chargeshoticon2"), DEFAULT_YELLOW, PowerupType.GunCharge, true),
            new Powerup(0, 0, TextureManager.Get("sandicon"), DEFAULT_YELLOW, PowerupType.RetroInitial, true),
            new Powerup(0, 0, TextureManager.Get("retroicon1"), DEFAULT_YELLOW, PowerupType.RetroPort, true),
            new Powerup(0, 0, TextureManager.Get("retroicon2"), DEFAULT_YELLOW, PowerupType.RetroStasis, true),
            new Powerup(0, 0, TextureManager.Get("drilliconredo1"), DEFAULT_YELLOW, PowerupType.DrillInitial, true),
            new Powerup(0, 0, TextureManager.Get("drilliconredo1"), DEFAULT_YELLOW, PowerupType.DrillSingle, true),
            new Powerup(0, 0, TextureManager.Get("drilliconredo2"), DEFAULT_YELLOW, PowerupType.DrillTriple, true),
            new Powerup(0, 0, TextureManager.Get("radaricon1"), DEFAULT_YELLOW, PowerupType.Radar, true),
        };

        public static void Initialize(int checkpoint)
        {
            enabled = true;
            if (checkpoint < 5)
            {
                radarPowerup = new Powerup(20, 17, TextureManager.Get("radaricon1"), Color.HotPink, PowerupType.Radar);
                radarPowerup.ableToBeCollected = true;
            }
            currentPowerup = null;
            previousEnemyDifficulty = -1;
            currentEnemyDifficulty = 0;
            switch (checkpoint)
            {
                case 0:
                    currentPowerup = new Powerup(15, 29, TextureManager.Get("boosticon"), DEFAULT_YELLOW, PowerupType.BoostInitial, true);
                    currentPowerup.progressNeededToAppear = COLLECTABLES_FOR_BOOST;
                            currentEnemyDifficulty = 0;
                    break;
                case 1:
                    currentPowerup = new Powerup(29, 15, TextureManager.Get("gunicon"), DEFAULT_YELLOW, PowerupType.GunInitial, true);
                    currentPowerup.progressNeededToAppear = COLLECTABLES_FOR_GUN;
                            currentEnemyDifficulty = 1;
                    break;
                case 2:
                    currentPowerup = new Powerup(15, 1, TextureManager.Get("sandicon"), DEFAULT_YELLOW, PowerupType.RetroInitial, true);
                    currentPowerup.progressNeededToAppear = SAND_FOR_RETRO;
                            currentEnemyDifficulty = 2;
                    break;
                case 3:
                    currentPowerup = new Powerup(1, 15, TextureManager.Get("drilliconredo1"), DEFAULT_YELLOW, PowerupType.DrillInitial, true);
                    currentPowerup.progressNeededToAppear = COLLECTABLES_FOR_DRILL;
                            currentEnemyDifficulty = 3;
                    break;
                case 4:
                    break;
                case 5:
                    Game1.levelManager.targetZoom = LevelManager.ZOOM_ESCAPE;
                    break;
            }
            powerups.Clear();
            powerupsToRemove.Clear();
            powerupsToAdd.Clear();
            if (currentPowerup != null)
            powerups.Add(currentPowerup);    
            enemyTimer = 0;
        }

        public static void Update(GameTime gameTime)
        {
            if (enabled)
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
                    p.UpdateDebug(gameTime);
            }

            if (previousEnemyDifficulty != currentEnemyDifficulty)
            {
                LevelManager.collectableLimit = LevelManager.COLLECTABLE_LIMITS[currentEnemyDifficulty];
                LevelManager.numCollectablesSpawnedThisPhase = 0;
                enemiesSpawnedThisPhase = 0;
            }
            previousEnemyDifficulty = currentEnemyDifficulty;

            enemyTimer += gameTime.getSeconds();
            if (enemyTimer >= ENEMY_SPAWN_INTERVAL)
            {
                if (currentEnemyDifficulty > 0)
                    spawnRandomEnemy(enemySpawnLocationsEasy);
                if (currentEnemyDifficulty > 1)
                    spawnRandomEnemy(enemySpawnLocationsMedium);
                if (currentEnemyDifficulty > 2)
                    spawnRandomEnemy(enemySpawnLocationsHard);
                if (currentEnemyDifficulty > 3)
                {
                    Level introLevel = Game1.levelManager.levels[LevelManager.STARTING_LEVEL.X, LevelManager.STARTING_LEVEL.Y];
                    while (introLevel.enemies.Count < MIN_ENEMIES_ON_SCREEN_AT_ALL_TIMES_HARDEST_MODE && enemiesSpawnedThisPhase < ENEMY_LIMITS_TOTAL_SPAWNED[currentEnemyDifficulty])
                    {
                        spawnRandomEnemy(enemySpawnLocationsHard);
                    }
                }
            }
        }

        private static void spawnRandomEnemy(int[][] locations)
        {
            Level introLevel = Game1.levelManager.levels[LevelManager.STARTING_LEVEL.X, LevelManager.STARTING_LEVEL.Y];
            if (introLevel.enemies.Count >= ENEMY_LIMITS_SAME_TIME[currentEnemyDifficulty])
                return;
            if (enemiesSpawnedThisPhase >= ENEMY_LIMITS_TOTAL_SPAWNED[currentEnemyDifficulty])
                return;
            bool forceSand = currentEnemyDifficulty == 2 && (enemiesSpawnedThisPhase % 3 == 0);
            bool foundLocation = false;
            int attempts = 0;
            int[] location = null;
            int MAX_ATTEMPTS = 5;
            while (!foundLocation && attempts < MAX_ATTEMPTS)
            {
                location = locations[Game1.rand.Next(locations.Length)];
                if (Math.Abs(location[0] - Hero.instance.tileX) + Math.Abs(location[1] - Hero.instance.tileY) >= MIN_SPAWN_DISTANCE_FROM_HERO)
                {
                    foundLocation = true;
                }
                attempts++;
            }
            Game1.levelManager.addEnemy(location[0], location[1], Game1.rand.Next(Enemy.TYPE_COUNT), introLevel, forceSand);
            enemiesSpawnedThisPhase++;
            enemyTimer = 0;
        }

        public static void UpdateDying(GameTime gameTime)
        {
            if (enabled)
            {
                foreach (Powerup p in powerups)
                    if (p.dying)
                        p.Update(gameTime);
                radarPowerup.Update(gameTime);
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (enabled)
            {
                foreach (Powerup p in powerups)
                    p.Draw(spriteBatch);
                radarPowerup.Draw(spriteBatch);
            }
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
            if (currentPowerup != null)
                currentPowerup.addToProgress(c);
        }

        public enum PowerupType
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

        public class Powerup: Collectable
        {
            public static readonly float EXCLAMATION_DURATION = 3f;
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
            private bool hasProgress;
            public int progressNeededToAppear = 0;
            private int progress = 0;
            public bool progressBySand = false;

            // emitters for effects
            private Emitter emitter1;
            private Emitter emitter2;
            private Emitter emitter3;

            // sequencing fields
            new public static readonly float MOVE_SPEED = 300f;
            public float moveSpeed = MOVE_SPEED;
            public int sequenceIndex = 0;
            public float timerInterval = 0;
            public float timer = 0;
            public bool flag1;
            public bool flag2;
            public float value1;

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
                            Powerup constantBoost = new Powerup(15, 28, TextureManager.Get("boosticon2"), new Color(255, 100, 100), PowerupType.BoostConstant);
                            Powerup burstBoost = new Powerup(15, 28, TextureManager.Get("boosticon1"), new Color(100, 100, 255), PowerupType.BoostBurst);
                            powerupsToAdd.Add(constantBoost);
                            powerupsToAdd.Add(burstBoost);
                        };
                        break;
                    case PowerupType.BoostConstant:
                        emitter1 = Emitter.getPrebuiltEmitter(PrebuiltEmitter.RocketBoostFire);
                        emitter1.active = true;
                        emitter1.angle = 0;
                        emitter1.startDistance = 20;
                        emitter1.valueToDeath = 45;
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
                                    emitter1.angle = (float)Math.PI / 2;
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
                                    emitter1.active = false;
                                    break;
                            }
                            emitter1.position = position;
                            emitter1.Update(currentGameTime);
                        };
                        collectedAction = delegate()
                        {
                            foreach (Powerup p in powerups)
                                if (p != this)
                                    powerupsToRemove.Add(p);
                            currentPowerup = new Powerup(29, 15, TextureManager.Get("gunicon"), DEFAULT_YELLOW, PowerupType.GunInitial, true);
                            currentPowerup.progressNeededToAppear = COLLECTABLES_FOR_GUN;
                            powerupsToAdd.Add(currentPowerup);
                            Hero.instance.powerupBoost = 2;
                            currentEnemyDifficulty = 1;
                            exclamate("Rocket Boost", Color.OrangeRed);
                            Game1.checkpoint = 1;
                        };
                        break;
                    case PowerupType.BoostBurst:
                        emitter1 = Emitter.getPrebuiltEmitter(PrebuiltEmitter.RocketBoostFire);
                        emitter1.active = false;
                        emitter1.angle = 0;
                        emitter1.startDistance = 20;
                        emitter1.valueToDeath = 45;
                        timerInterval = 0.5f;
                        moveSpeed = MOVE_SPEED * 0.66f;
                        perFrameAction = delegate()
                        {
                            timer += currentGameTime.getSeconds();
                            if (timer >= timerInterval)
                            {
                                if (emitter1.active)
                                {
                                    emitter1.active = false;
                                    moveSpeed = MOVE_SPEED * 0.66f;
                                }
                                else
                                {
                                    emitter1.active = true;
                                    moveSpeed = MOVE_SPEED * 1.5f;
                                }
                                timer = 0;
                            }
                            switch (sequenceIndex)
                            {
                                case 0:
                                    emitter1.angle = (float)Math.PI;
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
                                    emitter1.angle = (float)Math.PI / 2;
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
                                    emitter1.active = false;
                                    break;
                            }
                            emitter1.position = position;
                            emitter1.Update(currentGameTime);
                        };
                        collectedAction = delegate()
                        {
                            foreach (Powerup p in powerups)
                                if (p != this)
                                    powerupsToRemove.Add(p);
                            currentPowerup = new Powerup(29, 15, TextureManager.Get("gunicon"), DEFAULT_YELLOW, PowerupType.GunInitial, true);
                            currentPowerup.progressNeededToAppear = COLLECTABLES_FOR_GUN;
                            powerupsToAdd.Add(currentPowerup);
                            Hero.instance.powerupBoost = 1;
                            currentEnemyDifficulty = 1;
                            exclamate("Rocket Burst", Color.DarkTurquoise);
                            Game1.checkpoint = 1;
                        };
                        break;
                    case PowerupType.GunInitial:
                        perFrameAction = delegate()
                        {
                        };
                        collectedAction = delegate()
                        {
                            Powerup gunStraight = new Powerup(28, 15, TextureManager.Get("forwardshoticon1"), new Color(255, 50, 50), PowerupType.GunStraight);
                            Powerup gunSide = new Powerup(28, 14, TextureManager.Get("sideshoticon2"), new Color(100, 255, 100), PowerupType.GunSide);
                            Powerup gunCharge = new Powerup(28, 16, TextureManager.Get("chargeshoticon2"), new Color(255, 180, 80), PowerupType.GunCharge);
                            powerupsToAdd.Add(gunStraight);
                            powerupsToAdd.Add(gunSide);
                            powerupsToAdd.Add(gunCharge);
                        };
                        break;
                    case PowerupType.GunStraight:
                        moveSpeed = MOVE_SPEED * 0.8f;
                        timerInterval = 0.2f;
                        timer = 0.5f;
                        direction = Direction.Left;
                        flag1 = true;
                        perFrameAction = delegate()
                        {
                            if (flag1)
                            {
                                timer += currentGameTime.getSeconds();
                                if (timer >= timerInterval)
                                {
                                    Hero.instance.ammo.Add(new Bullet("bullet1", PrebuiltEmitter.SmallBulletSparks, Hero.EMITTER_STRAIGHT_COLOR, direction, Bullet.DISTANCE_LIMIT_NORMAL, Hero.BULLET_DAMAGE_NORMAL));
                                    Hero.instance.ammo.Last().position = position;
                                    Hero.instance.ammo.Last().scale = Hero.BULLET_NORMAL_SCALE;
                                    Hero.instance.ammo.Last().hitbox.originalRectangle.Height = (int)(20);
                                    Hero.instance.ammo.Last().hitbox.originalRectangle.Width = (int)(20);
                                    timer = 0;
                                }
                            }
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
                                    flag1 = false;
                                    break;
                            }
                        };
                        collectedAction = delegate()
                        {
                            foreach (Powerup p in powerups)
                                if (p != this)
                                    powerupsToRemove.Add(p);
                            currentPowerup = new Powerup(15, 1, TextureManager.Get("sandicon"), DEFAULT_YELLOW, PowerupType.RetroInitial, true);
                            currentPowerup.progressNeededToAppear = SAND_FOR_RETRO;
                            currentPowerup.progressBySand = true;
                            powerupsToAdd.Add(currentPowerup);
                            Hero.instance.powerupGun = 1;
                            currentEnemyDifficulty = 2;
                            exclamate("Forward Shot", Color.Red);
                            Game1.checkpoint = 2;
                        };
                        break;
                    case PowerupType.GunSide:
                        moveSpeed = MOVE_SPEED * 0.8f;
                        timerInterval = 0.1f;
                        timer = 0.05f;
                        direction = Direction.Down;
                        flag1 = true;
                        perFrameAction = delegate()
                        {
                            if (flag1)
                            {
                                timer += currentGameTime.getSeconds();
                                if (timer >= timerInterval)
                                {
                                    Direction dir1 = Direction.None;
                                    Direction dir2 = Direction.None;
                                    switch (direction)
                                    {
                                        case Direction.Up:
                                        case Direction.Down:
                                            dir1 = Direction.Left;
                                            dir2 = Direction.Right;
                                            break;
                                        case Direction.Left:
                                        case Direction.Right:
                                            dir1 = Direction.Up;
                                            dir2 = Direction.Down;
                                            break;
                                    }
                                    Hero.instance.ammo.Add(new Bullet("bullet2", PrebuiltEmitter.SmallBulletSparks, Hero.EMITTER_SIDE_COLOR, dir1, Bullet.DISTANCE_LIMIT_NORMAL, Hero.BULLET_DAMAGE_NORMAL));
                                    Hero.instance.ammo.Last().position = position;
                                    Hero.instance.ammo.Last().scale = Hero.BULLET_NORMAL_SCALE;
                                    Hero.instance.ammo.Last().hitbox.originalRectangle.Height = (int)(20);
                                    Hero.instance.ammo.Last().hitbox.originalRectangle.Width = (int)(20);
                                    Hero.instance.ammo.Add(new Bullet("bullet2", PrebuiltEmitter.SmallBulletSparks, Hero.EMITTER_SIDE_COLOR, dir2, Bullet.DISTANCE_LIMIT_NORMAL, Hero.BULLET_DAMAGE_NORMAL));
                                    Hero.instance.ammo.Last().position = position;
                                    Hero.instance.ammo.Last().scale = Hero.BULLET_NORMAL_SCALE;
                                    Hero.instance.ammo.Last().hitbox.originalRectangle.Height = (int)(20);
                                    Hero.instance.ammo.Last().hitbox.originalRectangle.Width = (int)(20);
                                    timer = 0;
                                }
                            }
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
                                    direction = Direction.Left;
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
                                    direction = Direction.Up;
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
                                    flag1 = false;
                                    break;
                            }
                        };
                        collectedAction = delegate()
                        {
                            foreach (Powerup p in powerups)
                                if (p != this)
                                    powerupsToRemove.Add(p);
                            currentPowerup = new Powerup(15, 1, TextureManager.Get("sandicon"), DEFAULT_YELLOW, PowerupType.RetroInitial, true);
                            currentPowerup.progressNeededToAppear = SAND_FOR_RETRO;
                            currentPowerup.progressBySand = true;
                            powerupsToAdd.Add(currentPowerup);
                            Hero.instance.powerupGun = 2;
                            currentEnemyDifficulty = 2;
                            exclamate("Side Shot", Color.Lime);
                            Game1.checkpoint = 2;
                        };
                        break;
                    case PowerupType.GunCharge:
                        moveSpeed = MOVE_SPEED * 0.9f;
                        emitter1 = Emitter.getPrebuiltEmitter(PrebuiltEmitter.ChargingSparks);
                        timerInterval = 0.2f;
                        timer = 1.1f;
                        direction = Direction.Down;
                        flag1 = true;
                        perFrameAction = delegate()
                        {
                            if (flag1)
                            {
                                timer += currentGameTime.getSeconds();
                                emitter.active = true;
                                if (timer < Hero.BULLET_CHARGE_TIME_SMALL)
                                {
                                    emitter.active = false;
                                }
                                else if (timer >= Hero.BULLET_CHARGE_TIME_SMALL && timer < Hero.BULLET_CHARGE_TIME_MEDIUM)
                                {
                                    emitter1.startSize = Hero.CHARGE_PARTICLES_SMALL_SCALE;
                                    Color c = Hero.CHARGE_COLOR_SMALL;
                                    emitter1.startColor = c;
                                    c.A = 255;
                                    emitter1.endColor = c;
                                }
                                else if (timer >= Hero.BULLET_CHARGE_TIME_MEDIUM && timer < Hero.BULLET_CHARGE_TIME_LARGE)
                                {
                                    if (timer >= 1.3f)
                                    {
                                        Hero.instance.ammo.Add(new Bullet("chargebullet2", PrebuiltEmitter.MediumBulletSparks, Hero.EMITTER_CHARGE_COLOR, direction, Bullet.DISTANCE_LIMIT_CHARGE, Hero.BULLET_DAMAGE_CHARGE_MEDIUM, true));
                                        Hero.instance.ammo.Last().scale = Hero.BULLET_MEDIUM_SCALE;
                                        Hero.instance.ammo.Last().hitbox.originalRectangle.Height = (int)(64 * Hero.BULLET_MEDIUM_SCALE);
                                        Hero.instance.ammo.Last().hitbox.originalRectangle.Width = (int)(64 * Hero.BULLET_MEDIUM_SCALE);
                                        Hero.instance.ammo.Last().position = new Vector2(this.position.X, this.position.Y);
                                        timer = 0;
                                    }
                                    emitter1.startSize = Hero.CHARGE_PARTICLES_MEDIUM_SCALE;
                                    Color c = Hero.CHARGE_COLOR_MEDIUM;
                                    emitter1.startColor = c;
                                    c.A = 255;
                                    emitter1.endColor = c;
                                }
                            }
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
                                    direction = Direction.Left;
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
                                    direction = Direction.Up;
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
                                    direction = Direction.Left;
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
                                    direction = Direction.Down;
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
                                    timer = 0;
                                    flag1 = false;
                                    emitter1.active = false;
                                    break;
                            }
                            emitter1.position = position;
                            emitter1.Update(currentGameTime);
                        };
                        collectedAction = delegate()
                        {
                            foreach (Powerup p in powerups)
                                if (p != this)
                                    powerupsToRemove.Add(p);
                            currentPowerup = new Powerup(15, 1, TextureManager.Get("sandicon"), DEFAULT_YELLOW, PowerupType.RetroInitial, true);
                            currentPowerup.progressNeededToAppear = SAND_FOR_RETRO;
                            currentPowerup.progressBySand = true;
                            powerupsToAdd.Add(currentPowerup);
                            Hero.instance.powerupGun = 3;
                            currentEnemyDifficulty = 2;
                            exclamate("Charge Beam", Color.Gold);
                            Game1.checkpoint = 2;
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
                        flag1 = true;
                        flag2 = true;
                        History.retroportSecs = 1.5f;
                        timerInterval = History.retroportSecs;
                        timer = -1f;
                        perFrameAction = delegate()
                        {
                            if (timer > 0 && flag2)
                                History.UpdateArena(currentGameTime);
                            if (flag1)
                            {
                                timer += currentGameTime.getSeconds();
                                if (timer >= timerInterval)
                                {
                                    Game1.levelManager.setCenterEntity(this);
                                    History.lastState = Game1.state;
                                    Game1.state = GameState.RetroPort;
                                    flag1 = false;
                                }
                            }
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
                                    if (flag2)
                                    {
                                        flag2 = false;
                                        History.retroportSecs = History.RETROPORT_BASE_SECS;
                                        History.clearFrames();
                                    }
                                    break;
                            }
                        };
                        collectedAction = delegate()
                        {
                            foreach (Powerup p in powerups)
                                if (p != this)
                                    powerupsToRemove.Add(p);
                            currentPowerup = new Powerup(1, 15, TextureManager.Get("drilliconredo1"), DEFAULT_YELLOW, PowerupType.DrillInitial, true);
                            currentPowerup.progressNeededToAppear = COLLECTABLES_FOR_DRILL;
                            powerupsToAdd.Add(currentPowerup);
                            Hero.instance.powerupRetro = 1;
                            Game1.levelManager.setCenterEntity(Hero.instance);
                            currentEnemyDifficulty = 3;
                            exclamate("RetroPort", Color.DimGray);
                            Game1.checkpoint = 3;
                        };
                        break;
                    case PowerupType.RetroStasis:
                        timerInterval = 0.5f;
                        flag1 = true;
                        flag2 = false;
                        perFrameAction = delegate()
                        {
                            seconds = currentGameTime.getSeconds(Hero.instance.heroTimeScale);
                            timer += seconds;
                            if (flag1 && timer > timerInterval)
                            {
                                Game1.levelManager.setCenterEntity(this);
                                RetroStasis.activate();
                                timerInterval = 1.5f;
                                timer = 0;
                                flag1 = false;
                                flag2 = true;
                            }
                            else if (flag2 && timer > timerInterval)
                            {
                                RetroStasis.deactivate();
                                flag2 = false;
                            }
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
                            currentPowerup = new Powerup(1, 15, TextureManager.Get("drilliconredo1"), DEFAULT_YELLOW, PowerupType.DrillInitial, true);
                            currentPowerup.progressNeededToAppear = COLLECTABLES_FOR_DRILL;
                            powerupsToAdd.Add(currentPowerup);
                            Hero.instance.powerupRetro = 2;
                            Game1.levelManager.setCenterEntity(Hero.instance);
                            currentEnemyDifficulty = 3;
                            exclamate("RetroStasis", Color.LightGray);
                            Game1.checkpoint = 3;
                        };
                        break;
                    case PowerupType.DrillInitial:                        
                        perFrameAction = delegate()
                        {
                        };
                        collectedAction = delegate()
                        {
                            History.clearFrames();
                            Powerup singleDrill = new Powerup(2, 15, TextureManager.Get("drilliconredo1"), new Color(100, 100, 255), PowerupType.DrillSingle);
                            Powerup tripleDrill = new Powerup(2, 15, TextureManager.Get("drilliconredo2"), new Color(100, 255, 100), PowerupType.DrillTriple);
                            powerupsToAdd.Add(singleDrill);
                            powerupsToAdd.Add(tripleDrill);
                        };
                        break;
                    case PowerupType.DrillSingle:
                        moveSpeed = MOVE_SPEED * 2;
                        timerInterval = 1.25f;
                        emitter1 = Emitter.getPrebuiltEmitter(PrebuiltEmitter.DrillSparks);
                        emitter1.active = false;
                        emitter1.startSize = 0.2f;
                        perFrameAction = delegate()
                        {
                            emitter1.Update(currentGameTime);
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
                                    if (!(tileX == 13 && tileY == 22))
                                    {
                                        position += RIGHT * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(13, 22);
                                        emitter1.position = new Vector2(LevelManager.STARTING_LEVEL.X * Level.TEX_SIZE + 14.5f * Level.TILE_SIZE, LevelManager.STARTING_LEVEL.Y * Level.TEX_SIZE + 22.5f * Level.TILE_SIZE);
                                        emitter1.active = true;
                                        sequenceIndex++;
                                    }
                                    break;
                                case 2:
                                    timer += currentGameTime.getSeconds();
                                    if (timer >= timerInterval)
                                    {
                                        Game1.levelManager.levels[Hero.instance.levelX, Hero.instance.levelY].drillWall(14, 22);
                                        emitter1.active = false;
                                        emitter1.startSize = 0.2f;
                                        sequenceIndex++;
                                        timer = 0;
                                    }
                                    else
                                    {
                                        float drillingRatio = timer / timerInterval;
                                        emitter1.startSize = 1.5f * drillingRatio + 0.2f;
                                    }
                                    break;
                                case 3:
                                    if (!(tileX == 15 && tileY == 22))
                                    {
                                        position += RIGHT * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(15, 22);
                                        emitter1.position = new Vector2(LevelManager.STARTING_LEVEL.X * Level.TEX_SIZE + 16.5f * Level.TILE_SIZE, LevelManager.STARTING_LEVEL.Y * Level.TEX_SIZE + 22.5f * Level.TILE_SIZE);
                                        emitter1.active = true;
                                        sequenceIndex++;
                                    }
                                    break;
                                case 4:
                                    timer += currentGameTime.getSeconds();
                                    if (timer >= timerInterval)
                                    {
                                        Game1.levelManager.levels[Hero.instance.levelX, Hero.instance.levelY].drillWall(16, 22);
                                        emitter1.active = false;
                                        emitter1.startSize = 0.2f;
                                        sequenceIndex++;
                                        timer = 0;
                                    }
                                    else
                                    {
                                        float drillingRatio = timer / timerInterval;
                                        emitter1.startSize = 1.5f * drillingRatio + 0.2f;
                                    }
                                    break;
                                case 5:
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
                                case 6:
                                    break;
                            }
                        };
                        collectedAction = delegate()
                        {
                            foreach (Powerup p in powerups)
                                if (p != this)
                                    powerupsToRemove.Add(p);
                            Hero.instance.powerupDrill = 1;
                            currentEnemyDifficulty = 4;
                            exclamate("Fast Drill", Color.IndianRed);
                            Game1.checkpoint = 4;
                        };
                        break;
                    case PowerupType.DrillTriple:
                        moveSpeed = MOVE_SPEED * 2;
                        timerInterval = 2f;
                        emitter1 = Emitter.getPrebuiltEmitter(PrebuiltEmitter.DrillSparks);
                        emitter1.active = false;
                        emitter2 = Emitter.getPrebuiltEmitter(PrebuiltEmitter.DrillSparks);
                        emitter2.active = false;
                        emitter3 = Emitter.getPrebuiltEmitter(PrebuiltEmitter.DrillSparks);
                        emitter3.active = false;
                        emitter1.startSize = 0.2f;
                        emitter2.startSize = 0.2f;
                        emitter3.startSize = 0.2f;
                        perFrameAction = delegate()
                        {
                            emitter1.Update(currentGameTime);
                            emitter2.Update(currentGameTime);
                            emitter3.Update(currentGameTime);
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
                                    if (!(tileX == 13 && tileY == 8))
                                    {
                                        position += RIGHT * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(13, 8);
                                        emitter1.position = new Vector2(LevelManager.STARTING_LEVEL.X * Level.TEX_SIZE + 14.5f * Level.TILE_SIZE, LevelManager.STARTING_LEVEL.Y * Level.TEX_SIZE + 8.5f * Level.TILE_SIZE);
                                        emitter1.active = true;
                                        emitter2.position = new Vector2(LevelManager.STARTING_LEVEL.X * Level.TEX_SIZE + 14.5f * Level.TILE_SIZE, LevelManager.STARTING_LEVEL.Y * Level.TEX_SIZE + 7.5f * Level.TILE_SIZE);
                                        emitter2.active = true;
                                        emitter3.position = new Vector2(LevelManager.STARTING_LEVEL.X * Level.TEX_SIZE + 14.5f * Level.TILE_SIZE, LevelManager.STARTING_LEVEL.Y * Level.TEX_SIZE + 9.5f * Level.TILE_SIZE);
                                        emitter3.active = true;
                                        sequenceIndex++;
                                    }
                                    break;
                                case 2:
                                    timer += currentGameTime.getSeconds();
                                    if (timer >= timerInterval)
                                    {
                                        Game1.levelManager.levels[Hero.instance.levelX, Hero.instance.levelY].drillWall(14, 8);
                                        Game1.levelManager.levels[Hero.instance.levelX, Hero.instance.levelY].drillWall(14, 7);
                                        Game1.levelManager.levels[Hero.instance.levelX, Hero.instance.levelY].drillWall(14, 9);
                                        emitter1.active = false;
                                        emitter2.active = false;
                                        emitter3.active = false;
                                        emitter1.startSize = 0.2f;
                                        emitter2.startSize = 0.2f;
                                        emitter3.startSize = 0.2f;
                                        sequenceIndex++;
                                        timer = 0;
                                    }
                                    else
                                    {
                                        float drillingRatio = timer / timerInterval;
                                        emitter1.startSize = 1.5f * drillingRatio + 0.2f;
                                        emitter2.startSize = 1.5f * drillingRatio + 0.2f;
                                        emitter3.startSize = 1.5f * drillingRatio + 0.2f;
                                    }
                                    break;
                                case 3:
                                    if (!(tileX == 15 && tileY == 8))
                                    {
                                        position += RIGHT * moveSpeed * seconds;
                                    }
                                    else
                                    {
                                        setPositionByTile(15, 8);
                                        emitter1.position = new Vector2(LevelManager.STARTING_LEVEL.X * Level.TEX_SIZE + 16.5f * Level.TILE_SIZE, LevelManager.STARTING_LEVEL.Y * Level.TEX_SIZE + 8.5f * Level.TILE_SIZE);
                                        emitter1.active = true;
                                        emitter2.position = new Vector2(LevelManager.STARTING_LEVEL.X * Level.TEX_SIZE + 16.5f * Level.TILE_SIZE, LevelManager.STARTING_LEVEL.Y * Level.TEX_SIZE + 7.5f * Level.TILE_SIZE);
                                        emitter2.active = true;
                                        emitter3.position = new Vector2(LevelManager.STARTING_LEVEL.X * Level.TEX_SIZE + 16.5f * Level.TILE_SIZE, LevelManager.STARTING_LEVEL.Y * Level.TEX_SIZE + 9.5f * Level.TILE_SIZE);
                                        emitter3.active = true;
                                        sequenceIndex++;
                                    }
                                    break;
                                case 4:
                                    timer += currentGameTime.getSeconds();
                                    if (timer >= timerInterval)
                                    {
                                        Game1.levelManager.levels[Hero.instance.levelX, Hero.instance.levelY].drillWall(16, 8);
                                        Game1.levelManager.levels[Hero.instance.levelX, Hero.instance.levelY].drillWall(16, 7);
                                        Game1.levelManager.levels[Hero.instance.levelX, Hero.instance.levelY].drillWall(16, 9);
                                        emitter1.active = false;
                                        emitter2.active = false;
                                        emitter3.active = false;
                                        emitter1.startSize = 0.2f;
                                        emitter2.startSize = 0.2f;
                                        emitter3.startSize = 0.2f;
                                        sequenceIndex++;
                                        timer = 0;
                                    }
                                    else
                                    {
                                        float drillingRatio = timer / timerInterval;
                                        emitter1.startSize = 1.5f * drillingRatio + 0.2f;
                                        emitter2.startSize = 1.5f * drillingRatio + 0.2f;
                                        emitter3.startSize = 1.5f * drillingRatio + 0.2f;
                                    }
                                    break;
                                case 5:
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
                                case 6:
                                    break;
                            }
                        };
                        collectedAction = delegate()
                        {
                            foreach (Powerup p in powerups)
                                if (p != this)
                                    powerupsToRemove.Add(p);
                            Hero.instance.powerupDrill = 2;
                            currentEnemyDifficulty = 4;
                            exclamate("Triple Drill", Color.ForestGreen);
                            Game1.checkpoint = 4;
                        };
                        break;
                    case PowerupType.Radar:
                        perFrameAction = delegate() { };
                        collectedAction = delegate()
                        {
                            Hero.instance.powerupRadar = 1;
                            Game1.levelManager.targetZoom = LevelManager.ZOOM_ESCAPE;
                            exclamate("Radar", Color.DarkOrchid);
                            Game1.checkpoint = 5;
                        };
                        break;
                }
                #endregion
            }

            public static void exclamate(string powerupName, Color color)
            {
                Game1.showExclamation(new string[] { "Acquired Power:", powerupName }, new Color[] { Color.White, color }, EXCLAMATION_DURATION);
            }

            public void addToProgress(Collectable c)
            {
                if (hasProgress && c != null && progress < progressNeededToAppear)
                {
                    bool addToProgress = progressBySand == c is Sand;
                    if (addToProgress)
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

            public void UpdateDebug(GameTime gameTime)
            {
                currentGameTime = gameTime;
                seconds = gameTime.getSeconds();
                background.Update(gameTime);
                Vector2 centerPos = position;
                tileX = ((int)centerPos.X % Level.TEX_SIZE) / Level.TILE_SIZE;
                tileY = ((int)centerPos.Y % Level.TEX_SIZE) / Level.TILE_SIZE;
                iconAlpha = 255;
                base.Update(gameTime);
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                if (!dying)
                {
                    if (emitter1 != null)
                        emitter1.Draw(spriteBatch);
                    if (emitter2 != null)
                        emitter2.Draw(spriteBatch);
                    if (emitter3 != null)
                        emitter3.Draw(spriteBatch);
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
