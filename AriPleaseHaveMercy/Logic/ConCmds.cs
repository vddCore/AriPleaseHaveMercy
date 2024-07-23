namespace AriPleaseHaveMercy.Logic;

using System.Text;
using Chroma.Commander;
using Chroma.Commander.Expressions;

public static class ConCmds
{
    [ConsoleCommand("help")]
    private static void HelpCommand(DebugConsole console, params ExpressionValue[] _)
    {
        console.Print("--- COMMANDS ---");
        foreach (var command in console.Commands)
        {
            var sb = new StringBuilder();
            sb.Append(command.Trigger);
                
            if (command.DefaultArguments != null)
            {
                sb.Append(" [");
                sb.Append(string.Join(
                    ' ', 
                    command.DefaultArguments.Select(
                        x => x.ToConsoleStringRepresentation()
                    )
                ));
                sb.Append("]");
            }

            sb.Append(" - ");
            sb.Append(command.Description);
            console.Print(sb.ToString());
        }

        console.Print("\n--- VARIABLES ---");
        foreach (var variable in console.Variables)
        {
            console.Print($"{variable.ConVarName} : {variable.Type} - {variable.Description}");
        }
    }
}