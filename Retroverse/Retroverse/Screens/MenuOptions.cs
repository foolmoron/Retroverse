using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Retroverse
{
    public class MenuOptions
    {
        public string Title { get; set; }
        private string backActionIndex = null;
        public Action<MenuOptionAction> BackAction { get; private set; }
        private List<KeyValuePair<string, Action<MenuOptionAction>>> optionsContainer = new List<KeyValuePair<string,Action<MenuOptionAction>>>();
        private bool[] enabledArray;
        public int Count { get { return optionsContainer.Count; } }

        public Action<MenuOptionAction> this[string option]
        {
            get
            {
                foreach(KeyValuePair<string, Action<MenuOptionAction>> pair in optionsContainer)
                {
                    if(pair.Key == option)
                        return pair.Value;
                }
                return null;
            }
            set
            {
                for(int i = 0; i < optionsContainer.Count; i++)
                {
                    KeyValuePair<string, Action<MenuOptionAction>> pair = optionsContainer[i];
                    if(pair.Key == option)
                        optionsContainer[i] = new KeyValuePair<string, Action<MenuOptionAction>>(pair.Key, value);
                }
                if (option == backActionIndex)
                    BackAction = value;
            }
        }

        public KeyValuePair<string, Action<MenuOptionAction>> this[int i]
        {
            get { return new KeyValuePair<string, Action<MenuOptionAction>>(optionsContainer[i].Key, optionsContainer[i].Value); } //return copied value to make this sort of access readonly
            set { optionsContainer[i] = value; }
        }

        public void SetEnabled(bool enabled, int index)
        {
            enabledArray[index] = enabled;
        }

        public void SetEnabled(bool enabled, int startIndex, int endIndex)
        {
            for(int i = startIndex; i <= endIndex; i++)
                SetEnabled(enabled, i);
        }

        public bool IsEnabled(int index)
        {
            return optionsContainer[index].Value != null &&  enabledArray[index];
        }

        public MenuOptions(string title, Dictionary<string, Action<MenuOptionAction>> optionsToActions, int backActionOptionIndex)
        {
            if (backActionOptionIndex >= optionsToActions.Count || backActionOptionIndex < 0)
                throw new ArgumentOutOfRangeException("backActionOptionIndex", "The back action index must be between 0 and the size of the options array");

            finalize(title, optionsToActions, optionsToActions.ElementAt(backActionOptionIndex).Key, optionsToActions.ElementAt(backActionOptionIndex).Value);
        }

        public MenuOptions(string title, Dictionary<string, Action<MenuOptionAction>> optionsToActions, string backActionOptionName)
        {
            if (!optionsToActions.Keys.Contains(backActionOptionName))
                throw new ArgumentOutOfRangeException("backActionOptionName", "The back action name must be inside the options array");

            finalize(title, optionsToActions, backActionOptionName, optionsToActions[backActionOptionName]);
        }

        public MenuOptions(string title, Dictionary<string, Action<MenuOptionAction>> optionsToActions, Action<MenuOptionAction> backAction)
        {
            string index = null;
            foreach (KeyValuePair<string, Action<MenuOptionAction>> pair in optionsToActions)
            {
                if (pair.Value == backAction)
                {
                    index = pair.Key;
                    break;
                }
            }
            finalize(title, optionsToActions, index, backAction);
        }

        private void finalize(string title, Dictionary<string, Action<MenuOptionAction>> optionsToActions, string backActionIndex, Action<MenuOptionAction> backAction)
        {
            Title = title;
            foreach(KeyValuePair<string, Action<MenuOptionAction>> pair in optionsToActions)
            {
                optionsContainer.Add(pair);
            }
            this.backActionIndex = backActionIndex;
            BackAction = backAction;
            enabledArray = new bool[optionsContainer.Count];
            for (int i = 0; i < enabledArray.Length; i++)
                enabledArray[i] = true;
        }

        public static bool IsLeftRightAction(KeyValuePair<string, Action<MenuOptionAction>> menuOptionPair)
        {
            return menuOptionPair.Key.StartsWith("<") && menuOptionPair.Key.EndsWith(">");
        }
    }
}
