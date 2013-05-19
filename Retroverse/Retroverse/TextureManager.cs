using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;

namespace Retroverse
{
    static class TextureManager
    {
        public const string SPRITES_ROOT = "Sprites";
        static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        public static ContentManager content = null;

        public static Texture2D Get(string key)
        {
            if (!textures.ContainsKey(key))
                throw new Exception("Texture " + key + " was not found. Is it included in project?");
            return textures[key];
        }

        public static void Add(string key)
        {
            try
            {
                textures.Add(key, content.Load<Texture2D>(SPRITES_ROOT + "/" + key));
            }
            catch (ArgumentException e) { }
        }

        public static void Add(string key, int frames)
        {
            try
            {
                for (int i = 0; i < frames; i++)
                    textures.Add(key + (i + 1), content.Load<Texture2D>(SPRITES_ROOT + "/" + key + (i + 1)));
            }
            catch (ArgumentException e) { }
        }

        public static void LoadSprites(ContentManager content)
        {
            TextureManager.content = content;
            DirectoryInfo spritesDirectory = new DirectoryInfo(content.RootDirectory + "\\" + SPRITES_ROOT);
            LoadSprites(content, spritesDirectory, null);
            int x = 5;
        }

        private static void LoadSprites(ContentManager content, DirectoryInfo directory, string directoryPrefix)
        {
            if (directory.Name != SPRITES_ROOT)
                directoryPrefix += directory.Name + "\\";
            DirectoryInfo[] subDirectories = directory.GetDirectories();
            foreach (DirectoryInfo subDirectory in subDirectories)
                LoadSprites(content, subDirectory, directoryPrefix);

            FileInfo[] filePaths = directory.GetFiles("*.*");
            foreach (FileInfo file in filePaths)
            {
                string key = "";
                if (directoryPrefix != null)
                    key += directoryPrefix.Replace('\\', '_').ToLower();
                key += file.Name.Split('.')[0];
                string fullname = directory.Name;
                textures.Add(key, content.Load<Texture2D>(SPRITES_ROOT + "\\" + directoryPrefix + file.Name.Split('.')[0]));
            }
        }
    }
}
