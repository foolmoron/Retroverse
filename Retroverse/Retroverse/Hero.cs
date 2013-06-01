using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;
using Microsoft.Xna.Framework.Input;

namespace Retroverse
{
    public class HeroSaveState
    {
        public PlayerIndex index;
        [XmlElement(Type = typeof(XmlColor))]
        public Color color;
        public string name;
        public int id;
        public List<PrisonerInfo> freedPrisoners;
        public int collectedGems;
        public int killedEnemyCount;
        public int hitByEnemyCount;

        private HeroSaveState() { }
        public HeroSaveState(Hero hero)
        {
            index = hero.PlayerIndex;
            color = hero.color;
            name = hero.prisonerName;
            id = hero.prisonerID;
            freedPrisoners = hero.FreedPrisoners;
            collectedGems = hero.CollectedGems;
            killedEnemyCount = hero.KilledEnemyCount;
            hitByEnemyCount = hero.HitByEnemyCount;
        }
    }

    public struct PrisonerInfo
    {
        public string id;
        public string name;
        [XmlElement(Type = typeof(XmlColor))]
        public Color color;
    }

    public class Hero : Entity, IReversible
    {
        public static readonly Color[] NEW_HERO_COLORS = new Color[RetroGame.MAX_PLAYERS] { Color.Transparent, Color.Transparent };
        public static readonly string[] NEW_HERO_NAMES = new string[RetroGame.MAX_PLAYERS] { "", "" };
        public static readonly int[] NEW_HERO_IDS = new int[RetroGame.MAX_PLAYERS] { 0, 0 };

        public static float HERO_TIMESCALE = 1f;
        public int playerIndex;
        public PlayerIndex PlayerIndex;
        public Bindings bindings;
        public string prisonerName;
        public int prisonerID;
        public Color color;
        public bool Alive { get; private set; }
        public bool Fugitive { get { return Alive && RetroGame.HasDrilled; } }

        //flashing
        public float flashTime;
        public Color flashColor;
        public int flashCount;
        public float individualFlashDuration;

        public int CollectedSand { get; set; }
        public int CollectedBombs { get; set; }
        public int CollectedGems { get; set; }
        public int KilledEnemyCount { get; set; }
        public int HitByEnemyCount { get; set; }

        public List<PrisonerInfo> FreedPrisoners = new List<PrisonerInfo>();

        private static readonly Dictionary<Direction, float> DIR_TO_ROTATION = new Dictionary<Direction, float>(){
            {Direction.Up, (float)Math.PI},
            {Direction.Down, 0},
            {Direction.Left, (float)Math.PI / 2f},
            {Direction.Right, (float)Math.PI * 3f / 2f},
        };

        public const float INITIAL_HEALTH = 10f;
        public float health = INITIAL_HEALTH;

        public const float REVIVE_HEALTH = INITIAL_HEALTH / 5;
        public PlayerPrisoner playerPrisoner;

        public int nextLevelX, nextLevelY, nextTileX, nextTileY;
        public Level nextLevel;
        public bool moved;

        public Dictionary<string, Powerup> Powerups { get; private set; }
        // add powerup-specific values here
        public float powerupCooldownModifier = 1f;
        public float globalMoveSpeedMultiplier = 1f;
        public bool teleportedThisFrame = false;

        public Vector2 movement;
        public const float MOVE_SPEED = 225f;

        public Hero(PlayerIndex PlayerIndex)
            : base(new Vector2(Level.TEX_SIZE * LevelManager.STARTING_LEVEL.X + (Level.TILE_SIZE * (LevelManager.STARTING_TILE.X + 0.5f)), 
                               Level.TEX_SIZE * LevelManager.STARTING_LEVEL.Y + (Level.TILE_SIZE * (LevelManager.STARTING_TILE.Y + 0.5f))),
                   new Hitbox(32, 32))
        {
            Alive = true;
            this.PlayerIndex = PlayerIndex;
            this.playerIndex = (int)PlayerIndex;
            bindings = new Bindings(PlayerIndex);
            updateCurrentLevelAndTile();

            Powerups = new Dictionary<string, Powerup>();
            setTexture("hero");
            direction = Direction.Up;
            scale = 32f / getTexture().Width;
        }

        public Hero(HeroSaveState saveState)
            : this(saveState.index)
        {
            color = saveState.color;
            maskingColor = saveState.color;
            prisonerName = saveState.name;
            prisonerID = saveState.id;
            Prisoner.TAKEN_IDS[prisonerID] = true;
            History.RegisterReversible(this);
            FreedPrisoners = saveState.freedPrisoners;
            CollectedGems = saveState.collectedGems;
            KilledEnemyCount = saveState.killedEnemyCount;
            HitByEnemyCount = saveState.hitByEnemyCount;
        }

        public void Initialize()
        {
#if DEBUG
            AddPowerup(typeof(Flamethrower));
            AddPowerup(typeof(TimedSpeedBoost));
            AddPowerup(typeof(AdrenalinePickup));
            AddPowerup(typeof(FireChains));
            AddPowerup(typeof(DrillFast));
            AddPowerup(typeof(RocketBurst));
            Inventory.StorePowerup(new DrillTriple(this));
            Inventory.StorePowerup(new ShieldSlow(this));
            Inventory.StorePowerup(new ShieldDamage(this));
            Inventory.StorePowerup(new BombTimed(this));
            Inventory.StorePowerup(new BombSet(this));
            AddPowerup(typeof(RescuePowerup));
            if (this == RetroGame.getHeroes()[0])
                AddPowerup(typeof(FireChains));
#endif

            color = NEW_HERO_COLORS[playerIndex];
            maskingColor = color;
            prisonerName = NEW_HERO_NAMES[playerIndex];
            prisonerID = NEW_HERO_IDS[playerIndex];
            Prisoner.TAKEN_IDS[prisonerID] = true;
            History.RegisterReversible(this);
        }

        public Powerup AddPowerup(Type powerupType, bool automaticallySetPowerupIcon = true)
        {
            Powerup powerup = null;
            if (powerupType.IsSubclassOf(typeof(CoOpPowerup)))
            {
                //gets the other hero from the list of heroes (or just another hero if for some reason there are more than 2 heroes)
                Hero otherHero = null;
                foreach (Hero h in RetroGame.getHeroes())
                {
                    if (h != this)
                    {
                        otherHero = h;
                        break;
                    }
                }
                if (otherHero == null)
                    return null;
                powerup = (Powerup)powerupType.GetConstructor(new Type[] { typeof(Hero), typeof(Hero) }).Invoke(new object[] { this, otherHero });
            }
            else
            {
                powerup = (Powerup)powerupType.GetConstructor(new Type[] { typeof(Hero) }).Invoke(new object[] { this });
            }

            if (Inventory.EquipPowerup(powerup, playerIndex, automaticallySetPowerupIcon))
            {
                Powerups.Add(powerup.GenericName, powerup);
                powerup.OnAddedToHero();
                if (powerup is IReversible)
                {
                    History.RegisterReversible((IReversible)powerup);
                }
                History.Clear();
            }
            return powerup;
        }

        public void RemovePowerup(string genericPowerupName, bool automaticallyStoreInInventory = true)
        {
            Powerup powerup = Powerups[genericPowerupName];
            Inventory.UnequipPowerup(playerIndex, powerup.Active, powerup);
            if (automaticallyStoreInInventory)
                Inventory.StorePowerup(powerup);
            Powerups.Remove(genericPowerupName);
            powerup.OnRemovedFromHero();
            if (powerup is IReversible)
            {
                History.UnRegisterReversible((IReversible)powerup);
            }
        }

        public Powerup GetPowerup(string genericPowerupName)
        {
            if (Powerups.Keys.Contains(genericPowerupName))
                return Powerups[genericPowerupName];
            return null;
        }

        public bool HasPowerup(string genericPowerupName)
        {
            return Powerups.Keys.Contains(genericPowerupName);
        }

        public void flashWithColor(Color flashColor, int flashCount, float individualFlashDuration)
        {
            flashTime = 0;
            this.flashColor = flashColor;
            this.flashCount = flashCount;
            this.individualFlashDuration = individualFlashDuration;
        }

        public void hitBy(Enemy e, float damage)
        {
            if (RetroGame.INVINCIBILITY)
                return;
            health -= damage;
            if (health <= 0)
            {
                if (attemptDie())
                {
                    playerPrisoner = new PlayerPrisoner(this, levelX, levelY, tileX, tileY);
                    RetroGame.EscapeScreen.levelManager.levels[levelX, levelY].prisoners.Add(playerPrisoner);
                    return;
                }
            }
            SoundManager.PlaySoundOnce("PlayerHit", playInReverseDuringReverse: true);
            flashWithColor(Color.Red.withAlpha(150), 2, 0.5f);
        }

        public bool attemptDie()
        {
            //On-death powerup activation
            if (HasPowerup("Retro"))
            {
                if (Powerups["Retro"] is RetroPort && History.CanRevert() && RetroGame.AvailableSand > 0)
                {
                    Powerups["Retro"].Activate(InputAction.None);
                    return false;
                }
            } 
            else if (HasPowerup("Health"))
            {
                if (Powerups["Health"] is FullHealthPickup)
                {
                    Powerups["Health"].Activate(InputAction.None);
                    return false;
                }
            }

            SoundManager.PlaySoundOnce("PlayerDead", playInReverseDuringReverse: true);

            Alive = false;
            if (RetroGame.getMainLiveHero() == null)
            {
                RetroGame.GameOver();
                return true;
            }

            LevelManager levelManager = RetroGame.EscapeScreen.levelManager;
            levelManager.SetCameraMode(RetroGame.EscapeScreen.levelManager.CameraMode);
            return true;
        }

        public void revive(Vector2? atPosition = null)
        {
            Alive = true;
            health = REVIVE_HEALTH;

            LevelManager levelManager = RetroGame.EscapeScreen.levelManager;
            levelManager.SetCameraMode(RetroGame.EscapeScreen.levelManager.CameraMode);

            if (playerPrisoner != null)
            {
                levelManager.collectablesToRemove.Add(playerPrisoner);
                playerPrisoner = null;
            }

            if(atPosition != null) 
                position = atPosition.Value;
            updateCurrentLevelAndTile();
            flashWithColor(Color.Cyan.withAlpha(150), 2, 0.75f);
        }

        public void collideWithRiotGuardWall()
        {
            if (RetroGame.INVINCIBILITY)
                return;
            SoundManager.PlaySoundOnce("PlayerHit", playInReverseDuringReverse: true);
            attemptDie();
        }

        public void AddCollectedPrisoner(Prisoner p)
        {
            FreedPrisoners.Add(new PrisonerInfo { id = p.id, name = p.name, color = p.maskingColor });
        }

        public override void OnInputAction(InputAction action, bool pressedThisFrame)
        {
            switch (action)
            {
                case InputAction.Action1: //default P1 - Space,         P2 - NumPad0,   360 - A
                case InputAction.Action2: //default P1 - Q,             P2 - NumPad9,   360 - B
                case InputAction.Action3: //default P1 - LeftShift,     P2 - Enter,     360 - X
                case InputAction.Action4: //default P1 - LeftControl,   P2 - Plus,      360 - Y
                    Inventory.ActivatePowerup(playerIndex, action);
                    break;
                case InputAction.Start: //default P1 - T, P2 - T, 360 - Start
                    if (pressedThisFrame)
                    {
                        RetroGame.PauseGame(this);
                    }
                    break;
                case InputAction.Escape: //default P1 - Escape, P2 - Escape, 360 - Back
                    if (pressedThisFrame)
                    {
                        RetroGame.PauseGame(this);
                    }
                    break;
                default:
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            //reset per-frame powerup modification fields BEFORE updating controls
            globalMoveSpeedMultiplier = 1;
            powerupCooldownModifier = 1;
            teleportedThisFrame = false;

            if (!Alive)
            {
                Powerups = (from pair in Powerups orderby pair.Value ascending select pair).ToDictionary(pair => pair.Key, pair => pair.Value);
                foreach (Powerup p in Powerups.Values)
                    p.Update(gameTime);
                return;
            }

            UpdateControls(bindings, gameTime);
#if DEBUG
            if (this == RetroGame.getHeroes()[0])
                UpdateDebugKeys();
#endif
            //remove expendable powerups
            for (int i = 0; i < Powerups.Count; i++)
            {
                Powerup p = Powerups.Values.ElementAt(i);
                if (p.toRemove)
                {
                    RemovePowerup(p.GenericName, false);
                    i--;
                }
            }

            Powerups = (from pair in Powerups orderby pair.Value ascending select pair).ToDictionary(pair => pair.Key, pair => pair.Value);
            foreach (Powerup p in Powerups.Values)
                p.Update(gameTime);

            float seconds = gameTime.getSeconds(HERO_TIMESCALE);
            movement = dirVector * MOVE_SPEED * globalMoveSpeedMultiplier * seconds;

            updateCurrentLevelAndTile();
            Level level = RetroGame.getLevels()[levelX, levelY];

            float nextX = position.X + movement.X;
            float nextY = position.Y + movement.Y;
            moved = true;
            int n;
            if (HERO_TIMESCALE > 0f)
            {
                switch (controllerDirection)
                {
                    case Direction.Up:
                    case Direction.Down:
                        moved = canMove(movement);
                        if (!moved)
                        {
                            n = (int)position.Y;
                            nextY = n - (n % Level.TILE_SIZE) + Level.TILE_SIZE / 2;
                        }
                        break;
                    case Direction.Left:
                    case Direction.Right:
                        moved = canMove(new Vector2(movement.X, 0));
                        if (!moved)
                        {
                            n = (int)position.X;
                            nextX = n - (n % Level.TILE_SIZE) + Level.TILE_SIZE / 2;
                        }
                        break;
                    default:
                        nextX = position.X;
                        nextY = position.Y;
                        break;
                }
            }
            if (controllerDirection != Direction.None)
            {
                direction = controllerDirection;
            }
            rotation = DIR_TO_ROTATION[direction];
            position = new Vector2(nextX, nextY);
            // check corners
            LevelManager levelManager = RetroGame.TopLevelManagerScreen.levelManager;
            if (moved &&
                (levelManager.collidesWithWall(new Vector2(getLeft().X, getTop().Y)) || //topleft
                levelManager.collidesWithWall(new Vector2(getLeft().X, getBottom().Y)) || //botleft
                levelManager.collidesWithWall(new Vector2(getRight().X, getBottom().Y)) || //botright
                levelManager.collidesWithWall(new Vector2(getRight().X, getTop().Y)))) //topright
            {
                switch (controllerDirection)
                {
                    case Direction.Up:
                    case Direction.Down:
                        n = (int)position.X;
                        nextX = n - (n % Level.TILE_SIZE) + Level.TILE_SIZE / 2;
                        break;
                    case Direction.Left:
                    case Direction.Right:
                        n = (int)position.Y;
                        nextY = n - (n % Level.TILE_SIZE) + Level.TILE_SIZE / 2;
                        break;
                    default:
                        break;
                }
            }
            position = new Vector2(nextX, nextY);
            nextTileX = -1;
            nextTileY = -1;
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
            nextLevelX = -1;
            nextLevelY = -1;
            nextLevel = null;
            if (nextTileX < 0)
            {
                nextLevelX = levelX - 1;
                nextLevelY = levelY;
                if (nextLevelX >= 0)
                    nextLevel = RetroGame.getLevels()[nextLevelX, nextLevelY];
                nextTileX = Level.GRID_SIZE - 1;
            }
            else if (nextTileX >= Level.GRID_SIZE)
            {
                nextLevelX = levelX + 1;
                nextLevelY = levelY;
                if (nextLevelX < LevelManager.MAX_LEVELS)
                    nextLevel = RetroGame.getLevels()[nextLevelX, nextLevelY];
                nextTileX = 0;
            }
            else if (nextTileY < 0)
            {
                nextLevelX = levelX;
                nextLevelY = levelY - 1;
                if (nextLevelY >= 0)
                    nextLevel = RetroGame.getLevels()[nextLevelX, nextLevelY];
                nextTileY = Level.GRID_SIZE - 1;
            }
            else if (nextTileY >= Level.GRID_SIZE)
            {
                nextLevelX = levelX;
                nextLevelY = levelY + 1;
                if (nextLevelY < LevelManager.MAX_LEVELS)
                    nextLevel = RetroGame.getLevels()[nextLevelX, nextLevelY];
                nextTileY = 0;
            }
            else
            {
                nextLevel = level;
            }            

            //collision with collectables
            if (Alive)
            {
                foreach (Level l in levelManager.CurrentLevels)
                {
                    foreach (Collectable c in l.collectables)
                        if (c.ableToBeCollected && hitbox.intersects(c.hitbox))
                            c.collectedBy(this);
                    foreach (Prisoner p in l.prisoners)
                        if (p.ableToBeCollected && hitbox.intersects(p.hitbox))
                            p.collectedBy(this);
                    foreach (PowerupIcon p in l.powerups)
                        if (p.ableToBeCollected && hitbox.intersects(p.hitbox))
                            p.collectedBy(this);
                }
            }

            //flashing
            float flashTotalDuration = individualFlashDuration * flashCount;
            if (flashTime < flashTotalDuration)
            {
                flashTime += seconds;
                float flashInterp = (flashTime % individualFlashDuration) / individualFlashDuration;
                float colorInterp =  1f - (Math.Abs(flashInterp - 0.5f) * 2); //map 0.0 - 0.5 to 0.0 - 1.0 and 0.5 - 1.0 to 1.0 - 0.0
                maskingColor = Color.Lerp(color, flashColor, colorInterp);
            }
            else
            {
                maskingColor = color;
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (Powerup p in Powerups.Values)
            {
                if (p.DrawBeforeHero)
                    p.Draw(spriteBatch);
            }
            if (Alive)
                base.Draw(spriteBatch);
            foreach (Powerup p in Powerups.Values)
            {
                if (!p.DrawBeforeHero)
                    p.Draw(spriteBatch);
            }
        }

        public void DrawHistorical(SpriteBatch spriteBatch, IMemento heroMemento)
        {
            if (heroMemento is HeroMemento)
            {
                HeroMemento pastHero = (HeroMemento)heroMemento;
                spriteBatch.Draw(getTexture(), pastHero.position, null, maskingColor.withAlpha(120), pastHero.rotation, new Vector2(getTexture().Width / 2, getTexture().Height / 2), scale, getFlip(), layer);
            }
        }

        public void UpdateDebugKeys()
        {
            //update debug keys
            if (pressedThisFrame(Keys.Y) && this == RetroGame.getHeroes()[0])
            {
                if (GetPowerup("Rocket") is RocketBurst)
                {
                    RemovePowerup("Rocket");
                    AddPowerup(typeof(RocketBoost));
                }
                else
                {
                    if (HasPowerup("Rocket"))
                        RemovePowerup("Rocket");
                    AddPowerup(typeof(RocketBurst));
                }
            }
            if (pressedThisFrame(Keys.U))
            {
                if (GetPowerup("Gun") is ShotForward)
                {
                    RemovePowerup("Gun");
                    AddPowerup(typeof(ShotSide));
                }
                else if (GetPowerup("Gun") is ShotSide)
                {
                    RemovePowerup("Gun");
                    AddPowerup(typeof(ShotCharge));
                }
                else if (GetPowerup("Gun") is ShotCharge)
                {
                    RemovePowerup("Gun");
                    AddPowerup(typeof(Flamethrower));
                }
                else if (GetPowerup("Flamethrower") is Flamethrower)
                {
                    RemovePowerup("Flamethrower");
                }
                else
                {
                    if (HasPowerup("Gun"))
                        RemovePowerup("Gun");
                    AddPowerup(typeof(ShotForward));
                }
            }
            else if (pressedThisFrame(Keys.I))
            {
                if (GetPowerup("Retro") is RetroPort)
                {
                    RemovePowerup("Retro");
                    AddPowerup(typeof(RetroStasis));
                }
                else
                {
                    if (HasPowerup("Retro"))
                        RemovePowerup("Retro");
                    AddPowerup(typeof(RetroPort));
                }
            }
            else if (pressedThisFrame(Keys.O))
            {
                if (GetPowerup("Drill") is DrillFast && !(GetPowerup("Drill") is DrillBasic))
                {
                    RemovePowerup("Drill");
                    AddPowerup(typeof(DrillTriple));
                }
                else if (GetPowerup("Drill") is DrillTriple)
                {
                    RemovePowerup("Drill");
                    AddPowerup(typeof(DrillBasic));
                }
                else
                {
                    if (HasPowerup("Drill"))
                        RemovePowerup("Drill");
                    AddPowerup(typeof(DrillFast));
                }
            }
            else if (pressedThisFrame(Keys.P))
            {
                if (GetPowerup("Radar") is RadarPowerup)
                {
                    RemovePowerup("Radar");
                }
                else
                {
                    AddPowerup(typeof(RadarPowerup));
                }
            }
            else if (pressedThisFrame(Keys.B))
            {
                if (GetPowerup("Bomb") is BombTimed)
                {
                    RemovePowerup("Bomb");
                    AddPowerup(typeof(BombSet));
                }
                else
                {
                    if (HasPowerup("Bomb"))
                        RemovePowerup("Bomb");
                    AddPowerup(typeof(BombTimed));
                }
            }
            else if (pressedThisFrame(Keys.V))
            {
                if (playerIndex == 0)
                {
                    if (GetPowerup("Chains") is FireChains)
                    {
                        RemovePowerup("Chains");
                    }
                    else
                    {
                        AddPowerup(typeof(FireChains));
                    }
                }
            }
            else if (pressedThisFrame(Keys.OemOpenBrackets))
            {
                RetroGame.AddSand();
                RetroGame.AddScore(10000);
                RetroGame.AddBomb();
                for (int i = 0; i < 10; i++)
                    RetroGame.AddGem();
            }
            else if (pressedThisFrame(Keys.OemCloseBrackets))
            {
                health -= INITIAL_HEALTH * 0.1f;
            }
            else if (pressedThisFrame(Keys.H)) /*MUSICTEST*/
                SoundManager.PlaySoundAsMusic("LowRumble");
            else if (pressedThisFrame(Keys.N))
                SoundManager.StopMusic();
            else if (pressedThisFrame(Keys.J))
                SoundManager.SetMusicReverse(true);
            else if (pressedThisFrame(Keys.M))
                SoundManager.SetMusicReverse(false);
            else if (pressedThisFrame(Keys.K))
                SoundManager.SetMusicPitch(-1f);
            else if (pressedThisFrame(Keys.OemComma))
                SoundManager.SetMusicPitch(1f);
            else if (pressedThisFrame(Keys.OemPeriod))
                SoundManager.SetMusicPitch(0f);
        }

        public IMemento GenerateMementoFromCurrentFrame()
        {
            return new HeroMemento(this);
        }

        private class HeroMemento : IMemento
        {
            public Object Target { get; set; }
            public Vector2 position;
            float health;
            Direction direction;
            public float rotation;
            bool teleportedThisFrame;
            bool alive;

            public HeroMemento(Hero target)
            {
                Target = target;
                position = target.position;
                health = target.health;
                direction = target.direction;
                rotation = target.rotation;
                teleportedThisFrame = target.teleportedThisFrame;
                alive = target.Alive;
            }

            public void Apply(float interpolationFactor, bool isNewFrame, IMemento nextFrame)
            {
                Hero target = (Hero)Target;
                if (nextFrame != null)
                {
                    float thisInterp = 1 - interpolationFactor;
                    float nextInterp = interpolationFactor;
                    HeroMemento next = (HeroMemento)nextFrame;
                    if (teleportedThisFrame)
                        target.position = position;
                    else
                        target.position = position * thisInterp + next.position * nextInterp;
                }
                else
                {
                    target.position = position;
                }
                if (alive)
                {
                    if (!target.Alive)
                        target.revive();
                    target.health = health;
                }
                target.direction = direction;
                target.rotation = rotation;
            }
        }
    }
}
