using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectMaze;
using System.Collections.Generic;
using System.Linq;

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

        [TestMethod]
        public void MoveBackTest()
        {
            List<Cell> cells = new();
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
        [TestMethod]
        public void GetNeighboursTest()
        {
            List<Cell> expected = new();
            Cell cellExpected1 = new Space { x = 0, y = 2 };
            Cell cellExpected2 = new Space { x = 2, y = 0 };
            expected.Add(cellExpected2);
            expected.Add(cellExpected1);

            Cell cell = new Space { x = 0, y = 0 };

            List<Cell> listNeighbours = new MainWindow().GetNeighbours(cell, 3, 3, testArray, false);

            Assert.IsTrue(string.Join("|", listNeighbours.Select(item => item.x + " " + item.y)).ToString() == string.Join("|", expected.Select(item => item.x + " " + item.y)).ToString());
        }
        [TestMethod]
        public void GetWallBetweenCellsTest()
        {
            Cell cell1 = new Space { x = 0, y = 0 };
            Cell cell2 = new Space { x = 2, y = 0 };
            Cell expected = new Space { x = 1, y = 0 };

            Cell WallBetweenCells = new MainWindow().GetWallBetweenCells(cell1, cell2);

            Assert.IsTrue(WallBetweenCells.x == expected.x && WallBetweenCells.y == expected.y);
        }
        [TestMethod]
        public void AddToTracesTest()
        {
            List<Cell> expected = new();
            Cell testCell = new Space { x = 2, y = 2 };
            expected.Add(testCell);

            List<Cell> traces = new();

            new MainWindow().AddToTraces(testCell, traces);
            Assert.AreEqual(string.Join("|", traces).ToString(), string.Join("|", expected).ToString());
        }
    }
}
