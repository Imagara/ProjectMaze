using ProjectMaze;
namespace UnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void MoveBackTest()
        {
            List<Cell> cells = new List<Cell>();
            Cell cell1 = new Space{x = 0, y = 0};
            Cell cell2 = new Space{x = 2, y = 0};
            cells.Add(cell1);
            cells.Add(cell2);

        }
    }
}