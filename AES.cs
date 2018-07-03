using System;
using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

public static class AES{
    static readonly string KEYCODE = "32345676901734597830122456759012";

    /// <summary>  
    /// 256位AES加密  
    /// </summary>  
    /// <param name="toEncrypt"></param>  
    /// <returns></returns>  
    public static string Encrypt(string toEncrypt)
    {
        // 256-AES key      
        byte[] keyArray = UTF8Encoding.UTF8.GetBytes(KEYCODE);
        byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = keyArray;
        rDel.Mode = CipherMode.ECB;
        rDel.Padding = PaddingMode.PKCS7;

        ICryptoTransform cTransform = rDel.CreateEncryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

        return Convert.ToBase64String(resultArray, 0, resultArray.Length);
    }

    /// <summary>  
    /// 256位AES解密  
    /// </summary>  
    /// <param name="toDecrypt"></param>  
    /// <returns></returns>  
    public static string Decrypt(string toDecrypt)
    {
        // 256-AES key      
        byte[] keyArray = UTF8Encoding.UTF8.GetBytes(KEYCODE);
        byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);

        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = keyArray;
        rDel.Mode = CipherMode.ECB;
        rDel.Padding = PaddingMode.PKCS7;

        ICryptoTransform cTransform = rDel.CreateDecryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

        return UTF8Encoding.UTF8.GetString(resultArray);
    }
    
}
