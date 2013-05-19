using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public class RadarPowerup : Powerup
    {
        public static int RADAR_BORDER_WIDTH = 2;
        public static int RADAR_CELL_WIDTHHEIGHT = 20;
        public static Color RADAR_BORDER_COLOR = Color.White;
        public static Color RADAR_WALL_INDICATOR_COLOR = Color.Red;

        public RadarPowerup(Hero hero)
            : base(hero)
        {
            /* Set these properties for your specific powerup */
            GenericName = "Radar";
            SpecificName = "Basic";
            Rank = 1; //when should this powerup be updated/drawn in relation to all other powerups? (lower ranks first)
            Active = false; //is this powerup activated with a button press?
            StoreOnly = true; //can the powerup be found randomly in a level, or can it only be bought in the store?
            Icon = TextureManager.Get("radaricon1"); //filename for this powerup's icon
            DrawBeforeHero = false; //should the powerup's effects be drawn before the sprite of the hero, or above? 
            GemCost = 0; //how many gems does it take to buy this from the store?
            TintColor = Color.MediumPurple; //what color should this powerup's icon and related effects be?
            Description = "Displays a map of\nsurrounding levels"; //give a short description (with appropriate newlines) of the powerup, for display to the player
        }

        public override void OnAddedToHero()
        {
            RetroGame.EscapeScreen.setZoom(ArenaCamera.ZOOM_ESCAPE);
        }

        public override void OnRemovedFromHero()
        {
            RetroGame.EscapeScreen.setZoom(ArenaCamera.INTRO_FINAL_ZOOM);
        }

        public override void Activate(InputAction activationAction)
        {
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override float GetPowerupCharge()
        {
            float charge = 1;
            return charge;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {           
        }

        public void DrawOnHUD(SpriteBatch spriteBatch, float hudScale)
        {
            if (RetroGame.TopLevelManagerScreen != RetroGame.EscapeScreen)
                return; // draw nothing if in store

            Hero[] heroes = RetroGame.getHeroes();
            Level[,] levels = RetroGame.EscapeScreen.levelManager.levels;

            int radarCellWidthHeight = (int)(RADAR_CELL_WIDTHHEIGHT * hudScale);
            int radarBaseHeight = (int)(1.2f * HUD.hudHeight);
            int radarBaseX = (int)(RetroGame.screenSize.X / 2 - (radarCellWidthHeight * 1.5) + RADAR_BORDER_WIDTH);
            Hero mainHero = heroes[0];
            for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++)
                {
                    Color[,] colorGrid = null;
                    int x = mainHero.levelX + i;
                    int y = mainHero.levelY + j;
                    if (x < 0 || y < 0 || x >= LevelManager.MAX_LEVELS || y >= LevelManager.MAX_LEVELS
                        || levels[x, y] == null
                        || levels[x, y].fragmentGrid[0, 0] == null)
                        colorGrid = new Color[2, 2] { { Color.Black, Color.Black }, { Color.Black, Color.Black } };
                    else
                    {
                        colorGrid = new Color[2, 2] { { levels[x, y].fragmentGrid[0, 0].color, levels[x, y].fragmentGrid[0, 1].color }, { levels[x, y].fragmentGrid[1, 0].color, levels[x, y].fragmentGrid[1, 1].color } };
                    }
                    for (int ii = 0; ii < 2; ii++)
                        for (int jj = 0; jj < 2; jj++)
                        {
                            Color c = colorGrid[ii, jj];
                            c.A = (byte)(c.A * 0.8); // translucentize it
                            int subCellWidthHeight = radarCellWidthHeight / 2 + 1;
                            spriteBatch.Draw(RetroGame.PIXEL, new Rectangle(radarBaseX + ((i + 1) * radarCellWidthHeight) + (ii * subCellWidthHeight), radarBaseHeight + ((j + 1) * radarCellWidthHeight) + (jj * subCellWidthHeight), subCellWidthHeight, subCellWidthHeight), c);
                        }
                }
            foreach (Hero hero in RetroGame.getHeroes())
            {
                float tilePercX = (float)hero.tileX / LevelContent.LEVEL_SIZE;
                float tilePercY = (float)hero.tileY / LevelContent.LEVEL_SIZE;
                float heroIndicatorX = radarBaseX + radarCellWidthHeight * (1 + tilePercX);
                float heroIndicatorY = radarBaseHeight + radarCellWidthHeight * (1 + tilePercY);
                spriteBatch.Draw(RetroGame.PIXEL, new Rectangle((int)heroIndicatorX, (int)heroIndicatorY, RADAR_BORDER_WIDTH * 2, RADAR_BORDER_WIDTH * 2), Color.Gold);
                spriteBatch.Draw(RetroGame.PIXEL, new Rectangle((int)heroIndicatorX + 1, (int)heroIndicatorY + 1, RADAR_BORDER_WIDTH * 2 - 2, RADAR_BORDER_WIDTH * 2 - 2), hero.color);
            }
            float mainHeroIndicatorX = radarBaseX + radarCellWidthHeight * (1 + (float)mainHero.tileX / LevelContent.LEVEL_SIZE);
            float wallOffset = RiotGuardWall.wallPosition - mainHero.position.X;
            float wallOffsetPixels = wallOffset * radarCellWidthHeight / (float)Level.TEX_SIZE + RetroGame.rand.Next(-1, 1); // add automatic shaking to hide 1-pixel rounding errors
            int wallCurrentLevel = (int)RiotGuardWall.wallPosition / Level.TEX_SIZE;
            int wallLevelsBehind = mainHero.levelX - wallCurrentLevel;
            if (wallLevelsBehind <= 1)
                spriteBatch.Draw(TextureManager.Get("riotguardwallbar"), new Rectangle((int)(mainHeroIndicatorX + wallOffsetPixels), radarBaseHeight, RADAR_BORDER_WIDTH * 3, (3 * radarCellWidthHeight) + RADAR_BORDER_WIDTH), RADAR_WALL_INDICATOR_COLOR);
            for (int i = 0; i < 4; i++)
            {
                spriteBatch.Draw(RetroGame.PIXEL, new Rectangle(radarBaseX + (i * radarCellWidthHeight), radarBaseHeight, RADAR_BORDER_WIDTH, (3 * radarCellWidthHeight) + RADAR_BORDER_WIDTH), RADAR_BORDER_COLOR);
            }
            for (int i = 0; i < 4; i++)
            {
                spriteBatch.Draw(RetroGame.PIXEL, new Rectangle(radarBaseX, radarBaseHeight + (i * radarCellWidthHeight), (3 * radarCellWidthHeight) + RADAR_BORDER_WIDTH, RADAR_BORDER_WIDTH), RADAR_BORDER_COLOR);
            }
        }
    }
}