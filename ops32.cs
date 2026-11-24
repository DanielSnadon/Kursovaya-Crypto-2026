internal static class ops32
{
    public static uint Add(uint left, uint right)
    {
        return unchecked(left + right);
    }

    public static uint Subtract(uint left, uint right)
    {
        return unchecked(left - right);
    }

    public static uint RotateLeft(uint value, uint count)
    {
        int shift = (int)(count % 32u);
        if (shift == 0)
        {
            return value;
        }

        return (value << shift) | (value >> (32 - shift));
    }

    public static uint RotateRight(uint value, uint count)
    {
        int shift = (int)(count % 32u);
        if (shift == 0)
        {
            return value;
        }

        return (value >> shift) | (value << (32 - shift));
    }
}