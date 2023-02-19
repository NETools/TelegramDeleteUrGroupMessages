using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp15
{
    internal static class ConsoleExtensions
    {
        public static int ReadLineInt32(string promptMessage)
        {
            Console.Write(promptMessage);
            var input = Console.ReadLine();
            int result = 0;
            if (input != null)
                result = int.Parse(input);
            return result;
        }

        public static string ReadLineString(string promptMessage)
        {
            Console.Write(promptMessage);
            var input = Console.ReadLine();
            return input;
        }

    }
}
