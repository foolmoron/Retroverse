using System.Collections.Generic;
using System.Linq;
using LevelPipeline;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Particles;

namespace Retroverse
{
	public class History
	{
		private static Queue<Queue<History>> qq = new Queue<Queue<History>>();
		public static float secsSinceLastRetroPort = 0;
		private static History retroHistory = null;
		private static Point lastLevel = new Point(-1, -1);
        private static GameTime currentGameTime;
        public static GameState lastState = GameState.Arena;

		// retroport values
		public static float RETROPORT_BASE_SECS = 2.5f;
        public static float retroportSecs = RETROPORT_BASE_SECS;
		private static bool cancel = false;

		// retroport reverting effect values
		public static List<int> queueDict = new List<int>();
		public static int[] queueIndices;
		public static int retroportFrames = 0;
		public static readonly float FRAME_VELOCITY_MIN = 10;
		public static readonly float FRAME_VELOCITY_MAX = 45;
		public static float frameVelocity = 0; //historical frames per second to rewind
		public static float frame = 0;
		public static int prevFrame = -1;
		public static float secsInRetroPort = 0;

		// retroport intro/outro effect values
		public static Boolean effectFinished = true;
        public static float EFFECT_FINISHED_RADIUS;
        private static float EFFECT_OUTRO_SPEEDUP_RADIUS;
        private static float effectOutroModifier = 1f;
		public static readonly float effectIntroVelocity = 2700f;
		public static readonly float effectOutroVelocity = 900f;

		// grayscale effect values
		public static float EFFECT_RADIUS_MAX;
		public static float EFFECT_RADIUS_MIN = 50;
		public static float effectRadius = float.PositiveInfinity;
		public static float effectPerc;
		public static float effectIntensity = 5f;
		public static readonly Func<float, float> effectFunc = (perc) =>
		{
            return perc;
		};
		public static readonly float EFFECT_FUNC_MAX = 0.0625f;

		private HeroHistory heroState;
		private LevelHistory levelState;
        private EnemyHistory enemyState;
        private BulletHistory bulletState;
        private PowerupHistory powerupState;

		private History() { }

		public static void UpdateArena(GameTime gameTime)
		{
            UpdateEscape(gameTime);
            qq.Last().Last().powerupState = new PowerupHistory();
		}

		public static void UpdateEscape(GameTime gameTime)
		{
			float seconds = gameTime.getSeconds();

			int heroX = Hero.instance.levelX;
			int heroY = Hero.instance.levelY;
			History h = new History();
			h.heroState = new HeroHistory();
			h.levelState = new LevelHistory(Game1.levelManager.levels[heroX, heroY]);
			h.enemyState = new EnemyHistory();
			h.bulletState = new BulletHistory();

			Point newLevel = new Point(heroX, heroY);
			if (lastLevel != newLevel)
				qq.Enqueue(new Queue<History>());
			lastLevel = newLevel;
            qq.Last().Enqueue(h);
			secsSinceLastRetroPort += seconds;
            if (secsSinceLastRetroPort >= retroportSecs)
			{
				if (qq.First().Count == 0)
					qq.Dequeue();
				retroHistory = qq.First().Dequeue();
			}

			if (effectFinished)
                effectRadius = Game1.screenSize.Y * 2f * Game1.levelManager.zoom;
			else
			{
                EFFECT_FINISHED_RADIUS = Game1.screenSize.Y * 3f * Game1.levelManager.zoom;
                EFFECT_OUTRO_SPEEDUP_RADIUS = Game1.screenSize.Y * Game1.levelManager.zoom;
                if (effectRadius < EFFECT_FINISHED_RADIUS)
                {
                    if (effectRadius >= EFFECT_OUTRO_SPEEDUP_RADIUS)
                        effectOutroModifier = 3f;
                    Game1.drawEffects = true;
                    Game1.currentEffect = Effects.OuterGrayscale;
                    effectRadius += effectOutroVelocity * effectOutroModifier * seconds;
                    Game1.currentEffect.Parameters["width"].SetValue(Game1.screenSize.X);
                    Game1.currentEffect.Parameters["height"].SetValue(Game1.screenSize.Y);
                    Game1.currentEffect.Parameters["radius"].SetValue(effectRadius);
                    Game1.currentEffect.Parameters["intensity"].SetValue(effectIntensity);
                    Game1.currentEffect.Parameters["zoom"].SetValue(Game1.levelManager.zoom);
                    Game1.currentEffect.Parameters["center"].SetValue(Game1.levelManager.center);
                }
                else
                {
                    effectFinished = true;
                }
			}

		}

		public static void UpdateRetro(GameTime gameTime)
		{
            currentGameTime = gameTime;
			float seconds = gameTime.getSeconds();

			if (effectRadius > EFFECT_RADIUS_MIN && effectFinished)
			{
				Game1.drawEffects = true;
				Game1.currentEffect = Effects.OuterGrayscale;
				effectRadius -= effectIntroVelocity * seconds;
				if (effectRadius < EFFECT_RADIUS_MIN)
				{
					effectRadius = EFFECT_RADIUS_MIN;
					effectFinished = false;
                }
				Game1.currentEffect.Parameters["width"].SetValue(Game1.screenSize.X);
				Game1.currentEffect.Parameters["height"].SetValue(Game1.screenSize.Y);
				Game1.currentEffect.Parameters["radius"].SetValue(effectRadius);
                Game1.currentEffect.Parameters["intensity"].SetValue(effectIntensity);
                Game1.currentEffect.Parameters["zoom"].SetValue(Game1.levelManager.zoom);
                Game1.currentEffect.Parameters["center"].SetValue(Game1.levelManager.center);
				return;
			}

			if (retroportFrames == 0)
			{
				int i = 0;
				int j = 0;
				queueIndices = new int[qq.Count];
				foreach (Queue<History> q in qq)
				{
					queueIndices[j] = retroportFrames;
					retroportFrames += q.Count;
					while (i < retroportFrames)
					{
						queueDict.Add(j);
						i++;
					}
					j++;
				}
			}

			float framesPerc = frame / retroportFrames;
			float perc2 = 2 * framesPerc;
			if (perc2 < 1)
				frameVelocity = FRAME_VELOCITY_MIN * (1 - perc2) + FRAME_VELOCITY_MAX * perc2;
			else
			{
				perc2 -= 1;
				frameVelocity = FRAME_VELOCITY_MIN * perc2 + FRAME_VELOCITY_MAX * (1 - perc2);
			}
			frame += frameVelocity * seconds;

			Game1.drawEffects = true;
			Game1.currentEffect = Effects.OuterGrayscale;
			effectRadius = EFFECT_RADIUS_MAX * effectFunc(framesPerc) + EFFECT_RADIUS_MIN;
			Game1.currentEffect.Parameters["width"].SetValue(Game1.screenSize.X);
			Game1.currentEffect.Parameters["height"].SetValue(Game1.screenSize.Y);
            Game1.currentEffect.Parameters["radius"].SetValue(effectRadius);
            Game1.currentEffect.Parameters["intensity"].SetValue(effectIntensity);
            Game1.currentEffect.Parameters["zoom"].SetValue(Game1.levelManager.zoom);
            Game1.currentEffect.Parameters["center"].SetValue(Game1.levelManager.center);

			int iframe = retroportFrames - (int)frame - 1;
			float interpolation = frame - (int)frame;
			if (iframe >= 0)
			{
				int queue = queueDict[iframe];
				History currentFrame = qq.ElementAt(queue).ElementAt(iframe - queueIndices[queue]);
				History nextFrame = null;
				if (iframe > 0)
				{
					int queue2 = queueDict[iframe - 1];
					nextFrame = qq.ElementAt(queue2).ElementAt((iframe - 1) - queueIndices[queue2]);
				}
				currentFrame.heroState.apply(interpolation, nextFrame);
				currentFrame.levelState.apply(interpolation, nextFrame);
                currentFrame.enemyState.apply(interpolation, nextFrame);
                currentFrame.bulletState.apply(interpolation, nextFrame);
                if (currentFrame.powerupState != null)
                    currentFrame.powerupState.apply(interpolation, nextFrame);
				prevFrame = iframe;
			}
			if (iframe < 0 || cancel)
			{
                clearFrames();
			}
		}

		public static void DrawHero(SpriteBatch spriteBatch)
		{
			if (retroHistory != null)
			{
				HeroHistory hHero = retroHistory.heroState;
				spriteBatch.Draw(hHero.tex, hHero.position, null, Color.White * 0.5f, hHero.rotation, new Vector2(hHero.tex.Width / 2, hHero.tex.Height / 2), Hero.instance.scale, SpriteEffects.None, 0);
			}
		}

		public static void DrawLevel(SpriteBatch spriteBatch, int xPos, int yPos)
		{
			if (retroHistory != null)
			{
				foreach (Queue<History> q in qq)
				{
					if (q.Count == 0)
						continue;
					LevelHistory lh = q.First().levelState;
					if (xPos == lh.x && yPos == lh.y)
					{
						spriteBatch.Draw(lh.getTexture(), new Vector2(lh.x * Level.TEX_SIZE, lh.y * Level.TEX_SIZE), null, Color.White * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
						break;
					}
				}
			}
		}

		public static void DrawEnemies(SpriteBatch spriteBatch)
		{
            if (retroHistory != null && qq.First().Count > 0)
            {
                EnemyHistory hEnemies = qq.First().First().enemyState;
                foreach (LevelEnemyHistory leh in hEnemies.levelHistories)
                    if (leh != null && leh.target.alive)
                        foreach (IndividualEnemyHistory hEnemy in leh.histories)
                        {
                            spriteBatch.Draw(hEnemy.tex, hEnemy.position, null, Color.White * 0.5f, hEnemy.rotation, new Vector2(hEnemy.tex.Width / 2, hEnemy.tex.Height / 2), 0.5f, SpriteEffects.None, 0);
                        }
			}
		}

		public static void setEffectRadiusMax()
		{
			EFFECT_RADIUS_MAX = Math.Max(Game1.screenSize.X, Game1.screenSize.Y) / 4;
		}

		public static bool canRevert()
		{
			return retroHistory != null;
		}

		public static void cancelRevert()
		{
			if (!effectFinished)
				cancel = true;
		}

        public static void clearFrames()
        {
            qq.Clear();
            queueDict.Clear();
            queueIndices = null;
            retroportFrames = 0;
            frame = 0;
            prevFrame = -1;
            secsInRetroPort = 0;
            frameVelocity = 0;
            lastLevel = new Point(-1, -1);
            secsSinceLastRetroPort = 0;
            retroHistory = null;
            cancel = false;
            Game1.levelManager.scrollMultiplier = 3f;
            Game1.state = lastState;
            Game1.drawEffects = false;
            RiotGuardWall.setReverse(false);
        }

		private class HeroHistory
		{
			public Vector2 position;
			public Texture2D tex;
			public int texFrame;
			public float rotation;
            public Direction dir;

            public Vector2 leftBoosterIdlePosition;
            public float leftBoosterIdleAngle;
            public float leftBoosterIdleLength;
            public Vector2 rightBoosterIdlePosition;
            public float rightBoosterIdleAngle;
            public float rightBoosterIdleLength;

            public Vector2 leftBoosterFiringPosition;
            public float leftBoosterFiringAngle;
            public float leftBoosterFiringLength;
            public Vector2 rightBoosterFiringPosition;
            public float rightBoosterFiringAngle;
            public float rightBoosterFiringLength;

            public Vector2 chargeEmitterPosition;
            public float chargeEmitterStartSize;
            public Color chargeEmitterStartColor;
            public Color chargeEmitterEndColor;
            public float chargeTimer;

            public EmitterHistory drill;
            public EmitterHistory drillLeft;
            public EmitterHistory drillRight;
            public float drillParticleSize;
            public float drillingTime;

			public HeroHistory()
			{
				position = Hero.instance.position;
				tex = Hero.instance.getTexture();
				texFrame = Hero.instance.getTextureFrame();
				rotation = Hero.instance.rotation;
                dir = Hero.instance.direction;
                leftBoosterIdlePosition = Hero.instance.leftBoosterIdle.position;
                leftBoosterIdleAngle = Hero.instance.leftBoosterIdle.angle;
                leftBoosterIdleLength = Hero.instance.leftBoosterIdle.valueToDeath;
                rightBoosterIdlePosition = Hero.instance.rightBoosterIdle.position;
                rightBoosterIdleAngle = Hero.instance.rightBoosterIdle.angle;
                rightBoosterIdleLength = Hero.instance.rightBoosterIdle.valueToDeath;
                leftBoosterFiringPosition = Hero.instance.leftBoosterFiring.position;
                leftBoosterFiringAngle = Hero.instance.leftBoosterFiring.angle;
                leftBoosterFiringLength = Hero.instance.leftBoosterFiring.valueToDeath;
                rightBoosterFiringPosition = Hero.instance.rightBoosterFiring.position;
                rightBoosterFiringAngle = Hero.instance.rightBoosterFiring.angle;
                rightBoosterFiringLength = Hero.instance.rightBoosterFiring.valueToDeath;
                chargeEmitterPosition = Hero.instance.chargeEmitter.position;
                chargeEmitterStartSize = Hero.instance.chargeEmitter.startSize;
                chargeEmitterStartColor = Hero.instance.chargeEmitter.startColor;
                chargeEmitterEndColor = Hero.instance.chargeEmitter.endColor;
                chargeTimer = Hero.instance.chargeTimer;
                drill = new EmitterHistory(Hero.instance.drillEmitter);
                drillLeft = new EmitterHistory(Hero.instance.drillEmitterLeft);
                drillRight = new EmitterHistory(Hero.instance.drillEmitterRight);
                drillParticleSize = Hero.instance.drillingRatio;
                drillingTime = Hero.instance.drillingTime;
			}

			public void apply(float interp, History nextFrame)
			{
                if (nextFrame == null)
                {
                    Hero.instance.position = position;
                    Hero.instance.rotation = rotation;
                    Hero.instance.direction = dir;
                    Hero.instance.leftBoosterIdle.position = leftBoosterIdlePosition;
                    Hero.instance.rightBoosterIdle.position = rightBoosterIdlePosition;
                    Hero.instance.leftBoosterFiring.position = leftBoosterFiringPosition;
                    Hero.instance.rightBoosterFiring.position = rightBoosterFiringPosition;
                    Hero.instance.chargeTimer = chargeTimer;
                }
                else
                {
                    float thisInterp = 1 - interp;
                    Hero.instance.position = position * thisInterp + nextFrame.heroState.position * interp;
                    Hero.instance.rotation = rotation;
                    Hero.instance.direction = dir;
                    Hero.instance.leftBoosterIdle.position = leftBoosterIdlePosition * thisInterp + nextFrame.heroState.leftBoosterIdlePosition * interp;
                    Hero.instance.rightBoosterIdle.position = rightBoosterIdlePosition * thisInterp + nextFrame.heroState.rightBoosterIdlePosition * interp;
                    Hero.instance.leftBoosterFiring.position = leftBoosterFiringPosition * thisInterp + nextFrame.heroState.leftBoosterFiringPosition * interp;
                    Hero.instance.rightBoosterFiring.position = rightBoosterFiringPosition * thisInterp + nextFrame.heroState.rightBoosterFiringPosition * interp;
                    Hero.instance.chargeTimer = chargeTimer * thisInterp + nextFrame.heroState.chargeTimer * interp;
                }
                Hero.instance.leftBoosterIdle.angle = leftBoosterIdleAngle;
                Hero.instance.leftBoosterIdle.valueToDeath = leftBoosterIdleLength;
                Hero.instance.rightBoosterIdle.angle = rightBoosterIdleAngle;
                Hero.instance.rightBoosterIdle.valueToDeath = rightBoosterIdleLength;
                Hero.instance.leftBoosterIdle.Update(currentGameTime);
                Hero.instance.rightBoosterIdle.Update(currentGameTime);
                Hero.instance.leftBoosterFiring.angle = leftBoosterFiringAngle;
                Hero.instance.leftBoosterFiring.valueToDeath = leftBoosterFiringLength;
                Hero.instance.rightBoosterFiring.angle = rightBoosterFiringAngle;
                Hero.instance.rightBoosterFiring.valueToDeath = rightBoosterFiringLength;
                Hero.instance.leftBoosterFiring.Update(currentGameTime);
                Hero.instance.rightBoosterFiring.Update(currentGameTime);


                Hero.instance.chargeEmitter.position = chargeEmitterPosition;
                Hero.instance.chargeEmitter.startSize = chargeEmitterStartSize;
                Hero.instance.chargeEmitter.startColor = chargeEmitterStartColor;
                Hero.instance.chargeEmitter.endColor = chargeEmitterEndColor;
                Hero.instance.chargeEmitter.Update(currentGameTime);

                Hero.instance.drillEmitter.active = drill.emitterActive;
                Hero.instance.drillEmitterLeft.active = drillLeft.emitterActive;
                Hero.instance.drillEmitterRight.active = drillRight.emitterActive;
                Hero.instance.drillEmitter.position = drill.emitterPosition;
                Hero.instance.drillEmitterLeft.position = drillLeft.emitterPosition;
                Hero.instance.drillEmitterRight.position = drillRight.emitterPosition;
                Hero.instance.drillEmitter.startSize = drillParticleSize;
                Hero.instance.drillEmitterLeft.startSize = drillParticleSize;
                Hero.instance.drillEmitterRight.startSize = drillParticleSize;
                Hero.instance.drillEmitter.Update(currentGameTime);
                Hero.instance.drillEmitterLeft.Update(currentGameTime);
                Hero.instance.drillEmitterRight.Update(currentGameTime);
                Hero.instance.drillingTime = drillingTime;
			}
		}

		private class LevelHistory
		{
			public Level target;
			public int x, y;
			public LevelContent.LevelTile[,] grid;
			public Texture2D tex;

			public LevelHistory(Level l)
			{
				target = l;
				x = l.xPos;
				y = l.yPos;
				grid = (LevelContent.LevelTile[,])l.grid.Clone();
			}

            public void apply(float interp, History nextFrame)
            {
                for (int i = 0; i < LevelContent.LEVEL_SIZE; i++)
                    for (int j = 0; j < LevelContent.LEVEL_SIZE; j++)
                        if (target.grid[i, j] != grid[i, j])
                            if (grid[i, j] == LevelContent.LevelTile.Black)
                            {
                                target.levelTexture = getTexture();
                                break;
                            }
                target.grid = grid;
            }

			public Texture2D getTexture()
			{
				if (tex == null)
				{
					tex = new Texture2D(Game1.graphicsDevice, Level.TEX_SIZE, Level.TEX_SIZE);
					Color[] tiledata = new Color[Level.TILE_SIZE * Level.TILE_SIZE];
					for (int i = 0; i < LevelContent.LEVEL_SIZE; i++)
						for (int j = 0; j < LevelContent.LEVEL_SIZE; j++)
						{
							if (Level.TILE_TO_TEXTURE[grid[i, j]] == null)
								continue;
							Level.TILE_TO_TEXTURE[grid[i, j]].GetData<Color>(tiledata);
							tex.SetData<Color>(0, new Rectangle(i * Level.TILE_SIZE, j * Level.TILE_SIZE, Level.TILE_SIZE, Level.TILE_SIZE), tiledata, 0, Level.TILE_SIZE * Level.TILE_SIZE);
						}
				}
				return tex;
			}
		}

		private class EnemyHistory
		{
			public LevelEnemyHistory[,] levelHistories = new LevelEnemyHistory[3, 3];

			public EnemyHistory()
			{
				for (int i = -1; i <= 1; i++)
					for (int j = -1; j <= 1; j++)
					{
                        int x = Hero.instance.levelX + i;
                        int y = Hero.instance.levelY + j;
                        if (x < 0 || x >= LevelManager.MAX_LEVELS || y < 0 || y >= LevelManager.MAX_LEVELS)
                            continue;
						levelHistories[i + 1, j + 1] = new LevelEnemyHistory(Game1.levelManager.levels[Hero.instance.levelX + i, Hero.instance.levelY + j]);
					}
			}

			public void apply(float interp, History nextFrame)
			{
				for (int i = -1; i <= 1; i++)
					for (int j = -1; j <= 1; j++)
					{
						LevelEnemyHistory h = levelHistories[i + 1, j + 1];
						if (h != null && h.target.alive)
						{
							LevelEnemyHistory nextLEFrame = null;
                            if (nextFrame != null)
                            {
                                LevelEnemyHistory frame = nextFrame.enemyState.levelHistories[i + 1, j + 1];
                                if (frame != null && frame.target == h.target)
                                    nextLEFrame = nextFrame.enemyState.levelHistories[i + 1, j + 1];
                                else
                                    for (int ii = -1; ii <= 1; ii++)
                                        for (int jj = -1; jj <= 1; jj++)
                                        {
                                            frame = nextFrame.enemyState.levelHistories[ii + 1, jj + 1];
                                            if (frame != null && frame.target == h.target)
                                                nextLEFrame = frame;
                                        }
                            }
							levelHistories[i + 1, j + 1].apply(interp, nextLEFrame);
						}
					}
			}
		}

		private class LevelEnemyHistory
		{
			public Level target;
			public int x, y;
			public IndividualEnemyHistory[] histories;

			public LevelEnemyHistory(Level l)
			{
				target = l;
				x = l.xPos;
				y = l.yPos;
				histories = new IndividualEnemyHistory[l.enemies.Count];
				int i = 0;
				foreach (Enemy e in l.enemies)
				{
					IndividualEnemyHistory h = new IndividualEnemyHistory(e, e.position, e.hp, e.getTexture(), e.rotation, e.getTextureFrame(), e.direction);
					histories[i++] = h;
				}
			}

			public void apply()
			{
				if (histories.Length > 0)
				target.enemies.Clear();
				foreach (IndividualEnemyHistory h in histories)
				{
                    Enemy e = new Enemy((int)h.target.roundPosition.X, (int)h.target.roundPosition.Y, h.target.type, target, h.position, h.hp, h.dir, h.texFrame, h.emitterHistory, h.dying);
                    e.emitter.position = e.position;
                    e.emitter.Update(currentGameTime);
                    target.enemies.Add(e); 
				}
			}

			public void apply(float interp, LevelEnemyHistory nextFrame)
			{
				if (nextFrame == null)
					apply();
				else if (histories.Length != nextFrame.histories.Length)
				{
                    if (target.xPos == nextFrame.target.xPos && target.yPos == nextFrame.target.yPos)
                    {
                        target.enemies.Clear();
                        float thisInterp = 1 - interp;
                        for (int i = 0; i < histories.Length; i++)
                        {
                            IndividualEnemyHistory h1 = histories[i];
                            IndividualEnemyHistory h2 = null;
                            for (int j = 0; j < nextFrame.histories.Length; j++)
                            {
                                if (h1.target == nextFrame.histories[j].target)
                                {
                                    h2 = nextFrame.histories[j];
                                    break;
                                }
                            }
                            if (h2 == null)
                                continue;
                            Enemy e = new Enemy((int)h1.target.roundPosition.X, (int)h1.target.roundPosition.Y, h1.target.type, target, h1.position * thisInterp + h2.position * interp, h1.hp, h1.dir, h1.texFrame, h1.emitterHistory, h1.dying);
                            e.emitter.position = e.position;
                            e.emitter.Update(currentGameTime);
                            target.enemies.Add(e);
                        }
                    }
                    else
                        throw new Exception("hey");
				}
				else
				{
					float thisInterp = 1 - interp;
					if (nextFrame.x != x || nextFrame.y != y)
					{
						apply();
					}
					else
					{
						target.enemies.Clear();
						for (int i = 0; i < histories.Length; i++)
						{
							IndividualEnemyHistory h1 = histories[i];
							IndividualEnemyHistory h2 = nextFrame.histories[i];
                            Enemy e = new Enemy((int)h1.target.roundPosition.X, (int)h1.target.roundPosition.Y, h1.target.type, target, h1.position * thisInterp + h2.position * interp, h1.hp, h1.dir, h1.texFrame, h1.emitterHistory, h1.dying);
                            e.emitter.position = e.position;
                            e.emitter.Update(currentGameTime);
							target.enemies.Add(e); 
						}
					}
				}
			}
		}

		private class IndividualEnemyHistory
		{
			public Enemy target;
			public Vector2 position;
			public int hp;
			public Texture2D tex;
			public float rotation;
			public int texFrame;
            public Direction dir;
            public EmitterHistory emitterHistory;
            public bool dying;

			public IndividualEnemyHistory(Enemy e, Vector2 pos, int health, Texture2D t, float rot, int f, Direction d)
			{
				target = e;
				position = pos;
				hp = health;
				tex = t;
				rotation = rot;
				texFrame = f;
				dir = d;
                emitterHistory = new EmitterHistory(e.emitter);
                dying = e.dying;
			}
		}

		private class BulletHistory
		{
			public IndividualBulletHistory[] histories;

			public BulletHistory()
			{
				histories = new IndividualBulletHistory[Hero.instance.ammo.Count];
				for (int i = 0; i < Hero.instance.ammo.Count; i++)
				{
					histories[i] = new IndividualBulletHistory(Hero.instance.ammo[i]);
				}
			}

			public void apply()
			{
				Hero.instance.ammo.Clear();
				foreach (IndividualBulletHistory h in histories)
				{
                    Bullet b = new Bullet(h.textureName, h.hitbox, h.damage, h.position, h.velocity, h.rotation, h.scale, h.phasing, h.texFrame, h.trailHistory, h.explosionHistory, h.dying);
                    b.trailEmitter.position = b.position;
                    b.explosionEmitter.position = b.position;
                    b.trailEmitter.Update(currentGameTime);
                    b.explosionEmitter.Update(currentGameTime);
					Hero.instance.ammo.Add(b);
				}
			}

			public void apply(float interp, History nextFrame)
			{
				if (nextFrame == null)
					apply();
				else if (histories.Length != nextFrame.bulletState.histories.Length)
				{
					Hero.instance.ammo.Clear();
					float thisInterp = 1 - interp;
					for (int i = 0; i < histories.Length; i++)
					{
						IndividualBulletHistory h1 = histories[i];
						IndividualBulletHistory h2 = null;
						for (int j = 0; j < nextFrame.bulletState.histories.Length; j++)
						{
							if (h1.target == nextFrame.bulletState.histories[j].target)
							{
								h2 = nextFrame.bulletState.histories[j];
								break;
							}
						}
						if (h2 == null)
							continue;
                        Bullet b = new Bullet(h1.textureName, h1.hitbox, h1.damage, h1.position * thisInterp + h2.position * interp, h1.velocity, h1.rotation, h1.scale, h1.phasing, h1.texFrame, h1.trailHistory, h1.explosionHistory, h1.dying);
                        b.trailEmitter.position = b.position;
                        b.explosionEmitter.position = b.position;
                        b.trailEmitter.Update(currentGameTime);
                        b.explosionEmitter.Update(currentGameTime);
						Hero.instance.ammo.Add(b);

					}
				}
				else
				{
					Hero.instance.ammo.Clear();
					float thisInterp = 1 - interp;
					for (int i = 0; i < histories.Length; i++)
					{
						IndividualBulletHistory h1 = histories[i];
						IndividualBulletHistory h2 = nextFrame.bulletState.histories[i];
                        Bullet b = new Bullet(h1.textureName, h1.hitbox, h1.damage, h1.position * thisInterp + h2.position * interp, h1.velocity, h1.rotation, h1.scale, h1.phasing, h1.texFrame, h1.trailHistory, h1.explosionHistory, h1.dying);
                        b.trailEmitter.position = b.position;
                        b.explosionEmitter.position = b.position;
                        b.trailEmitter.Update(currentGameTime);
                        b.explosionEmitter.Update(currentGameTime);
						Hero.instance.ammo.Add(b);
					}
				}
			}
		}

		private class IndividualBulletHistory
		{
			public Bullet target;
            public string textureName;
			public Vector2 position;
            public Vector2 velocity;
            public float rotation;
            public float scale;
            public bool phasing;
			public Hitbox hitbox;
			public int damage;
            public int texFrame;
            public EmitterHistory trailHistory;
            public EmitterHistory explosionHistory;
            public bool dying;

			public IndividualBulletHistory(Bullet b)
			{
				target = b;
                textureName = b.textureName;
				position = b.position;
				velocity = b.velocity;
                rotation = b.rotation;
                scale = b.scale;
                phasing = b.phasing;
				hitbox = b.hitbox;
				damage = b.damage;
                texFrame = b.getTextureFrame();
                trailHistory = new EmitterHistory(b.trailEmitter);
                explosionHistory = new EmitterHistory(b.explosionEmitter);
                dying = b.dying;
			}
		}

        public class EmitterHistory
        {
            public Emitter emitter;
            public Vector2 emitterPosition;
            public int emitCount;
            public bool emitterActive;

            public EmitterHistory(Emitter e)
            {
                this.emitter = e;
                this.emitterPosition = e.position;
                this.emitCount = e.particlesEmitted;
                this.emitterActive = e.active;
            }
        }

        public class PowerupHistory
        {			
            public IndividualPowerupHistory[] histories;

            public PowerupHistory()
			{
                histories = new IndividualPowerupHistory[Powerups.powerups.Count()];
                for (int i = 0; i < Powerups.powerups.Count; i++)
				{
                    histories[i] = new IndividualPowerupHistory(Powerups.powerups[i]);
				}
			}
            
            public void apply(float interp, History nextFrame)
            {
                if (nextFrame == null)
                {
                    foreach (IndividualPowerupHistory h in histories)
                    {
                        h.target.dying = h.dying;
                        if (!h.dying)
                        {
                            Powerups.Powerup p = h.target;
                            p.position = h.position;
                            p.direction = h.direction;
                            p.sequenceIndex = h.sequenceIndex;
                        }
                    }
                }
                else
                {
                    float thisInterp = 1 - interp;
                    for (int i = 0; i < histories.Length; i++)
                    {
                        IndividualPowerupHistory h1 = histories[i];
                        IndividualPowerupHistory h2 = nextFrame.powerupState.histories[i];
                        if (h1 == null || h2 == null)
                            continue;
                        h1.target.dying = h1.dying;
                        if (!h1.dying && !h2.dying)
                        {
                            Powerups.Powerup p = h1.target;
                            p.position = h1.position * thisInterp + h2.position * interp;
                            p.direction = h1.direction;
                            p.sequenceIndex = h1.sequenceIndex;
                        }
                    }
                }
            }
        }

        public class IndividualPowerupHistory
        {
            public Powerups.Powerup target;
			public Vector2 position;
            public Direction direction;
            public int sequenceIndex;
            public bool dying;

            public IndividualPowerupHistory(Powerups.Powerup p)
			{
				target = p;
				position = p.position;
                direction = p.direction;
                sequenceIndex = p.sequenceIndex;
                dying = p.dying;
			}
        }
	}
}
