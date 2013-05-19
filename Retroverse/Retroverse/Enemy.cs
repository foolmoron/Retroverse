using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;
using System.Diagnostics;

namespace Retroverse
{
    public class Enemy : Entity, IReversible
    {
        public const bool RAVE_PARTY_MODE = false && RetroGame.DEBUG;

        public const double CHANCE_TO_SPAWN_SAND_ON_DEATH = 0.025;
        public const int ENEMY_KILL_SCORE = 150;
        public const float STARTING_HEALTH = 5;
        public const float MOVE_SPEED = 150f;
        public const float ON_HIT_DAMAGE = 1;

        //powerup-specific fields
        public float globalMoveSpeedMultiplier = 1f;

        public Level level;
        public float health;
        public int type;
        private bool forceSandToSpawn;
        public static int idCounter = 0;
        public int id;

        public Direction prevDirection;
        public EnemyState state = EnemyState.Idle;

        public bool dying = false;
        public Emitter deathEmitter;

        public TargetedAI targetedAI;
        public AI idleAI;

        public static readonly Point[] aimPoints = { new Point(0, 0), new Point(0, 2), new Point(2, 0), new Point(0, -2) };
        public static readonly int TYPE_COUNT = aimPoints.Length;
        public Point aim, aimOffset;
        public Hero target;

        public HashSet<Point> visionLineNonWalls = new HashSet<Point>();
        public HashSet<Point> visionLineWalls = new HashSet<Point>();
        public Vector2 heroInVisionPos;

        public Enemy(int x, int y, int type, Level l, bool forceSandToSpawn = false)
            : base(new Vector2(Level.TEX_SIZE * l.xPos + (x * Level.TILE_SIZE) + Level.TILE_SIZE / 2, Level.TEX_SIZE * l.yPos + (y * Level.TILE_SIZE) + Level.TILE_SIZE / 2), new Hitbox(32, 32))
        {
            level = l;
            tileX = x;
            tileY = y;
            this.forceSandToSpawn = forceSandToSpawn;
            this.type = type;
            this.setTexture("enemy" + (type + 1));
            id = idCounter++;
            health = STARTING_HEALTH;
            scale = 0.5f;
            deathEmitter = Emitter.getPrebuiltEmitter(PrebuiltEmitter.EnemyDeathExplosion);
            idleAI = new TurnInPlaceAI((float)RetroGame.rand.NextDouble() * 2, RetroGame.rand.Next(5));
            targetedAI = new HeroChaseAI(true);
        }

        public override void Update(GameTime gameTime)
        {
            if (dying)
            {
                deathEmitter.position = position;
                deathEmitter.Update(gameTime);
            }
            else
            {
                AI ai = null;
                switch (state)
                {
                    case EnemyState.Idle:
                        ai = idleAI;
                        idleAI.Update(gameTime);
                        break;
                    case EnemyState.TargetingHero:
                        ai = targetedAI;
                        targetedAI.Update(gameTime);
                        break;
                }
                direction = ai.GetNextDirection(this);
                float moveSpeed = MOVE_SPEED * ai.GetNextMoveSpeedMultiplier(this);

                float seconds = gameTime.getSeconds((RetroGame.retroStatisActive) ? RetroGame.timeScale / 3 : RetroGame.timeScale);

                Vector2 movement = direction.toVector() * moveSpeed * globalMoveSpeedMultiplier * seconds;
                float nextX = position.X + movement.X;
                float nextY = position.Y + movement.Y;
                bool moved = true;
                int n;
                switch (direction)
                {
                    case Direction.Up:
                        moved = canMove(new Vector2(0, movement.Y));
                        if (!moved)
                        {
                            n = (int)position.Y;
                            nextY = n + Level.TILE_SIZE / 2 - (n % Level.TILE_SIZE);
                        }
                        rotation = (float)Math.PI;
                        break;
                    case Direction.Down:
                        moved = canMove(new Vector2(0, movement.Y));
                        if (!moved)
                        {
                            n = (int)position.Y;
                            nextY = n + Level.TILE_SIZE / 2 - (n % Level.TILE_SIZE);
                        }
                        rotation = 0;
                        break;
                    case Direction.Left:
                        moved = canMove(new Vector2(movement.X, 0));
                        if (!moved)
                        {
                            n = (int)position.X;
                            nextX = n - (n % Level.TILE_SIZE) + Level.TILE_SIZE / 2;
                        }
                        rotation = (float)Math.PI / 2f;
                        break;
                    case Direction.Right:
                        moved = canMove(new Vector2(movement.X, 0));
                        if (!moved)
                        {
                            n = (int)position.X;
                            nextX = n - (n % Level.TILE_SIZE) + Level.TILE_SIZE / 2;
                        }
                        rotation = (float)Math.PI * 3f / 2f;
                        break;
                    default:
                        nextX = position.X;
                        nextY = position.Y;
                        break;
                }

                // check corners
                if (moved &&
                    (level.collidesWithAnything(new Vector2(getLeft().X, getTop().Y), this) || //topleft
                    level.collidesWithAnything(new Vector2(getLeft().X, getBottom().Y), this) || //botleft
                    level.collidesWithAnything(new Vector2(getRight().X, getBottom().Y), this) || //botright
                    level.collidesWithAnything(new Vector2(getRight().X, getTop().Y), this))) //top right
                {
                    Direction directionToCorrect = (direction == Direction.None) ? prevDirection : direction;
                    switch (directionToCorrect)
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

                if (direction != Direction.None)
                    prevDirection = direction;

                position = new Vector2(nextX, nextY);

                level.enemyGrid[tileX, tileY] = null;
                updateCurrentLevelAndTile();
                level.enemyGrid[tileX, tileY] = this;
            }
            base.Update(gameTime);
            if (!dying)
            {
                foreach (Hero hero in RetroGame.getHeroes())
                    if (hero.Alive && hitbox.intersects(hero.hitbox))
                    {
                        hero.hitBy(this, ON_HIT_DAMAGE);
                        dieOnHero(hero);
                    }
                visionLineNonWalls.Clear();
                visionLineWalls.Clear();
                if (state == EnemyState.Idle)
                {
                    heroInVisionPos = Vector2.Zero;
                    searchForHeroes();
                }
            }

            maskingColor = Color.Lerp(Color.Black, Color.White, health / STARTING_HEALTH);
            globalMoveSpeedMultiplier = 1f;
        }

        public void searchForHeroes()
        {
            const float EPSILON = 0.001f;
            List<Hero> heroesInVision = new List<Hero>();
            visionLineNonWalls.Clear();
            visionLineWalls.Clear();
            foreach (Hero hero in RetroGame.getHeroes())
            {
                if (hero.Alive && hero.levelX == levelX && hero.levelY == levelY)
                {
                    Vector2 unitVectorFromEnemyToHero = hero.position - position;
                    unitVectorFromEnemyToHero.Normalize();
                    if (Math.Abs(unitVectorFromEnemyToHero.X) <= EPSILON) unitVectorFromEnemyToHero = Vector2.UnitY * Math.Sign(unitVectorFromEnemyToHero.Y);
                    else if ((1 - Math.Abs(unitVectorFromEnemyToHero.X)) <= EPSILON) unitVectorFromEnemyToHero = Vector2.UnitX * Math.Sign(unitVectorFromEnemyToHero.X);
                    if (Math.Abs(unitVectorFromEnemyToHero.Y) <= EPSILON) unitVectorFromEnemyToHero = Vector2.UnitX * Math.Sign(unitVectorFromEnemyToHero.X);
                    else if ((1 - Math.Abs(unitVectorFromEnemyToHero.Y)) <= EPSILON) unitVectorFromEnemyToHero = Vector2.UnitY * Math.Sign(unitVectorFromEnemyToHero.Y);

                    double angleToHero = unitVectorFromEnemyToHero.getAngleToHorizontal();
                    bool heroInCone = false;
                    switch (prevDirection)
                    {
                        case Direction.Up:
                            heroInCone = (angleToHero >= (5 * Math.PI / 4) && angleToHero <= (7 * Math.PI / 4));
                            break;
                        case Direction.Down:
                            heroInCone = (angleToHero >= (1 * Math.PI / 4) && angleToHero <= (3 * Math.PI / 4));
                            break;
                        case Direction.Left:
                            heroInCone = (angleToHero >= (3 * Math.PI / 4) && angleToHero <= (5 * Math.PI / 4));
                            break;
                        case Direction.Right:
                            heroInCone = (angleToHero >= (7 * Math.PI / 4) && angleToHero <= (8 * Math.PI / 4)) ||
                                         (angleToHero >= (0 * Math.PI / 4) && angleToHero <= (1 * Math.PI / 4));
                            break;
                    }
                    if (heroInCone)
                    {
                        //DDA algorithm in both X and Y directions to check all tiles in line of sight
                        bool canSeeHero = true;
                        const int step = Level.TILE_SIZE;
                        if (unitVectorFromEnemyToHero.X == 0)
                        {
                            int tX = (int)((position.X % Level.TEX_SIZE) / Level.TILE_SIZE);
                            int dir = Math.Sign(hero.position.Y - position.Y);
                            for (float currentY = position.Y; Math.Abs(hero.position.Y - currentY) >= step; currentY += step * dir)
                            {
                                int tY = (int)((currentY % Level.TEX_SIZE) / Level.TILE_SIZE);
                                if (level.grid[tX, tY] == LevelContent.LevelTile.Wall)
                                {
                                    canSeeHero = false;
                                    visionLineWalls.Add(new Point(tX, tY));
                                }
                                else
                                    visionLineNonWalls.Add(new Point(tX, tY));
                            }
                        }
                        else if (unitVectorFromEnemyToHero.Y == 0)
                        {
                            int tY = (int)((position.Y % Level.TEX_SIZE) / Level.TILE_SIZE);
                            int dir = Math.Sign(hero.position.X - position.X);
                            for (float currentX = position.X; Math.Abs(hero.position.X - currentX) >= step; currentX += step * dir)
                            {
                                int tX = (int)((currentX % Level.TEX_SIZE) / Level.TILE_SIZE);
                                if (level.grid[tX, tY] == LevelContent.LevelTile.Wall)
                                {
                                    canSeeHero = false;
                                    visionLineWalls.Add(new Point(tX, tY));
                                }
                                else
                                    visionLineNonWalls.Add(new Point(tX, tY));
                            }
                        }
                        else
                        {
                            float m = unitVectorFromEnemyToHero.Y / unitVectorFromEnemyToHero.X;
                            float intercept = ((position.Y % Level.TEX_SIZE) - (m * (position.X % Level.TEX_SIZE)));
                            int xDir = (hero.position.X - position.X >= 0) ? 1 : -1;
                            int yDir = (hero.position.Y - position.Y >= 0) ? 1 : -1;
                            int startX = (int)position.X % Level.TEX_SIZE;
                            int startY = (int)position.Y % Level.TEX_SIZE;
                            int endX = (int)hero.position.X % Level.TEX_SIZE;
                            int endY = (int)hero.position.Y % Level.TEX_SIZE;
                            int nextX = startX + (xDir * step) - (startX % step);
                            int nextY = startY + (yDir * step) - (startY % step);
                            float yk = (m * nextX) + intercept;
                            float xk = (nextY - intercept) / m;
                            for (int i = 0; (yDir == 1 && yk < endY) || (yDir == -1 && yk > endY); i++)
                            {
                                float x = nextX + (i * step * xDir);

                                int tX = (int)(x / Level.TILE_SIZE);
                                int tY = (int)(yk / Level.TILE_SIZE);

                                if (Level.tileWithinBounds(tX, tY))
                                    if (level.grid[tX, tY] == LevelContent.LevelTile.Wall)
                                    {
                                        canSeeHero = false;
                                        visionLineWalls.Add(new Point(tX, tY));
                                    }
                                    else
                                        visionLineNonWalls.Add(new Point(tX, tY));
                                if (Level.tileWithinBounds(tX - 1, tY))
                                    if (level.grid[tX - 1, tY] == LevelContent.LevelTile.Wall)
                                    {
                                        canSeeHero = false;
                                        visionLineWalls.Add(new Point(tX - 1, tY));
                                    }
                                    else
                                        visionLineNonWalls.Add(new Point(tX - 1, tY));

                                yk += Math.Abs(m * step) * yDir;
                            }
                            for (int i = 0; (xDir == 1 && xk < endX) || (xDir == -1 && xk > endX); i++)
                            {
                                float y = nextY + (i * step * yDir);

                                int tX = (int)(xk / Level.TILE_SIZE);
                                int tY = (int)(y / Level.TILE_SIZE);

                                if (Level.tileWithinBounds(tX, tY))
                                    if (level.grid[tX, tY] == LevelContent.LevelTile.Wall)
                                    {
                                        canSeeHero = false;
                                        visionLineWalls.Add(new Point(tX, tY));
                                    }
                                    else
                                        visionLineNonWalls.Add(new Point(tX, tY));
                                if (Level.tileWithinBounds(tX, tY - 1))
                                    if (level.grid[tX, tY - 1] == LevelContent.LevelTile.Wall)
                                    {
                                        canSeeHero = false;
                                        visionLineWalls.Add(new Point(tX, tY - 1));
                                    }
                                    else
                                        visionLineNonWalls.Add(new Point(tX, tY - 1));
                                xk += Math.Abs((1 / m) * step) * xDir;
                            }
                            //end DDA algorithm
                        }
                        if (canSeeHero)
                        {
                            heroesInVision.Add(hero);
                            heroInVisionPos = hero.position;
                        }
                    }
                }
            }
            if (heroesInVision.Count > 0)
            {
                heroesInVision.Sort(new Comparison<Hero>((hero1, hero2) =>
                {
                    float dist1 = Vector2.Distance(hero1.position, position);
                    float dist2 = Vector2.Distance(hero2.position, position);
                    return (int)(dist1 - dist2);
                }));
                state = EnemyState.TargetingHero;
                targetedAI.SetTarget(heroesInVision[0]);
            }
        }

        public void hitBy(Hero sourceOfDamage, float damage, bool aggro = true)
        {
            if (dying)
                return;
            health -= damage;
            if (health <= 0)
            {
                dieFromHero(sourceOfDamage);
                return;
            }

            if (aggro && sourceOfDamage != null)
            {
                state = EnemyState.TargetingHero;
                targetedAI.SetTarget(sourceOfDamage);
            }
        }

        private void die()
        {
            level.enemyGrid[tileX, tileY] = null;
            RetroGame.AddScore(ENEMY_KILL_SCORE);
            dying = true;
            double chance = RetroGame.rand.NextDouble();
            if (chance < CHANCE_TO_SPAWN_SAND_ON_DEATH || forceSandToSpawn)
            {
                int i = tileX;
                int j = tileY;
                level.collectables.Add(new Sand(Level.TEX_SIZE * level.xPos + i * Level.TILE_SIZE + 16, Level.TEX_SIZE * level.yPos + j * Level.TILE_SIZE + 16, level.xPos, level.yPos, i, j));
            }
        }

        public void dieOnHero(Hero hero)
        {
            hero.HitByEnemyCount++;
            die();
        }

        public void dieFromHero(Hero hero)
        {
            hero.KilledEnemyCount++;
            die();
            SoundManager.PlaySoundOnce("EnemyDeath", playInReverseDuringReverse: true);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (dying)
                deathEmitter.Draw(spriteBatch);
            else
            {
                base.Draw(spriteBatch);
                if (RAVE_PARTY_MODE)
                    DrawVisionDebug(spriteBatch);
            }
        }

        public void DrawVisionDebug(SpriteBatch spriteBatch)
        {
            foreach (Point point in visionLineNonWalls)
            {
                spriteBatch.Draw(RetroGame.PIXEL, new Rectangle(Level.TEX_SIZE * level.xPos + point.X * Level.TILE_SIZE, Level.TEX_SIZE * level.yPos + point.Y * Level.TILE_SIZE, Level.TILE_SIZE, Level.TILE_SIZE), Color.White.withAlpha(30));
            }
            foreach (Point point in visionLineWalls)
            {
                spriteBatch.Draw(RetroGame.PIXEL, new Rectangle(Level.TEX_SIZE * level.xPos + point.X * Level.TILE_SIZE, Level.TEX_SIZE * level.yPos + point.Y * Level.TILE_SIZE, Level.TILE_SIZE, Level.TILE_SIZE), Color.Red.withAlpha(30));
            }
            if (heroInVisionPos != Vector2.Zero)
            {
                int diagonalDistance = (int)Vector2.Distance(position, heroInVisionPos);
                float distX = position.X - heroInVisionPos.X;
                float distY = position.Y - heroInVisionPos.Y;
                float rotation = (float)Math.Atan(distY / distX);
                if (distX >= 0)
                    rotation += (float)Math.PI;

                Rectangle rec = new Rectangle((int)position.X, (int)(position.Y), diagonalDistance, 2);
                spriteBatch.Draw(RetroGame.PIXEL, rec, null, Color.White, rotation, new Vector2(0, 0), SpriteEffects.None, 0);
            }
        }

        public IMemento GenerateMementoFromCurrentFrame()
        {
            return new EnemyMemento(this);
        }

        private class EnemyMemento : IMemento
        {
            public Object Target { get; set; }
            EnemyState state;
            Entity AItarget;
            Vector2 position;
            float health;
            Direction direction;
            float rotation;
            bool dying;
            IMemento deathEmitterMemento;

            public EnemyMemento(Enemy target)
            {
                Target = target;
                state = target.state;
                AItarget = target.targetedAI.Target;
                position = target.position;
                health = target.health;
                direction = target.direction;
                rotation = target.rotation;
                dying = target.dying;
                deathEmitterMemento = target.deathEmitter.GenerateMementoFromCurrentFrame();
            }

            public void Apply(float interpolationFactor, bool isNewFrame, IMemento nextFrame)
            {
                Enemy target = (Enemy)Target;
                if (nextFrame != null)
                {
                    float thisInterp = 1 - interpolationFactor;
                    float nextInterp = interpolationFactor; 
                    EnemyMemento next = (EnemyMemento)nextFrame;
                    target.position = position * thisInterp + next.position * nextInterp;
                    deathEmitterMemento.Apply(interpolationFactor, isNewFrame, ((EnemyMemento) nextFrame).deathEmitterMemento);
                }
                else
                {
                    target.position = position;
                    deathEmitterMemento.Apply(interpolationFactor, isNewFrame, null);
                }
                target.state = state;
                target.targetedAI.Target = AItarget;
                target.health = health;
                target.direction = direction;
                target.rotation = rotation;
                target.dying = dying;
                target.maskingColor = Color.Lerp(Color.Black, Color.White, health / STARTING_HEALTH);
            }
        }
    }
}
