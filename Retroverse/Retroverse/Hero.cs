using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        
        public static readonly float MOVE_SPEED = 400f;
        
        //***************Jon**************
        public int powerUp1; //, 0= Normal, 1=Bursts, 2= Fast, 3=Reverse
        public int powerUp2; //, 0= Normal, 1=Ghost, (2=Drill1, 3=Drill2)
        public int powerUp3; // -1=Normal, 0=Front, 1=Side, 2=Charge;        *****Formerly gunType*****
        //*****************************

        public Hero()
            : base(new Hitbox(32, 32))
        {
            position = new Vector2(Level.TEX_SIZE * LevelManager.STARTING_LEVEL.X + Level.TILE_SIZE / 2, Level.TEX_SIZE * LevelManager.STARTING_LEVEL.Y + Level.TILE_SIZE / 2);
            this.setTexture("hero");
            direction = Direction.Up;
            ammo = new List<Bullet>();
            instance = this;
            powerUp1 = 0;
            powerUp2 = 2;
            powerUp3 = 2;
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
            if (powerUp3 == -1)
            { }
            else if (powerUp3 == 0)
            {
                ammo.Add(new Bullet(direction));
                ammo.Last().position = new Vector2(this.position.X, this.position.Y);
            }
            else if (powerUp3 == 1)
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
                ammo.Add(new Bullet(dirLeft));
                ammo.Last().position = new Vector2(this.position.X, this.position.Y);
                ammo.Add(new Bullet(dirRight));
                ammo.Last().position = new Vector2(this.position.X, this.position.Y);
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

        public void special()
        {
            if (History.canRevert() != null)
            {
                History.revert();
                Game1.levelManager.scrollMultiplier = 3f;
            }
        }

        public void special2()
        {
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            bulletTimer += seconds;

            if (powerUp3 == 2)
            {
                if (fired)
                {
                    chargeTimer += seconds;
                }
                else if (!fired && chargeTimer > 0 && chargeTimer < 1)
                {
                    ammo.Add(new Bullet(direction));
                    ammo.Last().position = new Vector2(this.position.X, this.position.Y);
                    fired = false;
                    chargeTimer = 0;
                }
                else if (!fired && chargeTimer > 1 && chargeTimer < 2)
                {
                    ammo.Add(new Bullet(direction));
                    ammo.Last().scale = 2;
                    ammo.Last().hitbox.originalRectangle.Height = 32;
                    ammo.Last().hitbox.originalRectangle.Width = 32;
                    ammo.Last().position = new Vector2(this.position.X, this.position.Y);
                    fired = false;
                    chargeTimer = 0;
                }
                else if (!fired && chargeTimer > 2)
                {
                    ammo.Add(new Bullet(direction));
                    ammo.Last().scale = 3;
                    ammo.Last().hitbox.originalRectangle.Height = 64;
                    ammo.Last().hitbox.originalRectangle.Width = 64;
                    ammo.Last().position = new Vector2(this.position.X, this.position.Y);
                    fired = false;
                    chargeTimer = 0;
                }
            }
            Vector2 movement = Controller.dirVector * MOVE_SPEED * seconds;

           //*****************Arbitrary gaining powerups*********************

            if (Game1.levelManager.collectablesToRemove.Count > 5)
                powerUp1 = 1;
            if (Game1.levelManager.collectablesToRemove.Count > 15)
                powerUp2 = 2;
            if (Game1.levelManager.collectablesToRemove.Count > 25)
                powerUp3 = 0;

           //***************************************** 
            if (powerUp1 == 1)
            {
                int count = gameTime.TotalGameTime.Milliseconds / 300;
                if (count % 6 == 0)
                    movement *= 3;
            }
            else if (powerUp1 == 2)
                movement *= 2;
            else if (powerUp1 == 3)
                movement *= -1;

            int x = (int)position.X;
            int y = (int)position.Y;

            levelX = x / Level.TEX_SIZE; // get which level you are in
            levelY = y / Level.TEX_SIZE;
            Level level = Game1.levelManager.levels[levelX, levelY];

            tileX = (x % Level.TEX_SIZE) / Level.TILE_SIZE; // get which tile you are moving to
            tileY = (y % Level.TEX_SIZE) / Level.TILE_SIZE;
            LevelPipeline.LevelContent.LevelTile tile = level.grid[tileX, tileY];
            if (powerUp2 == 2 && tile.Equals(LevelPipeline.LevelContent.LevelTile.Black)) //drill
            {
                level.drillWall(tileX, tileY);
            } 

            //*************************************

            float nextX = position.X + movement.X;
            float nextY = position.Y + movement.Y;
            //Console.WriteLine(position.X+ " "+ position.Y);
            bool moved = true;
            int n;
            switch (Controller.direction)
            {
                case Direction.Up:
                    moved = canMove(new Vector2(0, movement.Y));
                    if (!moved)
                    {
                        n = (int)position.Y;
                        nextY = n + Level.TILE_SIZE / 2 - (n % Level.TILE_SIZE);
                    }
                    rotation = 0;
                    break;
                case Direction.Down:
                    moved = canMove(new Vector2(0, movement.Y));
                    if (!moved)
                    {
                        n = (int)position.Y;
                        nextY = n + Level.TILE_SIZE / 2 - (n % Level.TILE_SIZE);
                    }
                    rotation = (float)Math.PI;
                    break;
                case Direction.Left:
                    moved = canMove(new Vector2(movement.X, 0));
                    if (!moved)
                    {
                        n = (int)position.X;
                        nextX = n - (n % Level.TILE_SIZE) + Level.TILE_SIZE / 2;
                    }
                    rotation = (float)Math.PI * 3f / 2f;
                    break;
                case Direction.Right:
                    moved = canMove(new Vector2(movement.X, 0));
                    if (!moved)
                    {
                        n = (int)position.X;
                        nextX = n - (n % Level.TILE_SIZE) + Level.TILE_SIZE / 2;
                    }
                    rotation = (float)Math.PI / 2f;
                    break;
                default:
                    nextX = position.X;
                    nextY = position.Y;
                    break;
            }
            if (Controller.direction != Direction.None)
                direction = Controller.direction;
            position = new Vector2(nextX, nextY);
            // check corners
            if (moved &&
                (Game1.levelManager.collidesWithWall(new Vector2(getLeft().X, getTop().Y)) || //topleft
                Game1.levelManager.collidesWithWall(new Vector2(getLeft().X, getBottom().Y)) || //botleft
                Game1.levelManager.collidesWithWall(new Vector2(getRight().X, getBottom().Y)) || //botright
                Game1.levelManager.collidesWithWall(new Vector2(getRight().X, getTop().Y)))) //topright
            {
                if (powerUp2 == 1)
                {
                    //remove wall
                }
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
            fired = false;
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            History.Draw(spriteBatch);
            base.Draw(spriteBatch);
        }
    }
}
