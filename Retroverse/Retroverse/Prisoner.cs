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
        public static readonly int MAX_IDS = 2000;
        public static BitArray AVAILABLE_IDS = new BitArray(MAX_IDS, false);
        public static int idsGiven = 0;
        public string name;
        public string id;
        public static readonly int PRISONER_SCORE = 1000;
        public int powerUp1; //, 0= Normal, 1=Bursts, 2= Fast, 3=Reverse
        public int powerUp2; //, 0= Normal, 1=Ghost, (2=Drill1, 3=Drill2)
        public int powerUp3; // -1=Normal, 0=Front, 1=Side, 2=Charge

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
        public float timeSinceLastTurn = (float)Game1.rand.NextDouble() * TIME_PER_TURN;
        static Prisoner()
        {
            if (MAX_IDS > 1337)
                AVAILABLE_IDS[1337] = true;
        }

        public Prisoner(Color color, string name, int x, int y, int levelX, int levelY, int tileX, int tileY):
            base(x, y, levelX, levelY, tileX, tileY)
        {
            this.setTexture("prisoner1");
            addsToProgress = false;
            this.name = name;
            int prisonerID;
            double r = Game1.rand.NextDouble();
            if (r < 0.45)
                prisonerID = Game1.rand.Next(MAX_IDS/100);
            else if (r < 0.80)
                prisonerID = Game1.rand.Next(MAX_IDS/10);
            else
                prisonerID = Game1.rand.Next(MAX_IDS);
            idsGiven++;
            if (idsGiven >= MAX_IDS)
                prisonerID = 1337;
            else
            {
                while (AVAILABLE_IDS[prisonerID])
                {
                    prisonerID = (prisonerID + 1) % MAX_IDS;
                }
                AVAILABLE_IDS[prisonerID] = true;
            }
            id = prisonerID.ToString("0000");
            emitter = Emitter.getPrebuiltEmitter(PrebuiltEmitter.PrisonerSparks);
            emitter.startColor = new Color(color.R, color.G, color.B, 255);
            emitter.endColor = new Color(color.R, color.G, color.B, 0);
            emitter.position = position;
            maskingColor = color;
            baseScore = PRISONER_SCORE;
            flashingIndex = (Game1.rand.Next(FLASHING_SEQUENCE_ELEMENTS) / 2) * 2;
            flashingTime = (float)Game1.rand.NextDouble() * flashingDelays[flashingIndex];
        }

        public override void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();
            if (dying)
            {
                Game1.exclamationText = "Saved prisoner " + name + " #" + id;
                Game1.exclamationColor = Color.Black;
            }

            timeSinceLastTurn += seconds;
            float time = timeSinceLastTurn;
            if (timeSinceLastTurn > TIME_PER_TURN)
            {
                timeSinceLastTurn = 0;
                switch (Game1.rand.Next(10))
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

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (!dying)
            {
                spriteBatch.Draw(TextureManager.Get("prisonerhat1"), position, null, Color.White, rotation, new Vector2(getTexture().Width / 2, getTexture().Height / 2), 0.5f, SpriteEffects.None, 0.5f);
                if (drawHelp)
                    spriteBatch.DrawString(Game1.FONT_PIXEL_SMALL, HELP_STRING, new Vector2(position.X - 30, position.Y - 40), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            }
        }
    }
}
