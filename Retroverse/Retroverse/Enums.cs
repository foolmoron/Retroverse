using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Retroverse
{
    public enum GameState { Arena, Escape };
    public enum ScreenSize { Small, Medium, Large, Huge };
    public enum FragmentPosition { LeftHalf, RightHalf, TopHalf, BottomHalf, TopLeftCorner, TopRightCorner, BottomLeftCorner, BottomRightCorner };
    public enum Direction { Invalid, None, Up, Down, Left, Right };
    public enum InputType { Gamepad, Keyboard };
    public enum InputAction { None, Up, Down, Left, Right, Action1, Action2, Action3, Action4, Start, Escape };
    public enum EnemyState { Idle, TargetingHero };
    public enum CameraMode { Arena, Escape };
    public enum TransitionMode { ToStatic, FromStatic };
    public enum PlayMode { Forward, Reverse };
    public enum MenuOptionAction { Click, Left, Right };
}
