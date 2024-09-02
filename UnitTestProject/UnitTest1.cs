using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectMaze;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        // □ ■ 🐔
        // □ ■  □
        //🚪 □  □
        Cell[,] testArray = new Cell[,]
        {
            {
                new Point{x = 0, y = 0, IsVisited = true},
                new Wall{x = 1, y = 0, IsVisited = true},
                new Player{x = 2, y = 0, IsVisited = true}
            },
            {
                new Space { x = 0, y = 1, IsVisited = true},
                new Wall { x = 1, y = 1, IsVisited = true},
                new Space { x = 2, y = 1, IsVisited = true}
            },
            {
                new Exit { x = 0, y = 2, IsVisited = true},
                new Space { x = 1, y = 2, IsVisited = true},
                new Space { x = 2, y = 2, IsVisited = true}
            }
        };
        Cell[,] testArray2 = new Cell[3, 3];


        [TestMethod]
        public void MoveBackTest()
        {
            List<Cell> cells = new List<Cell>();
            Cell cell1 = new Space { x = 0, y = 0 };
            Cell cell2 = new Space { x = 2, y = 0 };
            cells.Add(cell1);
            cells.Add(cell2);

            Cell expected = cell2;
            Assert.AreEqual(new MainWindow().MoveBack(cells), expected);
        }
        [TestMethod]
        public void GenerateRandomEmptyPositionTest()
        {
            Cell expected = new Space { x = 2, y = 2 };

            Cell RandomEmptyCell = new MainWindow().GenerateRandomEmptyCell(testArray, 3, 3);
            Assert.IsTrue(RandomEmptyCell.x == expected.x && RandomEmptyCell.y == expected.y);
        }

        //public void GetNeighboursTest()
        //{
        //    Cell expected = new Space { x = 0, y = 0 };
        //    //testArray2[0,0] =
        //    Assert.AreEqual(new MainWindow().GetNeighbours(testArray, 3, 3), expected);
        //}
        [TestMethod]
        public void GetWallBetweenCellsTest()
        {
            Cell cell1 = new Space { x = 0, y = 0 };
            Cell cell2 = new Space { x = 2, y = 0 };
            Cell expected = new Space { x = 1, y = 0 };

            Cell WallBetweenCells = new MainWindow().GetWallBetweenCells(cell1, cell2);

            Assert.IsTrue(WallBetweenCells.x == expected.x && WallBetweenCells.y == expected.y);
        }
    }
}
