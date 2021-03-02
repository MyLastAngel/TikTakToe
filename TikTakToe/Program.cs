using System;
using System.Collections.Generic;
using System.Linq;

/**
 Логика простая - поле для игры 3х3 представлено в виде линейного списка
 У каждой ячейки есть спиисок шагов - которые выстраивают победную линию
 По этим шагам и проверяется победа.

 Компьютер сначала ищет линию с 1 недостающей ячейкой для победы, если это линия компьютера - то ставит туда и завершает игру.
 Если это линия пользователя, то запомниает чтобы перекрыть, но ищет - вдруг есть линия своя для победы
 Если нечего перекрывать, то рандомно ходит в доступную ячейку
 */
namespace TikTakToe
{
    public class Program
    {
        #region Поля
        // Генератор шагов
        readonly static Random random = new Random();

        // Поле для игры
        readonly static BoxCell[] box = new BoxCell[9];
        #endregion

        public static void Main()
        {
            var isUser = false;

            // о первый ходит
            Console.Write("Who start? 1 - user/2 - computer (default - computer): ");
            var key = Console.ReadLine();
            var v = 0;
            if (int.TryParse(key, out v) && v == 1)
                isUser = true;

            // иницализация поля для игры
            for (var i = 0; i < 9; i++)
                box[i] = new BoxCell(i);

            while (true)
            {
                if (isUser)
                {
                    Console.Write("User move: ");
                    key = Console.ReadLine().ToUpper();

                    var index = GetIndex(key);
                    if (index == -1)
                    {
                        Console.WriteLine("Move out of range..");
                        continue;
                    }

                    if (box[index].State.HasValue)
                    {
                        var who = box[index].State.Value ? "User" : "Computer";
                        Console.WriteLine($"Already set: '{who}'");
                        continue;
                    }

                    box[index].State = true;
                }
                else
                {
                    // Ходим компьютером
                    ComputerMove();
                }

                if (End())
                    break;

                isUser = !isUser;
            }

            Console.WriteLine("Press any key");
            Console.ReadKey();
        }

        /// <summary>Получает Index в массиве из строки ввода</summary>
        static int GetIndex(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || key.Length < 2)
                return -1;

            var v = 0;
            if (!int.TryParse(key[1].ToString(), out v) || v < 0 || v > 3)
                return -1;

            var result = (v - 1) * 3;

            if (key[0] == 'A')
                result += 0;
            else if (key[0] == 'B')
                result += 1;
            else if (key[0] == 'C')
                result += 2;
            else
                return -1;


            return result;
        }
        /// <summary>Получает строку ввода из Index </summary>
        static string GetKey(int index)
        {
            var v = (int)(index / 3) + 1;

            var ch = index % 3;

            if (ch == 0)
                return $"A{v}";

            if (ch == 1)
                return $"B{v}";

            if (ch == 2)
                return $"C{v}";

            return "~~ERROR";
        }

        // ходит компьютер
        static void ComputerMove()
        {
            var emptyIndexes = new List<int>();
            var index = 0;
            int? moveIndex = null;

            foreach (var cell in box)
            {
                // Если у нас пустое поле или нет линий для этого поля
                if (!cell.State.HasValue)
                {
                    emptyIndexes.Add(cell.Index);
                    continue;
                }
                
                var isUser = cell.State.Value;

                foreach (var step in cell.Steps)
                {
                    var count = 1;

                    // Делаем 3 прыжка - тк 3 в ряд это победа
                    for (; count < 3; count++)
                    {
                        index = cell.Index + step * count;
                        var nextCell = box[index];

                        if (!nextCell.State.HasValue)
                        {
                            // Если у нас ктото близок к победе
                            // либо побеждаем - либо перекрываем
                            if (count == 2)
                            {
                                // Если компьютер побеждает - завершаем игру
                                if (!isUser)
                                {
                                    nextCell.State = false;
                                    Console.WriteLine($"Computer move: {GetKey(index)}");
                                    return;
                                }
                                else // Если у нас человек - то мы запоминает index  и смотрим - вдруг есть победа компьюетра
                                    moveIndex = index;
                            }

                            break;
                        }
                        else if (nextCell.State.Value != isUser)
                            break;
                    }

                    if (count == 3)
                        throw new InvalidOperationException("Мы пропустили чью то победу");
                }
            }

            // Закрываем поле пользователя
            if (moveIndex.HasValue)
            {
                box[moveIndex.Value].State = false;
                Console.WriteLine($"Computer move: {GetKey(moveIndex.Value)}");
                return;
            }


            // Получаем любое рандомное место и ставим ход
            var rnd = random.Next(0, emptyIndexes.Count - 1);
            index = emptyIndexes[rnd];

            box[index].State = false;
            Console.WriteLine($"Computer move: {GetKey(index)}");
        }

        // Проверка на победу
        static bool End()
        {
            foreach (var cell in box)
            {
                // Если у нас пустое поле или нет линий для этого поля
                if (!cell.State.HasValue || cell.Steps.Length == 0)
                    continue;

                var count = 1;
                var isUser = cell.State.Value;
                foreach (var step in cell.Steps)
                {
                    for (; count < 3; count++)
                    {
                        var nextCell = box[cell.Index + step * count];
                        // Если у нас у ячейки в линии нет значения или значение не равно родительскому
                        // то тут точн онет выигрыша
                        if (!nextCell.State.HasValue || nextCell.State.Value != isUser)
                            break;

                    }
                }

                // Нашли пробедителя
                if (count == 3)
                {
                    var who = isUser ? "User" : "Computer";
                    Console.WriteLine($"'{who}' is WIN!!!");
                    return true;
                }
            }

            // Если у нас нет больше ходов
            if (!box.Any(kv => !kv.State.HasValue))
            {
                Console.WriteLine("No more moves... game over.");
                return true;
            }

            return false;
        }

        #region Вспомгательные классы

        class BoxCell
        {
            #region Свойства
            public int Index { get; private set; }
            /// <summary>Шаги для проверки выгрышной линии</summary>
            public int[] Steps { get; private set; }

            /// <summary>
            /// true - user
            /// false - computer
            /// null - not set
            /// </summary>
            public bool? State { get; set; }
            #endregion

            public BoxCell(int index)
            {
                Index = index;

                switch (index)
                {
                    case 0:
                        Steps = new int[] { 1, 3, 4 };
                        break;
                    case 1:
                        Steps = new int[] { 3 };
                        break;
                    case 2:
                        Steps = new int[] { 2, 3 , -1};
                        break;
                    case 3:
                        Steps = new int[] { 1 };
                        break;
                    case 5:
                        Steps = new int[] { -1 };
                        break;
                    case 6:
                        Steps = new int[] { 1, -3 };
                        break;
                    case 7:
                        Steps = new int[] { -3 };
                        break;
                    case 8:
                        Steps = new int[] { -1, -3, -4 };
                        break;
                    default:
                        Steps = new int[0];
                        break;
                }
            }

            public override string ToString()
            {
                var who = "not set";
                if (State.HasValue)
                    who = State.Value ? "user" : "computer";

                return $"{Index} ({GetKey(Index)}) - {who}";
            }


        }

        #endregion
    }
}