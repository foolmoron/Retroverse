using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;
using LevelPipeline;

namespace Retroverse
{
    public class Hero : Entity
    {
        public static Hero instance;
        public List<Bullet> ammo;
        public readonly float BULLET_FIRE_INTERVAL = 0.2f; //secs
        public float bulletTimer = 0;
        public float chargeTimer = 0;
        public bool fired = false;
        public int levelX, levelY, tileX, tileY;
        public Emitter leftBooster;
        public Vector2 leftBoosterOffset;
        public Emitter rightBooster;
        public Vector2 rightBoosterOffset;
        public Emitter leftBoosterIdle;
        public Emitter rightBoosterIdle;
        public Emitter leftBoosterFiring;
        public Emitter rightBoosterFiring;
        public float boosterAngle;
        public static readonly float BOOSTER_LENGTH = 12;
        public static readonly float MOVE_SPEED = 200f;
        public static float moveSpeedMultiplier = 1f;
        private readonly Dictionary<Direction, float> DIR_TO_ROTATION = new Dictionary<Direction, float>(){
            {Direction.Up, (float)Math.PI},
            {Direction.Down, 0},
            {Direction.Left, (float)Math.PI / 2f},
            {Direction.Right, (float)Math.PI * 3f / 2f},
        };

        public static readonly float BOOST_CONSTANT_SPEED_MULTIPLIER = 1.5f;
        public static readonly float BOOST_BURST_SPEED_MULTIPLIER = 2.75f;
        public static readonly float BURST_DURATION = 0.5f; //secs
        public static readonly float BURST_COOLDOWN = 2f; //secs
        public float burstTimer = 0;
        public float burstRecharge = 0;
        public float timeInBurst = 0;
        public bool bursting = false;
        public static readonly Color BOOST_IDLE_RECHARGED_COLOR = new Color(0, 128, 255, 255);
        public static readonly Color BOOST_IDLE_NOT_RECHARGED_COLOR = new Color(255, 140, 0, 255);

        public static readonly Color EMITTER_STRAIGHT_COLOR = new Color(207, 17, 17, 255);
        public static readonly Color EMITTER_SIDE_COLOR = new Color(43, 186, 39, 255);
        public static readonly Color EMITTER_CHARGE_COLOR = new Color(204, 185, 67, 255);
        public static readonly int BULLET_DAMAGE_NORMAL = 2;
        public static readonly int BULLET_DAMAGE_CHARGE_SMALL = 1;
        public static readonly int BULLET_DAMAGE_CHARGE_MEDIUM = 2;
        public static readonly int BULLET_DAMAGE_CHARGE_LARGE = 3;
        public static readonly float BULLET_CHARGE_TIME_SMALL = 0.25f; //secs
        public static readonly float BULLET_CHARGE_TIME_MEDIUM = 1f; //secs
        public static readonly float BULLET_CHARGE_TIME_LARGE = 2f; //secs
        public static readonly Color CHARGE_COLOR_SMALL = new Color(248, 180, 100, 255);
        public static readonly Color CHARGE_COLOR_MEDIUM = new Color(248, 248, 56, 255);
        public static readonly Color CHARGE_COLOR_LARGE = new Color(248, 248, 175, 255);
        public static readonly float BULLET_NORMAL_SCALE = 0.375f;
        public static readonly float BULLET_SMALL_SCALE = 0.25f;
        public static readonly float BULLET_MEDIUM_SCALE = 0.5f;
        public static readonly float BULLET_LARGE_SCALE = 1f;
        public static readonly float CHARGE_PARTICLES_SMALL_SCALE = 0.25f;
        public static readonly float CHARGE_PARTICLES_MEDIUM_SCALE = 0.4f;
        public static readonly float CHARGE_PARTICLES_LARGE_SCALE = 0.7f;
        public Emitter chargeEmitter;

        public static readonly float DRILL_SINGLE_TIME = 1.5f; // seconds to drill
        public static readonly float DRILL_TRIPLE_TIME = 3f; // seconds to drill
        public Emitter drillEmitter;
        public Emitter drillEmitterLeft;
        public Emitter drillEmitterRight;
        public bool drilling = false;
        public float drillingTime = 0; // secs
        public float drillingRatio = 0; // secs

        // retrostasis values
        public static readonly float RETROSTASIS_DURATION = 5f;
        public static readonly float RETROSTASIS_TIMESCALE = 0.5f;
        public static readonly float RETROSTASIS_INITIAL_FREEZE_TIME_ALL = 1f; //secs
        public static readonly float RETROSTASIS_INITIAL_FREEZE_TIME_ENEMIES = RETROSTASIS_INITIAL_FREEZE_TIME_ALL + 0.5f; //secs
        public float timeInRetroStasis = 0f;
        public float effectInnerRadius;
        public float effectOuterRadius;
        public float effectIntensity = 2f;
        public static float EFFECT_INNERRADIUS_MAX = 100f;
        public static float EFFECT_FINISHED_RADIUS;
        public static readonly float effectIntroVelocity = 1800f;
        public static readonly float effectOutroVelocity = 900f;
        public bool effectFinished = true;
        public bool cancelRetroStasis = false;

        public static readonly float INVINCIBILITY_INTERVAL = 0.5f;
        public bool invincible = false;
        public float invincibilityTimer = 0;
        
        //***************Jon**************
        public int powerUp1; //, 0= Normal, 1=Bursts, 2= Fast, 3=Reverse
        public int powerUp2; //, 0= Normal, 1=singledrill, 2=tripledrill
        public int powerUp3; // 0=Normal, 1=Front, 2=Side, 3=Charge
        public int powerUp4; // 0=Normal, 1=RetroPort, 2=RetroStatis
        public int powerUp5; // 0=Normal, 1=Radar

        public Hero()
            : base(new Hitbox(32, 32))
        {
            position = new Vector2(Level.TEX_SIZE * LevelManager.STARTING_LEVEL.X + (Level.TILE_SIZE * (LevelManager.STARTING_TILE.X + 0.5f)), Level.TEX_SIZE * LevelManager.STARTING_LEVEL.Y + (Level.TILE_SIZE * (LevelManager.STARTING_TILE.Y + 0.5f)));
            setCurrentLevelAndTile();
            this.setTexture("hero");
            direction = Direction.Up;
            ammo = new List<Bullet>();
            instance = this;
            powerUp1 = 0;
            powerUp2 = 0;
            powerUp3 = 0;
            powerUp4 = 0;
            powerUp5 = 0;
            scale = 0.5f;
        }

        public static void Initialize()
        {
            instance.InitializeHero();
        }

        private void InitializeHero()
        {
            leftBoosterFiring = Emitter.getPrebuiltEmitter(PrebuiltEmitter.RocketBoostFire);
            rightBoosterFiring = Emitter.getPrebuiltEmitter(PrebuiltEmitter.RocketBoostFire);
            leftBoosterIdle = Emitter.getPrebuiltEmitter(PrebuiltEmitter.IdleBoostFire);
            rightBoosterIdle = Emitter.getPrebuiltEmitter(PrebuiltEmitter.IdleBoostFire);
            drillEmitter = Emitter.getPrebuiltEmitter(PrebuiltEmitter.DrillSparks);
            drillEmitterLeft = Emitter.getPrebuiltEmitter(PrebuiltEmitter.DrillSparks);
            drillEmitterRight = Emitter.getPrebuiltEmitter(PrebuiltEmitter.DrillSparks);
            chargeEmitter = Emitter.getPrebuiltEmitter(PrebuiltEmitter.ChargingSparks);
            leftBooster = leftBoosterIdle;
            rightBooster = rightBoosterIdle;
            moveSpeedMultiplier = 1;
        }

        public void spaceOrA()
        {
        }

        public void fire()
        {
            fired = true;
            if (bulletTimer < BULLET_FIRE_INTERVAL)
                return;
            bulletTimer = 0;
            if (powerUp3 == 0)
            { }
            else if (powerUp3 == 1)
            {
                ammo.Add(new Bullet("bullet1", PrebuiltEmitter.SmallBulletSparks, EMITTER_STRAIGHT_COLOR, direction, BULLET_DAMAGE_NORMAL));
                ammo.Last().position = new Vector2(this.position.X, this.position.Y);
                ammo.Last().scale = BULLET_NORMAL_SCALE;
                ammo.Last().hitbox.originalRectangle.Height = (int)(20);
                ammo.Last().hitbox.originalRectangle.Width = (int)(20);
            }
            else if (powerUp3 == 2)
            {
                Direction dirLeft = Direction.None, dirRight = Direction.None;
                switch (direction)
                {
                    case Direction.Up:
                        dirLeft = Direction.Left;
                        dirRight = Direction.Right;
                        break;
                    case Direction.Down:
                        dirLeft = Direction.Right;
                        dirRight = Direction.Left;
                        break;
                    case Direction.Left:
                        dirLeft = Direction.Down;
                        dirRight = Direction.Up;
                        break;
                    case Direction.Right:
                        dirLeft = Direction.Up;
                        dirRight = Direction.Down;
                        break;
                }
                ammo.Add(new Bullet("bullet2", PrebuiltEmitter.SmallBulletSparks, EMITTER_SIDE_COLOR, dirLeft, BULLET_DAMAGE_NORMAL));
                ammo.Last().position = new Vector2(this.position.X, this.position.Y);
                ammo.Last().scale = BULLET_NORMAL_SCALE;
                ammo.Last().hitbox.originalRectangle.Height = (int)(20);
                ammo.Last().hitbox.originalRectangle.Width = (int)(20);
                ammo.Add(new Bullet("bullet2", PrebuiltEmitter.SmallBulletSparks, EMITTER_SIDE_COLOR, dirRight, BULLET_DAMAGE_NORMAL));
                ammo.Last().position = new Vector2(this.position.X, this.position.Y);
                ammo.Last().scale = BULLET_NORMAL_SCALE;
                ammo.Last().hitbox.originalRectangle.Height = (int)(20);
                ammo.Last().hitbox.originalRectangle.Width = (int)(20);
            }
        }

        public void shiftOrB()
        {
            
        }

        public void ctrlOrRB()
        {
        }

        public void altOrXY()
        {
        }

        public void activateRetro()
        {
            if (Game1.availableSand <= 0)
                return;
            switch (Game1.state)
            {
                case GameState.Arena:
                case GameState.Escape:
                    if (powerUp4 == 1)
                    {
                        if (History.canRevert())
                        {
                            History.lastState = Game1.state;
                            Game1.state = GameState.RetroPort;
                            Game1.removeSand();
                        }
                    }
                    else if (powerUp4 == 2)
                    {
                        if (!Game1.retroStatisActive)
                        {
                            Game1.retroStatisActive = true;
                            Game1.timeScale = 0;
                            Game1.removeSand();
                        }
                        else
                        {
                            if (timeInRetroStasis >= RETROSTASIS_INITIAL_FREEZE_TIME_ALL)
                            {
                                cancelRetroStasis = true;
                            }
                        }
                    }
                    break;
                case GameState.RetroPort:
                    if (powerUp4 == 1)
                    {
                        History.cancelRevert();
                    }
                    break;
            }
        }

        public void special2()
        {
        }

        public void burst()
        {
            if (Game1.state == GameState.Arena || Game1.state == GameState.Escape)
                if (powerUp1 == 1 && !bursting && burstRecharge >= BURST_COOLDOWN)
                {
                    bursting = true;
                    moveSpeedMultiplier = BOOST_BURST_SPEED_MULTIPLIER;
                    timeInBurst = 0;
                }
        }

        public void collideWithEnemy(Enemy e)
        {
            if (Game1.INVINCIBILITY)
                return;
            if (!Game1.retroStatisActive)
            {
                if (Game1.availableSand > 0)
                {
                    activateRetro();
                }
                else
                    Game1.gameOver();
            }
        }

        public void collideWithRiotGuardWall()
        {
            if (Game1.INVINCIBILITY)
                return;
            if (!Game1.retroStatisActive)
            {
                if (Game1.availableSand > 0)
                {
                    activateRetro();
                    if (Game1.retroStatisActive)
                        RiotGuardWall.reverse(RETROSTASIS_DURATION);
                }
                else
                    Game1.gameOver();
            }
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds(1f);
            float heroTimeScale = 0f;
            if (Game1.retroStatisActive)
            {
                timeInRetroStasis += seconds;
                effectFinished = false;
                EFFECT_FINISHED_RADIUS = Game1.screenSize.Y * 1.3f * Game1.levelManager.zoom;
                if (effectOuterRadius < EFFECT_FINISHED_RADIUS)
                    effectOuterRadius += effectIntroVelocity * seconds;
                if (timeInRetroStasis >= RETROSTASIS_INITIAL_FREEZE_TIME_ALL)
                {
                    heroTimeScale = RETROSTASIS_TIMESCALE;
                    if (effectInnerRadius < timeInRetroStasis / RETROSTASIS_DURATION * EFFECT_INNERRADIUS_MAX)
                        effectInnerRadius = MathHelper.Clamp(effectInnerRadius + effectIntroVelocity * seconds, 0, timeInRetroStasis / RETROSTASIS_DURATION * EFFECT_INNERRADIUS_MAX);
                }
                if (timeInRetroStasis >=  RETROSTASIS_INITIAL_FREEZE_TIME_ENEMIES)
                {
                    Game1.timeScale = RETROSTASIS_TIMESCALE;
                }
                Game1.drawEffects = true;
                Game1.currentEffect = Effects.Grayscale;
                Game1.currentEffect.Parameters["width"].SetValue(Game1.screenSize.X);
                Game1.currentEffect.Parameters["height"].SetValue(Game1.screenSize.Y);
                Game1.currentEffect.Parameters["innerradius"].SetValue(effectInnerRadius);
                Game1.currentEffect.Parameters["outerradius"].SetValue(effectOuterRadius);
                Game1.currentEffect.Parameters["intensity"].SetValue(effectIntensity);
                Game1.currentEffect.Parameters["zoom"].SetValue(Game1.levelManager.zoom);
                Game1.currentEffect.Parameters["center"].SetValue(Game1.levelManager.center);
                if (timeInRetroStasis >= RETROSTASIS_DURATION)
                {
                    cancelRetroStasis = true;
                }
                if (cancelRetroStasis)
                {
                    Game1.retroStatisActive = false;
                    Game1.timeScale = 1f;
                    timeInRetroStasis = 0f;
                    cancelRetroStasis = false;
                }
            }
            else
            {
                heroTimeScale = 1f;
                if (effectFinished)
                {
                    effectInnerRadius = 0;
                    effectOuterRadius = 0;
                }
                else
                {
                    EFFECT_FINISHED_RADIUS = Game1.screenSize.Y * 1.3f * Game1.levelManager.zoom;
                    if (effectInnerRadius < EFFECT_FINISHED_RADIUS)
                    {
                        Game1.drawEffects = true;
                        Game1.currentEffect = Effects.Grayscale;
                        effectInnerRadius += effectOutroVelocity * seconds;
                        Game1.currentEffect.Parameters["width"].SetValue(Game1.screenSize.X);
                        Game1.currentEffect.Parameters["height"].SetValue(Game1.screenSize.Y);
                        Game1.currentEffect.Parameters["innerradius"].SetValue(effectInnerRadius);
                        Game1.currentEffect.Parameters["outerradius"].SetValue(effectOuterRadius);
                        Game1.currentEffect.Parameters["intensity"].SetValue(effectIntensity);
                        Game1.currentEffect.Parameters["zoom"].SetValue(Game1.levelManager.zoom);
                        Game1.currentEffect.Parameters["center"].SetValue(Game1.levelManager.center);
                    }
                    else
                        effectFinished = true;
                }
            }

            seconds = gameTime.getSeconds(heroTimeScale);
            bulletTimer += seconds;

            if (powerUp3 == 3)
            {
                if (fired)
                {
                    chargeTimer += seconds;
                }
                chargeEmitter.active = fired;
                if (chargeTimer < BULLET_CHARGE_TIME_SMALL)
                {
                    chargeEmitter.active = false;
                }
                else if (chargeTimer >= BULLET_CHARGE_TIME_SMALL && chargeTimer < BULLET_CHARGE_TIME_MEDIUM)
                {
                    if (!fired)
                    {
                        ammo.Add(new Bullet("chargebullet1", PrebuiltEmitter.SmallBulletSparks, EMITTER_CHARGE_COLOR, direction, BULLET_DAMAGE_CHARGE_SMALL));
                        ammo.Last().scale = BULLET_SMALL_SCALE;
                        ammo.Last().hitbox.originalRectangle.Height = (int)(64 * BULLET_SMALL_SCALE);
                        ammo.Last().hitbox.originalRectangle.Width = (int)(64 * BULLET_SMALL_SCALE);
                        ammo.Last().position = new Vector2(this.position.X, this.position.Y);
                        chargeTimer = 0;
                    }
                    chargeEmitter.startSize = CHARGE_PARTICLES_SMALL_SCALE;
                    Color c = CHARGE_COLOR_SMALL;
                    chargeEmitter.startColor = c;
                    c.A = 255;
                    chargeEmitter.endColor = c;
                }
                else if (chargeTimer >= BULLET_CHARGE_TIME_MEDIUM && chargeTimer < BULLET_CHARGE_TIME_LARGE)
                {
                    if (!fired)
                    {
                        ammo.Add(new Bullet("chargebullet2", PrebuiltEmitter.MediumBulletSparks, EMITTER_CHARGE_COLOR, direction, BULLET_DAMAGE_CHARGE_MEDIUM, true));
                        ammo.Last().scale = BULLET_MEDIUM_SCALE;
                        ammo.Last().hitbox.originalRectangle.Height = (int)(64 * BULLET_MEDIUM_SCALE);
                        ammo.Last().hitbox.originalRectangle.Width = (int)(64 * BULLET_MEDIUM_SCALE);
                        ammo.Last().position = new Vector2(this.position.X, this.position.Y);
                        chargeTimer = 0;
                    }
                    chargeEmitter.startSize = CHARGE_PARTICLES_MEDIUM_SCALE;
                    Color c = CHARGE_COLOR_MEDIUM;
                    chargeEmitter.startColor = c;
                    c.A = 255;
                    chargeEmitter.endColor = c;
                }
                else if (chargeTimer >= BULLET_CHARGE_TIME_LARGE)
                {
                    if (!fired)
                    {
                        ammo.Add(new Bullet("chargebullet3", PrebuiltEmitter.LargeBulletSparks, EMITTER_CHARGE_COLOR, direction, BULLET_DAMAGE_CHARGE_LARGE, true));
                        ammo.Last().scale = BULLET_LARGE_SCALE;
                        ammo.Last().hitbox.originalRectangle.Height = (int)(64 * BULLET_LARGE_SCALE);
                        ammo.Last().hitbox.originalRectangle.Width = (int)(64 * BULLET_LARGE_SCALE);
                        ammo.Last().position = new Vector2(this.position.X, this.position.Y);
                        chargeTimer = 0;
                    }
                    chargeEmitter.startSize = CHARGE_PARTICLES_LARGE_SCALE;
                    Color c = CHARGE_COLOR_LARGE;
                    chargeEmitter.startColor = c;
                    c.A = 255;
                    chargeEmitter.endColor = c;
                }
            }

            if (powerUp1 == 2)
            {
                moveSpeedMultiplier = BOOST_CONSTANT_SPEED_MULTIPLIER;
            }
            Vector2 movement = Controller.dirVector * MOVE_SPEED * moveSpeedMultiplier * seconds;

            if (bursting)
            {
                timeInBurst += seconds;
                if (timeInBurst >= BURST_DURATION)
                {
                    moveSpeedMultiplier = 1f;
                    burstRecharge = 0;
                    bursting = false;
                }
            }
            else
            {
                burstRecharge += seconds;
            }

            setCurrentLevelAndTile();
            Level level = Game1.levelManager.levels[levelX, levelY];

            float nextX = position.X + movement.X;
            float nextY = position.Y + movement.Y;
            //Console.WriteLine(position.X+ " "+ position.Y);
            bool moved = true;
            int n;
            if (heroTimeScale > 0f)
            {
                switch (Controller.direction)
                {
                    case Direction.Up:
                        moved = canMove(new Vector2(0, movement.Y));
                        if (!moved)
                        {
                            n = (int)position.Y;
                            nextY = n + Level.TILE_SIZE / 2 - (n % Level.TILE_SIZE);
                        }
                        leftBoosterOffset = new Vector2(-6, 12);
                        rightBoosterOffset = new Vector2(6, 12);
                        boosterAngle = (float)Math.PI / 2;
                        break;
                    case Direction.Down:
                        moved = canMove(new Vector2(0, movement.Y));
                        if (!moved)
                        {
                            n = (int)position.Y;
                            nextY = n + Level.TILE_SIZE / 2 - (n % Level.TILE_SIZE);
                        }
                        leftBoosterOffset = new Vector2(6, -12);
                        rightBoosterOffset = new Vector2(-6, -12);
                        boosterAngle = (float)Math.PI * 3 / 2;
                        break;
                    case Direction.Left:
                        moved = canMove(new Vector2(movement.X, 0));
                        if (!moved)
                        {
                            n = (int)position.X;
                            nextX = n - (n % Level.TILE_SIZE) + Level.TILE_SIZE / 2;
                        }
                        leftBoosterOffset = new Vector2(12, 6);
                        rightBoosterOffset = new Vector2(12, -6);
                        boosterAngle = 0;
                        break;
                    case Direction.Right:
                        moved = canMove(new Vector2(movement.X, 0));
                        if (!moved)
                        {
                            n = (int)position.X;
                            nextX = n - (n % Level.TILE_SIZE) + Level.TILE_SIZE / 2;
                        }
                        leftBoosterOffset = new Vector2(-12, -6);
                        rightBoosterOffset = new Vector2(-12, 6);
                        boosterAngle = (float)Math.PI;
                        break;
                    default:
                        nextX = position.X;
                        nextY = position.Y;
                        break;
                }
            }
            if (Controller.direction != Direction.None)
            {
                rotation = DIR_TO_ROTATION[Controller.direction];
                if (Controller.direction != direction) // reset drilling if direction changes
                    drillingTime = 0;
                direction = Controller.direction;
            }
            position = new Vector2(nextX, nextY);
            // check corners
            if (moved &&
                (Game1.levelManager.collidesWithWall(new Vector2(getLeft().X, getTop().Y)) || //topleft
                Game1.levelManager.collidesWithWall(new Vector2(getLeft().X, getBottom().Y)) || //botleft
                Game1.levelManager.collidesWithWall(new Vector2(getRight().X, getBottom().Y)) || //botright
                Game1.levelManager.collidesWithWall(new Vector2(getRight().X, getTop().Y)))) //topright
            {
                switch (Controller.direction)
                {
                    case Direction.Up:
                        n = (int)position.X;
                        nextX = n - (n % Level.TILE_SIZE) + Level.TILE_SIZE / 2;
                        break;
                    case Direction.Down:
                        n = (int)position.X;
                        nextX = n - (n % Level.TILE_SIZE) + Level.TILE_SIZE / 2;
                        break;
                    case Direction.Left:
                        n = (int)position.Y;
                        nextY = n + Level.TILE_SIZE / 2 - (n % Level.TILE_SIZE);
                        break;
                    case Direction.Right:
                        n = (int)position.Y;
                        nextY = n + Level.TILE_SIZE / 2 - (n % Level.TILE_SIZE);
                        break;
                    default:
                        break;
                }
            }
            position = new Vector2(nextX, nextY);

            int nextTileX = -1, nextTileY = -1;
            switch (direction)
            {
                case Direction.Up:
                    nextTileX = tileX;
                    nextTileY = tileY - 1;
                    break;
                case Direction.Down:
                    nextTileX = tileX;
                    nextTileY = tileY + 1;
                    break;
                case Direction.Left:
                    nextTileX = tileX - 1;
                    nextTileY = tileY;
                    break;
                case Direction.Right:
                    nextTileX = tileX + 1;
                    nextTileY = tileY;
                    break;
            }
            int nextLevelX = -1, nextLevelY = -1;
            Level nextLevel = null;
            if (nextTileX < 0)
            {
                nextLevelX = levelX - 1;
                nextLevelY = levelY;
                if (nextLevelX >= 0)
                    nextLevel = Game1.levelManager.levels[nextLevelX, nextLevelY];
                nextTileX = LevelContent.LEVEL_SIZE - 1;
            }
            else if (nextTileX >= LevelContent.LEVEL_SIZE)
            {
                nextLevelX = levelX + 1;
                nextLevelY = levelY;
                if (nextLevelX < LevelManager.MAX_LEVELS)
                    nextLevel = Game1.levelManager.levels[nextLevelX, nextLevelY];
                nextTileX = 0;
            }
            else if (nextTileY < 0)
            {
                nextLevelX = levelX;
                nextLevelY = levelY - 1;
                if (nextLevelY >= 0)
                    nextLevel = Game1.levelManager.levels[nextLevelX, nextLevelY];
                nextTileY = LevelContent.LEVEL_SIZE - 1;
            }
            else if (nextTileY >= LevelContent.LEVEL_SIZE)
            {
                nextLevelX = levelX;
                nextLevelY = levelY + 1;
                if (nextLevelY < LevelManager.MAX_LEVELS)
                    nextLevel = Game1.levelManager.levels[nextLevelX, nextLevelY];
                nextTileY = 0;
            }
            else
            {
                nextLevel = level;
            }
            float DRILL_OFFSET = 16;
            Vector2 drillOffset = Vector2.Zero, drillOffsetLeft = Vector2.Zero, drillOffsetRight = Vector2.Zero;
            float drillRotation;
            bool drillingLeft = false;
            bool drillingRight = false;
            switch (direction)
            {
                case Direction.Up:
                    drillOffset = new Vector2((levelX * Level.TEX_SIZE + tileX * Level.TILE_SIZE + Level.TILE_SIZE / 2) - position.X, -DRILL_OFFSET);
                    if (nextLevel != null)
                    {
                        if (nextTileX > 0)
                        {
                            drillingLeft = nextLevel.grid[nextTileX - 1, nextTileY] == LevelContent.LevelTile.Black;
                            drillOffsetLeft = new Vector2(drillOffset.X - Level.TILE_SIZE, drillOffset.Y);
                        }
                        else
                        {
                            drillingLeft = nextLevel.grid[nextTileX + 2, nextTileY] == LevelContent.LevelTile.Black;
                            drillOffsetLeft = new Vector2(drillOffset.X + 2 * Level.TILE_SIZE, drillOffset.Y);
                        }
                        if (nextTileX < LevelContent.LEVEL_SIZE - 1)
                        {
                            drillingRight = nextLevel.grid[nextTileX + 1, nextTileY] == LevelContent.LevelTile.Black;
                            drillOffsetRight = new Vector2(drillOffset.X + Level.TILE_SIZE, drillOffset.Y);
                        }
                        else
                        {
                            drillingRight = nextLevel.grid[nextTileX - 2, nextTileY] == LevelContent.LevelTile.Black;
                            drillOffsetRight = new Vector2(drillOffset.X - 2 * Level.TILE_SIZE, drillOffset.Y);
                        }
                    }
                    drillRotation = 0;
                    break;
                case Direction.Down:
                    drillOffset = new Vector2((levelX * Level.TEX_SIZE + tileX * Level.TILE_SIZE + Level.TILE_SIZE / 2) - position.X, DRILL_OFFSET);
                    if (nextLevel != null)
                    {
                        if (nextTileX > 0)
                        {
                            drillingRight = nextLevel.grid[nextTileX - 1, nextTileY] == LevelContent.LevelTile.Black;
                            drillOffsetRight = new Vector2(drillOffset.X - Level.TILE_SIZE, drillOffset.Y);
                        }
                        else
                        {
                            drillingRight = nextLevel.grid[nextTileX + 2, nextTileY] == LevelContent.LevelTile.Black;
                            drillOffsetRight = new Vector2(drillOffset.X + 2 * Level.TILE_SIZE, drillOffset.Y);
                        }
                        if (nextTileX < LevelContent.LEVEL_SIZE - 1)
                        {
                            drillingLeft = nextLevel.grid[nextTileX + 1, nextTileY] == LevelContent.LevelTile.Black;
                            drillOffsetLeft = new Vector2(drillOffset.X + Level.TILE_SIZE, drillOffset.Y);
                        }
                        else
                        {
                            drillingLeft = nextLevel.grid[nextTileX - 1, nextTileY] == LevelContent.LevelTile.Black;
                            drillOffsetLeft = new Vector2(drillOffset.X - 2 * Level.TILE_SIZE, drillOffset.Y);
                        }
                    }
                    drillRotation = (float)Math.PI;
                    break;
                case Direction.Left:
                    drillOffset = new Vector2(-DRILL_OFFSET, (levelY * Level.TEX_SIZE + tileY * Level.TILE_SIZE + Level.TILE_SIZE / 2) - position.Y);
                    if (nextLevel != null)
                    {
                        if (nextTileY > 0)
                        {
                            drillingRight = nextLevel.grid[nextTileX, nextTileY - 1] == LevelContent.LevelTile.Black;
                            drillOffsetRight = new Vector2(drillOffset.X, drillOffset.Y - Level.TILE_SIZE);
                        }
                        else
                        {
                            drillingRight = nextLevel.grid[nextTileX, nextTileY + 2] == LevelContent.LevelTile.Black;
                            drillOffsetRight = new Vector2(drillOffset.X, drillOffset.Y + 2 * Level.TILE_SIZE);
                        }
                        if (nextTileY < LevelContent.LEVEL_SIZE - 1)
                        {
                            drillingLeft = nextLevel.grid[nextTileX, nextTileY + 1] == LevelContent.LevelTile.Black;
                            drillOffsetLeft = new Vector2(drillOffset.X, drillOffset.Y + Level.TILE_SIZE);
                        }
                        else
                        {
                            drillingLeft = nextLevel.grid[nextTileX, nextTileY - 2] == LevelContent.LevelTile.Black;
                            drillOffsetLeft = new Vector2(drillOffset.X, drillOffset.Y - 2 * Level.TILE_SIZE);
                        }
                    }
                    drillRotation = (float)Math.PI / 2;
                    break;
                case Direction.Right:
                    drillOffset = new Vector2(DRILL_OFFSET, (levelY * Level.TEX_SIZE + tileY * Level.TILE_SIZE + Level.TILE_SIZE / 2) - position.Y);
                    if (nextLevel != null)
                    {
                        if (nextTileY > 0)
                        {
                            drillingLeft = nextLevel.grid[nextTileX, nextTileY - 1] == LevelContent.LevelTile.Black;
                            drillOffsetLeft = new Vector2(drillOffset.X, drillOffset.Y - Level.TILE_SIZE);
                        }
                        else
                        {
                            drillingLeft = nextLevel.grid[nextTileX, nextTileY + 2] == LevelContent.LevelTile.Black;
                            drillOffsetLeft = new Vector2(drillOffset.X, drillOffset.Y + 2 * Level.TILE_SIZE);
                        }
                        if (nextTileY < LevelContent.LEVEL_SIZE - 1)
                        {
                            drillingRight = nextLevel.grid[nextTileX, nextTileY + 1] == LevelContent.LevelTile.Black;
                            drillOffsetRight = new Vector2(drillOffset.X, drillOffset.Y + Level.TILE_SIZE);
                        }
                        else
                        {
                            drillingRight = nextLevel.grid[nextTileX, nextTileY - 2] == LevelContent.LevelTile.Black;
                            drillOffsetRight = new Vector2(drillOffset.X, drillOffset.Y - 2 * Level.TILE_SIZE);
                        }
                    }
                    drillRotation = (float)Math.PI * 3 / 2;
                    break;
            }
            drilling = false;
            if (nextLevel != null)
            {
                LevelContent.LevelTile nextTile = nextLevel.grid[nextTileX, nextTileY];
                if (!moved && nextTile.Equals(LevelContent.LevelTile.Black)) //drill
                {
                    if (powerUp2 == 1)
                    {
                        drilling = true;
                        drillingTime += seconds * moveSpeedMultiplier;
                        if (drillingTime >= DRILL_SINGLE_TIME)
                        {
                            nextLevel.drillWall(nextTileX, nextTileY);
                            drillingTime = 0;
                        }
                        drillingRatio = drillingTime / DRILL_SINGLE_TIME;
                    }
                    else if (powerUp2 == 2)
                    {
                        drilling = true;
                        drillingTime += seconds * moveSpeedMultiplier;
                        if (drillingTime >= DRILL_TRIPLE_TIME)
                        {
                            switch (direction)
                            {
                                case Direction.Up:
                                case Direction.Down:
                                    if (!nextLevel.drillWall(nextTileX + 1, nextTileY))
                                        nextLevel.drillWall(nextTileX - 2, nextTileY);
                                    if (!nextLevel.drillWall(nextTileX - 1, nextTileY))
                                        nextLevel.drillWall(nextTileX + 2, nextTileY);
                                    break;
                                case Direction.Left:
                                case Direction.Right:
                                    if (!nextLevel.drillWall(nextTileX, nextTileY + 1))
                                        nextLevel.drillWall(nextTileX, nextTileY - 2);
                                    if (!nextLevel.drillWall(nextTileX, nextTileY - 1))
                                        nextLevel.drillWall(nextTileX, nextTileY + 2);
                                    break;
                            }
                            nextLevel.drillWall(nextTileX, nextTileY);
                            drillingTime = 0;
                        }
                        drillingRatio = drillingTime / DRILL_TRIPLE_TIME;
                    }
                }
            }
            if (drilling)
            {
                if (powerUp2 == 1)
                {
                    drillEmitter.active = true;
                    drillEmitter.position = position + drillOffset;
                    drillEmitter.startSize = 1.5f * drillingRatio + 0.2f;
                }
                else
                {
                    drillEmitter.active = true;
                    drillEmitter.position = position + drillOffset;
                    drillEmitter.startSize = 1.5f * drillingRatio + 0.2f;
                    drillEmitterLeft.active = drillingLeft;
                    drillEmitterLeft.position = position + drillOffsetLeft;
                    drillEmitterLeft.startSize = 1.5f * drillingRatio + 0.2f;
                    drillEmitterRight.active = drillingRight;
                    drillEmitterRight.position = position + drillOffsetRight;
                    drillEmitterRight.startSize = 1.5f * drillingRatio + 0.2f;
                }
            }
            else
            {
                drillEmitter.active = false;
                drillingTime -= seconds * 4;
                if (drillingTime < 0)
                    drillingTime = 0;
                drillingRatio = drillingTime / DRILL_TRIPLE_TIME;
                if (powerUp2 == 2)
                {
                    drillEmitterLeft.active = false;
                    drillEmitterRight.active = false;
                }
            }

            // update particles
            if (bursting || powerUp1 == 2)
            {
                leftBoosterFiring.active = true;
                rightBoosterFiring.active = true;
                leftBoosterIdle.active = false;
                rightBoosterIdle.active = false;
            }
            else
            {
                leftBoosterIdle.active = true;
                rightBoosterIdle.active = true;
                leftBoosterFiring.active = false;
                rightBoosterFiring.active = false;
            }
            leftBoosterFiring.position = position + leftBoosterOffset;
            rightBoosterFiring.position = position + rightBoosterOffset;
            leftBoosterFiring.angle = boosterAngle;
            rightBoosterFiring.angle = boosterAngle;
            leftBoosterIdle.position = position + leftBoosterOffset;
            rightBoosterIdle.position = position + rightBoosterOffset;
            leftBoosterIdle.angle = boosterAngle;
            rightBoosterIdle.angle = boosterAngle;
            if (bursting || powerUp1 == 2)
            {
                float speed = movement.Length();
                leftBoosterFiring.valueToDeath = BOOSTER_LENGTH * (1 + speed / (MOVE_SPEED * seconds));
                rightBoosterFiring.valueToDeath = BOOSTER_LENGTH * (1 + speed / (MOVE_SPEED * seconds));
            }
            if (powerUp1 == 1)
            {
                if (burstRecharge >= BURST_COOLDOWN)
                {
                    leftBoosterIdle.valueToDeath = 12;
                    rightBoosterIdle.valueToDeath = 12;
                    leftBoosterIdle.startColor = BOOST_IDLE_RECHARGED_COLOR;
                    rightBoosterIdle.startColor = BOOST_IDLE_RECHARGED_COLOR;
                }
                else
                {
                    leftBoosterIdle.valueToDeath = 10;
                    rightBoosterIdle.valueToDeath = 10;
                    leftBoosterIdle.startColor = BOOST_IDLE_NOT_RECHARGED_COLOR;
                    rightBoosterIdle.startColor = BOOST_IDLE_NOT_RECHARGED_COLOR;
                }
            }
            chargeEmitter.position = position;
            if (seconds > 0)
            {
                leftBoosterIdle.Update(gameTime);
                rightBoosterIdle.Update(gameTime);
                leftBoosterFiring.Update(gameTime);
                rightBoosterFiring.Update(gameTime);
                drillEmitter.Update(gameTime); 
                if (powerUp2 == 2)
                {
                    drillEmitterLeft.Update(gameTime);
                    drillEmitterRight.Update(gameTime); 
                }
                if (powerUp3 == 3)
                {
                    chargeEmitter.Update(gameTime);
                }
            }

            fired = false;
            base.Update(gameTime);
        }

        public void setCurrentLevelAndTile()
        {
            int x = (int)position.X;
            int y = (int)position.Y;

            levelX = x / Level.TEX_SIZE; // get which level you are in
            levelY = y / Level.TEX_SIZE;

            tileX = (x % Level.TEX_SIZE) / Level.TILE_SIZE; // get which tile you are moving to
            tileY = (y % Level.TEX_SIZE) / Level.TILE_SIZE;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            History.DrawHero(spriteBatch);
            if (powerUp1 > 0)
            {
                leftBoosterIdle.Draw(spriteBatch);
                rightBoosterIdle.Draw(spriteBatch);
                leftBoosterFiring.Draw(spriteBatch);
                rightBoosterFiring.Draw(spriteBatch);
            }
            drillEmitter.Draw(spriteBatch); 
            if (powerUp2 == 2)
            {
                drillEmitterLeft.Draw(spriteBatch);
                drillEmitterRight.Draw(spriteBatch);
            }
            base.Draw(spriteBatch);
            if (powerUp3 == 3)
            {
                chargeEmitter.Draw(spriteBatch);
            }
        }
    }
}
