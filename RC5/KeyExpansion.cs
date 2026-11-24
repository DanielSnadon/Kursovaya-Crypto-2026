using System;
using System.Buffers.Binary;

internal static class Rc5KeyExpansion
{
    internal const int RoundCount = 12;
    private const int KeySizeBytes = 16;
    private const uint P = 0xB7E15163u;
    private const uint Q = 0x9E3779B9u;

    public static uint[] GenerateRoundKeys(byte[] key)
    {
        if (key == null)
            throw new ArgumentNullException("Ошибка GenerateRoundKeys RC5: ключ не может быть NULL.");
        if (key.Length != KeySizeBytes)
            throw new ArgumentException("Ошибка GenerateRoundKeys RC5: требуется шестнадцатибайтовый ключ.");

        uint[] words = new uint[KeySizeBytes / sizeof(uint)];
        for (int i = 0; i < words.Length; i++)
        {
            words[i] = BinaryPrimitives.ReadUInt32LittleEndian(key.AsSpan(i * sizeof(uint), sizeof(uint)));
        }

        uint[] subkeys = new uint[2 * (RoundCount + 1)];
        subkeys[0] = P;
        for (int i = 1; i < subkeys.Length; i++)
        {
            subkeys[i] = ops32.Add(subkeys[i - 1], Q);
        }

        uint u = 0;
        uint v = 0;
        int sIndex = 0;
        int wordIndex = 0;
        int iterations = 3 * Math.Max(subkeys.Length, words.Length);

        for (int step = 0; step < iterations; step++)
        {
            uint sum = ops32.Add(u, v);
            u = ops32.RotateLeft(ops32.Add(subkeys[sIndex], sum), 3u);
            subkeys[sIndex] = u;

            sum = ops32.Add(u, v);
            v = ops32.RotateLeft(ops32.Add(words[wordIndex], sum), sum);
            words[wordIndex] = v;

            sIndex = (sIndex + 1) % subkeys.Length;
            wordIndex = (wordIndex + 1) % words.Length;
        }

        return subkeys;
    }
}