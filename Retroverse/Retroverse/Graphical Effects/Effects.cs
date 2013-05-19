using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Retroverse
{
    public static class Effects
    {
        public static Effect Grayscale;
        public static Effect OuterGrayscale;
        public static Effect ColorHighlight;
        public static Effect Test;
        public static Effect RewindRandomStatic;
        public static Effect RewindDistortion;
        public static Effect RewindTexture;
        public static Effect StaticWithAlpha;
        public static Effect StoreIconShading;

        internal static void LoadContent(ContentManager Content)
        {
            Grayscale = Content.Load<Effect>("Effects\\RetroStasis");
            OuterGrayscale = Content.Load<Effect>("Effects\\RetroPort");
            ColorHighlight = Content.Load<Effect>("Effects\\ColorHighlight");
            Test = Content.Load<Effect>("Effects\\Test");
            RewindRandomStatic = Content.Load<Effect>("Effects\\RewindRandomStatic");
            RewindDistortion = Content.Load<Effect>("Effects\\RewindDistortion");
            RewindTexture = Content.Load<Effect>("Effects\\RewindTexture");
            StaticWithAlpha = Content.Load<Effect>("Effects\\StaticWithAlphaTexture");
            StoreIconShading = Content.Load<Effect>("Effects\\StoreIconShading");
        }
    }
}
