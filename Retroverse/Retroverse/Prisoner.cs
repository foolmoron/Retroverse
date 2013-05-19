using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Particles;
using Microsoft.Xna.Framework;
using System.Collections;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public class Prisoner : Collectable
    {
        public static readonly int MAX_IDS = 9999;
        public static BitArray TAKEN_IDS = new BitArray(MAX_IDS, false);
        public static int idsGiven = 0;
        public string name;
        public string id;
        public static readonly int PRISONER_SCORE = 1600;
        public int powerUp1; //, 0= Normal, 1=Bursts, 2= Fast, 3=Reverse
        public int powerUp2; //, 0= Normal, 1=Ghost, (2=Drill1, 3=Drill2)
        public int powerUp3; // -1=Normal, 0=Front, 1=Side, 2=Charge
        public static readonly float EXCLAMATION_DURATION = 3f;

        public static readonly string HELP_STRING = "HELP!";
        public bool drawHelp = false;
        public float flashingTime;
        public int flashingIndex = 0;
        public const int FLASHING_SEQUENCE_ELEMENTS = 8;
        public static float[] flashingDelays = new float[FLASHING_SEQUENCE_ELEMENTS] { 0.2f, //flash on after 0.1 secs
                                                                                0.2f, //flash off
                                                                                0.2f, //flash on
                                                                                0.6f, //flash off
                                                                                0.2f, //flash on
                                                                                0.2f, //flash off
                                                                                0.2f, //flash on
                                                                                1.0f, //flash off after 0.1 secs
                                                                              };

        public static readonly float TIME_PER_TURN = 1f;
        public float timeSinceLastTurn = (float)RetroGame.rand.NextDouble() * TIME_PER_TURN;
        static Prisoner()
        {
            TAKEN_IDS[0] = true;
            if (MAX_IDS > 1337)
                TAKEN_IDS[1337] = true;
        }

        public Prisoner(Color color, string name, int x, int y, int levelX, int levelY, int tileX, int tileY):
            base(x, y, levelX, levelY, tileX, tileY)
        {
            CollectedSound = "PrisonerRising";
            this.setTexture("prisoner1");
            addsToProgress = false;
            this.name = name;
            int prisonerID = getRandomPrisonerID();
            id = prisonerID.ToString("0000");
            TAKEN_IDS[prisonerID] = true;
            idsGiven++;
            emitter = Emitter.getPrebuiltEmitter(PrebuiltEmitter.PrisonerSparks);
            emitter.startColor = new Color(color.R, color.G, color.B, 255);
            emitter.endColor = new Color(color.R, color.G, color.B, 0);
            emitter.position = position;
            maskingColor = color;
            baseScore = PRISONER_SCORE;
            flashingIndex = (RetroGame.rand.Next(FLASHING_SEQUENCE_ELEMENTS) / 2) * 2;
            flashingTime = (float)RetroGame.rand.NextDouble() * flashingDelays[flashingIndex];
        }

        public static void Initialize()
        {
            TAKEN_IDS = new BitArray(MAX_IDS, false);
            idsGiven = 0;
        }

        public static int getRandomPrisonerID()
        {
            int prisonerID;
            double r = RetroGame.rand.NextDouble();
            if (r < 0.10)
                prisonerID = RetroGame.rand.Next(1, MAX_IDS / 100);
            else if (r < 0.45)
                prisonerID = RetroGame.rand.Next(MAX_IDS / 100, MAX_IDS / 10);
            else
                prisonerID = RetroGame.rand.Next(MAX_IDS / 10, MAX_IDS);
            if (idsGiven >= MAX_IDS)
                prisonerID = 1337;
            else
            {
                while (TAKEN_IDS[prisonerID])
                {
                    prisonerID = (prisonerID + 1) % MAX_IDS;
                }
            }
            return prisonerID;
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();

            timeSinceLastTurn += seconds;
            float time = timeSinceLastTurn;
            if (timeSinceLastTurn > TIME_PER_TURN)
            {
                timeSinceLastTurn = 0;
                switch (RetroGame.rand.Next(10))
                {
                    case 0:
                        rotation = 0;
                        break;
                    case 1:
                        rotation = (float)Math.PI / 2;
                        break;
                    case 2:
                        rotation = (float)Math.PI;
                        break;
                    case 3:
                        rotation = (float)Math.PI * 3 / 2;
                        break;
                    default:
                        timeSinceLastTurn = time;
                        break;
                }
            }

            flashingTime += seconds;
            if (flashingTime >= flashingDelays[flashingIndex])
            {
                drawHelp = !drawHelp;
                flashingTime -= flashingDelays[flashingIndex];
                flashingIndex = (flashingIndex + 1) % FLASHING_SEQUENCE_ELEMENTS;
            }

            base.Update(gameTime);
        }

        public override bool collectedBy(Entity e)
        {
            bool collected = base.collectedBy(e);
            if (collected)
            {
                HUD.DisplayExclamation(new string[] { "Rescued:", "" + name, "#" + id }, new Color[] { Color.White, Color.Lerp(Color.White, maskingColor, 0.5f), Color.Lerp(Color.White, maskingColor, 0.5f) }, EXCLAMATION_DURATION);
                ((Hero)e).AddCollectedPrisoner(this);
            }
            return collected;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (!dying)
            {
                spriteBatch.Draw(TextureManager.Get("prisonerhat1"), position, null, Color.White, rotation, new Vector2(getTexture().Width / 2, getTexture().Height / 2), 0.5f, SpriteEffects.None, 0.5f);
                if (drawHelp)
                    spriteBatch.DrawString(RetroGame.FONT_PIXEL_SMALL, HELP_STRING, new Vector2(position.X - 30, position.Y - 40), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            }
        }
    }
}
