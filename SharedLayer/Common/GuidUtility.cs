namespace SharedLayer.Common;

using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Security.Cryptography;
using System.Text;

public static class GuidUtility
{
    public static readonly Guid UrlNamespace = new Guid("6ba7b811-9dad-11d1-80b4-00c04fd430c8");

    /// <summary>
    /// تولید یک GUID تکرارپذیر (نسخه ۵) بر اساس یک namespace و یک نام
    /// </summary>
    public static Guid Create(Guid namespaceId, string name)
    {
        byte[] nameBytes = Encoding.UTF8.GetBytes(name);

        // تبدیل namespace به byte[] و اعمال ترتیب byte مناسب (big-endian)
        byte[] namespaceBytes = namespaceId.ToByteArray();
        SwapByteOrder(namespaceBytes);

        // تولید هش SHA1 از namespace + name
        byte[] hash;
        using (SHA1 sha1 = SHA1.Create())
        {
            sha1.TransformBlock(namespaceBytes, 0, namespaceBytes.Length, null, 0);
            sha1.TransformFinalBlock(nameBytes, 0, nameBytes.Length);
            hash = sha1.Hash!;
        }

        // تنظیم نسخه (version 5) و variant مطابق RFC 4122
        hash[6] = (byte)((hash[6] & 0x0F) | 0x50); // version 5
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80); // variant RFC

        // ساخت GUID با 16 بایت اول هش
        byte[] newGuid = new byte[16];
        Array.Copy(hash, 0, newGuid, 0, 16);
        SwapByteOrder(newGuid);
        return new Guid(newGuid);
    }

    private static void SwapByteOrder(byte[] guid)
    {
        void Swap(int a, int b)
        {
            byte t = guid[a];
            guid[a] = guid[b];
            guid[b] = t;
        }

        Swap(0, 3);
        Swap(1, 2);
        Swap(4, 5);
        Swap(6, 7);
    }
}

