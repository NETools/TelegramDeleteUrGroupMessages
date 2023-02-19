using ConsoleApp15;
using TL;

int api_code = ConsoleExtensions.ReadLineInt32("Type in api_id: ");
string api_hash = ConsoleExtensions.ReadLineString("Type in api_hash: ");

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

Console.ForegroundColor = ConsoleColor.Red;
Console.Write("ONLY PRESS ENTER WHEN YOU WANT TO REMOVE YOUR HISTORY FROM TELEGRAM!!!");
Console.ForegroundColor = ConsoleColor.Gray;

Console.ReadLine();

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
            Console.WriteLine($"{id}: Group {group.username}: {group.title}");
            Console.ForegroundColor = ConsoleColor.Gray;

            int offset = 0;
            bool done = false;
            while (!done)
            {
                var messages = await client.Messages_GetHistory(group.ToInputPeer(), offset);
                Console.WriteLine($"Searching batch {i}");
                var userMessages = messages.Messages.Where(p => p.From != null && p.From.ID == client.User.id)
                    .Select(p => (p.ID, p.ToString()));

                var deleteResult = await client.DeleteMessages(group.ToInputPeer(), userMessages.Select(p => p.ID).ToArray());
                userMessages.Select(p => p.Item2).ToList().ForEach(message =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"DELETED: {message}");
                    Console.ForegroundColor = ConsoleColor.Gray;

                    if (message != null && message.Contains("[ChatAddUser]"))
                        done = true;
                });

                offset = messages.Messages[messages.Messages.Length - 1].ID;

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Waiting 1 s..");
                Console.ForegroundColor = ConsoleColor.Gray;
                Thread.Sleep(1000);

            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"ALL YOUR MESSAGES DELETED FROM {group.title}");
            Console.ForegroundColor = ConsoleColor.Gray;
            break;
    }
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"DONE! YOU NEVER EXISTED ON TELEGRAM!");
Console.ForegroundColor = ConsoleColor.Gray;