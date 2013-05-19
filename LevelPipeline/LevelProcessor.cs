using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Retroverse;

namespace LevelPipeline
{
    [ContentProcessor(DisplayName = "Level Processor")]
    public class LevelProcessor : ContentProcessor<Texture2DContent, LevelContent>
    {
        public static readonly int CELL_SIZE = 16;

        public override LevelContent Process(Texture2DContent tex, ContentProcessorContext context)
        {
            string name = context.OutputFilename.Split('\\').Last().Split('.')[0];
            tex.ConvertBitmapType(typeof(PixelBitmapContent<Color>));
            PixelBitmapContent<Color> grid = (PixelBitmapContent<Color>)tex.Mipmaps[0];
            LevelContent level = new LevelContent();
            int w = grid.Width;
            int h = grid.Height;
            if (w == LevelContent.LEVEL_SIZE * CELL_SIZE && h == LevelContent.LEVEL_SIZE * CELL_SIZE)
                level.Init(name, LevelContent.Type.Full);
            else if (w == LevelContent.LEVEL_SIZE_HALF * CELL_SIZE && h == LevelContent.LEVEL_SIZE * CELL_SIZE)
                level.Init(name, LevelContent.Type.HalfVertical);
            else if (w == LevelContent.LEVEL_SIZE * CELL_SIZE && h == LevelContent.LEVEL_SIZE_HALF * CELL_SIZE)
                level.Init(name, LevelContent.Type.HalfHorizontal);
            else if (w == LevelContent.LEVEL_SIZE_HALF * CELL_SIZE && h == LevelContent.LEVEL_SIZE_HALF * CELL_SIZE)
                level.Init(name, LevelContent.Type.Corner);
            else
                throw new ArgumentException("Texture " + tex.Name + " is " + w + "x" + h + "; should be a combination of " + (LevelContent.LEVEL_SIZE_HALF * CELL_SIZE) + " and " + (LevelContent.LEVEL_SIZE * CELL_SIZE));

            //Process cell
            int levelWidth = level.levelWidth;
            int levelHeight = level.levelHeight;
            //System.Diagnostics.Debugger.Launch();
            for (int i = 0; i < levelWidth; i++)
                for (int j = 0; j < levelHeight; j++)
                {
                    //Individual cell
                    Dictionary<Color, int> colorCount = new Dictionary<Color, int>();
                    for (int k = i * CELL_SIZE; k < (i + 1) * CELL_SIZE; k++)
                        for (int l = j * CELL_SIZE; l < (j + 1) * CELL_SIZE; l++)
                        {
                            Color pixel = grid.GetPixel(k, l);
                            if (colorCount.ContainsKey(pixel))
                                colorCount[pixel] = colorCount[pixel] + 1;
                            else
                                colorCount.Add(pixel, 1);

                           // System.Diagnostics.Debugger.Launch();
                            // use top-leftmost pixel for level's color unless already specified
                            if (k == 0 && l == 0)
                                level.setColor(pixel);
                        }
                    // modeColor = most "popular" color in a given cell
                    Color modeColor = Color.White;
                    int minCount = 0;
                    foreach (KeyValuePair<Color, int> pair in colorCount)
                    {
                        if (pair.Value > minCount)
                        {
                            minCount = pair.Value;
                            modeColor = pair.Key;
                        }
                    }
                    LevelContent.LevelTile finalTile = LevelContent.LevelTile.Floor;
                    double minDistance = double.PositiveInfinity;
                    foreach (KeyValuePair<LevelContent.LevelTile, Color> pair in LevelContent.TILE_TO_COLOR)
                    {
                        Color c = pair.Value;
                        double distance = Math.Sqrt(Math.Pow(((int)modeColor.R - c.R), 2) +
                                                    Math.Pow(((int)modeColor.G - c.G), 2) +
                                                    Math.Pow(((int)modeColor.B - c.B), 2));
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            finalTile = pair.Key;
                        }
                    }
                    level.grid[i + j * levelWidth] = finalTile;
                }
            return level;
        }
    }
}