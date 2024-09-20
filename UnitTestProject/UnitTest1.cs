using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectMaze;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        MazeGenerator generator = new MazeGenerator(5, 5, false);
        // □ ■ 🐔
        // □ ■  □
        //🚪 □  □
        Cell[,] testArray = new Cell[,]
        {
            {
                new Point(0, 0),
                new Wall(1, 0),
                new Player(2, 0)
            },
            {
                new Space(0, 1),
                new Wall(1, 1),
                new Space(2, 1)
            },
            {
                new Exit(0,2),
                new Space(1,2),
                new Space(2,2)
            }
        };

        [TestMethod]
        public void MoveBackTest()
        {
            List<Cell> cells = new();
            Cell cell1 = new Space(0, 0);
            Cell cell2 = new Space(2, 0);
            cells.Add(cell1);
            cells.Add(cell2);

            Cell expected = cell2;
            Assert.AreEqual(generator.MoveBack(cells), expected);
        }
        [TestMethod]
        public void GenerateRandomEmptyPositionTest()
        {
            Cell expected = new Space(2, 2);

            Cell RandomEmptyCell = generator.GenerateRandomEmptyCell(testArray);
            Assert.IsTrue(RandomEmptyCell.x == expected.x && RandomEmptyCell.y == expected.y);
        }
        [TestMethod]
        public void GetNeighboursTest()
        {
            List<Cell> expected = new();
            Cell cellExpected1 = new Space(0, 2);
            Cell cellExpected2 = new Space(2, 0);
            expected.Add(cellExpected2);
            expected.Add(cellExpected1);

            Cell cell = new Space(0, 0);

            List<Cell> listNeighbours = generator.GetNeighbours(cell, 3, 3, testArray, false);

            Assert.IsTrue(string.Join("|", listNeighbours.Select(item => item.x + " " + item.y)).ToString()
                == string.Join("|", expected.Select(item => item.x + " " + item.y)).ToString());
        }
        [TestMethod]
        public void GetWallBetweenCellsTest()
        {
            Cell cell1 = new Space(0, 0);
            Cell cell2 = new Space(2, 0);
            Cell expected = new Space(1, 0);

            Cell WallBetweenCells = generator.GetWallBetweenCells(cell1, cell2);

            Assert.IsTrue(WallBetweenCells.x == expected.x && WallBetweenCells.y == expected.y);
        }
        [TestMethod]
        public void AddToTracesTest()
        {
            List<Cell> expected = new();
            Cell testCell = new Space(2, 2);
            expected.Add(testCell);

            List<Cell> traces = new();

            generator.AddToTraces(testCell, traces);
            Assert.AreEqual(string.Join("|", traces).ToString(), string.Join("|", expected).ToString());
        }
    }
}
