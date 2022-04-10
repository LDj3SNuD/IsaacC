using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace IsaacC
{
    public class Program
    {
        public const int WindowWidth = 47;
        public const int WindowHeight = 36;

        public const string Title = "IsaacC";

        public static void Main(string[] args)
        {
            Console.SetWindowPosition(0, 0);

            Console.WindowWidth = WindowWidth;
            Console.WindowHeight = WindowHeight;

            Console.BufferWidth = Console.WindowWidth;
            Console.BufferHeight = Console.WindowHeight;

            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.CursorVisible = false;

            Console.TreatControlCAsInput = true;

            Console.Clear();
            Console.ResetColor();

            Console.Title = $"{Title} - Generating Map ...";
            Rooms rooms = new();
            Console.Title = Title;

            Room room = rooms.GetRoomById(rooms.HomeRoomId);
            room.IsLocked = false;

            Player player0 = new();
            Trace.Assert(room.PlayField.TryPlacePlayer(player0));

            Shop shopHealth = new(room, ShopOrientation.Left,  ShopResource.Health, originY: room.PlayField.Height - 5, originX: 0);
            Shop shopDamage = new(room, ShopOrientation.Right, ShopResource.Damage, originY: room.PlayField.Height - 5, originX: room.PlayField.Width - 1);

            room.PlayField.PrintStats(init: true);
            rooms.PrintPreviewMap(init: true);

            const string controlsMessage = "Controls: WASD keys: Move ─ Arrow keys: Attack ─ Enter key: Action ─ Esc key: Exit";

            Scrolling scrolling = new();

            scrolling.Start($"Welcome to {Program.Title}! (https://github.com/LDj3SNuD/IsaacC) │ Find the Boss's room, defeat him and you will win! │ {controlsMessage}");

            room.Print(init: true);
            room.PlayField.Print();

            shopHealth.Print(room);
            shopDamage.Print(room);

            const int SubFrameTime = 30; // ms.

            Stopwatch sW = new();

            const int cKeyQueueLength = 5;

            Queue<ConsoleKey> cKeyQueue = new(cKeyQueueLength);

            bool exit = false;

            GameOverState gameOverState = GameOverState.None;

            do
            {
                Direction changeRoomDirection = Direction.None;

                /**/

                sW.Restart();

                room.PlayField.HandleEnemiesAI(rooms);

                room.PlayField.PrintStats();
                room.PlayField.Print();

                //sW.Stop();

                int dynamicSpeedTime = cKeyQueue.Count * (SubFrameTime / cKeyQueueLength);
                int timeLeft = Math.Max(0, (SubFrameTime - dynamicSpeedTime) - (int)sW.ElapsedMilliseconds);

                sW.Restart();

                while (timeLeft > (int)sW.ElapsedMilliseconds)
                {
                    Thread.Sleep(5);

                    if (Console.KeyAvailable)
                    {
                        if (cKeyQueue.Count == cKeyQueueLength) cKeyQueue.Dequeue();
                        cKeyQueue.Enqueue(Console.ReadKey(true).Key);
                    }
                }

                //sW.Stop();

                /**/

                sW.Restart();

                if (cKeyQueue.TryDequeue(out ConsoleKey cKey))
                {
                    switch (cKey)
                    {
                        case ConsoleKey.W:
                        {
                            if (room.PlayField.TryMovePlayerById(id: 0, Direction.Up, out changeRoomDirection))
                            {
                                BeepAsync(frequencyA: 800, frequencyB: 800, duration: 2);
                            }

                            break;
                        }

                        case ConsoleKey.S:
                        {
                            if (room.PlayField.TryMovePlayerById(id: 0, Direction.Down, out changeRoomDirection))
                            {
                                BeepAsync(frequencyA: 800, frequencyB: 800, duration: 2);
                            }

                            break;
                        }

                        case ConsoleKey.A:
                        {
                            if (room.PlayField.TryMovePlayerById(id: 0, Direction.Left, out changeRoomDirection))
                            {
                                BeepAsync(frequencyA: 800, frequencyB: 800, duration: 2);
                            }

                            break;
                        }

                        case ConsoleKey.D:
                        {
                            if (room.PlayField.TryMovePlayerById(id: 0, Direction.Right, out changeRoomDirection))
                            {
                                BeepAsync(frequencyA: 800, frequencyB: 800, duration: 2);
                            }

                            break;
                        }

                        case ConsoleKey.UpArrow:
                        {
                            if (room.PlayField.TryFireBulletById(id: 0, Direction.Up))
                            {
                                BeepAsync(frequencyA: 800, duration: 1);
                            }

                            break;
                        }

                        case ConsoleKey.DownArrow:
                        {
                            if (room.PlayField.TryFireBulletById(id: 0, Direction.Down))
                            {
                                BeepAsync(frequencyA: 800, duration: 1);
                            }

                            break;
                        }

                        case ConsoleKey.LeftArrow:
                        {
                            if (room.PlayField.TryFireBulletById(id: 0, Direction.Left))
                            {
                                BeepAsync(frequencyA: 800, duration: 1);
                            }

                            break;
                        }

                        case ConsoleKey.RightArrow:
                        {
                            if (room.PlayField.TryFireBulletById(id: 0, Direction.Right))
                            {
                                BeepAsync(frequencyA: 800, duration: 1);
                            }

                            break;
                        }

                        case ConsoleKey.Enter:
                        {
                            if (shopHealth.TryExchangeResource(room, out bool success) ||
                                shopDamage.TryExchangeResource(room, out success))
                            {
                                if (success)
                                {
                                    room.PlayField.PrintStats();

                                    shopHealth.Print(room);
                                    shopDamage.Print(room);

                                    BeepAsync(frequencyA: 800, duration: 250);
                                }
                                else
                                {
                                    BeepAsync(frequencyA: 600, duration: 250);
                                }
                            }

                            cKeyQueue.Clear();

                            break;
                        }

                        case ConsoleKey.Escape:
                        {
                            exit = true;

                            break;
                        }

                        default:
                        {
                            cKeyQueue.Clear();

                            break;
                        }
                    }
                }

                room.PlayField.PrintStats();
                room.PlayField.Print();

                //sW.Stop();

                dynamicSpeedTime = cKeyQueue.Count * (SubFrameTime / cKeyQueueLength);
                timeLeft = Math.Max(0, (SubFrameTime - dynamicSpeedTime) - (int)sW.ElapsedMilliseconds);

                sW.Restart();

                while (timeLeft > (int)sW.ElapsedMilliseconds)
                {
                    Thread.Sleep(5);

                    if (Console.KeyAvailable)
                    {
                        if (cKeyQueue.Count == cKeyQueueLength) cKeyQueue.Dequeue();
                        cKeyQueue.Enqueue(Console.ReadKey(true).Key);
                    }
                }

                //sW.Stop();

                /**/

                sW.Restart();

                room.PlayField.UpdateBulletsState();

                room.PlayField.PrintStats();
                room.PlayField.Print();

                //sW.Stop();

                dynamicSpeedTime = cKeyQueue.Count * (SubFrameTime / cKeyQueueLength);
                timeLeft = Math.Max(0, (SubFrameTime - dynamicSpeedTime) - (int)sW.ElapsedMilliseconds);

                sW.Restart();

                while (timeLeft > (int)sW.ElapsedMilliseconds)
                {
                    Thread.Sleep(5);

                    if (Console.KeyAvailable)
                    {
                        if (cKeyQueue.Count == cKeyQueueLength) cKeyQueue.Dequeue();
                        cKeyQueue.Enqueue(Console.ReadKey(true).Key);
                    }
                }

                //sW.Stop();

                /**/

                if (changeRoomDirection != Direction.None)
                {
                    room = rooms.ChangeRoom(changeRoomDirection);

                    room.PlayField.PrepareRoom(rooms, player0, changeRoomDirection);

                    if (room.IsLocked)
                    {
                        room.PlayField.PrintStats();

                        BeepAsync(frequencyA: 1000, frequencyB: 500, duration: 250);
                    }

                    rooms.PrintPreviewMap();

                    if (room.Id != rooms.HomeRoomId)
                    {
                        scrolling.Stop();
                    }
                    else
                    {
                        scrolling.Start($"{controlsMessage}");
                    }

                    room.Print();

                    room.PlayField.Print();

                    shopHealth.Print(room);
                    shopDamage.Print(room);
                }

                /**/

                if (room.PlayField.TryUnlockRoom())
                {
                    rooms.PrintPreviewMap();
                    room.Print();

                    if (room.Id != rooms.BossRoomId)
                    {
                        BeepAsync(frequencyA: 1000, frequencyB: 1500, duration: 250);
                    }
                }

                /**/

                if (gameOverState == GameOverState.None)
                {
                    gameOverState = room.PlayField.GetGameOverState();

                    HandleGameOver(gameOverState, scrolling);
                }
            }
            while (!exit);

            Console.OutputEncoding = System.Text.Encoding.Default;
            Console.CursorVisible = true;

            Console.TreatControlCAsInput = false;

            Console.Clear();
            Console.ResetColor();
        }

        private static void HandleGameOver(GameOverState gameOverState, Scrolling scrolling)
        {
            switch (gameOverState)
            {
                case GameOverState.YouWin:
                {
                    scrolling.Start("Game Over: You Win!");
                    scrolling.Lock();

                    BeepAsync(frequencyA: 1000, frequencyB: 1500, duration: 500, ramp: true);

                    break;
                }

                case GameOverState.YouLose:
                {
                    scrolling.Start("Game Over: You Lose!");
                    scrolling.Lock();

                    BeepAsync(frequencyA: 1000, frequencyB: 500, duration: 500, ramp: true);

                    break;
                }
            }
        }

        private static readonly object _lock = new();

        public static void Write(string str, int left, int top, ConsoleColor fColor = ConsoleColor.Gray, ConsoleColor bColor = ConsoleColor.Black)
        {
            lock (_lock)
            {
                Console.ForegroundColor = fColor;
                Console.BackgroundColor = bColor;

                Console.SetCursorPosition(left, top);
                Console.Write(str);

                Console.ResetColor();
            }
        }

        public static void BeepAsync(int frequencyA = 800, int frequencyB = 0, int duration = 200, bool ramp = false)
        {
            ThreadPool.QueueUserWorkItem((_) =>
            {
                if (!ramp)
                {
                    if (frequencyA != 0)
                    {
                        Console.Beep(frequencyA, frequencyB == 0 ? duration : duration / 2);
                    }

                    if (frequencyB != 0)
                    {
                        Console.Beep(frequencyB, frequencyA == 0 ? duration : duration / 2);
                    }
                }
                else
                {
                    const int Steps = 4;

                    int frequencyStep = (frequencyA - frequencyB) / (Steps - 1);
                    duration /= Steps;

                    for (int i = 1; i <= Steps; i++)
                    {
                        if (frequencyA != 0)
                        {
                            Console.Beep(frequencyA, duration);
                        }

                        frequencyA -= frequencyStep;
                    }
                }
            });
        }
    }

    [Flags]
    public enum DoorsState
    {
        None = 0,

        Up    = 1 << 0,
        Right = 1 << 1,
        Down  = 1 << 2,
        Left  = 1 << 3
    }

    public enum GameOverState { None, YouWin, YouLose }

    public enum CellType { Empty, Shop, Obstacle, Bullet, Player }

    public enum EnemyType { None, A, B, BossMaster, BossSlave, All }

    public enum Action { None, Stay, Move, Fire }
    public enum Direction { None, Up, Right, Down, Left }

    public record CellInfo
    {
        public int Id { get; private set; }

        public CellType CellType { get; private set; }

        public Direction BulletDirection { get; private set; }
        public int BulletDistance { get; set; }
        public int BulletDamage { get; set; }

        public CellInfo(
            int id = 0,
            CellType cellType = CellType.Empty,
            Direction bulletDirection = Direction.None,
            int bulletDistance = 0,
            int bulletDamage = 0)
        {
            Id = id;

            CellType = cellType;

            BulletDirection = bulletDirection;
            BulletDistance = bulletDistance;
            BulletDamage = bulletDamage;
        }

        public void Set(
            int id = 0,
            CellType cellType = CellType.Empty,
            Direction bulletDirection = Direction.None,
            int bulletDistance = 0,
            int bulletDamage = 0)
        {
            Id = id;

            CellType = cellType;

            BulletDirection = bulletDirection;
            BulletDistance = bulletDistance;
            BulletDamage = bulletDamage;
        }

        public void Set(CellInfo cellInfo)
        {
            Id = cellInfo.Id;

            CellType = cellInfo.CellType;

            BulletDirection = cellInfo.BulletDirection;
            BulletDistance = cellInfo.BulletDistance;
            BulletDamage = cellInfo.BulletDamage;
        }
    }

    public class Rooms
    {
        public const int SquareLength = 7; // Odd (>= 5).

        public int HomeRoomId { get; }
        public int BossRoomId { get; }

        public int Player0RoomId { get; private set; }

        private readonly Room[,] _rooms;

        private enum MaskType { And, Or }

        public Rooms()
        {
            _rooms = new Room[SquareLength, SquareLength];

            for (int y = 0; y < SquareLength; y++)
            {
                for (int x = 0; x < SquareLength; x++)
                {
                    _rooms[y, x] = new(YXToId(y, x));
                }
            }

            /**/

            List<int> _sideRoomIds = new();
            Dictionary<int, (MaskType type, DoorsState doorsState)> _roomDoorsStateMasks = new();

            int id = YXToId(0, 0);
            _sideRoomIds.Add(id);
            _roomDoorsStateMasks.Add(id, (MaskType.And, DoorsState.Right | DoorsState.Down));

            id = YXToId(0, SquareLength - 1);
            _sideRoomIds.Add(id);
            _roomDoorsStateMasks.Add(id, (MaskType.And, DoorsState.Left | DoorsState.Down));

            id = YXToId(SquareLength - 1, 0);
            _sideRoomIds.Add(id);
            _roomDoorsStateMasks.Add(id, (MaskType.And, DoorsState.Right | DoorsState.Up));

            id = YXToId(SquareLength - 1, SquareLength - 1);
            _sideRoomIds.Add(id);
            _roomDoorsStateMasks.Add(id, (MaskType.And, DoorsState.Left | DoorsState.Up));

            for (int x = 1; x < SquareLength - 1; x++)
            {
                id = YXToId(0, x);
                _sideRoomIds.Add(id);
                _roomDoorsStateMasks.Add(id, (MaskType.And, DoorsState.Left | DoorsState.Right | DoorsState.Down));

                id = YXToId(SquareLength - 1, x);
                _sideRoomIds.Add(id);
                _roomDoorsStateMasks.Add(id, (MaskType.And, DoorsState.Left | DoorsState.Right | DoorsState.Up));
            }

            for (int y = 1; y < SquareLength - 1; y++)
            {
                id = YXToId(y, 0);
                _sideRoomIds.Add(id);
                _roomDoorsStateMasks.Add(id, (MaskType.And, DoorsState.Right | DoorsState.Up | DoorsState.Down));

                id = YXToId(y, SquareLength - 1);
                _sideRoomIds.Add(id);
                _roomDoorsStateMasks.Add(id, (MaskType.And, DoorsState.Left | DoorsState.Up | DoorsState.Down));
            }

            const int SquareLengthHalf = SquareLength / 2;

            id = YXToId(SquareLengthHalf - 1, SquareLengthHalf);
            _roomDoorsStateMasks.Add(id, (MaskType.Or, DoorsState.Down));
            id = YXToId(SquareLengthHalf + 1, SquareLengthHalf);
            _roomDoorsStateMasks.Add(id, (MaskType.Or, DoorsState.Up));
            id = YXToId(SquareLengthHalf, SquareLengthHalf - 1);
            _roomDoorsStateMasks.Add(id, (MaskType.Or, DoorsState.Right));
            id = YXToId(SquareLengthHalf, SquareLengthHalf + 1);
            _roomDoorsStateMasks.Add(id, (MaskType.Or, DoorsState.Left));

            id = YXToId(SquareLengthHalf, SquareLengthHalf);
            _roomDoorsStateMasks.Add(id, (MaskType.Or, DoorsState.Left | DoorsState.Right | DoorsState.Up | DoorsState.Down));

            HomeRoomId = id;

            /**/

            Random rnd0 = new();
            Random rnd1 = new();

            do
            {
                BossRoomId = _sideRoomIds[rnd0.Next(0, _sideRoomIds.Count)];

                ResetRoomsVisitability();

                for (int y = 0; y < SquareLength; y++)
                {
                    do
                    {
                        for (int x = 0; x < SquareLength; x++)
                        {
                            Room room = _rooms[y, x];

                            if (_roomDoorsStateMasks.TryGetValue(room.Id, out var mask))
                            {
                                room.DoorsState = mask.type switch
                                {
                                    MaskType.And => (DoorsState)rnd1.Next(0, 16) & mask.doorsState,
                                    MaskType.Or  => (DoorsState)rnd1.Next(0, 16) | mask.doorsState,
                                    _ => throw new Exception(nameof(mask.type))
                                };

                                if (room.Id == BossRoomId)
                                {
                                    Trace.Assert(mask.type == MaskType.And);

                                    while (!(room.DoorsState == DoorsState.Left || room.DoorsState == DoorsState.Right ||
                                             room.DoorsState == DoorsState.Up   || room.DoorsState == DoorsState.Down))
                                    {
                                        room.DoorsState = (DoorsState)rnd1.Next(0, 16) & mask.doorsState;
                                    }
                                }
                            }
                            else
                            {
                                room.DoorsState = (DoorsState)rnd1.Next(0, 16);
                            }
                        }
                    }
                    while (!TryCheckAdjacentDoorsStateRooms(yMax: y));
                }

                CheckRoomsVisitability(IdToYX(HomeRoomId).y, IdToYX(HomeRoomId).x);
            }
            while(!IsRoomVisitableById(BossRoomId) ||
                GetVisitableRoomsCount() > (SquareLength * SquareLength) / 2 ||
                IsThereSquareOfVisitableRooms() || IsThereRectangleOfVisitableRooms() || IsThereLineOfVisitableRooms());

            /**/

            Player0RoomId = HomeRoomId;
        }

        private static int YXToId(int y, int x)
        {
            Trace.Assert(y >= 0 && y < SquareLength && x >= 0 && x < SquareLength);

            return y * SquareLength + x;
        }

        private static (int y, int x) IdToYX(int id)
        {
            Trace.Assert(id >= 0 && id < SquareLength * SquareLength);

            return (id / SquareLength, id % SquareLength);
        }

        private bool TryCheckAdjacentDoorsStateRooms(int yMax)
        {
            for (int y = 0; y <= yMax; y++)
            {
                for (int x = 0; x < SquareLength - 1; x++)
                {
                    if (_rooms[y, x].DoorsState.HasFlag(DoorsState.Right) ^ _rooms[y, x + 1].DoorsState.HasFlag(DoorsState.Left))
                    {
                        return false;
                    }
                }
            }

            for (int x = 0; x < SquareLength; x++)
            {
                for (int y = 0; y <= yMax - 1; y++)
                {
                    if (_rooms[y, x].DoorsState.HasFlag(DoorsState.Down) ^ _rooms[y + 1, x].DoorsState.HasFlag(DoorsState.Up))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void ResetRoomsVisitability()
        {
            for (int y = 0; y < SquareLength; y++)
            {
                for (int x = 0; x < SquareLength; x++)
                {
                    _rooms[y, x].IsVisitable = false;
                }
            }
        }

        private void CheckRoomsVisitability(int y, int x)
        {
            if (y < 0 || y >= SquareLength || x < 0 || x >= SquareLength)
            {
                return;
            }

            Room room = _rooms[y, x];

            if (room.IsVisitable)
            {
                return;
            }

            room.IsVisitable = true;

            if (room.DoorsState.HasFlag(DoorsState.Up))
            {
                CheckRoomsVisitability(y - 1, x);
            }

            if (room.DoorsState.HasFlag(DoorsState.Down))
            {
                CheckRoomsVisitability(y + 1, x);
            }

            if (room.DoorsState.HasFlag(DoorsState.Left))
            {
                CheckRoomsVisitability(y, x - 1);
            }

            if (room.DoorsState.HasFlag(DoorsState.Right))
            {
                CheckRoomsVisitability(y, x + 1);
            }
        }

        private bool IsRoomVisitableById(int id)
        {
            return _rooms[IdToYX(id).y, IdToYX(id).x].IsVisitable;
        }

        private int GetVisitableRoomsCount()
        {
            int count = 0;

            for (int y = 0; y < SquareLength; y++)
            {
                for (int x = 0; x < SquareLength; x++)
                {
                    if (_rooms[y, x].IsVisitable)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private bool IsThereSquareOfVisitableRooms() // Not allowed: 3 x 3.
        {
            for (int y = 1; y < SquareLength - 1; y++)
            {
                for (int x = 1; x < SquareLength - 1; x++)
                {
                    if (y == IdToYX(HomeRoomId).y &&
                        x == IdToYX(HomeRoomId).x)
                    {
                        continue;
                    }

                    if (_rooms[y - 1, x - 1].IsVisitable &&
                        _rooms[y - 1, x    ].IsVisitable &&
                        _rooms[y - 1, x + 1].IsVisitable &&
                        _rooms[y,     x - 1].IsVisitable &&
                        _rooms[y,     x    ].IsVisitable &&
                        _rooms[y,     x + 1].IsVisitable &&
                        _rooms[y + 1, x - 1].IsVisitable &&
                        _rooms[y + 1, x    ].IsVisitable &&
                        _rooms[y + 1, x + 1].IsVisitable)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsThereRectangleOfVisitableRooms(int limit = SquareLength - 3) // Allowed: 2 x <=limit or <=limit x 2.
        {
            for (int y = 0; y < SquareLength - 1; y++)
            {
                int max = 0;
                int cnt = 0;

                for (int x = 0; x < SquareLength; x++)
                {
                    if (_rooms[y, x].IsVisitable && _rooms[y + 1, x].IsVisitable)
                    {
                        cnt++;
                    }
                    else
                    {
                        max = Math.Max(max, cnt);
                        cnt = 0;
                    }
                }

                if (Math.Max(max, cnt) > limit)
                {
                    return true;
                }
            }

            for (int x = 0; x < SquareLength - 1; x++)
            {
                int max = 0;
                int cnt = 0;

                for (int y = 0; y < SquareLength; y++)
                {
                    if (_rooms[y, x].IsVisitable && _rooms[y, x + 1].IsVisitable)
                    {
                        cnt++;
                    }
                    else
                    {
                        max = Math.Max(max, cnt);
                        cnt = 0;
                    }
                }

                if (Math.Max(max, cnt) > limit)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsThereLineOfVisitableRooms(int limit = SquareLength - 2) // Allowed: 1 x <=limit or <=limit x 1.
        {
            for (int y = 0; y < SquareLength; y++)
            {
                int max = 0;
                int cnt = 0;

                for (int x = 0; x < SquareLength; x++)
                {
                    if (_rooms[y, x].IsVisitable)
                    {
                        cnt++;
                    }
                    else
                    {
                        max = Math.Max(max, cnt);
                        cnt = 0;
                    }
                }

                if (Math.Max(max, cnt) > limit)
                {
                    return true;
                }
            }

            for (int x = 0; x < SquareLength; x++)
            {
                int max = 0;
                int cnt = 0;

                for (int y = 0; y < SquareLength; y++)
                {
                    if (_rooms[y, x].IsVisitable)
                    {
                        cnt++;
                    }
                    else
                    {
                        max = Math.Max(max, cnt);
                        cnt = 0;
                    }
                }

                if (Math.Max(max, cnt) > limit)
                {
                    return true;
                }
            }

            return false;
        }

        public Room GetRoomById(int id)
        {
            return _rooms[IdToYX(id).y, IdToYX(id).x];
        }

        public Room ChangeRoom(Direction direction)
        {
            (int ySrc, int xSrc) = IdToYX(Player0RoomId);

            (int yDst, int xDst) = direction switch
            {
                Direction.Up    => (ySrc - 1, xSrc),
                Direction.Down  => (ySrc + 1, xSrc),
                Direction.Left  => (ySrc, xSrc - 1),
                Direction.Right => (ySrc, xSrc + 1),
                _ => throw new ArgumentException(nameof(direction))
            };

            Player0RoomId = YXToId(yDst, xDst);

            return _rooms[yDst, xDst];
        }

        public void PrintPreviewMap(bool init = false)
        {
            int width = 1 + (SquareLength * 2 + 1) + 1;
            int height = 1 + SquareLength + 1;

            int left = Program.WindowWidth - width - 2; // Right.
            int top = 1; // Up.

            if (init)
            {
                const ConsoleColor WallColor = ConsoleColor.Gray;

                Program.Write(    "┌" + new String('─', width - 2) + "┐", left, top, WallColor);
                for (int i = 1; i <= height - 2; i++)
                {
                    Program.Write("│" + new String(' ', width - 2) + "│", left, top + i, WallColor);
                }
                Program.Write(    "└" + new String('─', width - 2) + "┘", left, top + height - 1, WallColor);
            }

            const string HomeRoomSymbol = " ⌂";
            const string RoomSymbol = " ■";

            for (int y = 0; y < SquareLength; y++)
            {
                for (int x = 0; x < SquareLength; x++)
                {
                    Room room = _rooms[y, x];

                    if (room.IsVisitable)
                    {
                        ConsoleColor color;

                        if (room.Id == Player0RoomId)
                        {
                            if (room.Id != BossRoomId)
                            {
                                color = room.IsLocked ? ConsoleColor.DarkCyan : ConsoleColor.Cyan;
                            }
                            else
                            {
                                color = room.IsLocked ? ConsoleColor.DarkRed : ConsoleColor.Red;
                            }
                        }
                        else
                        {
                            color = room.IsLocked ? ConsoleColor.Gray : ConsoleColor.White;
                        }

                        Program.Write(room.Id == HomeRoomId ? HomeRoomSymbol : RoomSymbol, left + x * 2 + 1, top + y + 1, color);
                    }
                    else
                    {
                        Program.Write("  ", left + x * 2 + 1, top + y + 1, ConsoleColor.Black);
                    }
                }
            }
        }
    }

    public class Room
    {
        public const int DoorLength = 4;

        public const ConsoleColor WallColor = ConsoleColor.Gray;
        public const ConsoleColor LockedDoorColor = ConsoleColor.DarkGray;

        public int Width { get; }
        public int Height { get; }

        public int Left { get; }
        public int Top { get; }

        public int Id { get; }

        public DoorsState DoorsState { get; set; }
        public bool IsLocked { get; set; }

        public bool IsVisitable { get; set; }

        public PlayField PlayField { get; }

        public static int Level { get; set; }

        public Room(int id)
        {
            Width = 1 + ((DoorLength * 5) * 2 + 1) + 1;
            Height = 1 + (DoorLength * 5) + 1;

            Left = (Program.WindowWidth - Width) / 2; // Center.
            Top = Program.WindowHeight - Height - 1; // Down.

            Id = id;

            DoorsState = DoorsState.None;
            IsLocked = true;

            PlayField = new(this);
        }

        public void Print(bool init = false)
        {
            if (init)
            {
                Program.Write(    "╔" + new String('═', Width - 2) + "╗", Left, Top, WallColor);
                for (int i = 1; i <= Height - 2; i++)
                {
                    Program.Write("║" + new String(' ', Width - 2) + "║", Left, Top + i, WallColor);
                }
                Program.Write(    "╚" + new String('═', Width - 2) + "╝", Left, Top + Height - 1, WallColor);
            }

            /**/

            if (!DoorsState.HasFlag(DoorsState.Up))
            {
                Program.Write(new String('═', DoorLength * 2 + 1), Left + Width / 2 - (DoorLength * 2 + 1) / 2, Top, WallColor);
            }
            else if (IsLocked)
            {
                Program.Write(new String('─', DoorLength * 2 + 1), Left + Width / 2 - (DoorLength * 2 + 1) / 2, Top, LockedDoorColor);
            }
            else
            {
                Program.Write(new String(' ', DoorLength * 2 + 1), Left + Width / 2 - (DoorLength * 2 + 1) / 2, Top, ConsoleColor.Black);
            }

            /**/

            if (!DoorsState.HasFlag(DoorsState.Down))
            {
                Program.Write(new String('═', DoorLength * 2 + 1), Left + Width / 2 - (DoorLength * 2 + 1) / 2, Top + Height - 1, WallColor);
            }
            else if (IsLocked)
            {
                Program.Write(new String('─', DoorLength * 2 + 1), Left + Width / 2 - (DoorLength * 2 + 1) / 2, Top + Height - 1, LockedDoorColor);
            }
            else
            {
                Program.Write(new String(' ', DoorLength * 2 + 1), Left + Width / 2 - (DoorLength * 2 + 1) / 2, Top + Height - 1, ConsoleColor.Black);
            }

            /**/

            if (!DoorsState.HasFlag(DoorsState.Left))
            {
                for (int i = 0; i < DoorLength; i++) Program.Write("║", Left, Top + Height / 2 - DoorLength / 2 + i, WallColor);
            }
            else if (IsLocked)
            {
                for (int i = 0; i < DoorLength; i++) Program.Write("│", Left, Top + Height / 2 - DoorLength / 2 + i, LockedDoorColor);
            }
            else
            {
                for (int i = 0; i < DoorLength; i++) Program.Write(" ", Left, Top + Height / 2 - DoorLength / 2 + i, ConsoleColor.Black);
            }

            /**/

            if (!DoorsState.HasFlag(DoorsState.Right))
            {
                for (int i = 0; i < DoorLength; i++) Program.Write("║", Left + Width - 1, Top + Height / 2 - DoorLength / 2 + i, WallColor);
            }
            else if (IsLocked)
            {
                for (int i = 0; i < DoorLength; i++) Program.Write("│", Left + Width - 1, Top + Height / 2 - DoorLength / 2 + i, LockedDoorColor);
            }
            else
            {
                for (int i = 0; i < DoorLength; i++) Program.Write(" ", Left + Width - 1, Top + Height / 2 - DoorLength / 2 + i, ConsoleColor.Black);
            }
        }
    }

    public class PlayField
    {
        public const int ObstaclesPerPlayer = 1;

        public int Width { get; }
        public int Height { get; }

        public List<Player> Players { get; }

        private readonly CellInfo[,] _playField;
        private static CellInfo[,] _playFieldOld; // readonly.

        private readonly Room _room;

        private readonly Random _rndPlace;
        private readonly Random _rndPrepare;

        public PlayField(Room room)
        {
            Width = ((room.Width - 2) - 1) / 2;
            Height = room.Height - 2;

            Players = new();

            _playField = new CellInfo[Height, Width];
            _playFieldOld = new CellInfo[Height, Width];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    _playField[y, x] = new();
                    _playFieldOld[y, x] = new();
                }
            }

            _room = room;

            _rndPlace = new();
            _rndPrepare = new();
        }

        public bool TryPlacePlayer(Player player)
        {
            (int y, int x) GetYX()
            {
                if (player.Id == 0)
                {
                    return (Height / 2, Width / 2);
                }
                else
                {
                    return (_rndPlace.Next(1, Height - 1), _rndPlace.Next(1, Width - 1));
                }
            }

            if (GetEmptyCellCount() < 1)
            {
                return false;
            }

            while (true)
            {
                (int y, int x) = GetYX();

                if (_playField[y, x].CellType == CellType.Empty)
                {
                    _playField[y, x].Set(player.Id, CellType.Player);

                    Players.Add(player);

                    break;
                }
            }

            return true;
        }

        public bool TryPlaceObstacle(int count)
        {
            (int y, int x) GetYX()
            {
                return (_rndPlace.Next(1, Height - 1), _rndPlace.Next(1, Width - 1));
            }

            if (GetEmptyCellCount() < count)
            {
                return false;
            }

            while (count != 0)
            {
                (int y, int x) = GetYX();

                if (_playField[y, x].CellType == CellType.Empty)
                {
                    _playField[y, x].Set(cellType: CellType.Obstacle);

                    count--;
                }
            }

            return true;
        }

        public void PlaceShopPlaceholders(List<(int y, int x)> placeholders)
        {
            foreach ((int y, int x) in placeholders)
            {
                Trace.Assert(_playField[y, x].CellType == CellType.Empty);
                _playField[y, x].Set(cellType: CellType.Shop);
            }
        }

        public bool TryMovePlayerById(int id, Direction direction, out Direction changeRoomDirection)
        {
            changeRoomDirection = Direction.None;

            if (Players[id].Health == 0)
            {
                return false;
            }

            bool result = false;

            (int ySrc, int xSrc) = GetPlayerPositionById(id);

            (int yDst, int xDst) = direction switch
            {
                Direction.Up    => (ySrc - 1, xSrc),
                Direction.Down  => (ySrc + 1, xSrc),
                Direction.Left  => (ySrc, xSrc - 1),
                Direction.Right => (ySrc, xSrc + 1),
                _ => throw new ArgumentException(nameof(direction))
            };

            if (yDst >= 0 && yDst < Height && xDst >= 0 && xDst < Width)
            {
                CellInfo cellInfoSrc = _playField[ySrc, xSrc];
                CellInfo cellInfoDst = _playField[yDst, xDst];

                Player playerSrc = Players[cellInfoSrc.Id];
                Player playerDst = Players[cellInfoDst.Id];

                switch (cellInfoDst.CellType)
                {
                    case CellType.Empty:
                    {
                        cellInfoDst.Set(cellInfoSrc.Id, CellType.Player);
                        cellInfoSrc.Set();

                        result = true;

                        break;
                    }

                    case CellType.Bullet:
                    {
                        Trace.Assert(cellInfoSrc.Id != cellInfoDst.Id);

                        playerDst.FiredBullets--;

                        if (cellInfoSrc.Id == 0 || cellInfoDst.Id == 0)
                        {
                            playerSrc.Health = Math.Max(0, playerSrc.Health - cellInfoDst.BulletDamage);

                            if (playerSrc.Health == 0)
                            {
                                cellInfoDst.Set();

                                RemoveBulletsById(cellInfoSrc.Id);
                            }
                            else
                            {
                                cellInfoDst.Set(cellInfoSrc.Id, CellType.Player);
                            }

                            playerDst.HandleScore(this, playerSrc);
                        }
                        else
                        {
                            cellInfoDst.Set(cellInfoSrc.Id, CellType.Player);
                        }

                        cellInfoSrc.Set();

                        result = true;

                        break;
                    }

                    case CellType.Player:
                    {
                        Trace.Assert(cellInfoSrc.Id != cellInfoDst.Id);

                        if (cellInfoSrc.Id == 0 || cellInfoDst.Id == 0)
                        {
                            playerSrc.Health = Math.Max(0, playerSrc.Health - /*playerDst.Damage*/1); // Non-bullet damage.

                            if (playerSrc.Health == 0)
                            {
                                cellInfoSrc.Set();

                                RemoveBulletsById(cellInfoSrc.Id);
                            }

                            playerDst.HandleScore(this, playerSrc);
                        }

                        break;
                    }
                }
            }
            else if (!_room.IsLocked)
            {
                switch (direction)
                {
                    case Direction.Up:
                    {
                        if (yDst == -1 &&
                            xDst >= Width / 2 - Room.DoorLength / 2 &&
                            xDst <  Width / 2 + Room.DoorLength / 2)
                        {
                            if (_room.DoorsState.HasFlag(DoorsState.Up))
                            {
                                changeRoomDirection = direction;

                                RemoveBulletsById(id);

                                result = true;
                            }
                        }

                        break;
                    }

                    case Direction.Right:
                    {
                        if (yDst >= Height / 2 - Room.DoorLength / 2 &&
                            yDst <  Height / 2 + Room.DoorLength / 2 &&
                            xDst == Width)
                        {
                            if (_room.DoorsState.HasFlag(DoorsState.Right))
                            {
                                changeRoomDirection = direction;

                                RemoveBulletsById(id);

                                result = true;
                            }
                        }

                        break;
                    }

                    case Direction.Down:
                    {
                        if (yDst == Height &&
                            xDst >= Width / 2 - Room.DoorLength / 2 &&
                            xDst <  Width / 2 + Room.DoorLength / 2)
                        {
                            if (_room.DoorsState.HasFlag(DoorsState.Down))
                            {
                                changeRoomDirection = direction;

                                RemoveBulletsById(id);

                                result = true;
                            }
                        }

                        break;
                    }

                    case Direction.Left:
                    {
                        if (yDst >= Height / 2 - Room.DoorLength / 2 &&
                            yDst <  Height / 2 + Room.DoorLength / 2 &&
                            xDst == -1)
                        {
                            if (_room.DoorsState.HasFlag(DoorsState.Left))
                            {
                                changeRoomDirection = direction;

                                RemoveBulletsById(id);

                                result = true;
                            }
                        }

                        break;
                    }
                }
            }

            return result;
        }

        public bool TryFireBulletById(int id, Direction direction)
        {
            if (Players[id].Health == 0)
            {
                return false;
            }

            if (IsDirectionH(direction) && Players[id].FiredBullets == Players[id].MaxFireableBulletsH ||
                IsDirectionV(direction) && Players[id].FiredBullets == Players[id].MaxFireableBulletsV)
            {
                return false;
            }

            bool result = false;

            (int ySrc, int xSrc) = GetPlayerPositionById(id);

            (int yDst, int xDst) = direction switch
            {
                Direction.Up    => (ySrc - 1, xSrc),
                Direction.Down  => (ySrc + 1, xSrc),
                Direction.Left  => (ySrc, xSrc - 1),
                Direction.Right => (ySrc, xSrc + 1),
                _ => throw new ArgumentException(nameof(direction))
            };

            if (yDst >= 0 && yDst < Height && xDst >= 0 && xDst < Width)
            {
                CellInfo cellInfoSrc = _playField[ySrc, xSrc];
                CellInfo cellInfoDst = _playField[yDst, xDst];

                Player playerSrc = Players[cellInfoSrc.Id];
                Player playerDst = Players[cellInfoDst.Id];

                switch (cellInfoDst.CellType)
                {
                    case CellType.Empty:
                    {
                        playerSrc.FiredBullets++;

                        cellInfoDst.Set(cellInfoSrc.Id, CellType.Bullet, direction, bulletDistance: 1, playerSrc.Damage);

                        result = true;

                        break;
                    }

                    case CellType.Bullet:
                    {
                        Trace.Assert(cellInfoSrc.Id != cellInfoDst.Id);

                        if (playerSrc.Damage == cellInfoDst.BulletDamage)
                        {
                            playerDst.FiredBullets--;

                            cellInfoDst.Set();

                            result = true;
                        }
                        else if (playerSrc.Damage > cellInfoDst.BulletDamage)
                        {
                            playerSrc.FiredBullets++;
                            playerDst.FiredBullets--;

                            cellInfoDst.Set(cellInfoSrc.Id, CellType.Bullet, direction, bulletDistance: 1, playerSrc.Damage - cellInfoDst.BulletDamage);

                            result = true;
                        }
                        else
                        {
                            cellInfoDst.BulletDamage -= playerSrc.Damage;
                        }

                        break;
                    }

                    case CellType.Player:
                    {
                        Trace.Assert(cellInfoSrc.Id != cellInfoDst.Id);

                        if (cellInfoSrc.Id == 0 || cellInfoDst.Id == 0)
                        {
                            playerDst.Health = Math.Max(0, playerDst.Health - playerSrc.Damage);

                            if (playerDst.Health == 0)
                            {
                                cellInfoDst.Set();

                                RemoveBulletsById(cellInfoDst.Id);
                            }

                            playerSrc.HandleScore(this, playerDst);

                            result = true;
                        }

                        break;
                    }
                }
            }

            return result;
        }

        public void UpdateBulletsState()
        {
            for (int id = Players.Count - 1; id >= 0; id--)
            {
                for (int bulletDistance = GetMaxBulletDistanceById(id); bulletDistance > 0; bulletDistance--)
                {
                    for (int ySrc = 0; ySrc < Height; ySrc++)
                    {
                        for (int xSrc = 0; xSrc < Width; xSrc++)
                        {
                            CellInfo cellInfoSrc = _playField[ySrc, xSrc];
                            Player playerSrc = Players[cellInfoSrc.Id];

                            if (cellInfoSrc.CellType == CellType.Bullet && cellInfoSrc.Id == id && cellInfoSrc.BulletDistance == bulletDistance)
                            {
                                if (IsDirectionH(cellInfoSrc.BulletDirection) && cellInfoSrc.BulletDistance == playerSrc.MaxBulletDistanceH(this) ||
                                    IsDirectionV(cellInfoSrc.BulletDirection) && cellInfoSrc.BulletDistance == playerSrc.MaxBulletDistanceV(this))
                                {
                                    playerSrc.FiredBullets--;

                                    cellInfoSrc.Set();

                                    continue;
                                }

                                (int yDst, int xDst) = cellInfoSrc.BulletDirection switch
                                {
                                    Direction.Up    => (ySrc - 1, xSrc),
                                    Direction.Down  => (ySrc + 1, xSrc),
                                    Direction.Left  => (ySrc, xSrc - 1),
                                    Direction.Right => (ySrc, xSrc + 1),
                                    _ => throw new Exception(nameof(cellInfoSrc.BulletDirection))
                                };

                                if (yDst < 0 || yDst >= Height || xDst < 0 || xDst >= Width)
                                {
                                    playerSrc.FiredBullets--;

                                    cellInfoSrc.Set();

                                    continue;
                                }

                                CellInfo cellInfoDst = _playField[yDst, xDst];
                                Player playerDst = Players[cellInfoDst.Id];

                                switch (cellInfoDst.CellType)
                                {
                                    case CellType.Empty:
                                    {
                                        cellInfoDst.Set(cellInfoSrc);
                                        cellInfoDst.BulletDistance++;

                                        cellInfoSrc.Set();

                                        break;
                                    }

                                    case CellType.Shop:
                                    case CellType.Obstacle:
                                    {
                                        playerSrc.FiredBullets--;

                                        cellInfoSrc.Set();

                                        break;
                                    }

                                    case CellType.Bullet:
                                    {
                                        Trace.Assert(cellInfoSrc.Id != cellInfoDst.Id);

                                        if (cellInfoSrc.BulletDamage == cellInfoDst.BulletDamage)
                                        {
                                            playerSrc.FiredBullets--;
                                            playerDst.FiredBullets--;

                                            cellInfoSrc.Set();
                                            cellInfoDst.Set();
                                        }
                                        else if (cellInfoSrc.BulletDamage > cellInfoDst.BulletDamage)
                                        {
                                            playerDst.FiredBullets--;

                                            cellInfoDst.Set(cellInfoSrc.Id, CellType.Bullet, cellInfoSrc.BulletDirection, cellInfoSrc.BulletDistance + 1, cellInfoSrc.BulletDamage - cellInfoDst.BulletDamage);
                                            cellInfoSrc.Set();
                                        }
                                        else
                                        {
                                            playerSrc.FiredBullets--;

                                            cellInfoDst.BulletDamage -= cellInfoSrc.BulletDamage;
                                            cellInfoSrc.Set();
                                        }

                                        break;
                                    }

                                    case CellType.Player:
                                    {
                                        Trace.Assert(cellInfoSrc.Id != cellInfoDst.Id);

                                        playerSrc.FiredBullets--;

                                        if (cellInfoSrc.Id == 0 || cellInfoDst.Id == 0)
                                        {
                                            playerDst.Health = Math.Max(0, playerDst.Health - cellInfoSrc.BulletDamage);

                                            if (playerDst.Health == 0)
                                            {
                                                cellInfoDst.Set();

                                                RemoveBulletsById(cellInfoDst.Id);
                                            }

                                            playerSrc.HandleScore(this, playerDst);
                                        }

                                        cellInfoSrc.Set();

                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool IsDirectionH(Direction direction)
        {
            return direction == Direction.Right || direction == Direction.Left;
        }

        private static bool IsDirectionV(Direction direction)
        {
            return direction == Direction.Up || direction == Direction.Down;
        }

        private int GetEmptyCellCount()
        {
            int count = 0;

            for (int y = 1; y < Height - 1; y++)
            {
                for (int x = 1; x < Width - 1; x++)
                {
                    if (_playField[y, x].CellType == CellType.Empty)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private void FlipPlayerPositionById(int id, bool prev = false, bool flipY = false, bool flipX = false)
        {
            (int ySrc, int xSrc) = GetPlayerPositionById(id, prev);

            (int yDst, int xDst) = (flipY, flipX) switch
            {
                (false, false) => (ySrc,              xSrc),
                (false, true)  => (ySrc,              Width - 1 - xSrc),
                (true,  false) => (Height - 1 - ySrc, xSrc),
                (true,  true)  => (Height - 1 - ySrc, Width - 1 - xSrc)
            };

            if (prev) (ySrc, xSrc) = GetPlayerPositionById(id);
            _playField[ySrc, xSrc].Set();

            Trace.Assert(_playField[yDst, xDst].CellType == CellType.Empty);
            _playField[yDst, xDst].Set(id, CellType.Player);
        }

        private int GetMaxBulletDistanceById(int id)
        {
            int maxBulletDistance = 0;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    CellInfo cellInfo = _playField[y, x];

                    if (cellInfo.Id == id && cellInfo.CellType == CellType.Bullet)
                    {
                        maxBulletDistance = Math.Max(maxBulletDistance, cellInfo.BulletDistance);
                    }
                }
            }

            return maxBulletDistance;
        }

        private void RemoveBulletsById(int id)
        {
            Players[id].FiredBullets = 0;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    CellInfo cellInfo = _playField[y, x];

                    if (cellInfo.Id == id && cellInfo.CellType == CellType.Bullet)
                    {
                        cellInfo.Set();
                    }
                }
            }
        }

        public (int y, int x) GetPlayerPositionById(int id, bool prev = false)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    CellInfo cellInfo = !prev ? _playField[y, x] : _playFieldOld[y, x];

                    if (cellInfo.Id == id && cellInfo.CellType == CellType.Player)
                    {
                        return (y, x);
                    }
                }
            }

            throw new ArgumentException(nameof(id));
        }

        public bool TryGetEnemiesStatsByEnemyType(EnemyType enemyType, out (int enemiesHealth, int enemiesCount, Player lastEnemy) enemiesStats)
        {
            enemiesStats = default;

            bool res = false;

            foreach (Player player in Players)
            {
                if (player.Id != 0 && (player.EnemyType == enemyType || enemyType == EnemyType.All))
                {
                    enemiesStats.enemiesHealth += player.Health;

                    if (player.Health != 0)
                    {
                        enemiesStats.enemiesCount++;
                    }

                    enemiesStats.lastEnemy = player;

                    res = true;
                }
            }

            return res;
        }

        public bool TryGetPointingDirectionsBySrcAndDstId(int srcId, int dstId, out List<Direction> directions, bool reverseDirections = false)
        {
            if (srcId == dstId || Players[srcId].Health == 0 || Players[dstId].Health == 0)
            {
                directions = default;

                return false;
            }

            (int ySrc, int xSrc) = GetPlayerPositionById(srcId);
            (int yDst, int xDst) = GetPlayerPositionById(dstId);

            if (dstId == 0)
            {
                if (Math.Abs(xSrc - xDst) == 1 || Math.Abs(xSrc - xDst) > Players[srcId].MaxBulletDistanceH(this) ||
                    Math.Abs(ySrc - yDst) == 1 || Math.Abs(ySrc - yDst) > Players[srcId].MaxBulletDistanceV(this))
                {
                    directions = default;

                    return false;
                }
            }

            directions = new();

            if (ySrc == yDst)
            {
                if (xSrc < xDst)
                {
                    directions.Add(!reverseDirections ? Direction.Right : Direction.Left);
                }
                else if (xSrc > xDst)
                {
                    directions.Add(!reverseDirections ? Direction.Left : Direction.Right);
                }
            }
            else if (xSrc == xDst)
            {
                if (ySrc < yDst)
                {
                    directions.Add(!reverseDirections ? Direction.Down : Direction.Up);
                }
                else if (ySrc > yDst)
                {
                    directions.Add(!reverseDirections ? Direction.Up : Direction.Down);
                }
            }
            else if (xSrc > xDst && ySrc < yDst) // Q1.
            {
                directions.Add(!reverseDirections ? Direction.Left : Direction.Right);
                directions.Add(!reverseDirections ? Direction.Down : Direction.Up);
            }
            else if (xSrc < xDst && ySrc < yDst) // Q2.
            {
                directions.Add(!reverseDirections ? Direction.Right : Direction.Left);
                directions.Add(!reverseDirections ? Direction.Down : Direction.Up);
            }
            else if (xSrc < xDst && ySrc > yDst) // Q3.
            {
                directions.Add(!reverseDirections ? Direction.Right : Direction.Left);
                directions.Add(!reverseDirections ? Direction.Up : Direction.Down);
            }
            else if (xSrc > xDst && ySrc > yDst) // Q4.
            {
                directions.Add(!reverseDirections ? Direction.Left : Direction.Right);
                directions.Add(!reverseDirections ? Direction.Up : Direction.Down);
            }

            Trace.Assert(directions.Count != 0);

            return true;
        }

        public void HandleEnemiesAI(Rooms rooms)
        {
            for (int id = Players.Count - 1; id > 0; id--)
            {
                Action action = Players[id].GetNextAction();

                switch (action)
                {
                    case Action.Move:
                    {
                        int dstId = 0;

                        if (rooms.Player0RoomId == rooms.BossRoomId)
                        {
                            if (id > 1)
                            {
                                dstId = 1;
                            }
                        }

                        Direction direction = Players[id].GetNextDirection(this, action, dstId);

                        TryMovePlayerById(id, direction, out _);

                        break;
                    }

                    case Action.Fire:
                    {
                        Direction direction = Players[id].GetNextDirection(this, action);

                        TryFireBulletById(id, direction);

                        break;
                    }
                }
            }
        }

        public void PrepareRoom(Rooms rooms, Player player0, Direction changeRoomDirection)
        {
            if (_room.IsLocked)
            {
                Trace.Assert(rooms.Player0RoomId != rooms.HomeRoomId);
                Trace.Assert(Players.Count == 0);

                if (rooms.Player0RoomId != rooms.BossRoomId)
                {
                    int playersCount = 1 + (Room.Level + 1);

                    Trace.Assert(player0.Id == 0);
                    Trace.Assert(TryPlacePlayer(player0));

                    for (int id = 1; id < playersCount; id++)
                    {
                        EnemyType enemyType = (EnemyType)_rndPrepare.Next((int)EnemyType.A, (int)EnemyType.B + 1);
                        Trace.Assert(TryPlacePlayer(new(id, enemyType)));
                    }

                    TryPlaceObstacle(playersCount * PlayField.ObstaclesPerPlayer);
                }
                else
                {
                    int playersCount = 1 + 1 + Math.Max(4, Room.Level);

                    Trace.Assert(player0.Id == 0);
                    Trace.Assert(TryPlacePlayer(player0));

                    Trace.Assert(TryPlacePlayer(new(id: 1, EnemyType.BossMaster)));
                    for (int id = 2; id < playersCount; id++)
                    {
                        Trace.Assert(TryPlacePlayer(new(id, EnemyType.BossSlave)));
                    }

                    TryPlaceObstacle(playersCount * PlayField.ObstaclesPerPlayer);
                }
            }

            FlipPlayerPositionById(id: 0, prev: true, IsDirectionV(changeRoomDirection), IsDirectionH(changeRoomDirection));
        }

        public bool TryUnlockRoom()
        {
            if (_room.IsLocked)
            {
                if (TryGetEnemiesStatsByEnemyType(EnemyType.All, out var enemiesStats) && enemiesStats.enemiesHealth == 0)
                {
                    _room.IsLocked = false;

                    Room.Level++;

                    return true;
                }
            }

            return false;
        }

        public GameOverState GetGameOverState()
        {
            if (Players[0].Health == 0)
            {
                return GameOverState.YouLose;
            }
            else if (TryGetEnemiesStatsByEnemyType(EnemyType.BossMaster, out var enemiesStatsBM) &&
                     TryGetEnemiesStatsByEnemyType(EnemyType.BossSlave,  out var enemiesStatsBS) &&
                     enemiesStatsBM.enemiesHealth + enemiesStatsBS.enemiesHealth == 0)
            {
                return GameOverState.YouWin;
            }
            else
            {
                return GameOverState.None;
            }
        }

        private static int _hashCode;

        public void PrintStats(bool init = false)
        {
            //int width = 21;
            //int height = 8;

            int left = 2; // Left.
            int top = 1; // Up.

            if (init)
            {
                const ConsoleColor WallColor = ConsoleColor.Gray;

                Program.Write("┌──────────┬────────┐", left, top + 0, WallColor);
                Program.Write("│          │        │", left, top + 1, WallColor);
                Program.Write("├──────────┴────────┤", left, top + 2, WallColor);
                Program.Write("│                   │", left, top + 3, WallColor);
                Program.Write("├───────────────────┤", left, top + 4, WallColor);
                Program.Write("│                   │", left, top + 5, WallColor);
                Program.Write("│                   │", left, top + 6, WallColor);
                Program.Write("└───────────────────┘", left, top + 7, WallColor);
            }

            int hashCode = GetHashCode();

            if (hashCode != _hashCode)
            {
                _hashCode = hashCode;

                Player player0 = Players[0];
                Trace.Assert(player0.Id == 0);

                Program.Write("¤", left + 2, top + 1, ConsoleColor.Yellow);
                Program.Write($"{player0.Score}".PadRight(6), left + 4, top + 1, ConsoleColor.Black, ConsoleColor.Gray);

                Program.Write("Lvl", left + 13, top + 1);
                Program.Write($"{Room.Level}".PadRight(2), left + 17, top + 1, ConsoleColor.Black, ConsoleColor.Gray);

                if (player0.Health != 0)
                {
                    Program.Write("♥", left + 2, top + 3, player0.Color);
                    Program.Write($"{player0.Health}".PadRight(4), left + 4, top + 3);
                    Program.Write("†", left + 9, top + 3, player0.BulletColor);
                    Program.Write($"{player0.Damage}".PadRight(2), left + 11, top + 3);
                }
                else
                {
                    Program.Write(new String(' ', 17), left + 2, top + 3, ConsoleColor.Black);
                }

                if (TryGetEnemiesStatsByEnemyType(EnemyType.A, out var enemiesStats) ||
                    TryGetEnemiesStatsByEnemyType(EnemyType.BossMaster, out enemiesStats))
                {
                    if (enemiesStats.enemiesHealth != 0)
                    {
                        Program.Write("♥", left + 2, top + 5, enemiesStats.lastEnemy.Color);
                        Program.Write($"{enemiesStats.enemiesHealth}".PadRight(4), left + 4, top + 5);
                        Program.Write("†", left + 9, top + 5, enemiesStats.lastEnemy.BulletColor);
                        Program.Write($"{enemiesStats.lastEnemy.Damage}".PadRight(2), left + 11, top + 5);
                        Program.Write($"({enemiesStats.enemiesCount})".PadRight(4), left + 15, top + 5, ConsoleColor.DarkGray);
                    }
                    else
                    {
                        Program.Write(new String(' ', 17), left + 2, top + 5, ConsoleColor.Black);
                    }
                }

                if (TryGetEnemiesStatsByEnemyType(EnemyType.B, out enemiesStats) ||
                    TryGetEnemiesStatsByEnemyType(EnemyType.BossSlave, out enemiesStats))
                {
                    if (enemiesStats.enemiesHealth != 0)
                    {
                        Program.Write("♥", left + 2, top + 6, enemiesStats.lastEnemy.Color);
                        Program.Write($"{enemiesStats.enemiesHealth}".PadRight(4), left + 4, top + 6);
                        Program.Write("†", left + 9, top + 6, enemiesStats.lastEnemy.BulletColor);
                        Program.Write($"{enemiesStats.lastEnemy.Damage}".PadRight(2), left + 11, top + 6);
                        Program.Write($"({enemiesStats.enemiesCount})".PadRight(4), left + 15, top + 6, ConsoleColor.DarkGray);
                    }
                    else
                    {
                        Program.Write(new String(' ', 17), left + 2, top + 6, ConsoleColor.Black);
                    }
                }
            }
        }

        public void Print()
        {
            int left = _room.Left + 1;
            int top = _room.Top + 1;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (_playField[y, x] != _playFieldOld[y, x])
                    {
                        _playFieldOld[y, x].Set(_playField[y, x]);

                        CellInfo cellInfo = _playField[y, x];

                        switch (cellInfo.CellType)
                        {
                            case CellType.Obstacle:
                            {
                                Program.Write(Obstacle.GetSymbol(), left + x * 2, top + y, Obstacle.GetColor());

                                break;
                            }

                            case CellType.Bullet:
                            {
                                Program.Write(Players[cellInfo.Id].BulletSymbol, left + x * 2, top + y, Players[cellInfo.Id].BulletColor);

                                break;
                            }

                            case CellType.Empty:
                            {
                                Program.Write("  ", left + x * 2, top + y, ConsoleColor.Black);

                                break;
                            }

                            case CellType.Player:
                            {
                                Program.Write(Players[cellInfo.Id].Symbol, left + x * 2, top + y, Players[cellInfo.Id].Color);

                                break;
                            }
                        }
                    }
                }
            }
        }

        public override int GetHashCode()
        {
            HashCode hash = new();

            hash.Add(Room.Level);

            Players.ForEach((player) => hash.Add(player.GetHashCode()));

            return hash.ToHashCode();
        }
    }

    public class Player
    {
        public int Id { get; }
        public EnemyType EnemyType { get; }

        public string Symbol { get; }
        public ConsoleColor Color { get; }

        public string BulletSymbol { get; }
        public ConsoleColor BulletColor { get; }

        public int Score { get; set; }

        public int Health { get; set; }
        public int Damage { get; set; }

        public int FiredBullets { get; set; }

        public Func<PlayField, int> MaxBulletDistanceH { get; }
        public Func<PlayField, int> MaxBulletDistanceV { get; }

        public int MaxFireableBulletsH { get; }
        public int MaxFireableBulletsV { get; }

        private readonly (int stayWeight, int moveWeight, int fireWeight) _actionWeights;
        private readonly (int randomWeight, int pointingWeight, int dodgingWeight, int holdWeight) _directionWeightsForMove;
        private readonly (int randomWeight, int pointingWeight, int dodgingWeight, int holdWeight) _directionWeightsForFire;

        private readonly Random _rndWeightNext;
        private readonly Random _rndDirection;

        private Direction _holdDirectionForMove;
        private Direction _holdDirectionForFire;

        public Player(int id = 0, EnemyType enemyType = EnemyType.None)
        {
            Id = id;
            EnemyType = enemyType;

            if (id == 0)
            {
                Symbol = " ◊";
                Color = ConsoleColor.Cyan;

                BulletSymbol = " ∙";
                BulletColor = ConsoleColor.DarkCyan;

                Health = 15;
                Damage = 1;

                MaxBulletDistanceH = (playField) => playField.Width / 2;
                MaxBulletDistanceV = (playField) => playField.Height / 2;

                MaxFireableBulletsH = 3;
                MaxFireableBulletsV = 3;
            }
            else
            {
                switch (enemyType)
                {
                    case EnemyType.A:
                    {
                        Symbol = " ○";
                        Color = ConsoleColor.Green;

                        BulletSymbol = " ∙";
                        BulletColor = ConsoleColor.DarkGreen;

                        Health = 5;
                        Damage = 1;

                        MaxBulletDistanceH = (playField) => (playField.Width * 2) / 3;
                        MaxBulletDistanceV = (playField) => (playField.Height * 2) / 3;

                        MaxFireableBulletsH = 4;
                        MaxFireableBulletsV = 4;

                        _actionWeights = (stayWeight: 43, moveWeight: 43, fireWeight: 13);
                        _directionWeightsForMove = (randomWeight: 21, pointingWeight: 1, dodgingWeight: 54, holdWeight: 24);
                        _directionWeightsForFire = (randomWeight: 1, pointingWeight: 74, dodgingWeight: 1, holdWeight: 24);

                        break;
                    }

                    case EnemyType.B:
                    {
                        Symbol = " ○";
                        Color = ConsoleColor.Blue;

                        BulletSymbol = " ∙";
                        BulletColor = ConsoleColor.DarkBlue;

                        Health = 10;
                        Damage = 1;

                        MaxBulletDistanceH = (playField) => (playField.Width * 1) / 3;
                        MaxBulletDistanceV = (playField) => (playField.Height * 1) / 3;

                        MaxFireableBulletsH = 2;
                        MaxFireableBulletsV = 2;

                        _actionWeights = (stayWeight: 43, moveWeight: 43, fireWeight: 13);
                        _directionWeightsForMove = (randomWeight: 21, pointingWeight: 54, dodgingWeight: 1, holdWeight: 24);
                        _directionWeightsForFire = (randomWeight: 1, pointingWeight: 74, dodgingWeight: 1, holdWeight: 24);

                        break;
                    }

                    case EnemyType.BossMaster:
                    {
                        Symbol = " ☼";
                        Color = ConsoleColor.Red;

                        BulletSymbol = " ∙";
                        BulletColor = ConsoleColor.DarkRed;

                        Health = 30;
                        Damage = 2;

                        MaxBulletDistanceH = (playField) => playField.Width;
                        MaxBulletDistanceV = (playField) => playField.Height;

                        MaxFireableBulletsH = 6;
                        MaxFireableBulletsV = 6;

                        _actionWeights = (stayWeight: 53, moveWeight: 33, fireWeight: 13);
                        _directionWeightsForMove = (randomWeight: 11, pointingWeight: 64, dodgingWeight: 1, holdWeight: 24);
                        _directionWeightsForFire = (randomWeight: 1, pointingWeight: 74, dodgingWeight: 1, holdWeight: 24);

                        break;
                    }

                    case EnemyType.BossSlave:
                    {
                        Symbol = " ☼";
                        Color = ConsoleColor.Magenta;

                        BulletSymbol = " ∙";
                        BulletColor = ConsoleColor.DarkMagenta;

                        Health = 15;
                        Damage = 1;

                        MaxBulletDistanceH = (playField) => playField.Width / 2;
                        MaxBulletDistanceV = (playField) => playField.Height / 2;

                        MaxFireableBulletsH = 3;
                        MaxFireableBulletsV = 3;

                        _actionWeights = (stayWeight: 33, moveWeight: 53, fireWeight: 13);
                        _directionWeightsForMove = (randomWeight: 11, pointingWeight: 64, dodgingWeight: 1, holdWeight: 24);
                        _directionWeightsForFire = (randomWeight: 1, pointingWeight: 74, dodgingWeight: 1, holdWeight: 24);

                        break;
                    }
                }

                _rndWeightNext = new();
                _rndDirection = new();

                _holdDirectionForMove = Direction.None;
                _holdDirectionForFire = Direction.None;
            }
        }

        public Action GetNextAction()
        {
            Action action = Action.None;

            int weight = _rndWeightNext.Next(1, 101);

            if (weight >= 1 && weight <= _actionWeights.stayWeight)
            {
                action = Action.Stay;
            }
            else if (weight > _actionWeights.stayWeight && weight <= _actionWeights.stayWeight + _actionWeights.moveWeight)
            {
                action = Action.Move;
            }
            else if (weight > _actionWeights.stayWeight + _actionWeights.moveWeight && weight <= 100)
            {
                action = Action.Fire;
            }

            Trace.Assert(action != Action.None);

            return action;
        }

        public Direction GetNextDirection(PlayField playField, Action action, int dstId = 0)
        {
            if (action != Action.Move && action != Action.Fire)
            {
                throw new ArgumentException(nameof(action));
            }

            var directionWeights = action == Action.Move ? _directionWeightsForMove : _directionWeightsForFire;

            Direction direction = Direction.None;

            void PointingOrDodgingDirection(bool dodgingDirection)
            {
                if (playField.TryGetPointingDirectionsBySrcAndDstId(srcId: Id, dstId, out var directions, dodgingDirection))
                {
                    if (directions.Count == 1)
                    {
                        direction = directions[0];
                    }
                    else if (directions.Count == 2)
                    {
                        direction = directions[_rndDirection.Next(0, 2)];
                    }
                }
                else
                {
                    direction = GetNextDirection(playField, action, dstId);
                }
            }

            int weight = _rndWeightNext.Next(1, 101);

            if (weight >= 1 && weight <= directionWeights.randomWeight)
            {
                direction = (Direction)_rndDirection.Next((int)Direction.Up, (int)Direction.Left + 1);
            }
            else if (weight > directionWeights.randomWeight && weight <= directionWeights.randomWeight + directionWeights.pointingWeight)
            {
                PointingOrDodgingDirection(dodgingDirection: false);
            }
            else if (weight >  directionWeights.randomWeight + directionWeights.pointingWeight &&
                     weight <= directionWeights.randomWeight + directionWeights.pointingWeight + directionWeights.dodgingWeight)
            {
                PointingOrDodgingDirection(dodgingDirection: true);
            }
            else if (weight > directionWeights.randomWeight + directionWeights.pointingWeight + directionWeights.dodgingWeight && weight <= 100)
            {
                direction = action == Action.Move ? _holdDirectionForMove : _holdDirectionForFire;

                if (direction == Direction.None)
                {
                    direction = GetNextDirection(playField, action, dstId);
                }
            }

            Trace.Assert(direction != Direction.None);

            _holdDirectionForMove = action == Action.Move ? direction : _holdDirectionForMove;
            _holdDirectionForFire = action == Action.Fire ? direction : _holdDirectionForFire;

            return direction;
        }

        public void HandleScore(PlayField playField, Player player)
        {
            switch (player.EnemyType)
            {
                case EnemyType.A:
                case EnemyType.B:
                case EnemyType.BossSlave:
                {
                    Score += 1;

                    if (player.Health == 0)
                    {
                        Score += 10;
                    }

                    playField.TryGetEnemiesStatsByEnemyType(player.EnemyType, out var enemiesStats);

                    if (enemiesStats.enemiesHealth == 0)
                    {
                        Score += 100;
                    }

                    playField.TryGetEnemiesStatsByEnemyType(EnemyType.All, out enemiesStats);

                    if (enemiesStats.enemiesHealth == 0)
                    {
                        Score += 1000;
                    }

                    break;
                }

                case EnemyType.BossMaster:
                {
                    Score += 3;

                    if (player.Health == 0)
                    {
                        Score += 30;
                    }

                    playField.TryGetEnemiesStatsByEnemyType(player.EnemyType, out var enemiesStats);

                    if (enemiesStats.enemiesHealth == 0)
                    {
                        Score += 300;
                    }

                    playField.TryGetEnemiesStatsByEnemyType(EnemyType.All, out enemiesStats);

                    if (enemiesStats.enemiesHealth == 0)
                    {
                        Score += 3000;
                    }

                    break;
                }
            }
        }

        public override int GetHashCode()
        {
            HashCode hash = new();

            hash.Add(Id);
            hash.Add(EnemyType);

            hash.Add(Score);

            hash.Add(Health);
            hash.Add(Damage);

            return hash.ToHashCode();
        }
    }

    public static class Obstacle
    {
        public static string GetSymbol()
        {
            return " ░";
        }

        public static ConsoleColor GetColor()
        {
            return ConsoleColor.DarkGray;
        }
    }

    public enum ShopOrientation { Left, Right }
    public enum ShopResource { Health, Damage }

    public class Shop
    {
        public int RoomId { get; }

        public ShopOrientation ShopOrientation { get; }
        public ShopResource ShopResource { get; }

        public int ExchangePositionY { get; }
        public int ExchangePositionX { get; }

        private int _originY;
        private int _originX;

        private readonly Stopwatch _sW;

        // originY/X: ShopOrientation.Left  -> Top-Left  (room.PlayField related).
        // originY/X: ShopOrientation.Right -> Top-Right (room.PlayField related).
        public Shop(Room room, ShopOrientation shopOrientation, ShopResource shopResource, int originY, int originX)
        {
            RoomId = room.Id;

            ShopOrientation = shopOrientation;
            ShopResource = shopResource;

            _originY = originY;
            _originX = originX;

            _sW = Stopwatch.StartNew();

            List<(int y, int x)> placeholders = new();

            switch (shopOrientation)
            {
                case ShopOrientation.Left:
                {
                    ExchangePositionY = originY + 2;
                    ExchangePositionX = originX + 7;

                    for (int y = 0; y <= 4; y++)
                    {
                        for (int x = 0; x <= 6; x++)
                        {
                            placeholders.Add((originY + y, originX + x));
                        }
                    }

                    placeholders.Add((originY + 1, originX + 7));
                    placeholders.Add((originY + 3, originX + 7));

                    break;
                }

                case ShopOrientation.Right:
                {
                    ExchangePositionY = originY + 2;
                    ExchangePositionX = originX - 7;

                    for (int y = 0; y <= 4; y++)
                    {
                        for (int x = 0; x <= 6; x++)
                        {
                            placeholders.Add((originY + y, originX - x));
                        }
                    }

                    placeholders.Add((originY + 1, originX - 7));
                    placeholders.Add((originY + 3, originX - 7));

                    break;
                }
            }

            room.PlayField.PlaceShopPlaceholders(placeholders);
        }

        private int GetSellAmount()
        {
            return ShopResource switch
            {
                ShopResource.Health => Math.Max(90,  Math.Max(1, Room.Level) * 60),
                ShopResource.Damage => Math.Max(450, Math.Max(1, Room.Level) * 300),
                _ => throw new Exception(nameof(ShopResource))
            };
        }

        private int GetBuyAmount()
        {
            return ShopResource switch
            {
                ShopResource.Health => 1,
                ShopResource.Damage => 1,
                _ => throw new Exception(nameof(ShopResource))
            };
        }

        public bool TryExchangeResource(Room room, out bool success)
        {
            success = false;

            if (room.Id != RoomId)
            {
                return false;
            }

            (int player0Y, int player0X) = room.PlayField.GetPlayerPositionById(id: 0);

            if (player0Y != ExchangePositionY || player0X != ExchangePositionX)
            {
                return false;
            }

            if (_sW.ElapsedMilliseconds >= 250)
            {
                _sW.Restart();
            }
            else
            {
                return false;
            }

            Player player0 = room.PlayField.Players[0];
            Trace.Assert(player0.Id == 0);

            int sellAmount = GetSellAmount();
            int buyAmount = GetBuyAmount();

            success = player0.Score >= sellAmount;

            if (success)
            {
                player0.Score -= sellAmount;

                switch (ShopResource)
                {
                    case ShopResource.Health:
                    {
                        player0.Health += buyAmount;

                        break;
                    }

                    case ShopResource.Damage:
                    {
                        player0.Damage += buyAmount;

                        break;
                    }
                }
            }

            return true;
        }

        public void Print(Room room)
        {
            if (room.Id != RoomId)
            {
                return;
            }

            int left = room.Left + 1;
            int top = room.Top + 1;

            left += _originX * 2;
            top += _originY;

            Player player0 = room.PlayField.Players[0];
            Trace.Assert(player0.Id == 0);

            int sellAmount = GetSellAmount();
            int buyAmount = GetBuyAmount();

            bool active = player0.Score >= sellAmount;

            switch (ShopOrientation)
            {
                case ShopOrientation.Left:
                {
                    ConsoleColor color = active ? ConsoleColor.White : ConsoleColor.DarkGray;
                    Program.Write(" ┌───────┬───┐",   left, top + 0, color);
                    Program.Write(" │       │   ├──", left, top + 1, color);
                    Program.Write(" ├───────┤ ↓ │",   left, top + 2, color);
                    Program.Write(" │       │   ├──", left, top + 3, color);
                    Program.Write(" └───────┴───┘",   left, top + 4, color);

                    color = active ? ConsoleColor.Red : ConsoleColor.DarkRed;
                    Program.Write($"-{sellAmount}".PadRight(5), left + 3, top + 1, color);
                    color = active ? ConsoleColor.Green : ConsoleColor.DarkGreen;
                    Program.Write($"+{buyAmount}".PadRight(5), left + 3, top + 3, color);

                    Program.Write("¤", left + 11, top + 1, ConsoleColor.Yellow);

                    switch (ShopResource)
                    {
                        case ShopResource.Health:
                        {
                            Program.Write("♥", left + 11, top + 3, player0.Color);

                            break;
                        }

                        case ShopResource.Damage:
                        {
                            Program.Write("†", left + 11, top + 3, player0.BulletColor);

                            break;
                        }
                    }

                    break;
                }

                case ShopOrientation.Right:
                {
                    ConsoleColor color = active ? ConsoleColor.White : ConsoleColor.DarkGray;
                    Program.Write(  " ┌───┬───────┐", left - 12, top + 0, color);
                    Program.Write(" ──┤   │       │", left - 14, top + 1, color);
                    Program.Write(  " │ ↓ ├───────┤", left - 12, top + 2, color);
                    Program.Write(" ──┤   │       │", left - 14, top + 3, color);
                    Program.Write(  " └───┴───────┘", left - 12, top + 4, color);

                    color = active ? ConsoleColor.Red : ConsoleColor.DarkRed;
                    Program.Write($"-{sellAmount}".PadRight(5), left - 5, top + 1, color);
                    color = active ? ConsoleColor.Green : ConsoleColor.DarkGreen;
                    Program.Write($"+{buyAmount}".PadRight(5), left - 5, top + 3, color);

                    Program.Write("¤", left - 9, top + 1, ConsoleColor.Yellow);

                    switch (ShopResource)
                    {
                        case ShopResource.Health:
                        {
                            Program.Write("♥", left - 9, top + 3, player0.Color);

                            break;
                        }

                        case ShopResource.Damage:
                        {
                            Program.Write("†", left - 9, top + 3, player0.BulletColor);

                            break;
                        }
                    }

                    break;
                }
            }
        }
    }

    public class Scrolling
    {
        private volatile string _message;
        private volatile bool _enabled;
        private volatile bool _locked;

        public Scrolling()
        {
            ThreadPool.QueueUserWorkItem((_) =>
            {
                while (true)
                {
                    Program.Write(new String(' ', Program.WindowWidth), left: 0, top: 10, ConsoleColor.Black);
                    Program.Write(new String('─', Program.WindowWidth), left: 0, top: 11, ConsoleColor.DarkGray);
                    Program.Write(new String(' ', Program.WindowWidth), left: 0, top: 12, ConsoleColor.Black);

                    while (!_enabled)
                    {
                        Thread.Sleep(100);
                    }

                    const int offset = 0;

                    string message = _message.PadLeft(_message.Length + Program.WindowWidth - offset * 2);
                    message = message.PadRight(message.Length + Program.WindowWidth - offset * 2);

                    Program.Write(new String('─', Program.WindowWidth), left: 0, top: 10, ConsoleColor.DarkGray);
                    Program.Write(new String(' ', Program.WindowWidth), left: 0, top: 11, ConsoleColor.Black, ConsoleColor.Gray);
                    Program.Write(new String('─', Program.WindowWidth), left: 0, top: 12, ConsoleColor.DarkGray);

                    while (_enabled)
                    {
                        for (int i = 0; i <= message.Length - (Program.WindowWidth - offset * 2); i++)
                        {
                            Program.Write(message.Substring(i, Program.WindowWidth - offset * 2), left: offset, top: 11, ConsoleColor.Black, ConsoleColor.Gray);

                            Thread.Sleep(100);

                            if (!_enabled)
                            {
                                break;
                            }
                        }
                    }
                }
            });
        }

        public void Start(string message = null)
        {
            if (_locked)
            {
                return;
            }

            if (message != null)
            {
                Trace.Assert(!_enabled);

                _message = message;
            }

            Trace.Assert(_message != null);

            _enabled = true;
        }

        public void Stop()
        {
            if (_locked)
            {
                return;
            }

            _enabled = false;
        }

        public void Lock()
        {
            _locked = true;
        }

        public void Unlock()
        {
            _locked = false;
        }
    }
}