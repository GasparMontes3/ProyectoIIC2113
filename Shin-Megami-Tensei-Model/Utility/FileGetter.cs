namespace Shin_Megami_Tensei_Model;

public class FileGetter
{
    private ViewPrinter view;
    public FileGetter(ViewPrinter view) => this.view = view;
    public string[] GetTeamsArrayFromPath(string teamsLocation)
    {
        view.PrintChooseFile();
        return PickTeamsFile(teamsLocation);
    }

    public string[] PickTeamsFile(string teamsLocation)
    {
        string[] allTeamFilesArray = CreateTeamFilesArray(teamsLocation);
        for (int i = 0; i < allTeamFilesArray.Length; i++)
        {
            view.PrintFileName(i, Path.GetFileName(allTeamFilesArray[i]));
        }
        return SelectTeamsFileFromInput(allTeamFilesArray);
    }

    public string[] CreateTeamFilesArray(string teamsLocation)
    {
        return Directory.GetFiles(teamsLocation, "*.txt", SearchOption.TopDirectoryOnly);
    }

    public string[] SelectTeamsFileFromInput(string[] allTeamFilesArray)
    {
        int input = view.ReturnIntFromUserInput();
        if (input != -1 && input < allTeamFilesArray.Length)
        {
            return ReturnContentsFromSelectedTeamFile(input, allTeamFilesArray);
        }
        return Array.Empty<string>();
    }

    string[] ReturnContentsFromSelectedTeamFile(int input, string[] allTeamFilesArray)
    {
        return File.ReadAllLines(allTeamFilesArray[input]);
    }
}