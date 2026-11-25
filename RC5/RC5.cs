using System;
using System.Buffers.Binary;

public sealed class Rc5
{
    private uint[] subkeys = Array.Empty<uint>();

    public void SetKey(byte[] key)
    {
        subkeys = Rc5KeyExpansion.GenerateRoundKeys(key);
    }

    private static (uint a, uint b) ReadBlockWords(byte[] block)
    {
        if (block is null)
        {
            throw new ArgumentNullException(nameof(block));
        }

        if (block.Length != 8)
        {
            throw new ArgumentException("Ошибка ReadBlockWords RC5: шифрование требует 8-байтовый ввод.");
        }

        uint a = BinaryPrimitives.ReadUInt32LittleEndian(block.AsSpan(0, 4));
        uint b = BinaryPrimitives.ReadUInt32LittleEndian(block.AsSpan(4, 4));

        return (a, b);
    }

    public byte[] EncryptBlock(byte[] block)
    {
        uint[] keys = subkeys;
        if (keys.Length == 0)
        {
            throw new InvalidOperationException("Ошибка EncryptBlock RC5: шифрование требует установленный с помощью setkey() ключ.");
        }

        var (a, b) = ReadBlockWords(block);

        a = ops32.Add(a, keys[0]);
        b = ops32.Add(b, keys[1]);

        for (int round = 1; round <= Rc5KeyExpansion.RoundCount; round++)
        {
            a = ops32.RotateLeft(a ^ b, b);
            a = ops32.Add(a, keys[2 * round]);

            b = ops32.RotateLeft(b ^ a, a);
            b = ops32.Add(b, keys[2 * round + 1]);
        }

        byte[] result = new byte[8];
        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(0, 4), a);
        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(4, 4), b);
        return result;
    }

    public byte[] DecryptBlock(byte[] block)
    {
        uint[] keys = subkeys;
        if (keys.Length == 0)
        {
            throw new InvalidOperationException("Call SetKey before decrypting.");
        }

        var (a, b) = ReadBlockWords(block);

        for (int round = Rc5KeyExpansion.RoundCount; round >= 1; round--)
        {
            b = ops32.Subtract(b, keys[2 * round + 1]);
            b = ops32.RotateRight(b, a) ^ a;

            a = ops32.Subtract(a, keys[2 * round]);
            a = ops32.RotateRight(a, b) ^ b;
        }

        a = ops32.Subtract(a, keys[0]);
        b = ops32.Subtract(b, keys[1]);

        byte[] result = new byte[8];
        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(0, 4), a);
        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(4, 4), b);
        return result;
    }
}