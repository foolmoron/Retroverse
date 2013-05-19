using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Retroverse
{
    public static class HUD
    {
        public static readonly float HUD_WIDTH = 0.6f; //percentage of screen size
        public static readonly Color HUD_COLOR = Color.Navy;
        public static readonly int[] SIZES = new int[] { 51, 66, 82, 101 };
        public static int hudHeight = SIZES[0];

        public static readonly Vector2 SAND_POSITION = new Vector2(0.00f, 0.58f);
        public static readonly Vector2 BOMB_POSITION = new Vector2(0.20f, 0.58f);
        public static readonly Vector2 GEMS_POSITION = new Vector2(0.40f, 0.58f);
        public static readonly Vector2 PRIS_POSITION = new Vector2(0.60f, 0.58f);
        public static readonly Vector2 DISTANCE_TO_ESCAPE_POSITION = new Vector2(0.60f, 0.0f);
        public const float HUD_ICON_SCALE = 0.30f;
        public const float HUD_TEXT_SCALE = 0.7f;
        public const float HUD_TEXT_OFFSET = 64 * HUD_ICON_SCALE;

        //store icon
        public static GraphicsDevice graphicsDevice;
        public static SpriteBatch storeSpriteBatch;
        public static Texture2D storeIcon;
        public static Texture2D storeIconBorder;
        public static float storeBorderInterp = 0;
        public static int storeBorderInterpModifier = 1;
        public const float STORE_BORDER_INTERP_VELOCITY = 1f; //secs
        public static RenderTarget2D storeIconTarget;

        public static readonly Vector2 STORE_POSITION = new Vector2(0.81f, 0.50f);
        public const float STORE_ICON_SCALE = 0.64f;
        public static readonly Color STORE_UNCHARGED_COLOR = Color.Gray;
        public static readonly Color STORE_PARTLYCHARGED_COLOR = Color.White;
        public static readonly Color STORE_FULLYCHARGED_COLOR = Color.Cyan;
        public const float STORE_START_KEY_SCALE = 0.75f;
        public const float STORE_START_BUTTON_SCALE = 1.0f;

        //cell names
        public static Texture2D cellBorderTex;
        public static float CELL_OFFSETX = 0.085f;
        public static float CELL_YPOS = 0.25f;
        public static float CELL_WIDTH = 0.10f;

        //score
        public static float SCORE_XPOS = 0.175f;
        public static readonly Color SCORE_BORDER_COLOR = new Color(25, 184, 236);
        public static readonly Color[] SCORE_TEXT_COLORS = new Color[10] {
                new Color(255, 255, 255),
                new Color(255, 50, 50),
                
                new Color(150, 60, 255),
                new Color(255, 165, 0),
                
                new Color(255, 255, 50),
                new Color(50, 255, 50),
                
                new Color(255, 105, 180),
                new Color(220, 130, 80),
                
                new Color(255, 218, 185),
                new Color(12, 238, 188),
            };
        public static readonly Color SCORE_TEXT_COLOR_ZERO = Color.Gray;
        public static readonly int SCORE_DIGITS = 8;
        public static readonly int SCORE_BORDER_WIDTHHEIGHT = 2;
        public static readonly float SCORE_CELL_RELATIVEWIDTH = 0.048f;
        public static readonly float SCORE_CELL_RELATIVEHEIGHT = 0.50f;
        public static readonly float SCORE_TEXT_BASE_SCALE = 0.3f;

        public static readonly float RELATIVESPACE_BETWEEN_SCORE_AND_SAND = 0.23f;

        public static readonly int SAND_SIZE = 17;
        public static readonly int SPACE_BETWEEN_SAND = 1;

        public static readonly float WALL_SPEED_POSITION_RELATIVE_X = 0.6f;
        public static readonly float WALL_SPEED_POSITION_RELATIVE_Y = 0.70f;

        public static readonly float[] HUD_SCALES = { 1f, 1.30f, 1.60f, 1.95f };
        public static float hudScale = 1f;
        public static float modifiedScale { get { return (1 + hudScale) / 2; } }

        //on-screen text excalamation
        public static SpriteFont FONT_EXCLAMATION;
        private static string[] exclamationStrings;
        private static Color[] exclamationColors;
        private static float exclamationDurationSecs;
        private static float currentExclamationTime;
        public static bool showExclamation;

        public static void LoadContent(ContentManager Content)
        {
            FONT_EXCLAMATION = Content.Load<SpriteFont>("Fonts\\pixel28"); /* http://www.dafont.com/visitor.font + http://xbox.create.msdn.com/en-US/education/catalog/utility/bitmap_font_maker */
        }

        public static void Initialize(GraphicsDevice graphics)
        {
            if (graphicsDevice != graphics)
            {
                graphicsDevice = graphics;
                storeIcon = TextureManager.Get("storeicon");
                storeIconBorder = TextureManager.Get("storeiconborder");
                if(storeIconTarget != null)
                    storeIconTarget.Dispose();
                storeIconTarget = new RenderTarget2D(graphicsDevice, storeIconBorder.Width, storeIconBorder.Height * 2);
                if(storeSpriteBatch != null)
                    storeSpriteBatch.Dispose();
                storeSpriteBatch = new SpriteBatch(graphicsDevice);
            }
            Inventory.Initialize();
            exclamationDurationSecs = 0;
            currentExclamationTime = 0;
            showExclamation = false;
            cellBorderTex = TextureManager.Get("hudcellborder");
        }

        public static void DisplayExclamation(string message, Color color, float duration)
        {
            exclamationStrings = new string[] { message };
            exclamationColors = new Color[] { color };
            exclamationDurationSecs = duration;
            currentExclamationTime = 0;
        }

        public static void DisplayExclamation(string[] strings, Color[] colors, float durationSecs)
        {
            if (strings.Length == 0 || colors.Length == 0 || strings.Length != colors.Length)
            {
                throw new ArgumentOutOfRangeException("The arguments messages and colors must both be of the same size and not be empty.");
            }
            exclamationStrings = strings;
            exclamationColors = colors;
            exclamationDurationSecs = durationSecs;
            currentExclamationTime = 0;
        }

        public static void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();

            showExclamation = false;
            if (exclamationDurationSecs > 0)
            {
                currentExclamationTime += seconds;
                if (currentExclamationTime <= exclamationDurationSecs)
                    showExclamation = true;
            }

            if (RetroGame.StoreCharge >= 1)
            {
                storeBorderInterp += seconds * STORE_BORDER_INTERP_VELOCITY * storeBorderInterpModifier;
                if(storeBorderInterp >= 1 || storeBorderInterp < 0)
                    storeBorderInterpModifier *= -1;
                storeBorderInterp = MathHelper.Clamp(storeBorderInterp, 0, 1);
            } 
            else
                storeBorderInterp = 0;
        }

        public static void UpdateScale()
        {
            hudScale = HUD_SCALES[(int)RetroGame.currentScreenSizeMode];
        }

        public static void PreDraw()
        {
            float storeCharge = RetroGame.StoreCharge;
            Color unchargedColor = STORE_UNCHARGED_COLOR;
            Color storeColor = (storeCharge >= 1) ? STORE_FULLYCHARGED_COLOR : STORE_PARTLYCHARGED_COLOR;

            graphicsDevice.SetRenderTarget(storeIconTarget);
            graphicsDevice.Clear(Color.Transparent);
            Effect effect = Effects.StoreIconShading;
            effect.Parameters["unchargedColor"].SetValue(unchargedColor.ToVector4());
            effect.Parameters["chargedColor"].SetValue(storeColor.ToVector4());
            effect.Parameters["shadingPercentage"].SetValue(storeCharge);

            if (storeCharge >= 1)
            {
                storeSpriteBatch.Begin();
                byte alpha = (byte)(storeBorderInterp * 255);
                storeSpriteBatch.Draw(storeIconBorder, Vector2.Zero, STORE_FULLYCHARGED_COLOR.withAlpha(alpha));
                Hero mainPlayer = RetroGame.getMainLiveHero();
                if (mainPlayer != null)
                {
                    SpriteFont startFont = (mainPlayer.currentInputType == InputType.Keyboard) ? RetroGame.FONT_HUD_KEYS : RetroGame.FONT_HUD_XBOX;
                    string startString = mainPlayer.bindings.getHUDIconCharacter(mainPlayer.currentInputType, InputAction.Start);
                    float storeStartScale = (mainPlayer.currentInputType == InputType.Keyboard) ? STORE_START_KEY_SCALE : STORE_START_BUTTON_SCALE;
                    storeSpriteBatch.DrawString(startFont, startString, new Vector2(storeIconTarget.Width / 2f, storeIconTarget.Height / 2f), Color.White.withAlpha(alpha), 0, new Vector2(startFont.MeasureString(startString).X / 2, 0), storeStartScale, SpriteEffects.None, 0);
                }
                storeSpriteBatch.End();
            }
            storeSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
                                   DepthStencilState.None, RasterizerState.CullCounterClockwise, effect);
            storeSpriteBatch.Draw(storeIcon, new Vector2(storeIconTarget.Width / 2f, storeIconTarget.Height / 4f), null, Color.White, 0, new Vector2(storeIcon.Width / 2f, storeIcon.Height / 2f), 1, SpriteEffects.None, 0);
            storeSpriteBatch.End();
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            Vector2 screenSize = RetroGame.screenSize;
            int score = RetroGame.Score;
            int availableSand = RetroGame.AvailableSand;

            if (!(RetroGame.TopScreen is InventoryScreen))
                Inventory.DrawEquipped(spriteBatch);

            float hudWidth = screenSize.X * HUD_WIDTH;
            float HUD_INITIAL_POSITION = screenSize.X * (1 - HUD_WIDTH) / 2;
            float xPos = HUD_INITIAL_POSITION;
            float yPos = 0;
            spriteBatch.Draw(RetroGame.PIXEL, new Rectangle((int)xPos, 0, (int)hudWidth, hudHeight), HUD_COLOR);

            // score
            float xPosScore = xPos + (SCORE_XPOS * hudWidth);
            float yPosScore = yPos;
            float scoreBorderWidthHeight = (float)Math.Ceiling(SCORE_BORDER_WIDTHHEIGHT * hudScale);
            for (int i = 0; i <= SCORE_DIGITS; i++)
            {
                spriteBatch.Draw(RetroGame.PIXEL, new Rectangle((int)(xPosScore + i * SCORE_CELL_RELATIVEWIDTH * screenSize.X), (int)(yPosScore), (int)scoreBorderWidthHeight, (int)(SCORE_CELL_RELATIVEHEIGHT * hudHeight + scoreBorderWidthHeight)), SCORE_BORDER_COLOR);
            }
            spriteBatch.Draw(RetroGame.PIXEL, new Rectangle((int)(xPosScore), (int)(yPosScore), (int)(SCORE_DIGITS * SCORE_CELL_RELATIVEWIDTH * screenSize.X + scoreBorderWidthHeight), (int)scoreBorderWidthHeight), SCORE_BORDER_COLOR);
            spriteBatch.Draw(RetroGame.PIXEL, new Rectangle((int)(xPosScore), (int)(yPosScore + SCORE_CELL_RELATIVEHEIGHT * hudHeight), (int)(SCORE_DIGITS * SCORE_CELL_RELATIVEWIDTH * screenSize.X + scoreBorderWidthHeight), (int)scoreBorderWidthHeight), SCORE_BORDER_COLOR);
            xPosScore += (SCORE_CELL_RELATIVEWIDTH * screenSize.X) / 2;
            yPosScore += (SCORE_CELL_RELATIVEHEIGHT * hudHeight) / 2;
            float scoreTextScale = SCORE_TEXT_BASE_SCALE * hudScale;
            for (int i = SCORE_DIGITS - 1; i >= 0; i--)
            {
                int dividedScore = score / (int)Math.Pow(10, i);
                int digit = dividedScore % 10;
                Color dcolor = (dividedScore == 0) ? SCORE_TEXT_COLOR_ZERO : SCORE_TEXT_COLORS[digit];
                string dstring = "" + digit;
                Vector2 dimensions = RetroGame.FONT_PIXEL_HUGE.MeasureString(dstring);
                spriteBatch.DrawString(RetroGame.FONT_PIXEL_HUGE, "" + digit, new Vector2(xPosScore, yPosScore), dcolor, 0, dimensions / 2, scoreTextScale, SpriteEffects.None, 0);
                xPosScore += (SCORE_CELL_RELATIVEWIDTH * screenSize.X);
            }

            xPos = hudWidth * SAND_POSITION.X + HUD_INITIAL_POSITION;
            yPos = hudHeight * SAND_POSITION.Y;
            spriteBatch.Draw(TextureManager.Get("sandiconhud"), new Vector2(xPos, yPos), null, Color.White, 0, Vector2.Zero, HUD_ICON_SCALE * hudScale, SpriteEffects.None, 0);
            spriteBatch.DrawString(RetroGame.FONT_PIXEL_SMALL, RetroGame.AvailableSand.ToString("000"), new Vector2(xPos + (HUD_TEXT_OFFSET * hudScale), yPos), Color.White, 0, Vector2.Zero, HUD_TEXT_SCALE * hudScale, SpriteEffects.None, 0);
            xPos = hudWidth * BOMB_POSITION.X + HUD_INITIAL_POSITION;
            yPos = hudHeight * BOMB_POSITION.Y;
            spriteBatch.Draw(TextureManager.Get("bomb"), new Vector2(xPos, yPos), null, Color.White, 0, Vector2.Zero, HUD_ICON_SCALE * hudScale, SpriteEffects.None, 0);
            spriteBatch.DrawString(RetroGame.FONT_PIXEL_SMALL, RetroGame.AvailableBombs.ToString("000"), new Vector2(xPos + (HUD_TEXT_OFFSET * hudScale), yPos), Color.White, 0, Vector2.Zero, HUD_TEXT_SCALE * hudScale, SpriteEffects.None, 0);
            xPos = hudWidth * GEMS_POSITION.X + HUD_INITIAL_POSITION;
            yPos = hudHeight * GEMS_POSITION.Y;
            spriteBatch.Draw(TextureManager.Get("collectable3"), new Vector2(xPos, yPos), null, Color.White, 0, Vector2.Zero, HUD_ICON_SCALE * hudScale, SpriteEffects.None, 0);
            spriteBatch.DrawString(RetroGame.FONT_PIXEL_SMALL, RetroGame.AvailableGems.ToString("000"), new Vector2(xPos + (HUD_TEXT_OFFSET * hudScale), yPos), Color.White, 0, Vector2.Zero, HUD_TEXT_SCALE * hudScale, SpriteEffects.None, 0);
            xPos = hudWidth * PRIS_POSITION.X + HUD_INITIAL_POSITION;
            yPos = hudHeight * PRIS_POSITION.Y;
            spriteBatch.Draw(TextureManager.Get("prisoner1"), new Vector2(xPos, yPos), null, Color.White, 0, Vector2.Zero, HUD_ICON_SCALE * hudScale, SpriteEffects.None, 0);
            spriteBatch.DrawString(RetroGame.FONT_PIXEL_SMALL, RetroGame.TotalPrisoners.ToString("000"), new Vector2(xPos + (HUD_TEXT_OFFSET * hudScale), yPos), Color.White, 0, Vector2.Zero, HUD_TEXT_SCALE * hudScale, SpriteEffects.None, 0);
            
            //store
            xPos = hudWidth * STORE_POSITION.X + HUD_INITIAL_POSITION;
            yPos = hudHeight * STORE_POSITION.Y;
            spriteBatch.Draw(storeIconTarget, new Vector2(xPos, yPos), null, Color.White, 0, Vector2.Zero, STORE_ICON_SCALE * hudScale, SpriteEffects.None, 0);

            //cell names
            xPos = hudWidth * CELL_OFFSETX + HUD_INITIAL_POSITION;
            yPos = hudHeight * CELL_YPOS;
            string cellName = Level.GetCellName(RetroGame.getHeroes()[0].levelX, RetroGame.getHeroes()[0].levelY);
            Vector2 nameDims = RetroGame.FONT_PIXEL_SMALL.MeasureString(cellName);
            float nameRatio = nameDims.Y / nameDims.X;

            float cellBorderWidthWithX = nameDims.X * modifiedScale * 0.8f;
            float cellBorderHeightWithX = cellBorderWidthWithX * nameRatio;

            float cellBorderHeightWithY = nameDims.Y * modifiedScale * 0.8f;
            float cellBorderWidthWithY = cellBorderHeightWithY / nameRatio;

            Vector2 cellBorderScale = new Vector2(((cellBorderWidthWithX + cellBorderWidthWithY) / 2) / cellBorderTex.Width, ((cellBorderHeightWithX + cellBorderHeightWithY) / 2) / cellBorderTex.Width);
            Vector2 nameScale = new Vector2(((cellBorderWidthWithX + cellBorderWidthWithY) * 0.85f / 2) / nameDims.X, ((cellBorderHeightWithX + cellBorderHeightWithY) * 0.85f / 2) / nameDims.Y);

            spriteBatch.Draw(cellBorderTex, new Vector2(xPos, yPos), null, Color.White, 0, new Vector2(cellBorderTex.Width / 2f, cellBorderTex.Height / 2f), cellBorderScale, SpriteEffects.None, 0);
            spriteBatch.DrawString(RetroGame.FONT_PIXEL_SMALL, cellName, new Vector2(xPos, yPos), Color.White, 0, nameDims / 2, nameScale, SpriteEffects.None, 0);
            if (RetroGame.NUM_PLAYERS == 2)
            {
                xPos = hudWidth * (1 - CELL_OFFSETX) + HUD_INITIAL_POSITION;
                yPos = hudHeight * CELL_YPOS;
                cellName = Level.GetCellName(RetroGame.getHeroes()[1].levelX, RetroGame.getHeroes()[1].levelY);
                nameDims = RetroGame.FONT_PIXEL_SMALL.MeasureString(cellName);
                nameRatio = nameDims.Y / nameDims.X;

                cellBorderWidthWithX = nameDims.X * modifiedScale * 0.8f;
                cellBorderHeightWithX = cellBorderWidthWithX * nameRatio;

                cellBorderHeightWithY = nameDims.Y * modifiedScale * 0.8f;
                cellBorderWidthWithY = cellBorderHeightWithY / nameRatio;

                cellBorderScale = new Vector2(((cellBorderWidthWithX + cellBorderWidthWithY) / 2) / cellBorderTex.Width, ((cellBorderHeightWithX + cellBorderHeightWithY) / 2) / cellBorderTex.Width);
                nameScale = new Vector2(((cellBorderWidthWithX + cellBorderWidthWithY) * 0.85f / 2) / nameDims.X, ((cellBorderHeightWithX + cellBorderHeightWithY) * 0.85f / 2) / nameDims.Y);

                spriteBatch.Draw(cellBorderTex, new Vector2(xPos, yPos), null, Color.White, 0, new Vector2(cellBorderTex.Width / 2f, cellBorderTex.Height / 2f), cellBorderScale, SpriteEffects.None, 0);
                spriteBatch.DrawString(RetroGame.FONT_PIXEL_SMALL, cellName, new Vector2(xPos, yPos), Color.White, 0, nameDims / 2, nameScale, SpriteEffects.None, 0);
            }

            //radar
            Hero heroWithRadar = null;
            foreach(Hero hero in RetroGame.getHeroes())
                if (hero.HasPowerup("Radar"))
                    heroWithRadar = hero;

            if (heroWithRadar != null)
            {
                ((RadarPowerup) heroWithRadar.GetPowerup("Radar")).DrawOnHUD(spriteBatch, hudScale);
            }

            if (showExclamation && exclamationStrings != null && exclamationStrings.Length > 0)
            {
                float exclamationScale = (hudScale + 1.25f) / 2;
                float space = FONT_EXCLAMATION.MeasureString(" ").X;
                string fullString = "";
                foreach (string s in exclamationStrings)
                    fullString += s + " ";
                fullString = fullString.Trim();
                Vector2 fullDimensions = FONT_EXCLAMATION.MeasureString(fullString);
                float initialPos = (screenSize.X - fullDimensions.X * exclamationScale) / 2;
                float offset = 0;
                for (int i = 0; i < exclamationStrings.Length; i++)
                {
                    string s = exclamationStrings[i];
                    Color c = exclamationColors[i];
                    float stringWidth = FONT_EXCLAMATION.MeasureString(s).X;
                    spriteBatch.DrawString(FONT_EXCLAMATION, s, new Vector2(initialPos + offset, (screenSize.Y * 0.8f - fullDimensions.Y * exclamationScale) / 2), c, 0, Vector2.Zero, exclamationScale, SpriteEffects.None, 0);
                    offset += (stringWidth + space) * exclamationScale;
                }
            }
        }
    }
}
