using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace Retroverse
{
    public static class Powerups
    {
        public const string INSTANT = "Instant";
        public static readonly Type[] SPECIAL_INTRO_POWERUPS =
        {
            typeof(DrillBasic),
            typeof(ShotBasic),
            typeof(RadarPowerup),
            typeof(RescuePowerup),
        };

        public static readonly Type DEFAULT_POWERUP = typeof(FullHealthPickup);
        public static IEnumerable<Type> PowerupTypes { get { return (RetroGame.NUM_PLAYERS == 2) ? powerupTypes.Union(powerupTypesCoOp) : powerupTypes; } }
        public static IEnumerable<Type> PowerupTypesWild { get { return (RetroGame.NUM_PLAYERS == 2) ? powerupTypes.Union(powerupTypesCoOpWild) : powerupTypesWild; } }
        private static List<Type> powerupTypesWild = new List<Type>();
        private static List<Type> powerupTypes = new List<Type>();
        private static List<Type> powerupTypesCoOpWild = new List<Type>();
        private static List<Type> powerupTypesCoOp = new List<Type>();
        private static Type[] ignoredPowerupTypes = new Type[] { typeof(HealthPickup), typeof(SandPickup), typeof(BombPickup), typeof(RevivePickup) }
            .Union(SPECIAL_INTRO_POWERUPS).ToArray(); // ignore the intro powerups too
        private static Random rand = RetroGame.rand;

        public static Dictionary<Type, Powerup> DummyPowerups = new Dictionary<Type, Powerup>();

        public static void LoadContent(ContentManager Content)
        {
            //determine wild powerups
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            Hero dummyHero = new Hero(PlayerIndex.One);
            foreach (Type type in myAssembly.GetTypes())
            {
                if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Powerup)))
                {
                    Object powerup = null;
                    bool coop = type.IsSubclassOf(typeof(CoOpPowerup));
                    if (coop)
                        powerup = type.GetConstructor(new Type[] { typeof(Hero), typeof(Hero) }).Invoke(new object[] { dummyHero, dummyHero });
                    else
                        powerup = type.GetConstructor(new Type[] { typeof(Hero) }).Invoke(new object[] { dummyHero });
                    if (!ignoredPowerupTypes.Contains(type))
                    {
                        List<Type> typesList = coop ? powerupTypesCoOp : powerupTypes;
                        List<Type> typesListWild = coop ? powerupTypesCoOpWild : powerupTypesWild;
                        if (!((Powerup)powerup).StoreOnly)
                            typesListWild.Add(type);
                        typesList.Add(type);
                    }
                    DummyPowerups[type] = (Powerup)powerup;
                }
            }   
        }

        public static Type RandomPowerupType(List<Type> except = null)
        {
            IEnumerable<Type> nonExcludedTypes = (except == null) ? PowerupTypes : PowerupTypes.Except(except);
            if (!nonExcludedTypes.Any())
                return DEFAULT_POWERUP;
            int randType = rand.Next(nonExcludedTypes.Count());
            return nonExcludedTypes.ElementAt(randType);
        }

        public static Type RandomWildPowerupType(List<Type> except = null)
        {
            IEnumerable<Type> nonExcludedTypes = (except == null) ? PowerupTypesWild : PowerupTypesWild.Except(except);
            if (!nonExcludedTypes.Any())
                return DEFAULT_POWERUP;
            int randType = rand.Next(nonExcludedTypes.Count());
            return nonExcludedTypes.ElementAt(randType);
        }

        public static bool IsInstant(Powerup powerup)
        {
            return powerup.GenericName == INSTANT;
        }
    }
}
