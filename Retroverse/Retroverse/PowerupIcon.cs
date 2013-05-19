using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particles;

namespace Retroverse
{
    public class PowerupIcon : Collectable
    {
        public static readonly int BACKGROUND_ANIMATION_TIMESTEP = 100; //ms
        public static readonly float BACKGROUND_DRAWSCALE = 0.6f;
        public static readonly Color DEFAULT_YELLOW = new Color(255, 255, 50);
        public static readonly float ICON_DRAWSCALE = 0.45f;
        public static readonly float EXCLAMATION_DURATION = 3f;
        public const int POWERUP_COLLECTED_SCORE = 2500;

        public Type powerupType;
        public bool isCoOp = false;
        public bool subtractCostOnCollected = false;
        public AnimatedTexture background;
        public Texture2D icon;
        public Color iconBackgroundTint;

        public static float POWERUP_DETAILS_TEXT_BASE_SCALE = 0.80f;
        public static float POWERUP_DETAILS_ICON_BASE_SCALE = 0.50f;
        public bool DrawDetails { get; set; }
        public bool DetailsAboveIcon { get; set; }
        public Color DetailsCostColor { get; set; }

        public PowerupIcon(int x, int y, int levelX, int levelY, int tileX, int tileY, Type powerupType) :
            base(x, y, levelX, levelY, tileX, tileY)
        {
            DrawDetails = false;
            DetailsAboveIcon = true;
            CollectedSound = "CollectPowerup";
            background = new AnimatedTexture("fairyglow", 4, BACKGROUND_ANIMATION_TIMESTEP);
            isCoOp = powerupType.IsSubclassOf(typeof(CoOpPowerup));
            this.powerupType = powerupType;
            this.icon = Powerups.DummyPowerups[powerupType].Icon;
            this.iconBackgroundTint = Powerups.DummyPowerups[powerupType].TintColor;
            texture = null;
            baseScore = POWERUP_COLLECTED_SCORE;
            emitter = Emitter.getPrebuiltEmitter(PrebuiltEmitter.PrisonerSparks);
            emitter.startColor = new Color(iconBackgroundTint.R, iconBackgroundTint.G, iconBackgroundTint.B, 255);
            emitter.endColor = new Color(iconBackgroundTint.R, iconBackgroundTint.G, iconBackgroundTint.B, 0);
        }

        public static void exclamate(string header, string exclamation, Color color)
        {
            HUD.DisplayExclamation(new string[] { header, exclamation }, new Color[] { Color.White, color }, EXCLAMATION_DURATION);
        }

        public override bool collectedBy(Entity e)
        {
            if (isCoOp && RetroGame.NUM_PLAYERS < 2)
                return false;
            bool collected = base.collectedBy(e);
            if (collected)
            {
                if (e is Hero)
                {
                    Hero hero = (Hero)e;
                    Hero otherHero = null;
                    Powerup powerup;

                    foreach (Hero h in RetroGame.getHeroes())
                    {
                        if (h != hero)
                        {
                            otherHero = h;
                            break;
                        }
                    }

                    if (isCoOp)
                        powerup = (Powerup)powerupType.GetConstructor(new Type[] { typeof(Hero), typeof(Hero) }).Invoke(new object[] { hero, otherHero });
                    else
                        powerup = (Powerup)powerupType.GetConstructor(new Type[] { typeof(Hero) }).Invoke(new object[] { hero });

                    if (subtractCostOnCollected)
                    {
                        RetroGame.RemoveGems(Powerups.DummyPowerups[powerup.GetType()].GemCost); //use the latest gem cost for the powerup type
                        if (powerupType == typeof(HealthPickup))
                            Powerups.DummyPowerups[typeof(HealthPickup)].GemCost += HealthPickup.COST_INCREASE_PER_PURCHASE;
                        else if (powerupType == typeof(RevivePickup))
                            Powerups.DummyPowerups[typeof(RevivePickup)].GemCost += RevivePickup.COST_INCREASE_PER_PURCHASE;
                    }
                    powerup.OnCollectedByHero(hero);
                    if (Powerups.IsInstant(powerup))
                    {
                        powerup.OnAddedToHero();
                        exclamate("Used", powerup.GenericName + ": " + powerup.SpecificName, iconBackgroundTint);
                    }
                    else
                    {
                        if (hero.HasPowerup(powerup.GenericName))
                        {
                            Inventory.StorePowerup(powerup);
                            exclamate("Stored", powerup.GenericName + ": " + powerup.SpecificName, iconBackgroundTint);
                        }
                        else
                        {
                            powerup = hero.AddPowerup(powerupType);
                            exclamate("Acquired", powerup.GenericName + ": " + powerup.SpecificName, iconBackgroundTint);
                        }
                    }
                }
            }
            return collected;
        }

        public override void Update(GameTime gameTime)
        {
            background.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!dying)
            {
                Color backgroundColor = Color.Lerp(DEFAULT_YELLOW, iconBackgroundTint, 0.50f);
                Color iconColor = Color.White;
                spriteBatch.Draw(background.getTexture(), position, null, backgroundColor, rotation, new Vector2(background.getTexture().Width / 2, background.getTexture().Height / 2), BACKGROUND_DRAWSCALE, SpriteEffects.None, 1);
                spriteBatch.Draw(icon, position, null, iconColor, rotation, new Vector2(icon.Width / 2, icon.Height / 2), ICON_DRAWSCALE, SpriteEffects.None, 1);
                DrawIconDetails(spriteBatch);
            }
            base.Draw(spriteBatch);
        }

        public void DrawIconDetails(SpriteBatch spriteBatch)
        {
            if (DrawDetails)
            {
                Vector2 screenSize = RetroGame.screenSize;
                Powerup dummyPowerup = Powerups.DummyPowerups[powerupType];
                SpriteFont font = RetroGame.FONT_DEBUG;
                float modifiedScale = HUD.modifiedScale;
                float detailsScale = POWERUP_DETAILS_TEXT_BASE_SCALE;
                float iconScale = POWERUP_DETAILS_ICON_BASE_SCALE * modifiedScale;
                string title = dummyPowerup.GenericName + ": " + dummyPowerup.SpecificName;
                Vector2 titleDims = RetroGame.FONT_DEBUG.MeasureString(title) * detailsScale;
                Vector2 descriptionDims = RetroGame.FONT_DEBUG.MeasureString(dummyPowerup.Description) * detailsScale;
                Vector2 offset = new Vector2(-titleDims.X / 2, screenSize.Y * 0.02f);

                Color titleColor;
                if (dummyPowerup.TintColor.getLuminosity() <= 127)
                    titleColor = Color.Lerp(dummyPowerup.TintColor, Color.Black, 0.5f);
                else
                    titleColor = Color.Lerp(dummyPowerup.TintColor, Color.White, 0.5f);
                spriteBatch.DrawString(RetroGame.FONT_DEBUG, title, position + offset, titleColor, 0, Vector2.Zero, detailsScale, SpriteEffects.None, 0);
                offset.Y += titleDims.Y;
                spriteBatch.DrawString(RetroGame.FONT_DEBUG, dummyPowerup.Description, position + offset, Color.White, 0, Vector2.Zero, detailsScale, SpriteEffects.None, 0);
                offset.Y += descriptionDims.Y;
                spriteBatch.Draw(TextureManager.Get("collectable3"), position + offset, null, Color.White, 0, Vector2.Zero, iconScale, SpriteEffects.None, 0);
                offset.X += TextureManager.Get("collectable3").Width * iconScale;
                offset.Y += TextureManager.Get("collectable3").Height * iconScale / 4;
                spriteBatch.DrawString(RetroGame.FONT_PIXEL_SMALL, dummyPowerup.GemCost.ToString("000"), position + offset, DetailsCostColor, 0, Vector2.Zero, detailsScale, SpriteEffects.None, 0);

            }
        }
    }
}
