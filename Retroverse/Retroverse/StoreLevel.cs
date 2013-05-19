using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public class StoreLevel : Level
    {
        public static readonly int[] RANDOM_POWERUPS_TILEX = new int[] { 7, 15, 22 };
        public const int RANDOM_POWERUPS_TILEY = 10;
        
        public const int NUM_RANDOM_POWERUPS = 3;
        public static readonly Type[] alreadyUsedTypes = new Type[NUM_RANDOM_POWERUPS];

        public static readonly Point HEALTH_POSITION = new Point(7, 20);
        public static readonly Point SAND_POSITION = new Point(12, 20);
        public static readonly Point BOMB_POSITION = new Point(17, 20);
        public static readonly Point REVIVE_POSITION = new Point(22, 20);

        public static readonly List<PowerupIcon> icons = new List<PowerupIcon>();

        public StoreLevel(LevelManager levelManager, LevelFragment storeLevelFragment, IList<Type> powerupTypesToUse, int xPos, int yPos)
            : base(levelManager, storeLevelFragment, xPos, yPos)
        {
            alreadyUsedTypes.Initialize();
            PowerupIcon icon;
            for (int i = 0; i < NUM_RANDOM_POWERUPS; i++)
            {
                int tileX = RANDOM_POWERUPS_TILEX[i];
                int tileY = RANDOM_POWERUPS_TILEY;
                Type powerupType = null;
                if (powerupTypesToUse == null)
                    powerupType = Powerups.RandomPowerupType(except: alreadyUsedTypes.Union(Inventory.AllCurrentlyOwnedPowerupsTypes).ToList());
                else
                    powerupType = powerupTypesToUse[i];
                icon = levelManager.newPowerup(powerupType, tileX, tileY, this);
                icon.DrawDetails = true;
                icon.DetailsAboveIcon = true;
                icon.subtractCostOnCollected = true;
                powerups.Add(icon);
                icons.Add(icon);
                alreadyUsedTypes[i] = icon.powerupType;
            }

            icon = levelManager.newRegeneratingPowerup(typeof(HealthPickup), HEALTH_POSITION.X, HEALTH_POSITION.Y, this);
            icon.DrawDetails = true;
            icon.DetailsAboveIcon = false;
            icon.subtractCostOnCollected = true;
            powerups.Add(icon);
            icons.Add(icon);
            icon = levelManager.newRegeneratingPowerup(typeof(SandPickup), SAND_POSITION.X, SAND_POSITION.Y, this);
            icon.DrawDetails = true;
            icon.DetailsAboveIcon = false;
            icon.subtractCostOnCollected = true;
            powerups.Add(icon);
            icons.Add(icon);
            icon = levelManager.newRegeneratingPowerup(typeof(BombPickup), BOMB_POSITION.X, BOMB_POSITION.Y, this);
            icon.DrawDetails = true;
            icon.DetailsAboveIcon = false;
            icon.subtractCostOnCollected = true;
            powerups.Add(icon);
            icons.Add(icon);
            icon = levelManager.newPowerup(typeof(RevivePickup), REVIVE_POSITION.X, REVIVE_POSITION.Y, this);
            icon.DrawDetails = true;
            icon.DetailsAboveIcon = false;
            icon.subtractCostOnCollected = true;
            powerups.Add(icon);
            icons.Add(icon);
        }

        public void Update(GameTime gameTime)
        {
            foreach(PowerupIcon icon in icons)
            {
                if (RetroGame.AvailableGems < Powerups.DummyPowerups[icon.powerupType].GemCost)
                {
                    icon.DetailsCostColor = Color.Red;
                    icon.ableToBeCollected = false;
                }
                else
                {
                    icon.DetailsCostColor = Color.White;
                    icon.ableToBeCollected = true;
                }
            }   
        }

        public override bool drillWall(int tileX, int tileY)
        {
            return false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}
