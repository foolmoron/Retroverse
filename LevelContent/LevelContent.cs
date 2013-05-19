using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Retroverse
{
    public class LevelContent
    {
        public static readonly Color DEFAULT_COLOR = Color.Coral;

        public const int LEVEL_SIZE = 31;
        public const int LEVEL_SIZE_HALF = 16;

        public enum Type { Full, HalfHorizontal, HalfVertical, Corner };
        public enum LevelTile {Floor, Red, Gem, Blue, Prisoner, Enemy, Wall, Powerup, Special1, Special2, Special3, Special4, Hero1, Hero2};

        public static readonly Dictionary<LevelTile, Color> TILE_TO_COLOR = new Dictionary<LevelTile, Color>(){
            {LevelTile.Floor, new Color(255,255,255)},
            {LevelTile.Red, new Color(255,0,0)},
            {LevelTile.Gem, new Color(0,255,0)},
            {LevelTile.Blue, new Color(0,0,255)},
            {LevelTile.Prisoner, new Color(255,255,0)},
            {LevelTile.Enemy, new Color(255,0,255)},
            {LevelTile.Wall, new Color(0,0,0)},
            {LevelTile.Powerup, new Color(0,255,255)},
            {LevelTile.Special1, new Color(0,255,205)},
            {LevelTile.Special2, new Color(0,255,155)},
            {LevelTile.Special3, new Color(0,255,105)},
            {LevelTile.Special4, new Color(0,255,175)},
            {LevelTile.Hero1, new Color(200, 200, 200)},
            {LevelTile.Hero2, new Color(100, 100, 100)},
        };

        public Type type;
        public int levelWidth;
        public int levelHeight;
        public LevelTile[] grid;
        public string name;
        public Color color;

        public LevelContent() { }

        public void Init(string name, Type levelType)
        {
            this.name = name;
            type = levelType;
            levelWidth = LevelContent.LEVEL_SIZE;
            levelHeight = LevelContent.LEVEL_SIZE;
            switch (levelType)
            {
                case LevelContent.Type.Full:
                    break;
                case LevelContent.Type.HalfHorizontal:
                    levelWidth = LevelContent.LEVEL_SIZE;
                    levelHeight = LevelContent.LEVEL_SIZE_HALF;
                    break;
                case LevelContent.Type.HalfVertical:
                    levelWidth = LevelContent.LEVEL_SIZE_HALF;
                    levelHeight = LevelContent.LEVEL_SIZE;
                    break;
                case LevelContent.Type.Corner:
                    levelWidth = LevelContent.LEVEL_SIZE_HALF;
                    levelHeight = LevelContent.LEVEL_SIZE_HALF;
                    break;
            }
            grid = new LevelTile[levelWidth * levelHeight];
        }

        public void setColor(Color color)
        {
            this.color = color;
        }
    }
}
