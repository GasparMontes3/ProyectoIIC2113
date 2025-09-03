using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei_View.ConsoleLib;

namespace Shin_Megami_Tensei_Models;

public class FileGetter
{
    public string[] GetTeamsArrayFromPath(View view, string teamsLocation)
    {
        view.WriteLine("Elige un archivo para cargar los equipos");
        return PickTeamsFile(view, teamsLocation);
    }

    public string[] PickTeamsFile(View view, string teamsLocation)
    {
        string[] allTeamFilesArray = CreateTeamFilesArray(teamsLocation);
        for (int i = 0; i < allTeamFilesArray.Length; i++)
        {
            view.WriteLine($"{i}: {Path.GetFileName(allTeamFilesArray[i])}");
        }
        return SelectTeamsFileFromInput(view, allTeamFilesArray);
    }

    public string[] CreateTeamFilesArray(string teamsLocation)
    {
        return Directory.GetFiles(teamsLocation, "*.txt", SearchOption.TopDirectoryOnly);
    }

    public string[] SelectTeamsFileFromInput(View view, string[] allTeamFilesArray)
    {
        int input = ReturnIntFromUserInput(view);
        if (input != -1 && input < allTeamFilesArray.Length)
        {
            return ReturnContentsFromSelectedTeamFile(input, allTeamFilesArray);
        }
        return Array.Empty<string>();
    }

    public int ReturnIntFromUserInput(View view)
    {
        string input = view.ReadLine();
        try
        {
            int.TryParse(input, out int n);
            return n;
        } 
        catch (Exception)
        {
            Console.WriteLine("Valor invalido");
            return -1;
        }
    }

    public bool CheckIfInputIsValid(int input, int min, int max)
    {
        if (input >= min && input <= max)
        {
            return true;
        }
        return false;
    }

    string[] ReturnContentsFromSelectedTeamFile(int input, string[] allTeamFilesArray)
    {
        return File.ReadAllLines(allTeamFilesArray[input]);
    }
}