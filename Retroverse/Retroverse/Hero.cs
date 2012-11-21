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
        public float heroTimeScale = 1f;
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

        public static readonly float INVINCIBILITY_INTERVAL = 0.5f;
        public bool invincible = false;
        public float invincibilityTimer = 0;

        public int powerupBoost; //, 0= Normal, 1=Bursts, 2= Fast, 3=Reverse
        public int powerupDrill; //, 0= Normal, 1=singledrill, 2=tripledrill
        public int powerupGun; // 0=Normal, 1=Front, 2=Side, 3=Charge
        public int powerupRetro; // 0=Normal, 1=RetroPort, 2=RetroStatis
        public int powerupRadar; // 0=Normal, 1=Radar

        public Hero()
            : base(new Hitbox(32, 32))
        {
            position = new Vector2(Level.TEX_SIZE * LevelManager.STARTING_LEVEL.X + (Level.TILE_SIZE * (LevelManager.STARTING_TILE.X + 0.5f)), Level.TEX_SIZE * LevelManager.STARTING_LEVEL.Y + (Level.TILE_SIZE * (LevelManager.STARTING_TILE.Y + 0.5f)));
            setCurrentLevelAndTile();
            this.setTexture("hero");
            direction = Direction.Up;
            ammo = new List<Bullet>();
            instance = this;
            powerupBoost = 0;
            powerupDrill = 0;
            powerupGun = 0;
            powerupRetro = 0;
            powerupRadar = 0;
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

        public void fire()
        {
            fired = true;
            if (bulletTimer < BULLET_FIRE_INTERVAL)
                return;
            bulletTimer = 0;
            if (powerupGun == 0)
            { }
            else if (powerupGun == 1)
            {
                ammo.Add(new Bullet("bullet1", PrebuiltEmitter.SmallBulletSparks, EMITTER_STRAIGHT_COLOR, direction, Bullet.DISTANCE_LIMIT_NORMAL, BULLET_DAMAGE_NORMAL));
                ammo.Last().position = new Vector2(this.position.X, this.position.Y);
                ammo.Last().scale = BULLET_NORMAL_SCALE;
                ammo.Last().hitbox.originalRectangle.Height = (int)(20);
                ammo.Last().hitbox.originalRectangle.Width = (int)(20);
            }
            else if (powerupGun == 2)
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
                ammo.Add(new Bullet("bullet2", PrebuiltEmitter.SmallBulletSparks, EMITTER_SIDE_COLOR, dirLeft, Bullet.DISTANCE_LIMIT_NORMAL, BULLET_DAMAGE_NORMAL));
                ammo.Last().position = new Vector2(this.position.X, this.position.Y);
                ammo.Last().scale = BULLET_NORMAL_SCALE;
                ammo.Last().hitbox.originalRectangle.Height = (int)(20);
                ammo.Last().hitbox.originalRectangle.Width = (int)(20);
                ammo.Add(new Bullet("bullet2", PrebuiltEmitter.SmallBulletSparks, EMITTER_SIDE_COLOR, dirRight, Bullet.DISTANCE_LIMIT_NORMAL, BULLET_DAMAGE_NORMAL));
                ammo.Last().position = new Vector2(this.position.X, this.position.Y);
                ammo.Last().scale = BULLET_NORMAL_SCALE;
                ammo.Last().hitbox.originalRectangle.Height = (int)(20);
                ammo.Last().hitbox.originalRectangle.Width = (int)(20);
            }
        }

        public bool activateRetro()
        {
            bool successful = false;
            switch (Game1.state)
            {
                case GameState.Arena:
                case GameState.Escape:
                    if (powerupRetro == 1)
                    {
                        if (Game1.availableSand > 0 && History.canRevert())
                        {
                            History.lastState = Game1.state;
                            Game1.state = GameState.RetroPort;
                            Game1.removeSand();
                            successful = true;
                        }
                    }
                    else if (powerupRetro == 2)
                    {
                        if (Game1.availableSand > 0 && RetroStasis.canActivate())
                        {
                            RetroStasis.activate();
                            Game1.removeSand();
                            successful = true;
                        }
                        else
                        {
                            if (RetroStasis.canDeactivate())
                            {
                                RetroStasis.deactivate();
                                successful = true;
                            }
                        }
                    }
                    break;
                case GameState.RetroPort:
                    if (powerupRetro == 1)
                    {
                        History.cancelRevert();
                        successful = true;
                    }
                    break;
            }
            return successful;
        }

        public void special2()
        {
        }

        public void burst()
        {
            if (Game1.state == GameState.Arena || Game1.state == GameState.Escape)
                if (powerupBoost == 1 && !bursting && burstRecharge >= BURST_COOLDOWN)
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
                if (Game1.availableSand > 0 && powerupRetro == 1 && History.canRevert())
                    activateRetro();
                else if (Game1.availableSand > 0 && powerupRetro == 2 && RetroStasis.canActivate())
                    activateRetro();
                else
                    Game1.gameOver();
            }
        }

        public void collideWithRiotGuardWall()
        {
            if (Game1.INVINCIBILITY)
                return;
            if (!Game1.retroStatisActive && Game1.state != GameState.RetroPort)
            {
                if (Game1.availableSand > 0 && powerupRetro == 1 && History.canRevert())
                {
                    activateRetro();
                    RiotGuardWall.setReverse(true);
                }
                else if (Game1.availableSand > 0 && powerupRetro == 2 && RetroStasis.canActivate())
                {
                    activateRetro();
                    RiotGuardWall.setReverse(true);
                }
                else
                    Game1.gameOver();         
            }
        }

        public float getPowerupCharge(int powerup)
        {
            float charge = 0;
            switch (powerup)
            {
                case 0:
                    if (powerupBoost == 1)
                        if (bursting) charge = 0;
                        else charge = burstRecharge / BURST_COOLDOWN;
                    else if (powerupBoost == 2)
                        charge = 1;
                    break;
                case 1:
                    if (powerupGun == 1)
                        charge = bulletTimer / BULLET_FIRE_INTERVAL;
                    else if (powerupGun == 2)
                        charge = bulletTimer / BULLET_FIRE_INTERVAL;
                    else if (powerupGun == 3)
                        if (chargeTimer < BULLET_CHARGE_TIME_SMALL)
                            charge = chargeTimer / BULLET_CHARGE_TIME_SMALL;
                        else if (chargeTimer >= BULLET_CHARGE_TIME_SMALL && chargeTimer < BULLET_CHARGE_TIME_MEDIUM)
                            charge = (chargeTimer - BULLET_CHARGE_TIME_SMALL) / (BULLET_CHARGE_TIME_MEDIUM - BULLET_CHARGE_TIME_SMALL);
                        else if (chargeTimer >= BULLET_CHARGE_TIME_MEDIUM && chargeTimer < BULLET_CHARGE_TIME_LARGE)
                            charge = (chargeTimer - BULLET_CHARGE_TIME_MEDIUM) / (BULLET_CHARGE_TIME_LARGE - BULLET_CHARGE_TIME_MEDIUM);
                        else if (chargeTimer >= BULLET_CHARGE_TIME_LARGE)
                            charge = 1;
                    break;
                case 2:
                    if (powerupRetro == 1)
                        if (Game1.state == GameState.RetroPort) charge = 0;
                        else charge = History.secsSinceLastRetroPort / History.retroportSecs;
                    else if (powerupRetro == 2)
                        charge = RetroStasis.getChargePercentage();
                    break;
                case 3:
                    if (powerupDrill == 0) charge = 0;
                    else charge = 1;
                    break;
                case 4:
                    if (powerupRadar == 0) charge = 0;
                    else charge = 1;
                    break;
            }
            return MathHelper.Clamp(charge, 0, 1);
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds(1f);

            RetroStasis.Update(gameTime);

            seconds = gameTime.getSeconds(heroTimeScale);
            bulletTimer += seconds;

            if (powerupGun == 3)
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
                        Bullet b = new Bullet("chargebullet1", PrebuiltEmitter.SmallBulletSparks, EMITTER_CHARGE_COLOR, direction, Bullet.DISTANCE_LIMIT_CHARGE, BULLET_DAMAGE_CHARGE_SMALL);
                        ammo.Add(b);
                        b.scale = BULLET_SMALL_SCALE;
                        b.hitbox.originalRectangle.Height = (int)(64 * BULLET_SMALL_SCALE);
                        b.hitbox.originalRectangle.Width = (int)(64 * BULLET_SMALL_SCALE);
                        b.position = new Vector2(this.position.X, this.position.Y);
                        b.explosionEmitter.startSize = 1f;
                        b.explosionEmitter.endSize = 1f;
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
                        Bullet b = new Bullet("chargebullet2", PrebuiltEmitter.MediumBulletSparks, EMITTER_CHARGE_COLOR, direction, Bullet.DISTANCE_LIMIT_CHARGE, BULLET_DAMAGE_CHARGE_MEDIUM, true);
                        ammo.Add(b);
                        b.scale = BULLET_MEDIUM_SCALE;
                        b.hitbox.originalRectangle.Height = (int)(64 * BULLET_MEDIUM_SCALE);
                        b.hitbox.originalRectangle.Width = (int)(64 * BULLET_MEDIUM_SCALE);
                        b.position = new Vector2(this.position.X, this.position.Y);
                        b.explosionEmitter.startSize = 1f;
                        b.explosionEmitter.endSize = 1f;
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
                        Bullet b = new Bullet("chargebullet3", PrebuiltEmitter.LargeBulletSparks, EMITTER_CHARGE_COLOR, direction, Bullet.DISTANCE_LIMIT_CHARGE, BULLET_DAMAGE_CHARGE_LARGE, true);
                        ammo.Add(b);
                        b.scale = BULLET_LARGE_SCALE;
                        b.hitbox.originalRectangle.Height = (int)(64 * BULLET_LARGE_SCALE);
                        b.hitbox.originalRectangle.Width = (int)(64 * BULLET_LARGE_SCALE);
                        b.position = new Vector2(this.position.X, this.position.Y);
                        b.explosionEmitter.startSize = 1f;
                        b.explosionEmitter.endSize = 1f;
                        chargeTimer = 0;
                    }
                    chargeEmitter.startSize = CHARGE_PARTICLES_LARGE_SCALE;
                    Color c = CHARGE_COLOR_LARGE;
                    chargeEmitter.startColor = c;
                    c.A = 255;
                    chargeEmitter.endColor = c;
                }
            }

            if (powerupBoost == 0)
            {
                moveSpeedMultiplier = 1f;
            }
            else if (powerupBoost == 2)
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
                    if (powerupDrill == 1)
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
                    else if (powerupDrill == 2)
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
                if (powerupDrill == 1)
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
                if (powerupDrill == 2)
                {
                    drillEmitterLeft.active = false;
                    drillEmitterRight.active = false;
                }
            }

            // update particles
            if (bursting || powerupBoost == 2)
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
            if (bursting || powerupBoost == 2)
            {
                float speed = movement.Length();
                leftBoosterFiring.valueToDeath = BOOSTER_LENGTH * (1 + speed / (MOVE_SPEED * seconds));
                rightBoosterFiring.valueToDeath = BOOSTER_LENGTH * (1 + speed / (MOVE_SPEED * seconds));
            }
            if (powerupBoost == 1)
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
                if (powerupDrill == 2)
                {
                    drillEmitterLeft.Update(gameTime);
                    drillEmitterRight.Update(gameTime); 
                }
                if (powerupGun == 3)
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
            if (powerupBoost > 0)
            {
                leftBoosterIdle.Draw(spriteBatch);
                rightBoosterIdle.Draw(spriteBatch);
                leftBoosterFiring.Draw(spriteBatch);
                rightBoosterFiring.Draw(spriteBatch);
            }
            drillEmitter.Draw(spriteBatch); 
            if (powerupDrill == 2)
            {
                drillEmitterLeft.Draw(spriteBatch);
                drillEmitterRight.Draw(spriteBatch);
            }
            base.Draw(spriteBatch);
            if (powerupGun == 3)
            {
                chargeEmitter.Draw(spriteBatch);
            }
        }
    }
}
