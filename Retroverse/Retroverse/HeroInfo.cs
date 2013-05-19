using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public static class HeroInfo
    {
        //info
        public const string INFO_TITLE = "Prisoner Report";
        public static readonly Vector2 INFO_TITLE_POS = new Vector2(0.725f, 0.2f);
        public const float INFO_TITLE_SCALE = 0.79f;

        public const float INFO_LEFT_ALIGNMENT = 0.56f;
        public const float INFO_TOP_ALIGNMENT = 0.225f;
        public const float INFO_VERTICAL_SPACING = 0.04f;
        public const string INFO_STATUS_TITLE = "Status: ";
        public static readonly Vector2 INFO_STATUS_POS = new Vector2(INFO_LEFT_ALIGNMENT, INFO_TOP_ALIGNMENT);
        public const string INFO_STATUS_FUGITIVE = "Fugitive";
        public static readonly Color INFO_STATUS_COLOR_ALIVE = Color.DarkCyan;
        public const string INFO_STATUS_CAPTURED = "Captured";
        public static readonly Color INFO_STATUS_COLOR_DEAD = Color.Red;
        public const float INFO_ICON_SCALE = 0.5f;
        public const float INFO_COUNT_XPOS = 0.825f;
        public const string INFO_COLLECTED_GEMS = "Collected ";
        public static readonly Vector2 INFO_COLLECTED_GEMS_POS = new Vector2(INFO_LEFT_ALIGNMENT, INFO_TOP_ALIGNMENT + INFO_VERTICAL_SPACING);
        public const string INFO_KILLED_ENEMIES = "Killed ";
        public static readonly Vector2 INFO_KILLED_ENEMIES_POS = new Vector2(INFO_LEFT_ALIGNMENT, INFO_TOP_ALIGNMENT + 2 * INFO_VERTICAL_SPACING);
        public const string INFO_HITBY_ENEMIES = "Hit by ";
        public static readonly Vector2 INFO_HITBY_ENEMIES_POS = new Vector2(INFO_LEFT_ALIGNMENT, INFO_TOP_ALIGNMENT + 3 * INFO_VERTICAL_SPACING);
        public const string INFO_FREED_PRISONERS = "Freed ";
        public static readonly Vector2 INFO_FREED_PRISONERS_POS = new Vector2(INFO_LEFT_ALIGNMENT, INFO_TOP_ALIGNMENT + 4 * INFO_VERTICAL_SPACING);


        public static void Draw(Hero hero, SpriteBatch spriteBatch, Vector2 texSize)
        {
            //info
            spriteBatch.DrawString(RetroGame.FONT_PIXEL_LARGE, INFO_TITLE, INFO_TITLE_POS * texSize, Color.Black, 0, RetroGame.FONT_PIXEL_LARGE.MeasureString(INFO_TITLE) / 2, INFO_TITLE_SCALE, SpriteEffects.None, 0);
            Vector2 pos = INFO_STATUS_POS * texSize;
            spriteBatch.DrawString(RetroGame.FONT_PIXEL_SMALL, INFO_STATUS_TITLE, pos, Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
            pos.X += RetroGame.FONT_PIXEL_SMALL.MeasureString(INFO_STATUS_TITLE).X;
            spriteBatch.DrawString(RetroGame.FONT_PIXEL_SMALL, hero.Fugitive ? INFO_STATUS_FUGITIVE : INFO_STATUS_CAPTURED, pos, hero.Fugitive ? INFO_STATUS_COLOR_ALIVE : INFO_STATUS_COLOR_DEAD, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);

            pos = INFO_COLLECTED_GEMS_POS * texSize;
            spriteBatch.DrawString(RetroGame.FONT_PIXEL_SMALL, INFO_COLLECTED_GEMS, pos, Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
            pos.X += RetroGame.FONT_PIXEL_SMALL.MeasureString(INFO_COLLECTED_GEMS).X;
            spriteBatch.Draw(TextureManager.Get("collectable3"), pos, null, Color.White, 0, Vector2.Zero, INFO_ICON_SCALE, SpriteEffects.None, 0);
            pos = new Vector2(INFO_COUNT_XPOS * texSize.X, pos.Y);
            spriteBatch.DrawString(RetroGame.FONT_PIXEL_SMALL, hero.CollectedGems.ToString("000"), pos, Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);

            pos = INFO_KILLED_ENEMIES_POS * texSize;
            spriteBatch.DrawString(RetroGame.FONT_PIXEL_SMALL, INFO_KILLED_ENEMIES, pos, Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
            pos.X += RetroGame.FONT_PIXEL_SMALL.MeasureString(INFO_KILLED_ENEMIES).X;
            spriteBatch.Draw(TextureManager.Get("enemy1"), pos, null, Color.White, 0, Vector2.Zero, INFO_ICON_SCALE, SpriteEffects.None, 0);
            pos = new Vector2(INFO_COUNT_XPOS * texSize.X, pos.Y);
            spriteBatch.DrawString(RetroGame.FONT_PIXEL_SMALL, hero.KilledEnemyCount.ToString("000"), pos, Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);

            pos = INFO_HITBY_ENEMIES_POS * texSize;
            spriteBatch.DrawString(RetroGame.FONT_PIXEL_SMALL, INFO_HITBY_ENEMIES, pos, Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
            pos.X += RetroGame.FONT_PIXEL_SMALL.MeasureString(INFO_HITBY_ENEMIES).X;
            spriteBatch.Draw(TextureManager.Get("enemy2"), pos, null, Color.White, 0, Vector2.Zero, INFO_ICON_SCALE, SpriteEffects.None, 0);
            pos = new Vector2(INFO_COUNT_XPOS * texSize.X, pos.Y);
            spriteBatch.DrawString(RetroGame.FONT_PIXEL_SMALL, hero.HitByEnemyCount.ToString("000"), pos, Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);

            pos = INFO_FREED_PRISONERS_POS * texSize;
            spriteBatch.DrawString(RetroGame.FONT_PIXEL_SMALL, INFO_FREED_PRISONERS, pos, Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
            pos.X += RetroGame.FONT_PIXEL_SMALL.MeasureString(INFO_FREED_PRISONERS).X;
            spriteBatch.Draw(TextureManager.Get("prisoner1"), pos, null, Color.White, 0, Vector2.Zero, INFO_ICON_SCALE, SpriteEffects.None, 0);
            pos = new Vector2(INFO_COUNT_XPOS * texSize.X, pos.Y);
            spriteBatch.DrawString(RetroGame.FONT_PIXEL_SMALL, hero.FreedPrisoners.Count.ToString("000"), pos, Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
        }
    }
}
