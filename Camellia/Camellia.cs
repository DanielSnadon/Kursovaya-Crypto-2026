using System;
using System.Buffers.Binary;

public sealed class Camellia
{
    private static readonly byte[] sBox1 =
    {
        0x70, 0x82, 0x2C, 0xEC, 0xB3, 0x27, 0xC0, 0xE5, 0xE4, 0x85, 0x57, 0x35, 0xEA, 0x0C, 0xAE, 0x41,
        0x23, 0xEF, 0x6B, 0x93, 0x45, 0x19, 0xA5, 0x21, 0xED, 0x0E, 0x4F, 0x4E, 0x1D, 0x65, 0x92, 0xBD,
        0x86, 0xB8, 0xAF, 0x8F, 0x7C, 0xEB, 0x1F, 0xCE, 0x3E, 0x30, 0xDC, 0x5F, 0x5E, 0xC5, 0x0B, 0x1A,
        0xA6, 0xE1, 0x39, 0xCA, 0xD5, 0x47, 0x5D, 0x3D, 0xD9, 0x01, 0x5A, 0xD6, 0x51, 0x56, 0x6C, 0x4D,
        0x8B, 0x0D, 0x9A, 0x66, 0xFB, 0xCC, 0xB0, 0x2D, 0x74, 0x12, 0x2B, 0x20, 0xF0, 0xB1, 0x84, 0x99,
        0xDF, 0x4C, 0xCB, 0xC2, 0x34, 0x7E, 0x76, 0x05, 0x6D, 0xB7, 0xA9, 0x31, 0xD1, 0x17, 0x04, 0xD7,
        0x14, 0x58, 0x3A, 0x61, 0xDE, 0x1B, 0x11, 0x1C, 0x32, 0x0F, 0x9C, 0x16, 0x53, 0x18, 0xF2, 0x22,
        0xFE, 0x44, 0xCF, 0xB2, 0xC3, 0xB5, 0x7A, 0x91, 0x24, 0x08, 0xE8, 0xA8, 0x60, 0xFC, 0x69, 0x50,
        0xAA, 0xD0, 0xA0, 0x7D, 0xA1, 0x89, 0x62, 0x97, 0x54, 0x5B, 0x1E, 0x95, 0xE0, 0xFF, 0x64, 0xD2,
        0x10, 0xC4, 0x00, 0x48, 0xA3, 0xF7, 0x75, 0xDB, 0x8A, 0x03, 0xE6, 0xDA, 0x09, 0x3F, 0xDD, 0x94,
        0x87, 0x5C, 0x83, 0x02, 0xCD, 0x4A, 0x90, 0x33, 0x73, 0x67, 0xF6, 0xF3, 0x9D, 0x7F, 0xBF, 0xE2,
        0x52, 0x9B, 0xD8, 0x26, 0xC8, 0x37, 0xC6, 0x3B, 0x81, 0x96, 0x6F, 0x4B, 0x13, 0xBE, 0x63, 0x2E,
        0xE9, 0x79, 0xA7, 0x8C, 0x9F, 0x6E, 0xBC, 0x8E, 0x29, 0xF5, 0xF9, 0xB6, 0x2F, 0xFD, 0xB4, 0x59,
        0x78, 0x98, 0x06, 0x6A, 0xE7, 0x46, 0x71, 0xBA, 0xD4, 0x25, 0xAB, 0x42, 0x88, 0xA2, 0x8D, 0xFA,
        0x72, 0x07, 0xB9, 0x55, 0xF8, 0xEE, 0xAC, 0x0A, 0x36, 0x49, 0x2A, 0x68, 0x3C, 0x38, 0xF1, 0xA4,
        0x40, 0x28, 0xD3, 0x7B, 0xBB, 0xC9, 0x43, 0xC1, 0x15, 0xE3, 0xAD, 0xF4, 0x77, 0xC7, 0x80, 0x9E,
    };

    private static byte RotateLeftByte(byte value, int count)
    {
        return (byte)(((value << count) | (value >> (8 - count))) & 0xFF);
    }

    private static byte SBox2(byte value) => RotateLeftByte(sBox1[value], 1);
    private static byte SBox3(byte value) => RotateLeftByte(sBox1[value], 7);
    private static byte SBox4(byte value) => sBox1[RotateLeftByte(value, 1)];
    private static (ulong left, ulong right) ReadBlockWords(byte[] block)
    {
        if (block is null)
        {
            throw new ArgumentNullException(nameof(block));
        }

        if (block.Length != 16)
        {
            throw new ArgumentException("Ошибка ReadBlockWords Camellia: для чтения необходим блок размером 16 байт.");
        }

        ulong left = BinaryPrimitives.ReadUInt64BigEndian(block.AsSpan(0, 8));
        ulong right = BinaryPrimitives.ReadUInt64BigEndian(block.AsSpan(8, 8));

        return (left, right);
    }

    private static ulong F(ulong halfBlock, ulong roundKey)
    {
        Span<byte> bytes = stackalloc byte[8];
        BinaryPrimitives.WriteUInt64BigEndian(bytes, halfBlock ^ roundKey);

        byte t1 = sBox1[bytes[0]];
        byte t2 = SBox2(bytes[1]);
        byte t3 = SBox3(bytes[2]);
        byte t4 = SBox4(bytes[3]);
        byte t5 = SBox2(bytes[4]);
        byte t6 = SBox3(bytes[5]);
        byte t7 = SBox4(bytes[6]);
        byte t8 = sBox1[bytes[7]];

        bytes[0] = (byte)(t1 ^ t3 ^ t4 ^ t6 ^ t7 ^ t8);
        bytes[1] = (byte)(t1 ^ t2 ^ t4 ^ t5 ^ t7 ^ t8);
        bytes[2] = (byte)(t1 ^ t2 ^ t3 ^ t5 ^ t6 ^ t8);
        bytes[3] = (byte)(t2 ^ t3 ^ t4 ^ t5 ^ t6 ^ t7);
        bytes[4] = (byte)(t1 ^ t2 ^ t6 ^ t7 ^ t8);
        bytes[5] = (byte)(t2 ^ t3 ^ t5 ^ t7 ^ t8);
        bytes[6] = (byte)(t3 ^ t4 ^ t5 ^ t6 ^ t8);
        bytes[7] = (byte)(t1 ^ t4 ^ t5 ^ t6 ^ t7);

        return BinaryPrimitives.ReadUInt64BigEndian(bytes);
    }

    private static ulong FL(ulong value, ulong subkey)
    {
        uint left = (uint)(value >> 32);
        uint right = (uint)(value & uint.MaxValue);

        uint keyLeft = (uint)(subkey >> 32);
        uint keyRight = (uint)(subkey & uint.MaxValue);

        right ^= ops32.RotateLeft(left & keyLeft, 1u);
        left ^= right | keyRight;

        return ((ulong)left << 32) | right;
    }

    private static ulong FLInverse(ulong value, ulong subkey)
    {
        uint left = (uint)(value >> 32);
        uint right = (uint)(value & uint.MaxValue);

        uint keyLeft = (uint)(subkey >> 32);
        uint keyRight = (uint)(subkey & uint.MaxValue);

        left ^= right | keyRight;
        right ^= ops32.RotateLeft(left & keyLeft, 1u);

        return ((ulong)left << 32) | right;
    }

    private static (ulong left, ulong right) BuildKA(ulong klLeft, ulong klRight)
    {
        const ulong sigma1 = 0xA09E667F3BCC908BUL;
        const ulong sigma2 = 0xB67AE8584CAA73B2UL;
        const ulong sigma3 = 0xC6EF372FE94F82BEUL;
        const ulong sigma4 = 0x54FF53A5F1D36F1CUL;

        ulong d1 = klLeft;
        ulong d2 = klRight;

        d2 ^= F(d1, sigma1);
        d1 ^= F(d2, sigma2);

        d1 ^= klLeft;
        d2 ^= klRight;

        d2 ^= F(d1, sigma3);
        d1 ^= F(d2, sigma4);

        return (d1, d2);
    }

    private static (ulong left, ulong right) RotateKey(UInt128 key, int shift)
    {
        UInt128 rotated = UInt128.RotateLeft(key, shift);
        ulong left = (ulong)(rotated >> 64);
        ulong right = (ulong)(rotated & ulong.MaxValue);
        return (left, right);
    }

    private static (ulong[] whiteningKeys, ulong[] roundKeys, ulong[] layerKeys)
        ExpandKey128(ulong klLeft, ulong klRight)
    {
        var (kaLeft, kaRight) = BuildKA(klLeft, klRight);

        UInt128 kl = ((UInt128)klLeft << 64) | klRight;
        UInt128 ka = ((UInt128)kaLeft << 64) | kaRight;

        var kl15 = RotateKey(kl, 15);
        var kl45 = RotateKey(kl, 45);
        var kl60 = RotateKey(kl, 60);
        var kl77 = RotateKey(kl, 77);
        var kl94 = RotateKey(kl, 94);
        var kl111 = RotateKey(kl, 111);

        var ka15 = RotateKey(ka, 15);
        var ka30 = RotateKey(ka, 30);
        var ka45 = RotateKey(ka, 45);
        var ka60 = RotateKey(ka, 60);
        var ka94 = RotateKey(ka, 94);
        var ka111 = RotateKey(ka, 111);

        ulong[] whiteningKeys = { klLeft, klRight, ka111.left, ka111.right };
        ulong[] layerKeys = { ka30.left, ka30.right, kl77.left, kl77.right };
        ulong[] roundKeys =
        {
            kaLeft, kaRight,
            kl15.left, kl15.right,
            ka15.left, ka15.right,
            kl45.left, kl45.right,
            ka45.left, kl60.right,
            ka60.left, ka60.right,
            kl94.left, kl94.right,
            ka94.left, ka94.right,
            kl111.left, kl111.right
        };

        return (whiteningKeys, roundKeys, layerKeys);
    }
}