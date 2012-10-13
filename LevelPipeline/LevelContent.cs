using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LevelPipeline
{
    public class LevelContent
    {
        public static readonly int LEVEL_SIZE = 31;
        public enum LevelTile {White, Red, Green, Blue, Yellow, Purple, Black};
        public static readonly Dictionary<LevelTile, Color> TILE_TO_COLOR = new Dictionary<LevelTile, Color>(){
            {LevelTile.White, new Color(255,255,255)},
            {LevelTile.Red, new Color(255,0,0)},
            {LevelTile.Green, new Color(0,255,0)},
            {LevelTile.Blue, new Color(0,0,255)},
            {LevelTile.Yellow, new Color(255,255,0)},
            {LevelTile.Purple, new Color(255,0,255)},
            {LevelTile.Black, new Color(0,0,0)}
        };
        public LevelTile[] grid = new LevelTile[LEVEL_SIZE * LEVEL_SIZE];

        public LevelContent()
        {
        }
    }
}
