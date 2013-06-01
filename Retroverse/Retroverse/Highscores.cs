using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public class HeroHighscore
    {
        public int id;
        public string name;
        [XmlElement(Type = typeof(XmlColor))] public Color color;
        public int score;
        
        public HeroHighscore() { }
        public HeroHighscore(Hero hero)
        {
            id = hero.prisonerID;
            name = hero.prisonerName;
            color = hero.color;
            score = RetroGame.Score;
        }
    }

    public class HeroHighscoreCoop
    {
        public int id1, id2;
        public string name1, name2;
        [XmlElement(Type = typeof(XmlColor))] public Color color1, color2;
        public int score;

        public HeroHighscoreCoop() { }
        public HeroHighscoreCoop(Hero hero1, Hero hero2)
        {
            id1 = hero1.prisonerID;
            name1 = hero1.prisonerName;
            color1 = hero1.color;
            id2 = hero2.prisonerID;
            name2 = hero2.prisonerName;
            color2 = hero2.color;
            score = RetroGame.Score;
        }
    }

    public static class Highscores
    {
        public static List<HeroHighscore> oldHighscoresSolo;
        public static List<HeroHighscoreCoop> oldHighscoresCoop;
        public static List<HeroHighscore> highscoresSolo;
        public static List<HeroHighscoreCoop> highscoresCoop;
        public static List<HeroHighscore> SOLO { get { return oldHighscoresSolo; } set { highscoresSolo = value; } }
        public static List<HeroHighscoreCoop> COOP { get { return oldHighscoresCoop; } set { highscoresCoop = value; } }
        public const int HIGHSCORE_COUNT = 5;

        [Flags]
        public enum DrawMode { Solo = 0x01, Coop = 0x02, Both = Solo | Coop };
        public const string TITLE_TEXT = "MOST WANTED";
        public const float TITLE_SCALE = 1.0f;
        public const float TITLE_GAP = 50f * TITLE_SCALE;
        public const string SOLO_TEXT = "SOLO";
        public const string COOP_TEXT = "CO-OP";
        public const float SUBTITLE_SCALE = 0.80f;
        public const float SUBTITLE_GAP = 55f * SUBTITLE_SCALE;

        public static float CHART_WIDTH = 352f;
        public static float CHART_LINE_HEIGHT = 4f;
        
        public const float GAP_BEFORE_SOLO = 15f;
        public const float GAP_TO_COOP = 10f;
        public const float GAP_BEFORE_COOP = 20f;
        public const float NAME_SCALE_SOLO = 0.85f;
        public const float NAME_SCALE_COOP = 0.7f;
        public static float NAME_LUMINOSITY_LIMIT = 0.66f;
        public static readonly Vector2 OFFSET_P1 = new Vector2(0, -8);
        public static readonly Vector2 OFFSET_P2 = new Vector2(0, 8);
        public static readonly Vector2 NAME_ORIGIN = new Vector2(0, 31 / 2f);
        public const float SCORE_SCALE = 1.2f;
        public const string SCORE_FORMAT = "0";
        public const float SCORE_GAP = 35f * SCORE_SCALE;

        public static int currentHighscorePosition = HIGHSCORE_COUNT;
        public static HeroHighscore currentSoloHighscore;
        public static HeroHighscoreCoop currentCoopHighscore;
        
        public static readonly Color CURRENT_HIGHSCORE_REGULAR_COLOR = Color.Black;
        public static readonly Color CURRENT_HIGHSCORE_HIGHLIGHTED_COLOR = Color.Cyan;
        public const float CURRENT_HIGHSCORE_COLOR_SPEED = 1f;
        public static float currentHighscoreColorInterp = 0;
        public static int currentHighscoreColorInterpModifier = 1;
        public static Color currentHighscoreColor = CURRENT_HIGHSCORE_REGULAR_COLOR;

        public static void Initialize(bool resetScoresToDefault = false)
        {
            if (oldHighscoresSolo == null || resetScoresToDefault)
            {
                oldHighscoresSolo = new List<HeroHighscore>(5)
                {
                    new HeroHighscore {id = 999,    name = "JeffZero",  color = Color.CornflowerBlue,   score = 1000000},
                    new HeroHighscore {id = 712,    name = "RPGlord",   color = Color.Chartreuse,       score = 500000},
                    new HeroHighscore {id = 21,     name = "tyder21",   color = Color.Purple,           score = 250000},
                    new HeroHighscore {id = 100,    name = "BIGPUN",    color = Color.Red,              score = 100000},
                    new HeroHighscore {id = 2,      name = "ngirl",     color = Color.Aquamarine,       score = 50000},
                };
            }
            if (oldHighscoresCoop == null || resetScoresToDefault)
            {
                oldHighscoresCoop = new List<HeroHighscoreCoop>(5)
                {
                    new HeroHighscoreCoop {id1 = 1337,     name1 = "Fool",      color1 = Color.Firebrick,   score = 1000000,
                                           id2 = 6969,     name2 = "FAH",       color2 = Color.Indigo},
                    new HeroHighscoreCoop {id1 = 2196,     name1 = "Santa",     color1 = Color.Coral,       score = 500000,
                                           id2 = 0909,     name2 = "Natwaf",    color2 = Color.Crimson},
                    new HeroHighscoreCoop {id1 = 13,       name1 = "XIII",      color1 = Color.DeepPink,    score = 250000,
                                           id2 = 964,      name2 = "LotM",      color2 = Color.SlateBlue},
                    new HeroHighscoreCoop {id1 = 8523,     name1 = "Vlado",     color1 = Color.Gold,        score = 100000,
                                           id2 = 4444,     name2 = "Cishir",    color2 = Color.Purple},
                    new HeroHighscoreCoop {id1 = 1988,     name1 = "Adam",      color1 = Color.Gray,        score = 50000,
                                           id2 = 101,      name2 = "Koala",     color2 = Color.Sienna},
                };
            }
            highscoresSolo = new List<HeroHighscore>(oldHighscoresSolo);
            highscoresCoop = new List<HeroHighscoreCoop>(oldHighscoresCoop);

            currentHighscorePosition = HIGHSCORE_COUNT;
            if (RetroGame.NUM_PLAYERS == 1)
            {
                currentSoloHighscore = new HeroHighscore(RetroGame.getHeroes()[0]);
                for (int i = 0; i < HIGHSCORE_COUNT; i++) //check for existing highscore for these players
                    if (currentSoloHighscore.id == highscoresSolo[i].id && currentSoloHighscore.name == highscoresSolo[i].name && currentSoloHighscore.color == highscoresSolo[i].color)
                    {
                        highscoresSolo[i] = currentSoloHighscore;
                        currentHighscorePosition = i;
                        break;
                    }
            }
            else if (RetroGame.NUM_PLAYERS == 2)
            {
                currentCoopHighscore = new HeroHighscoreCoop(RetroGame.getHeroes()[0], RetroGame.getHeroes()[1]);
                for (int i = 0; i < HIGHSCORE_COUNT; i++) //check for existing highscore for these players
                    if (currentCoopHighscore.id1 == highscoresCoop[i].id1 && currentCoopHighscore.name1 == highscoresCoop[i].name1 && currentCoopHighscore.color1 == highscoresCoop[i].color1 &&
                        currentCoopHighscore.id2 == highscoresCoop[i].id2 && currentCoopHighscore.name2 == highscoresCoop[i].name2 && currentCoopHighscore.color2 == highscoresCoop[i].color2)
                    {
                        highscoresCoop[i] = currentCoopHighscore;
                        currentHighscorePosition = i;
                        break;
                    }
            }
        }

        public static void Save()
        {
            oldHighscoresSolo = highscoresSolo;
            oldHighscoresCoop = highscoresCoop;
        }

        public static void Update(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();
            bool coop = RetroGame.NUM_PLAYERS == 2;
            int newPosition;
            if (!coop)
            {
                currentSoloHighscore.score = RetroGame.Score;
                for (newPosition = currentHighscorePosition; newPosition > 0; newPosition--)
                {
                    if (currentSoloHighscore.score >= highscoresSolo[newPosition - 1].score)
                        continue;
                    break;
                }
                if (newPosition != currentHighscorePosition)
                {
                    if (currentHighscorePosition < HIGHSCORE_COUNT)
                        highscoresSolo.RemoveAt(currentHighscorePosition);
                    else
                        highscoresSolo.RemoveAt(HIGHSCORE_COUNT - 1);
                    highscoresSolo.Insert(newPosition, currentSoloHighscore);
                    currentHighscorePosition = newPosition;
                }
            }
            else
            {
                currentCoopHighscore.score = RetroGame.Score;
                for (newPosition = currentHighscorePosition; newPosition > 0; newPosition--)
                {
                    if (currentCoopHighscore.score >= highscoresCoop[newPosition - 1].score)
                        continue;
                    break;
                }
                if (newPosition != currentHighscorePosition)
                {
                    if (currentHighscorePosition < HIGHSCORE_COUNT)
                        highscoresCoop.RemoveAt(currentHighscorePosition);
                    else
                        highscoresCoop.RemoveAt(HIGHSCORE_COUNT - 1);
                    highscoresCoop.Insert(newPosition, currentCoopHighscore);
                    currentHighscorePosition = newPosition;
                }
            }
            currentHighscoreColorInterp += seconds * currentHighscoreColorInterpModifier * CURRENT_HIGHSCORE_COLOR_SPEED;
            if (currentHighscoreColorInterp > 1 || currentHighscoreColorInterp < 0)
            {
                currentHighscoreColorInterpModifier *= -1;
                currentHighscoreColorInterp = MathHelper.Clamp(currentHighscoreColorInterp, 0, 1);
            }
            currentHighscoreColor = Color.Lerp(CURRENT_HIGHSCORE_REGULAR_COLOR, CURRENT_HIGHSCORE_HIGHLIGHTED_COLOR, currentHighscoreColorInterp);
        }

        public static void Draw(DrawMode drawMode, SpriteBatch spriteBatch, Vector2 position, float baseScale)
        {
            bool coop = RetroGame.NUM_PLAYERS == 2;
            spriteBatch.DrawString(RetroGame.FONT_PIXEL_LARGE, TITLE_TEXT, position, Color.Black, 0, Vector2.Zero, TITLE_SCALE * baseScale, SpriteEffects.None, 0);
            position.Y += 42;
            if (drawMode.HasFlag(DrawMode.Solo))
            {
                spriteBatch.DrawString(RetroGame.FONT_PIXEL_LARGE, SOLO_TEXT, position, Color.Black, 0, Vector2.Zero, SUBTITLE_SCALE * baseScale, SpriteEffects.None, 0);
                position.Y += SUBTITLE_GAP;
                spriteBatch.Draw(RetroGame.PIXEL, new Rectangle((int)position.X, (int)position.Y, (int)(CHART_WIDTH * baseScale), (int)(CHART_LINE_HEIGHT * baseScale)), Color.Black);
                position.Y += GAP_BEFORE_SOLO * baseScale;
                for (int i = 0; i < HIGHSCORE_COUNT; i++)
                {
                    spriteBatch.DrawString(RetroGame.FONT_PIXEL_SMALL, "#" + highscoresSolo[i].id.ToString("0000") + " " + highscoresSolo[i].name, position, highscoresSolo[i].color.darkenIfTooLight(NAME_LUMINOSITY_LIMIT), 0, NAME_ORIGIN, 0.75f * baseScale, SpriteEffects.None, 0);
                    string scoreString = highscoresSolo[i].score.ToString(SCORE_FORMAT);
                    Vector2 scoreDims = RetroGame.FONT_PIXEL_SMALL.MeasureString(scoreString);
                    Color scoreColor = (!coop && currentHighscorePosition == i) ? currentHighscoreColor : Color.Black;
                    spriteBatch.DrawString(RetroGame.FONT_PIXEL_SMALL, highscoresSolo[i].score.ToString(SCORE_FORMAT), new Vector2(position.X + (CHART_WIDTH * baseScale), position.Y), scoreColor, 0, scoreDims * new Vector2(1, 0.5f), SCORE_SCALE * baseScale, SpriteEffects.None, 0);
                    position.Y += SCORE_GAP;
                }
                position.Y -= SCORE_GAP;
                position.Y += GAP_TO_COOP * baseScale;
            }
            if (drawMode.HasFlag(DrawMode.Coop))
            {
                spriteBatch.DrawString(RetroGame.FONT_PIXEL_LARGE, COOP_TEXT, position, Color.Black, 0, Vector2.Zero, SUBTITLE_SCALE * baseScale, SpriteEffects.None, 0);
                position.Y += SUBTITLE_GAP;
                spriteBatch.Draw(RetroGame.PIXEL, new Rectangle((int)position.X, (int)position.Y, (int)(CHART_WIDTH * baseScale), (int)(CHART_LINE_HEIGHT * baseScale)), Color.Black);
                position.Y += GAP_BEFORE_COOP * baseScale;
                for (int i = 0; i < HIGHSCORE_COUNT; i++)
                {
                    spriteBatch.DrawString(RetroGame.FONT_PIXEL_SMALL, "#" + highscoresCoop[i].id1.ToString("0000") + " " + highscoresCoop[i].name1, position + (OFFSET_P1 * baseScale), highscoresCoop[i].color1.darkenIfTooLight(NAME_LUMINOSITY_LIMIT), 0, NAME_ORIGIN, NAME_SCALE_COOP * baseScale, SpriteEffects.None, 0);
                    spriteBatch.DrawString(RetroGame.FONT_PIXEL_SMALL, "#" + highscoresCoop[i].id2.ToString("0000") + " " + highscoresCoop[i].name2, position + (OFFSET_P2 * baseScale), highscoresCoop[i].color2.darkenIfTooLight(NAME_LUMINOSITY_LIMIT), 0, NAME_ORIGIN, NAME_SCALE_COOP * baseScale, SpriteEffects.None, 0);
                    string scoreString = highscoresCoop[i].score.ToString(SCORE_FORMAT);
                    Vector2 scoreDims = RetroGame.FONT_PIXEL_SMALL.MeasureString(scoreString);
                    Color scoreColor = (coop && currentHighscorePosition == i) ? currentHighscoreColor : Color.Black;
                    spriteBatch.DrawString(RetroGame.FONT_PIXEL_SMALL, highscoresCoop[i].score.ToString(SCORE_FORMAT), new Vector2(position.X + (CHART_WIDTH * baseScale), position.Y), scoreColor, 0, scoreDims * new Vector2(1, 0.5f), SCORE_SCALE * baseScale, SpriteEffects.None, 0);
                    position.Y += SCORE_GAP;
                }
            }
        }
    }
}
