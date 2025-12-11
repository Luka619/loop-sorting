namespace LoopSorting
{
    public readonly struct Block
    {
        public BlockColor Color { get; }
        public bool Hidden { get; }

        public Block(BlockColor color, bool hidden = false)
        {
            Color = color;
            Hidden = hidden;
        }

        public override string ToString()
        {
            return Hidden ? $"{Color}(Hidden)" : Color.ToString();
        }
    }
}
