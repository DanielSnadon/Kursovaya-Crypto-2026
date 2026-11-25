// something will appear here soon...

using System;

var rc5 = new Rc5();
rc5.SetKey(Convert.FromHexString("915F4619BE41B2516355A50110A9CE91"));

byte[] plaintext = Convert.FromHexString("21A5DBEE154B8F6D");
byte[] expectedCiphertext = Convert.FromHexString("F7C013AC5B2B8952");

byte[] encrypted = rc5.EncryptBlock(plaintext);
byte[] decrypted = rc5.DecryptBlock(expectedCiphertext);

Console.WriteLine($"Шифрование: {Convert.ToHexString(encrypted)}");
Console.WriteLine($"Расшифрование: {Convert.ToHexString(decrypted)}");

if (!encrypted.AsSpan().SequenceEqual(expectedCiphertext) ||
    !decrypted.AsSpan().SequenceEqual(plaintext))
{
    throw new InvalidOperationException("Ой...");
}