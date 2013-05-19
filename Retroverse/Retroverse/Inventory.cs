using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Retroverse
{
    public static class Inventory
    {
        public static readonly Color COOLDOWN_COLOR = new Color(60, 60, 125);

        private static List<Type> allCurrentlyOwnedPowerupTypes = null;
        public static List<Type> AllCurrentlyOwnedPowerupsTypes
        {
            get
            {
                if (allCurrentlyOwnedPowerupTypes == null)
                {
                    calculateAllCurrentlyOwnedPowerupTypes();
                }
                return allCurrentlyOwnedPowerupTypes;
            }
        }

        //powerups
        public const int MAX_ACTIVE_POWERUPS = 4;
        public const int MAX_PASSIVE_POWERUPS = 9;
        internal struct PowerupList
        {
            public Powerup[] actives;
            public Powerup[] passives;
        }
        internal static PowerupList[] powerupLists;

        //powerup HUD
        public const int CELL_SIZE = 20;
        public const int SPACE_BETWEEN_CELLS = 4;
        public const float EQUIPPED_BASE_VERTICAL_OFFSET_RELATIVE = 0.125f;
        public const int POWERUP_ICON_TEXTURE_SIZE = 64;
        public const float ACTIVE_POWERUP_RELATIVE_SIZE = 1.5f;

        //storage
        public static Texture2D arrowTex;
        public const int STORAGE_ACTIVE_POWERUPS_PER_ROW = 8;
        public const int STORAGE_PASSIVE_POWERUPS_PER_ROW = 12;
        public const int STORAGE_ACTIVE_ROWS = 2;
        public const int STORAGE_PASSIVE_ROWS = 2;
        public const int CELL_STORAGE_SIZE = 30;
        public const float ACTIVE_POWERUP_STORAGE_RELATIVE_SIZE = 1.5f;

        //error warning
        public const string WARNING_SAME_GENERIC_NAME = "Cannot equip two of \nthe same type of powerup";
        public const string WARNING_BAD_ACTIVENESS = "Cannot put active \npowerup in passive slot";
        public static readonly Vector2 WARNING_POS = new Vector2(0.5f, 0.035f);
        public const float WARNING_SCALE = 1.0f;
        public static readonly Color WARNING_INITIAL_COLOR = Color.Red;
        public static readonly Color WARNING_FINAL_COLOR = Color.White;
        public const float WARNING_FLASH_TIME = 1f;
        public static float warningFlashTime = 0;
        public static Color warningColor;
        public static string warning = "";
        public static Point[] warnedIcons = null;

        //acquired powerup flashing
        internal class AcquiredIcon {
            internal Point pos;
            internal float time; 
        }
        public static readonly Color ACQUIRED_COLOR = Color.Cyan;
        public const float ACQUIRED_FLASH_TIME = 2.5f;
        internal static List<AcquiredIcon> acquiredIcons = new List<AcquiredIcon>();
        public static bool shouldAlsoDrawStorageIcons = false;

        public const float CURSOR_BASE_SCALE = 0.5f;
        public static readonly Point[] cursors = new Point[2] { new Point(0, RetroGame.MAX_PLAYERS), new Point(1, RetroGame.MAX_PLAYERS) };
        public static Point?[] selectedCursors = new Point?[2];
        public static readonly Vector2 CURSOR_OFFSET_SINGLE = new Vector2(0.5f, 0);
        public static readonly Vector2[] CURSOR_OFFSETS_DOUBLE = new Vector2[2] { new Vector2(0.3f, 0), new Vector2(0.7f, 0) };

        public static readonly Vector2[] POWERUP_DESCRIPTION_RELATIVE_POSITIONS = new Vector2[] { new Vector2(0.15f, 0.09f), new Vector2(0.55f, 0.09f) };
        public const float POWERUP_DESCRIPTION_BASE_SCALE = 0.6f;
        public static readonly Color POWERUP_DESCRIPTION_COLOR = Color.Black;

        public static readonly float[] cursorBobOffsets = new float[2] { MIN_BOB_OFFSET, MIN_BOB_OFFSET };
        public static readonly int[] cursorBobMultipliers = new int[2] { 1, 1 };
        public const float MIN_BOB_OFFSET = -0.1f;
        public const float MAX_BOB_OFFSET = 0.2f;
        public const float BOB_OFFSET_VELOCITY = 1f;

        internal static readonly InventoryPowerupIcon[][] storageGrid = new InventoryPowerupIcon[STORAGE_ACTIVE_ROWS + STORAGE_PASSIVE_ROWS + RetroGame.MAX_PLAYERS][]
        {
            null, //current loadout player 1
            null, //current loadout player 2
            new InventoryPowerupIcon[STORAGE_ACTIVE_POWERUPS_PER_ROW],
            new InventoryPowerupIcon[STORAGE_ACTIVE_POWERUPS_PER_ROW],
            new InventoryPowerupIcon[STORAGE_PASSIVE_POWERUPS_PER_ROW],
            new InventoryPowerupIcon[STORAGE_PASSIVE_POWERUPS_PER_ROW],
        };
        public static readonly Vector2 STORAGE_POSITION_RELATIVE = new Vector2(0.5f, 0.75f);

        public const float HERO_UI_HORIZONTAL_OFFSET_RELATIVE = 0.01f;
        public const float HERO_ID_VERTICAL_OFFSET_RELATIVE = 0.01f;
        public const float HERO_ID_TEXT_BASE_SCALE = 1f;
        public const float HERO_NAME_VERTICAL_OFFSET_RELATIVE = 0.045f;
        public const float HERO_NAME_TEXT_BASE_SCALE = 0.75f;
        public const float HERO_HEALTH_VERTICAL_OFFSET_RELATIVE = 0.080f;
        public const float HERO_HEALTH_WIDTH_RELATIVE = 0.125f;
        public const float HERO_HEALTH_HEIGHT_RELATIVE = HERO_HEALTH_WIDTH_RELATIVE / 4;
        public static Texture2D healthBorderTex;
        public static Texture2D healthFillingTex;

        public static void Initialize()
        {
            arrowTex = TextureManager.Get("arrow");
            healthBorderTex = TextureManager.Get("healthborder");
            healthFillingTex = TextureManager.Get("healthfilling");

            powerupLists = new PowerupList[RetroGame.NUM_PLAYERS];
            for (int i = 0; i < powerupLists.Length; i++)
            {
                powerupLists[i].actives = new Powerup[MAX_ACTIVE_POWERUPS];
                powerupLists[i].passives = new Powerup[MAX_PASSIVE_POWERUPS];
            }

            /*inventory icons*/
            //equipped
            for (int i = 0; i < RetroGame.NUM_PLAYERS; i++)
            {
                storageGrid[i] = new InventoryPowerupIcon[MAX_ACTIVE_POWERUPS + MAX_PASSIVE_POWERUPS];
                for (int j = 0; j < MAX_ACTIVE_POWERUPS; j++)
                {
                    Powerup powerup = powerupLists[i].actives[j];
                    storageGrid[i][j] = new InventoryPowerupIcon(i, j, true, powerup, (InputAction)Enum.Parse(typeof(InputAction), "Action" + (j + 1)));
                }
                for (int j = MAX_ACTIVE_POWERUPS; j < MAX_ACTIVE_POWERUPS + MAX_PASSIVE_POWERUPS; j++)
                {
                    Powerup powerup = powerupLists[i].passives[j - MAX_ACTIVE_POWERUPS];
                    storageGrid[i][j] = new InventoryPowerupIcon(i, j, false, powerup, null);                    
                }
            }
            //storage
            for (int i = RetroGame.MAX_PLAYERS; i < RetroGame.MAX_PLAYERS + STORAGE_ACTIVE_ROWS + STORAGE_PASSIVE_ROWS; i++)
            {
                if (i < RetroGame.MAX_PLAYERS + STORAGE_ACTIVE_ROWS) // active
                {
                    storageGrid[i] = new InventoryPowerupIcon[STORAGE_ACTIVE_POWERUPS_PER_ROW];
                    for (int j = 0; j < STORAGE_ACTIVE_POWERUPS_PER_ROW; j++)
                    {
                        storageGrid[i][j] = new InventoryPowerupIcon(i, j, true, null, null);
                    }
                }
                else //passive
                {
                    storageGrid[i] = new InventoryPowerupIcon[STORAGE_PASSIVE_POWERUPS_PER_ROW];
                    for (int j = 0; j < STORAGE_PASSIVE_POWERUPS_PER_ROW; j++)
                    {
                        storageGrid[i][j] = new InventoryPowerupIcon(i, j, false, null, null);
                    }
                }
            }
        }

        public static void Reset()
        {
            selectedCursors = new Point?[2];
        }

        public static void ActivatePowerup(int playerIndex, InputAction action)
        {
            int activePowerupIndex = 0;
            switch (action)
            {
                case InputAction.Action1:
                    activePowerupIndex = 0;
                    break;
                case InputAction.Action2:
                    activePowerupIndex = 1;
                    break;
                case InputAction.Action3:
                    activePowerupIndex = 2;
                    break;
                case InputAction.Action4:
                    activePowerupIndex = 3;
                    break;
            }
            if (storageGrid[playerIndex][activePowerupIndex] != null && storageGrid[playerIndex][activePowerupIndex].HasPowerup)
                storageGrid[playerIndex][activePowerupIndex].Powerup.Activate(action);
        }

        public static bool EquipPowerup(Powerup powerup, int playerIndex, bool automaticallySetPowerupIcon = true)
        {
            Powerup[] powerups = null;
            if (powerup.Active)
                powerups = powerupLists[playerIndex].actives;
            else
                powerups = powerupLists[playerIndex].passives;
            for (int i = 0; i < powerups.Length; i++)
            {
                if (powerups[i] == null)
                {
                    powerups[i] = powerup;
                    if (automaticallySetPowerupIcon)
                    {
                        Point storageLocation = new Point(playerIndex, -1);
                        if (powerup.Active)
                            storageLocation.Y = i;
                        else
                            storageLocation.Y = i + MAX_ACTIVE_POWERUPS;
                        storageGrid[storageLocation.X][storageLocation.Y].SetPowerup(powerup);
                        flashAcquiredPowerup(storageGrid[storageLocation.X][storageLocation.Y]);
                    }
                    allCurrentlyOwnedPowerupTypes = null; //recalculate list
                    return true;
                }
            }
            StorePowerup(powerup);
            return false;
        }

        public static bool StorePowerup(Powerup powerup)
        {
            int initialRow = 0;
            int maxRow = 0;
            if (powerup.Active)
            {
                initialRow = RetroGame.MAX_PLAYERS;
                maxRow = RetroGame.MAX_PLAYERS + STORAGE_ACTIVE_ROWS;
            }
            else
            {
                initialRow = RetroGame.MAX_PLAYERS + STORAGE_ACTIVE_ROWS;
                maxRow = RetroGame.MAX_PLAYERS + STORAGE_ACTIVE_ROWS + STORAGE_PASSIVE_ROWS;
            }

            Point firstOpenSpace = new Point(-1, -1);
            for (int i = initialRow; i < maxRow; i++)
                for (int j = 0; j < storageGrid[i].Length; j++)
                {
                    if (!storageGrid[i][j].HasPowerup && firstOpenSpace.X < 0)
                    {
                        firstOpenSpace = new Point(i, j);
                    }
                    if (storageGrid[i][j].HasPowerup && storageGrid[i][j].Powerup.GetType() == powerup.GetType())
                        return false;
                }
            storageGrid[firstOpenSpace.X][firstOpenSpace.Y].SetPowerup(powerup);
            allCurrentlyOwnedPowerupTypes = null; //recalculate list
            flashAcquiredPowerup(storageGrid[firstOpenSpace.X][firstOpenSpace.Y]);
            return true;
        }

        public static void UnequipPowerup(int playerIndex, bool active, Powerup powerup)
        {
            Powerup[] powerups = null;
            if (active)
                powerups = powerupLists[playerIndex].actives;
            else
                powerups = powerupLists[playerIndex].passives;
            for (int i = 0; i < powerups.Length; i++)
            {
                if (powerups[i] == powerup)
                {
                    powerups[i] = null;
                    if (powerup.Active)
                        storageGrid[playerIndex][i].SetPowerup(null);
                    else
                        storageGrid[playerIndex][i + MAX_ACTIVE_POWERUPS].SetPowerup(null);
                    break;
                }
            }
        }

        public static void SortPowerups(int playerIndex)
        {
            PowerupList powerupList = powerupLists[playerIndex];
            Array.Sort(powerupList.actives);
            Array.Sort(powerupList.passives);
        }

        private static void calculateAllCurrentlyOwnedPowerupTypes()
        {
            //all equipped and unequipped powerups
            allCurrentlyOwnedPowerupTypes = new List<Type>();
            for (int i = 0; i < storageGrid.Length; i++)
            {
                if (i < RetroGame.MAX_PLAYERS && i >= RetroGame.NUM_PLAYERS)
                    continue;
                if (storageGrid[i] != null)
                    for (int j = 0; j < storageGrid[i].Length; j++)
                    {
                        if(storageGrid[i][j].HasPowerup)
                            allCurrentlyOwnedPowerupTypes.Add(storageGrid[i][j].Powerup.GetType());
                    }
            }
        }

        public static void UpdateCursorPosition(int cursorIndex, InputAction direction)
        {
            Point cursor = cursors[cursorIndex];
            int powerupsInRow = storageGrid[cursor.Y].Length;
            switch (direction)
            {
                case InputAction.Up:
                    if (cursor.Y < RetroGame.MAX_PLAYERS)
                    {
                        if (cursor.X > 0)
                            cursor.X--;
                        else
                            cursor.X = MAX_ACTIVE_POWERUPS + MAX_PASSIVE_POWERUPS - 1;
                    }
                    else
                    {
                        if (cursor.Y == RetroGame.MAX_PLAYERS)
                        {
                            cursor.Y = cursorIndex; // switch to equipped powerup icons
                            cursor.X = 0;
                        }
                        else if (cursor.Y > RetroGame.MAX_PLAYERS)
                            cursor.Y--;
                        else
                            cursor.Y = STORAGE_ACTIVE_ROWS + STORAGE_PASSIVE_ROWS + RetroGame.MAX_PLAYERS - 1;
                        int powerupsInNewRow = storageGrid[cursor.Y].Length;
                        float percCurrentRow = (float)cursor.X / powerupsInRow;
                        int nextX = (int)(powerupsInNewRow * percCurrentRow);
                        cursor.X = nextX;
                    }
                    break;
                case InputAction.Down:
                    if (cursor.Y < RetroGame.MAX_PLAYERS)
                    {
                        if (cursor.X < MAX_ACTIVE_POWERUPS + MAX_PASSIVE_POWERUPS - 1)
                            cursor.X++;
                        else
                            cursor.X = 0;
                    }
                    else
                    {
                        if (cursor.Y < STORAGE_ACTIVE_ROWS + STORAGE_PASSIVE_ROWS + RetroGame.MAX_PLAYERS - 1)
                            cursor.Y++;
                        else
                            cursor.Y = RetroGame.MAX_PLAYERS;
                        int powerupsInNewRow = storageGrid[cursor.Y].Length;
                        float percCurrentRow = (float)cursor.X / powerupsInRow;
                        int nextX = (int)(powerupsInNewRow * percCurrentRow);
                        cursor.X = nextX;
                    }
                    break;
                case InputAction.Left:
                    if (cursor.Y < RetroGame.MAX_PLAYERS)
                    {
                        cursor.Y = RetroGame.MAX_PLAYERS;
                        int powerupsInNewRow = storageGrid[cursor.Y].Length;
                        if (cursorIndex == Player.One)
                            cursor.X = powerupsInNewRow - 1;
                        else if (cursorIndex == Player.Two)
                            cursor.X = 0;
                    }
                    else
                    {
                        if (cursor.X == 0 && cursorIndex == Player.One)
                        {
                            cursor.Y = cursorIndex; // switch to equipped powerup icons
                            cursor.X = 0; // switch to equipped powerup icons
                        }
                        else if (cursor.X > 0)
                            cursor.X--;
                        else
                            cursor.X = powerupsInRow - 1;
                    }
                    break;
                case InputAction.Right:
                    if (cursor.Y < RetroGame.MAX_PLAYERS)
                    {
                        cursor.Y = RetroGame.MAX_PLAYERS;
                        int powerupsInNewRow = storageGrid[cursor.Y].Length;
                        if (cursorIndex == Player.One)
                            cursor.X = 0;
                        else if (cursorIndex == Player.Two)
                            cursor.X = powerupsInNewRow - 1;
                    }
                    else
                    {
                        if (cursor.X == (powerupsInRow - 1) && cursorIndex == Player.Two)
                        {
                            cursor.Y = cursorIndex; // switch to equipped powerup icons
                            cursor.X = 0; // switch to equipped powerup icons
                        }
                        else if (cursor.X < powerupsInRow - 1)
                            cursor.X++;
                        else
                            cursor.X = 0;
                    }
                    break;
            }
            cursors[cursorIndex] = cursor;

            //reset bob animation
            cursorBobMultipliers[cursorIndex] = 1;
            cursorBobOffsets[cursorIndex] = 0;

            //skip over selected powerup icons
            bool onSelectedCursor = false;
            foreach (Point? selectedCursor in selectedCursors)
            {
                if (selectedCursor != null && selectedCursor.Value == cursor)
                {
                    onSelectedCursor = true;
                    break;
                }
            }
            if (onSelectedCursor)
                UpdateCursorPosition(cursorIndex, direction);
        }
        
        public static void UpdateCursorBobAnimation(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();
            for (int i = 0; i < cursorBobOffsets.Length; i++)
            {
                cursorBobOffsets[i] += BOB_OFFSET_VELOCITY * cursorBobMultipliers[i] * seconds;
                if (cursorBobOffsets[i] < MIN_BOB_OFFSET || cursorBobOffsets[i] >= MAX_BOB_OFFSET)
                    cursorBobMultipliers[i] *= -1;
            }
        }

        public static void UpdateWarning(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();
            if (warning != "")
            {
                warningFlashTime += seconds;
                if (warningFlashTime >= WARNING_FLASH_TIME)
                    warningFlashTime = WARNING_FLASH_TIME;
                float interp = warningFlashTime / WARNING_FLASH_TIME;
                warningColor = Color.Lerp(WARNING_INITIAL_COLOR, WARNING_FINAL_COLOR, interp);
            }
        }

        public static void UpdateAcquired(GameTime gameTime)
        {
            float seconds = gameTime.getSeconds();
            bool acquiredIconInStorage = false;
            if (acquiredIcons.Count > 0)
            {
                for(int i = 0; i < acquiredIcons.Count; i++)
                {
                    AcquiredIcon acquiredIcon = acquiredIcons[i];
                    acquiredIcon.time += seconds;
                    if (acquiredIcon.time >= ACQUIRED_FLASH_TIME)
                    {
                        acquiredIcons.RemoveAt(i);
                        i--;
                    }
                    if (acquiredIcon.pos.X >= RetroGame.MAX_PLAYERS)
                    {
                        acquiredIconInStorage = true;
                    }
                }
            }
            shouldAlsoDrawStorageIcons = acquiredIconInStorage;
        }

        public static void SelectWithCursor(int cursorIndex)
        {
            if (selectedCursors[cursorIndex] == null)
            {
                selectedCursors[cursorIndex] = cursors[cursorIndex];
                cursors[cursorIndex] = getNextCursorPositionClosestToCenter(cursors[cursorIndex]);

                int otherCursor = Player.One;
                if (cursorIndex == Player.One)
                    otherCursor = Player.Two;
                if (selectedCursors[cursorIndex] == cursors[otherCursor])
                    cursors[otherCursor] = getNextCursorPositionClosestToCenter(cursors[otherCursor]);
                SoundManager.PlaySoundOnce("ButtonForward");
            }
            else
            {
                bool swapSuccessful = false;
                InventoryPowerupIcon alreadySelectedIcon = storageGrid[selectedCursors[cursorIndex].Value.Y][selectedCursors[cursorIndex].Value.X];
                InventoryPowerupIcon newlySelectedIcon = storageGrid[cursors[cursorIndex].Y][cursors[cursorIndex].X];
                if (alreadySelectedIcon.Active == newlySelectedIcon.Active)
                {
                    Hero hero = RetroGame.getHeroes()[cursorIndex];
                    if ((selectedCursors[cursorIndex].Value.Y < RetroGame.MAX_PLAYERS && cursors[cursorIndex].Y < RetroGame.MAX_PLAYERS) ||
                        (selectedCursors[cursorIndex].Value.Y >= RetroGame.MAX_PLAYERS && cursors[cursorIndex].Y >= RetroGame.MAX_PLAYERS))
                    {
                        alreadySelectedIcon.swapWith(newlySelectedIcon, hero, null);
                        swapSuccessful = true;
                    }
                    else
                    {
                        if (selectedCursors[cursorIndex].Value.Y < RetroGame.MAX_PLAYERS) //swap out
                        {
                            if (newlySelectedIcon.Powerup == null || !hero.HasPowerup(newlySelectedIcon.Powerup.GenericName) ||
                                (alreadySelectedIcon.Powerup != null && (newlySelectedIcon.Powerup.GenericName == alreadySelectedIcon.Powerup.GenericName)))
                            {
                                alreadySelectedIcon.swapWith(newlySelectedIcon, hero, false);
                                swapSuccessful = true;
                            }
                            else
                            {
                                InventoryPowerupIcon sameGenericNameIcon = null;
                                foreach (InventoryPowerupIcon icon in storageGrid[alreadySelectedIcon.rowIndex])
                                {
                                    if (icon.Powerup != null && (icon.Powerup.GenericName == newlySelectedIcon.Powerup.GenericName))
                                    {
                                        sameGenericNameIcon = icon;
                                        break;
                                    }
                                }
                                displaySwapWarning(warning = WARNING_SAME_GENERIC_NAME, sameGenericNameIcon, newlySelectedIcon);
                            }
                        }
                        else //swap in
                        {
                            if (alreadySelectedIcon.Powerup == null || !hero.HasPowerup(alreadySelectedIcon.Powerup.GenericName) ||
                                (newlySelectedIcon.Powerup != null && (alreadySelectedIcon.Powerup.GenericName == newlySelectedIcon.Powerup.GenericName)))
                            {
                                alreadySelectedIcon.swapWith(newlySelectedIcon, hero, true);
                                swapSuccessful = true;
                            }
                            else
                            {
                                InventoryPowerupIcon sameGenericNameIcon = null;
                                foreach (InventoryPowerupIcon icon in storageGrid[newlySelectedIcon.rowIndex])
                                {
                                    if (icon.Powerup != null && (icon.Powerup.GenericName == alreadySelectedIcon.Powerup.GenericName))
                                    {
                                        sameGenericNameIcon = icon;
                                        break;
                                    }
                                }
                                displaySwapWarning(warning = WARNING_SAME_GENERIC_NAME, alreadySelectedIcon, sameGenericNameIcon);
                            }
                        }
                    }
                }
                else
                    displaySwapWarning(WARNING_BAD_ACTIVENESS, alreadySelectedIcon, newlySelectedIcon);
                if (swapSuccessful)
                {
                    displaySwapWarning("");
                    selectedCursors[cursorIndex] = null;
                    SoundManager.PlaySoundOnce("ButtonForward");
                }
                else
                {
                    SoundManager.PlaySoundOnce("ButtonFailure");
                }
            }
        }

        private static void displaySwapWarning(string warningString, InventoryPowerupIcon warnedIcon1 = null, InventoryPowerupIcon warnedIcon2 = null)
        {
            warning = warningString;
            warningFlashTime = 0;
            if (warning != "" && warnedIcon1 != null && warnedIcon2 != null)
            {
                warnedIcons = new Point[] { new Point(warnedIcon1.rowIndex, warnedIcon1.iconIndex), new Point(warnedIcon2.rowIndex, warnedIcon2.iconIndex) };
            }
            else
            {
                warnedIcons = null;
            }
        }

        private static void flashAcquiredPowerup(InventoryPowerupIcon icon)
        {
            if (icon != null)
            {
                acquiredIcons.Add(new AcquiredIcon{ pos = new Point(icon.rowIndex, icon.iconIndex), time = 0 });
            }
        }

        public static void GoBack(int cursorIndex, bool exitScreenIfNothingSelected)
        {
            SoundManager.PlaySoundOnce("ButtonBack");
            if (selectedCursors[cursorIndex] == null)
            {
                if (exitScreenIfNothingSelected)
                {
                    RetroGame.PopScreen();
                    return;
                }
            }
            else
            {
                Point oldSelectedPosition = selectedCursors[cursorIndex].Value;
                selectedCursors[cursorIndex] = null;
                cursors[cursorIndex] = oldSelectedPosition;
            }
        }

        private static Point getNextCursorPositionClosestToCenter(Point cursor)
        {
            int directionTowardsCenter = 0;
            if (cursor.X >= storageGrid[cursor.Y].Length / 2)
                directionTowardsCenter = -1;
            else
                directionTowardsCenter = 1;

            Point newCursor = cursor;
            const int ATTEMPT_LIMIT = 4;
            for (int i = 0; i < ATTEMPT_LIMIT; i++)
            {
                newCursor.X += directionTowardsCenter;
                bool onSelectedCursor = false;
                foreach (Point? selectedCursor in selectedCursors)
                {
                    if (selectedCursor != null && selectedCursor.Value == newCursor)
                    {
                        onSelectedCursor = true;
                        break;
                    }
                }
                if (!onSelectedCursor)
                    break;
            }
            return newCursor;
        }

        public static void DrawEquipped(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < RetroGame.NUM_PLAYERS; i++)
            {
                for (int j = 0; j < storageGrid[i].Length; j++)
                {
                    storageGrid[i][j].Draw(spriteBatch, HUD.hudScale);
                }
            }
            if (acquiredIcons.Count > 0)
            {
                foreach (AcquiredIcon acquiredIcon in acquiredIcons)
                {
                    storageGrid[acquiredIcon.pos.X][acquiredIcon.pos.Y].DrawAcquired(spriteBatch, acquiredIcon.time);
                }
            }
            if (shouldAlsoDrawStorageIcons)
            {
                DrawStorageIcons(spriteBatch);
            }
            DrawHeroInfo(spriteBatch);
        }

        public static void DrawHeroInfo(SpriteBatch spriteBatch)
        {
            //draw Hero info
            Vector2 screenSize = RetroGame.screenSize;
            foreach (Hero hero in RetroGame.getHeroes())
            {
                float heroUIHorizontalOffsetRelative = 0;
                float heroUIOriginXRelative = 0;
                if (hero.playerIndex == Player.One)
                {
                    heroUIHorizontalOffsetRelative = HERO_UI_HORIZONTAL_OFFSET_RELATIVE;
                    heroUIOriginXRelative = 0;
                }
                else if (hero.playerIndex == Player.Two)
                {
                    heroUIHorizontalOffsetRelative = (1 - HERO_UI_HORIZONTAL_OFFSET_RELATIVE);
                    heroUIOriginXRelative = 1;

                }
                Vector2 idPosition = new Vector2(heroUIHorizontalOffsetRelative, HERO_ID_VERTICAL_OFFSET_RELATIVE) * screenSize;
                Vector2 namePosition = new Vector2(heroUIHorizontalOffsetRelative, HERO_NAME_VERTICAL_OFFSET_RELATIVE) * screenSize;
                Vector2 healthPosition = new Vector2(heroUIHorizontalOffsetRelative, HERO_HEALTH_VERTICAL_OFFSET_RELATIVE) * screenSize;

                float modifiedScale = HUD.modifiedScale;
                SpriteFont uiFont = RetroGame.FONT_PIXEL_SMALL;
                string id = "#" + hero.prisonerID.ToString("0000");
                Vector2 stringOrigin = new Vector2(heroUIOriginXRelative * uiFont.MeasureString(id).X, 0);
                spriteBatch.DrawString(uiFont, id, idPosition, Color.White, 0, stringOrigin, HERO_ID_TEXT_BASE_SCALE * modifiedScale, SpriteEffects.None, 0);
                string name = hero.prisonerName;
                stringOrigin = new Vector2(heroUIOriginXRelative * uiFont.MeasureString(name).X, 0);
                spriteBatch.DrawString(uiFont, name, namePosition, Color.White, 0, stringOrigin, HERO_NAME_TEXT_BASE_SCALE * modifiedScale, SpriteEffects.None, 0);

                float healthPerc = hero.health / Hero.INITIAL_HEALTH;
                float healthBarWidth = HERO_HEALTH_WIDTH_RELATIVE * screenSize.X;
                float healthBarHeight = HERO_HEALTH_HEIGHT_RELATIVE * screenSize.Y;
                Color borderColor = (healthPerc > 0) ? Color.White : Color.Black;
                Vector2 healthOrigin = new Vector2(heroUIOriginXRelative * healthBorderTex.Width, 0);
                spriteBatch.Draw(healthFillingTex, new Rectangle((int)healthPosition.X, (int)healthPosition.Y, (int)(healthBarWidth * healthPerc), (int)healthBarHeight),
                    null, Color.White, 0, healthOrigin, SpriteEffects.None, 0);
                spriteBatch.Draw(healthBorderTex, new Rectangle((int)healthPosition.X, (int)healthPosition.Y, (int)(healthBarWidth), (int)healthBarHeight),
                    null, borderColor, 0, healthOrigin, SpriteEffects.None, 0);
            }
        }

        public static void DrawStorageIcons(SpriteBatch spriteBatch)
        {

            for (int i = 0; i < storageGrid.Length; i++)
            {
                if (i < RetroGame.MAX_PLAYERS && i >= RetroGame.NUM_PLAYERS)
                    continue;
                if (storageGrid[i] != null)
                    for (int j = 0; j < storageGrid[i].Length; j++)
                    {
                        storageGrid[i][j].Draw(spriteBatch, HUD.hudScale);
                    }
            }
            for (int i = 0; i < RetroGame.NUM_PLAYERS; i++)
            {
                if (storageGrid[i] != null)
                    for (int j = 0; j < storageGrid[i].Length; j++)
                    {
                        storageGrid[i][j].DrawBinding(spriteBatch, HUD.hudScale);
                    }
            }
            if (warnedIcons != null)
            {
                storageGrid[warnedIcons[0].X][warnedIcons[0].Y].DrawWarning(spriteBatch);
                storageGrid[warnedIcons[1].X][warnedIcons[1].Y].DrawWarning(spriteBatch);
            }
            if (acquiredIcons.Count > 0)
            {
                foreach (AcquiredIcon acquiredIcon in acquiredIcons)
                {
                    storageGrid[acquiredIcon.pos.X][acquiredIcon.pos.Y].DrawAcquired(spriteBatch, acquiredIcon.time);
                }
            }
        }

        public static void DrawStorage(SpriteBatch spriteBatch)
        {
            if (warning != ""){
                Vector2 warningPos = RetroGame.screenSize * WARNING_POS;
                Vector2 dims = RetroGame.FONT_PIXEL_SMALL.MeasureString(warning);
                spriteBatch.DrawString(RetroGame.FONT_PIXEL_SMALL, warning, warningPos, warningColor, 0, dims / 2, WARNING_SCALE * HUD.modifiedScale, SpriteEffects.None, 0);
            }

            DrawStorageIcons(spriteBatch);

            Point cursor1 = cursors[Player.One];
            Point cursor2 = cursors[Player.Two];
            if (cursor1 == cursor2 && RetroGame.NUM_PLAYERS == 2)
            {
                Vector2 offset1 = CURSOR_OFFSETS_DOUBLE[Player.One];
                offset1.Y += cursorBobOffsets[Player.One];
                Color colorOne = RetroGame.getHeroes()[Player.One].color;
                Vector2 offset2 = CURSOR_OFFSETS_DOUBLE[Player.Two];
                offset2.Y += cursorBobOffsets[Player.Two];
                Color colorTwo = RetroGame.getHeroes()[Player.Two].color;
                storageGrid[cursor1.Y][cursor1.X].DrawTwoCursors(spriteBatch, colorOne, colorTwo, offset1, offset2, HUD.hudScale);
            }
            else
            {
                Vector2 offset1 = CURSOR_OFFSET_SINGLE;
                offset1.Y += cursorBobOffsets[Player.One];
                Color colorOne = RetroGame.getHeroes()[Player.One].color;
                storageGrid[cursor1.Y][cursor1.X].DrawOneCursor(spriteBatch, colorOne, offset1, HUD.hudScale);
                if (RetroGame.NUM_PLAYERS == 2)
                {
                    Vector2 offset2 = CURSOR_OFFSET_SINGLE;
                    offset2.Y += cursorBobOffsets[Player.Two];
                    Color colorTwo = RetroGame.getHeroes()[Player.Two].color;
                    storageGrid[cursor2.Y][cursor2.X].DrawOneCursor(spriteBatch, colorTwo, offset2, HUD.hudScale);
                }
            }

            float modifiedScale = HUD.modifiedScale;
            if (storageGrid[cursor1.Y][cursor1.X].HasPowerup)
                storageGrid[cursor1.Y][cursor1.X].DrawDescription(spriteBatch, RetroGame.screenSize * POWERUP_DESCRIPTION_RELATIVE_POSITIONS[Player.One], POWERUP_DESCRIPTION_COLOR, POWERUP_DESCRIPTION_BASE_SCALE * modifiedScale);
            if (RetroGame.NUM_PLAYERS >= 2 && storageGrid[cursor2.Y][cursor2.X].HasPowerup)
                storageGrid[cursor2.Y][cursor2.X].DrawDescription(spriteBatch, RetroGame.screenSize * POWERUP_DESCRIPTION_RELATIVE_POSITIONS[Player.Two], POWERUP_DESCRIPTION_COLOR, POWERUP_DESCRIPTION_BASE_SCALE * modifiedScale);

            for (int i = 0; i < selectedCursors.Length; i++)
            {
                Vector2 offset = CURSOR_OFFSET_SINGLE;
                offset.Y += MIN_BOB_OFFSET;
                if (selectedCursors[i] != null)
                    storageGrid[selectedCursors[i].Value.Y][selectedCursors[i].Value.X].DrawOneCursor(spriteBatch, RetroGame.getHeroes()[i].color.withAlpha(150), offset, HUD.hudScale);
            }
            DrawHeroInfo(spriteBatch);
        }

        internal class InventoryPowerupIcon
        {
            public bool Active { get; private set; }
            public Powerup Powerup { get; private set; }
            private InputAction? action;
            public int iconIndex { get; private set; }
            public int rowIndex { get; private set; }
            public bool HasPowerup { get { return Powerup != null; } private set { } }

            private int xPos, yPos, size;

            public InventoryPowerupIcon(int rowIndex, int iconIndex, bool active, Powerup powerup, InputAction? action)
            {
                this.Powerup = powerup;
                this.action = action;
                this.iconIndex = iconIndex;
                this.rowIndex = rowIndex;
                this.Active = active;
            }

            public void SetPowerup(Powerup powerup)
            {
                this.Powerup = powerup;

                Powerup[] powerups = null;
                int index = -1;
                if (rowIndex < RetroGame.MAX_PLAYERS){
                    if (Active)
                    {
                        powerups = powerupLists[rowIndex].actives;
                        index = iconIndex;
                    }
                    else
                    {
                        powerups = powerupLists[rowIndex].passives;
                        index = iconIndex - MAX_ACTIVE_POWERUPS;
                    }
                    powerups[index] = powerup;
                }
            }

            private int getXForIcon(int rowIndex, int iconIndex, int iconSize, float hudScale)
            {
                if (rowIndex == Player.One)
                {
                    return (int)(SPACE_BETWEEN_CELLS * hudScale);
                }
                else if (rowIndex == Player.Two)
                {
                    return (int)(RetroGame.screenSize.X - (SPACE_BETWEEN_CELLS * hudScale));
                }
                else if (rowIndex < RetroGame.MAX_PLAYERS + STORAGE_ACTIVE_ROWS + STORAGE_PASSIVE_ROWS)
                {
                    int cellsInRow = 0;
                    if (rowIndex < RetroGame.MAX_PLAYERS + STORAGE_ACTIVE_ROWS)
                        cellsInRow = STORAGE_ACTIVE_POWERUPS_PER_ROW;
                    else
                        cellsInRow = STORAGE_PASSIVE_POWERUPS_PER_ROW;
                    float totalRowWidth = cellsInRow * (iconSize + (SPACE_BETWEEN_CELLS * hudScale));
                    float center = RetroGame.screenSize.X / 2;
                    return (int)(iconIndex * ((SPACE_BETWEEN_CELLS * hudScale) + iconSize) - totalRowWidth / 2 + center);
                }
                return 0;
            }

            private int getYForIcon(int rowIndex, int iconIndex, int iconSize, float hudScale)
            {
                if (rowIndex >= 0 && rowIndex < RetroGame.MAX_PLAYERS)
                {
                    if (iconIndex < MAX_ACTIVE_POWERUPS)
                    {
                        float baseY = (EQUIPPED_BASE_VERTICAL_OFFSET_RELATIVE * RetroGame.screenSize.Y);
                        int activeIconIndex = iconIndex;
                        return (int)(activeIconIndex * ((SPACE_BETWEEN_CELLS * hudScale) + iconSize) + baseY);
                    }
                    else if (iconIndex < MAX_ACTIVE_POWERUPS + MAX_PASSIVE_POWERUPS)
                    {
                        float baseY = (EQUIPPED_BASE_VERTICAL_OFFSET_RELATIVE * RetroGame.screenSize.Y) + (MAX_ACTIVE_POWERUPS * (SPACE_BETWEEN_CELLS + (CELL_SIZE * ACTIVE_POWERUP_RELATIVE_SIZE)) * hudScale);
                        int passiveIconIndex = iconIndex - MAX_ACTIVE_POWERUPS;
                        return (int)(passiveIconIndex * ((SPACE_BETWEEN_CELLS * hudScale) + iconSize) + baseY);
                    }
                }
                else if (rowIndex < RetroGame.MAX_PLAYERS + STORAGE_ACTIVE_ROWS)
                {
                    float baseY = RetroGame.screenSize.Y * STORAGE_POSITION_RELATIVE.Y;
                    int activeRowIndex = rowIndex - RetroGame.MAX_PLAYERS;
                    return (int)(activeRowIndex * (iconSize + (SPACE_BETWEEN_CELLS * hudScale)) + baseY);
                }
                else if (rowIndex < RetroGame.MAX_PLAYERS + STORAGE_ACTIVE_ROWS + STORAGE_PASSIVE_ROWS)
                {
                    float baseY = RetroGame.screenSize.Y * STORAGE_POSITION_RELATIVE.Y;
                    baseY += STORAGE_ACTIVE_ROWS * ((SPACE_BETWEEN_CELLS + (CELL_STORAGE_SIZE * ACTIVE_POWERUP_STORAGE_RELATIVE_SIZE)) * hudScale);
                    int passiveRowIndex = rowIndex - (RetroGame.MAX_PLAYERS + STORAGE_ACTIVE_ROWS);
                    return (int)(passiveRowIndex * (iconSize + (SPACE_BETWEEN_CELLS * hudScale)) + baseY);
                }
                return 0;
            }

            private int getSizeForIcon(int rowIndex, bool active, float hudScale)
            {
                if (rowIndex >= 0 && rowIndex < RetroGame.MAX_PLAYERS)
                {
                    if (active)
                        return (int)(CELL_SIZE * ACTIVE_POWERUP_RELATIVE_SIZE * hudScale);
                    else
                        return (int)(CELL_SIZE * hudScale);
                }
                else if (rowIndex < RetroGame.MAX_PLAYERS + STORAGE_ACTIVE_ROWS + STORAGE_PASSIVE_ROWS)
                {
                    if (active)
                        return (int)(CELL_STORAGE_SIZE * ACTIVE_POWERUP_STORAGE_RELATIVE_SIZE * hudScale);
                    else
                        return (int)(CELL_STORAGE_SIZE * hudScale);
                }
                return 0;
            }

            private Vector2 getOriginOffsetForIcon(int rowIndex, int iconIndex)
            {
                if (rowIndex == Player.One)
                {
                    return Vector2.Zero;
                }
                else if (rowIndex == Player.Two)
                {
                    return new Vector2(1, 0);
                }
                return Vector2.Zero;
            }

            public void swapWith(InventoryPowerupIcon secondIcon, Hero hero, bool? equipOrUnequipThis)
            {
                Powerup secondPowerup = secondIcon.Powerup;
                Powerup firstPowerup = this.Powerup;
                if (equipOrUnequipThis != null)
                {
                    if (equipOrUnequipThis.Value) // equipping
                    {
                        if (secondPowerup != null)
                            hero.RemovePowerup(secondPowerup.GenericName, false);
                        if (firstPowerup != null)
                            firstPowerup = hero.AddPowerup(firstPowerup.GetType(), false);
                    }
                    else // uneqipping
                    {
                        if (firstPowerup != null)
                            hero.RemovePowerup(firstPowerup.GenericName, false);
                        if (secondPowerup != null)
                            secondPowerup = hero.AddPowerup(secondPowerup.GetType(), false);
                    }
                }
                this.SetPowerup(secondPowerup);
                secondIcon.SetPowerup(firstPowerup);
            }

            public void Draw(SpriteBatch spriteBatch, float hudScale)
            {
                hudScale = (hudScale + 1) / 2;
                float spaceBetweenCells = SPACE_BETWEEN_CELLS * hudScale;
                size = getSizeForIcon(rowIndex, Active, hudScale);
                xPos = getXForIcon(rowIndex, iconIndex, size, hudScale);
                yPos = getYForIcon(rowIndex, iconIndex, size, hudScale);
                Vector2 originOffset = getOriginOffsetForIcon(rowIndex, iconIndex);

                spriteBatch.Draw(RetroGame.PIXEL, new Rectangle(xPos, yPos, size, size), null, Color.White, 0, originOffset, SpriteEffects.None, 0);
                if (Powerup != null)
                {
                    Texture2D powerupIcon = Powerup.Icon;
                    bool displayPowerupBind = Powerup.Active;
                    float powerupCharge = Powerup.GetPowerupCharge();
                    if (powerupIcon != null)
                    {
                        float powerupIconScale = (float)size / POWERUP_ICON_TEXTURE_SIZE;
                        spriteBatch.Draw(powerupIcon, new Vector2(xPos, yPos), null, Color.White, 0, originOffset * powerupIcon.Width, powerupIconScale, SpriteEffects.None, 0);
                    }
                    if (rowIndex < RetroGame.MAX_PLAYERS)
                    {
                        if (powerupCharge < 1)
                            spriteBatch.Draw(RetroGame.PIXEL, new Rectangle((int)xPos, (int)yPos, (int)size, (int)size), null, COOLDOWN_COLOR.withAlpha(75), 0, originOffset, SpriteEffects.None, 0);
                        float maskSize = size + 1;
                        spriteBatch.Draw(RetroGame.PIXEL, new Rectangle((int)xPos, (int)(yPos), (int)size, (int)(maskSize * (1 - powerupCharge))), null, COOLDOWN_COLOR.withAlpha(200), 0, originOffset, SpriteEffects.None, 0);
                    }

                    float bindingOffset = 0;
                    if (rowIndex < RetroGame.MAX_PLAYERS && Active)
                    {
                        switch (rowIndex)
                        {
                            case 0:
                                bindingOffset = size + spaceBetweenCells;
                                break;
                            case 1:
                                bindingOffset = -(size + spaceBetweenCells);
                                break;
                        }
                        RetroGame.getHeroes()[rowIndex].DrawBinding(spriteBatch, action.Value, new Vector2(xPos + bindingOffset, yPos + size / 2), originOffset, hudScale);
                    }
                }
            }

            public void DrawWarning(SpriteBatch spriteBatch)
            {
                Vector2 originOffset = getOriginOffsetForIcon(rowIndex, iconIndex);
                float interp = warningFlashTime / WARNING_FLASH_TIME;
                spriteBatch.Draw(RetroGame.PIXEL, new Rectangle(xPos, yPos, size, size), null, Color.Lerp(WARNING_INITIAL_COLOR, Color.Transparent, interp), 0, originOffset, SpriteEffects.None, 0);
            }

            public void DrawAcquired(SpriteBatch spriteBatch, float acquiredFlashTime)
            {
                Vector2 originOffset = getOriginOffsetForIcon(rowIndex, iconIndex);
                float interp = acquiredFlashTime / ACQUIRED_FLASH_TIME;
                spriteBatch.Draw(RetroGame.PIXEL, new Rectangle(xPos, yPos, size, size), null, Color.Lerp(ACQUIRED_COLOR, Color.Transparent, interp), 0, originOffset, SpriteEffects.None, 0);
            }

            public void DrawBinding(SpriteBatch spriteBatch, float hudScale)
            {
                hudScale = (hudScale + 1) / 2;
                float spaceBetweenCells = SPACE_BETWEEN_CELLS * hudScale;
                Vector2 originOffset = getOriginOffsetForIcon(rowIndex, iconIndex);
                float bindingOffset = 0;
                if (rowIndex < RetroGame.MAX_PLAYERS && Active)
                {
                    switch (rowIndex)
                    {
                        case 0:
                            bindingOffset = size + spaceBetweenCells;
                            break;
                        case 1:
                            bindingOffset = -(size + spaceBetweenCells);
                            break;
                    }
                    RetroGame.getHeroes()[rowIndex].DrawBinding(spriteBatch, action.Value, new Vector2(xPos + bindingOffset, yPos + size / 2), originOffset, hudScale);
                }
            }

            public void DrawDescription(SpriteBatch spriteBatch, Vector2 position, Color descriptionColor, float textScale)
            {
                spriteBatch.DrawString(RetroGame.FONT_DEBUG, Powerup.GenericName + ": " + Powerup.SpecificName, position, Powerup.TintColor, 0, Vector2.Zero, textScale, SpriteEffects.None, 0);
                position.Y += RetroGame.FONT_DEBUG.MeasureString(Powerup.GenericName + ": " + Powerup.SpecificName).Y * textScale;
                spriteBatch.DrawString(RetroGame.FONT_DEBUG, Powerup.Description, position, descriptionColor, 0, Vector2.Zero, textScale, SpriteEffects.None, 0);
            }

            private float getArrowRotation(int rowIndex)
            {
                if (rowIndex == Player.One)
                    return (float)-Math.PI / 2;
                else if (rowIndex == Player.Two)
                    return (float)Math.PI / 2;
                else
                    return (float)Math.PI;
            }

            private int getArrowXPos(Vector2 relativeOffset)
            {
                if (rowIndex < RetroGame.MAX_PLAYERS)
                {
                    if (rowIndex == Player.One)
                        return (int)(xPos + (size * (relativeOffset.Y + 1)));
                    else //if (rowIndex == Player.Two)
                        return (int)(xPos - (size * (relativeOffset.Y + 1)));
                }
                else
                    return (int)(xPos + (size * relativeOffset.X));
            }

            private int getArrowYPos(Vector2 relativeOffset)
            {
                if (rowIndex < RetroGame.MAX_PLAYERS)
                {
                    return (int)(yPos + (size * relativeOffset.X));
                }
                else
                    return (int)(yPos - (size * relativeOffset.Y));
            }

            public void DrawOneCursor(SpriteBatch spriteBatch, Color color, Vector2 relativeOffset, float hudScale)
            {
                float rotation = getArrowRotation(rowIndex);
                int cursorSize = (int)(arrowTex.Width * CURSOR_BASE_SCALE * hudScale);
                int arrowXPos = getArrowXPos(relativeOffset);
                int arrowYPos = getArrowYPos(relativeOffset);

                spriteBatch.Draw(arrowTex, new Rectangle(arrowXPos, arrowYPos, cursorSize, cursorSize), null, color, rotation, new Vector2(arrowTex.Width / 2, 0), SpriteEffects.None, 0);
                //spriteBatch.Draw(RetroGame.PIXEL, new Rectangle(arrowXPos, arrowYPos, 3, 3), null, Color.Orange, rotation, new Vector2(0.5f, 0), SpriteEffects.None, 0);
            }

            public void DrawTwoCursors(SpriteBatch spriteBatch, Color color1, Color color2, Vector2 relativeOffset1, Vector2 relativeOffset2, float hudScale)
            {
                float rotation = getArrowRotation(rowIndex);
                int cursorSize = (int)(arrowTex.Width * CURSOR_BASE_SCALE * hudScale);

                int arrowXPos = getArrowXPos(relativeOffset1);
                int arrowYPos = getArrowYPos(relativeOffset1);
                spriteBatch.Draw(arrowTex, new Rectangle(arrowXPos, arrowYPos, cursorSize, cursorSize), null, color1, rotation, new Vector2(arrowTex.Width / 2, 0), SpriteEffects.None, 0);
                arrowXPos = getArrowXPos(relativeOffset2);
                arrowYPos = getArrowYPos(relativeOffset2);
                spriteBatch.Draw(arrowTex, new Rectangle(arrowXPos, arrowYPos, cursorSize, cursorSize), null, color2, rotation, new Vector2(arrowTex.Width / 2, 0), SpriteEffects.None, 0);
            }
        }

        public static InventorySaveState GetState()
        {
            return new InventorySaveState(null);
        }

        public static void LoadState(InventorySaveState state)
        {
            state.Restore();
        }
    }

    public class PowerupTypeList
    {
        public string[] actives;
        public string[] passives;

        private PowerupTypeList() { }
        internal PowerupTypeList(Inventory.PowerupList lists)
        {
            actives = new string[lists.actives.Length];
            for (int i = 0; i < actives.Length; i++)
            {
                if (lists.actives[i] != null)
                    actives[i] = lists.actives[i].GetType().FullName;
            }
            passives = new string[lists.passives.Length];
            for (int i = 0; i < passives.Length; i++)
            {
                if (lists.passives[i] != null)
                    passives[i] = lists.passives[i].GetType().FullName;
            }
        }
    }

    public class InventorySaveState
    {
        public PowerupTypeList[] typeLists;
        public List<string> storedPowerupTypes;

        private InventorySaveState() { }

        public InventorySaveState(string dummyParam)
        {
            typeLists = new PowerupTypeList[RetroGame.NUM_PLAYERS];
            for (int i = 0; i < RetroGame.NUM_PLAYERS; i++)
            {
                typeLists[i] = new PowerupTypeList(Inventory.powerupLists[i]);
            }
            storedPowerupTypes = new List<string>();
            for (int i = RetroGame.NUM_PLAYERS; i < Inventory.storageGrid.Length; i++)
                if (Inventory.storageGrid[i] != null)
                    for (int j = 0; j < Inventory.storageGrid[i].Length; j++)
                        if (Inventory.storageGrid[i][j].HasPowerup)
                            storedPowerupTypes.Add(Inventory.storageGrid[i][j].Powerup.GetType().FullName);
        }

        public void Restore()
        {
            for(int i = 0; i < typeLists.Length; i++)
                if (typeLists[i] != null){
                    for(int j = 0; j < typeLists[i].actives.Length; j++)
                    {
                        if (typeLists[i].actives[j] != null)
                            RetroGame.getHeroes()[i].AddPowerup(Type.GetType(typeLists[i].actives[j]));
                    }
                    for (int j = 0; j < typeLists[i].passives.Length; j++)
                    {
                        if (typeLists[i].passives[j] != null)
                            RetroGame.getHeroes()[i].AddPowerup(Type.GetType(typeLists[i].passives[j]));
                    }
                }
            foreach(string storedPowerupTypeName in storedPowerupTypes)
            {
                Inventory.StorePowerup(Powerups.DummyPowerups[Type.GetType(storedPowerupTypeName)]);
            }
        }
    }
}
