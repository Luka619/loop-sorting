namespace LoopSorting
{
    public readonly struct Block
    {
        public BlockColor Color { get; }

        public Block(BlockColor color)
        {
            Color = color;
        }

        public override string ToString()
        {
            return Color.ToString();
        }
    }
}
