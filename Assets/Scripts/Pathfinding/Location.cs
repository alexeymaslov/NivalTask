namespace NivalTask.Pathfinding
{
    // Нода на квадратной сетке
    public struct Location
    {
        public readonly int X;
        public readonly int Y;
        
        public Location(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
