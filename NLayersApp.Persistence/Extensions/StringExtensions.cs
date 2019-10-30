using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace NLayersApp.Persistence.Extensions
{
    public static class StringExtensions
    {
        public static Guid ToCriptedGuid(this string source)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(source));
                return new Guid(hash);
            }
        }
    }
}
