using System;
using System.Collections.Generic;
using System.Linq;
namespace ProjectMaze
{
    internal class MazeGenerator
    {
        #region Maze settings
        private int rows, columns, difficultyIndex;
        private bool? isTurnSeedCheckBoxChecked;
        #endregion

        internal MazeGenerator(int rows, int columns, bool? isTurnSeedCheckBoxChecked, int difficultyIndex = 0)
        {
            this.rows = rows;
            this.columns= columns;
            this.isTurnSeedCheckBoxChecked = isTurnSeedCheckBoxChecked;
            this.difficultyIndex = difficultyIndex;
        }

        internal Cell GenerateRandomEmptyCell(Cell[,] mapArray)
        {
            Random rnd = new Random();
            int x, y;

            do // Нахождение свободной ячейки
            {
                x = rnd.Next(0, columns - 1);
                if (x % 2 != 0)
                    x++;
                y = rnd.Next(0, rows - 1);
                if (y % 2 != 0)
                    y++;
            } while (mapArray[x, y] is not Space);

            Cell emptyCell = new Space(x, y);
            return emptyCell;
        }
        internal List<Cell> GetNeighbours(Cell cell, int width, int height, Cell[,] mapArray, bool isVisitedCheck = true)
        {
            //Дистанция ходьбы
            int walkDist = 2;

            //Расположение клетки
            int x = cell.x;
            int y = cell.y;

            //Соседние клетки
            Cell left = new Space(x - walkDist, y);
            Cell up = new Space(x, y - walkDist);
            Cell right = new Space(x + walkDist, y);
            Cell down = new Space(x, y + walkDist);

            List<Cell> nlist = [left, up, right, down];
            List<Cell> newlist = new();

            foreach (Cell n in nlist)
            {
                if (n.x >= 0 && n.x < width && n.y >= 0 && n.y < height) // Если в пределах лабиринта
                {
                    //Проверка была ли посещена клетка
                    if (isVisitedCheck && (mapArray[n.x, n.y] == null || !mapArray[n.x, n.y].IsVisited))
                        newlist.Add(n);
                    //Без проверки
                    else if (!isVisitedCheck)
                        newlist.Add(n);
                }
            }
            return newlist;
        }
        internal Cell GetWallBetweenCells(Cell first, Cell second)
        {
            //Получение клетки между двумя другими
            int x, y;
            x = second.x - first.x;
            y = second.y - first.y;
            x /= 2; y /= 2;
            Cell cell = new Space(first.x + x, first.y + y);
            return cell;
        }
        internal int GetAllPointsCount()
        {
            //Получение количества семян, которые должны будут сгенерированы(или их общее количество) на карте
            if (isTurnSeedCheckBoxChecked == false)//Если семена отключены 
                return 0;
            return Convert.ToInt32(1 + ((difficultyIndex + 1) * 0.75 * ((columns + rows)/10)));
        }
        private Cell GetRandomUnvisitedCell(Cell[,] mapArray)
        {
            //Получение случайной не посещенной клетки
            for (int i = 0; i < columns; i += 2)
            {
                for (int j = 0; j < rows; j += 2)
                {
                    if (mapArray[i, j] == null)
                        return new Space(i, j);
                }
            }
            return null;
        }
        internal void AddToTraces(Cell cell, List<Cell> traces)
        {
            //Добавление в список с историей пройденных клеток
            traces.Add(cell);
            //Если список слишком большой - удаляется первая клетка в списке
            if (traces.Count > rows / 2 * columns / 2)
                traces.RemoveAt(0);
        }
        public Cell MoveBack(List<Cell> traces)
        {
            //Передвижение на прошлую клетку (из списка с историей пройденных клеток)
            Cell cell = traces.Last();
            traces.RemoveAt(traces.Count() - 1);
            return cell;
        }

        internal Cell[,] GetGeneratedMap()
        {
            Console.WriteLine($"\n\n\nГенерация...");

            Console.WriteLine($"Размер лабиринта = {rows}x{columns}");

            List<Cell> traces = new();
            Cell[,] mapArray = new Cell[columns, rows];

            //Стартовая точка генерации
            Cell startCell = new Space(0, 0, true);
            mapArray[startCell.x, startCell.y] = startCell;
            Console.WriteLine($"Стартовая точка генерации: [{startCell.x}][{startCell.y}]");
            Cell currentCell = startCell;
            Random rnd = new Random();
            bool isRandomGenerated = false;

            List<Cell> neighbours;
            do
            {
                if (isRandomGenerated)//Если следующая ячейка генерируется случайно
                {
                    neighbours = GetNeighbours(currentCell, columns, rows, mapArray, !isRandomGenerated);
                    isRandomGenerated = false;
                }
                else
                {
                    neighbours = GetNeighbours(currentCell, columns, rows, mapArray);
                }

                if (neighbours.Count() != 0) //Если есть соседние клетки
                {
                    int rand = rnd.Next(neighbours.Count());
                    Cell neighbourCell = neighbours[rand];
                    neighbourCell.IsVisited = true;
                    mapArray[neighbourCell.x, neighbourCell.y] = neighbourCell;
                    Console.WriteLine($"Переход на точку: [{neighbourCell.x}][{neighbourCell.y}]");

                    //Удаление стены между новой и прошлой клеткой
                    Cell wall = GetWallBetweenCells(currentCell, neighbourCell);
                    mapArray[wall.x, wall.y] = wall;

                    //Добавление клетки в список с историей ходов
                    AddToTraces(currentCell, traces);
                    currentCell = neighbourCell;
                }
                else if (neighbours.Count() == 0 && traces.Count > 0 && currentCell != traces.First()) //Если нет соседей и можно вернутся назад
                {
                    //Ход назад
                    currentCell = MoveBack(traces);
                    Console.WriteLine($"Возврат на [{currentCell.x}][{currentCell.y}]");
                }
                else if (GetRandomUnvisitedCell(mapArray) != null)
                {
                    //Генерация следующей клетки случайно
                    currentCell = GetRandomUnvisitedCell(mapArray);
                    Console.WriteLine($"Переход к следующей случайной клетке: [{currentCell.x}][{currentCell.y}]");
                    isRandomGenerated = true;
                }
                else
                {
                    //Лабиринт сгенерирован, выход из цикла
                    Console.WriteLine($"Все клетки посещены. Лабиринт сгенерирован");
                    break;
                }
            } while (true);

            #region Создание выхода
            Cell exitRandomCell = GenerateRandomEmptyCell(mapArray);
            Exit exit = new Exit(exitRandomCell.x, exitRandomCell.y);
            mapArray[exitRandomCell.x, exitRandomCell.y] = exit;
            Console.WriteLine($"Выход был создан [{exitRandomCell.x}][{exitRandomCell.y}]");
            #endregion

            #region Создание Seeds
            for (int i = 0; i < GetAllPointsCount(); i++)
            {
                Cell randomCell = GenerateRandomEmptyCell(mapArray);
                if (randomCell == null)
                    break;
                Point point = new Point(randomCell.x, randomCell.y);
                mapArray[randomCell.x, randomCell.y] = point;
                Console.WriteLine($"Семечко было создано [{randomCell.x}][{randomCell.y}]");
            }
            Console.WriteLine($"Всего семян было создано: {GetAllPointsCount()}");
            #endregion

            return mapArray;
        }
    }
}
