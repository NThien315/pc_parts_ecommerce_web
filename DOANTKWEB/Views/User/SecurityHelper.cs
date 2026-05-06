using System;
using System.Security.Cryptography;
using System.Text;

namespace DOANTKWEB.Helpers
{
    public static class SecurityHelper
    {
        // Hàm mã hóa MD5 (Dùng chung cho cả hệ thống)
        public static string HashPassword(string password)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(password);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}