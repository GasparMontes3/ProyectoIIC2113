using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei_Models;

namespace Shin_Megami_Tensei;

public class Game
{
    private string teamsLocation;
    private View view;
    private TeamBuilder teamBuilder;
    private FileGetter fileGetter;
    private Battle battle;
    public Game(View _view, string teamsFolder)
    {
        teamsLocation =  teamsFolder;
        view = _view;
        teamBuilder = new TeamBuilder();
        fileGetter = new FileGetter();
    }
    
    public void Play()
    {
        string[] teamsArray = fileGetter.GetTeamsArrayFromPath(view, teamsLocation);
        (string, List<Unit>, string, List<Unit>) gameBoard = teamBuilder.BuildTeams(teamsArray, view);
        if (gameBoard.Item1 != "")
        {
            battle = new Battle(view, gameBoard);
            battle.BeginBattle();
        }
        else view.WriteLine("Archivo de equipos inválido");
    }
}