using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("USAGE: ChatConsole accountName password characterName realmName");
                return;
            }

            string accountName = args[0];
            string password = args[1];
            string characterName = args[2];
            string realmName = args[3];

            Chat c = new Chat(accountName, password, characterName, realmName);

            string current = "";
            while ((current = Console.ReadLine()) != "/quit")
            {
                c.Process(current);
            }
            c.Close();
        }
    }
}
