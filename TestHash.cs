using System;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main()
    {
        string motDePasse = ""admin123"";
        string salted = ""PretBancaire_"" + motDePasse;
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(salted));
        var sb = new StringBuilder();
        foreach (byte b in bytes)
            sb.Append(b.ToString(""x2""));
        Console.WriteLine(sb.ToString());
    }
}
