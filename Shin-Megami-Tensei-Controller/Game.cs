using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei_Model;

namespace Shin_Megami_Tensei;

public class Game
{
    private string teamsLocation;
    private ViewPrinter view;
    private GameBoardBuilder gameBoardBuilder;
    private FileGetter fileGetter;
    private Battle battle;
    public Game(View _view, string teamsFolder)
    {
        teamsLocation =  teamsFolder;
        view = new ViewPrinter(_view);
        gameBoardBuilder = new GameBoardBuilder();
        fileGetter = new FileGetter(view);
    }
    
    public void Play()
    {
        string[] teamsArray = fileGetter.GetTeamsArrayFromPath(teamsLocation);
        (string, List<Unit>, string, List<Unit>) gameBoard = gameBoardBuilder.BuildTeams(view, teamsArray);
        if (gameBoard.Item1 != "")
        {
            battle = new Battle(view, gameBoard);
            battle.BeginBattle();
        }
        else view.PrintInvalidTeams();
    }
}