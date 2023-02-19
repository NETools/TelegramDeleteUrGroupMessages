using ConsoleApp15;
using TL;

int api_code = ConsoleExtensions.ReadLineInt32("Type in api_id: ");
string api_hash = ConsoleExtensions.ReadLineString("Type in api_hash: ");

WTelegram.Helpers.Log = (a, b) => { };

WTelegram.Client client = new WTelegram.Client(api_code, api_hash);

string telNo = ConsoleExtensions.ReadLineString("Type in telphone number (+00xxxxxxxx): ");

await DoLogin(telNo);

async Task DoLogin(string loginInfo)
{
    while (client.User == null)
        switch (await client.Login(loginInfo))
        {
            case "verification_code": loginInfo = ConsoleExtensions.ReadLineString("Type in verification code (see app): "); break;
            case "name": loginInfo = ConsoleExtensions.ReadLineString("Type in name: "); break;
            case "password": loginInfo = ConsoleExtensions.ReadLineString("Type in password: "); break;
            default: loginInfo = null; break;
        }
    Console.WriteLine($"We are logged-in as {client.User} (id {client.User.id})");
}


var chats = await client.Messages_GetAllChats();

Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine("You are registered in following groups: ");
Console.ForegroundColor = ConsoleColor.Gray;

foreach (var (id, chat) in chats.chats)
{
    switch (chat)
    {
        case Channel group when group.IsGroup && !group.IsChannel:

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"Delete: {id}: Group {group.username}: {group.title} (type y for yes)? ");
            Console.ForegroundColor = ConsoleColor.Gray;

            var userMsg = Console.ReadLine();
            if (userMsg != null && !userMsg.ToUpper().Equals("Y"))
                continue;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Deleting: {id}: Group {group.username}: {group.title} (press escape to end searching your messages)!");
            Console.ForegroundColor = ConsoleColor.Gray;

            int offset = 0;
            bool done = false;
            int numMessages = 0;

            Task.Run(() =>
            {
                while (true)
                {
                    if (Console.ReadKey().Key == ConsoleKey.Escape)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("[*] Ending searching");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        done = true;
                        break;
                    }
                }
            });

            bool foundJoined = false;

            while (!done)
            {
                var messages = await client.Messages_GetHistory(group.ToInputPeer(), offset);
                Console.WriteLine($"Searching messages from offset {offset}");
                var userMessages = messages.Messages.Where(p => p.From != null && p.From.ID == client.User.id)
                    .Select(p => (p.ID, p.ToString()));

                var messageIds = userMessages.Select(p => p.ID).ToArray();
                var deleteResult = await client.DeleteMessages(group.ToInputPeer(), messageIds);
                
                numMessages += messageIds.Length;

                userMessages.Select(p => p.Item2).ToList().ForEach(message =>
                {
                    Console.ForegroundColor = foundJoined ? ConsoleColor.DarkRed : ConsoleColor.Red;
                    Console.WriteLine($"DELETED: {message}");
                    Console.ForegroundColor = ConsoleColor.Gray;

                    if (message != null && message.Contains("[ChatAddUser]"))
                        foundJoined = true;
                });

                offset = messages.Messages[messages.Messages.Length - 1].ID;

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Waiting 1 s..");
                Console.ForegroundColor = ConsoleColor.Gray;
                Thread.Sleep(1000);

            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"A total of {numMessages} messages deleted from {group.title}");
            Console.ForegroundColor = ConsoleColor.Gray;
            break;
    }
}