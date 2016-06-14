using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Extant.Extensions
{
    public static class MemoryStreamExtensions
    {
        public static void MoveToStart(this MemoryStream ms)
        {
            ms.Position = 0;
        }

        public static void MoveToEnd(this MemoryStream ms)
        {
            ms.Position = ms.Length;
        }

        public static void Shift(this MemoryStream ms, long amount)
        {
            if (amount <= 0)
                return;

            byte[] buffer = ms.GetBuffer();
            for (long i = amount; i < ms.Length; i++)
                buffer[i - amount] = buffer[i];

            ms.Position = (ms.Position - amount).AsPositiveOrDefault();
            ms.SetLength((ms.Length - amount).AsPositiveOrDefault());
        }
    }
}
