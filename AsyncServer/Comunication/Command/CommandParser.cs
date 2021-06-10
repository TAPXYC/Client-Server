using System;
using System.Diagnostics;




class CommandParser
{
    public static Command ParceMessage(string message)
    {
        string Answer = string.Empty;
        Action Action = null;

        message.Replace("<EOF>", "");

        string[] cmd = message.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

        switch (cmd[0])
        {
            case "Shutdown":
                int timeBefore = int.Parse(cmd[1]);
                Action = () => Process.Start("shutdown", $"/s /t {timeBefore}");
                Answer = $"StartShutdown_{timeBefore}";
                break;

            case "UnShutdown":
                Action = () => Process.Start("shutdown", $"/a");
                Answer = $"CancelShutdown";
                break;

            case "MakeBring":
                Action = () => Console.Beep(); 
                Answer = "Ok";
                break;

            default:
                Answer = "Ok";
                break;
        }

        return new Command(Answer, Action);
    }


}
