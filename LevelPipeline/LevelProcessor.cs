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
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    ///
    /// TODO: change the ContentProcessor attribute to specify the correct
    /// display name for this processor.
    /// </summary>
    [ContentProcessor(DisplayName = "Level Processor")]
    public class LevelProcessor : ContentProcessor<Texture2DContent, LevelContent>
    {
        public static readonly int CELL_SIZE = 16;

        public override LevelContent Process(Texture2DContent tex, ContentProcessorContext context)
        {
            tex.ConvertBitmapType(typeof(PixelBitmapContent<Color>));
            PixelBitmapContent<Color> grid = (PixelBitmapContent<Color>)tex.Mipmaps[0];
            LevelContent level = new LevelContent();
            int w = grid.Width;
            int h = grid.Height;
            if (w != LevelContent.LEVEL_SIZE * CELL_SIZE || h != LevelContent.LEVEL_SIZE * CELL_SIZE)
            {
                throw new ArgumentException("Texture " + tex.Name + " is " + w + "x" + h + "; should be " + (LevelContent.LEVEL_SIZE * CELL_SIZE) + "x" + (LevelContent.LEVEL_SIZE * CELL_SIZE));
            }
            //Process cell
            for (int i = 0; i < LevelContent.LEVEL_SIZE; i++)
                for (int j = 0; j < LevelContent.LEVEL_SIZE; j++)
                {
                    //Individual cell
                    Dictionary<Color, int> colorCount = new Dictionary<Color, int>();
                    int avgR = 0;
                    int avgG = 0;
                    int avgB = 0;
                    for (int k = i * CELL_SIZE; k < (i + 1) * CELL_SIZE; k++)
                        for (int l = j * CELL_SIZE; l < (j + 1) * CELL_SIZE; l++)
                        {
                            Color pixel = grid.GetPixel(k, l);
                            if (colorCount.ContainsKey(pixel))
                                colorCount[pixel] = colorCount[pixel] + 1;
                            else
                                colorCount.Add(pixel, 1);
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
                    LevelContent.LevelTile finalTile = LevelContent.LevelTile.White;
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
                    level.grid[i + j * LevelContent.LEVEL_SIZE] = finalTile;
                }
            return level;
        }
    }
}