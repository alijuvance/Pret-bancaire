using System;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main()
    {
        string Salt = "PretBancaire_";
        string motDePasse = "admin123";
        string salted = Salt + motDePasse;
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(salted));
        var sb = new StringBuilder();
        foreach (byte b in bytes)
            sb.Append(b.ToString("x2"));
        Console.WriteLine(sb.ToString());
    }
}
