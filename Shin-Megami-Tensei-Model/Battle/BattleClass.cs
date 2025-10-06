namespace Shin_Megami_Tensei_Model;

public class Battle
{
    private Player _player1;
    private Player _player2;
    private ViewPrinter view;
    
    public Battle(ViewPrinter view, (string, List<Unit>, string, List<Unit>) gameBoard)
    {
        this.view = view;
        _player1 = new Player(view, (gameBoard.Item1, gameBoard.Item2));
        _player2 = new Player(view, (gameBoard.Item3, gameBoard.Item4));
        _player1.SetAttributes(_player2);
        _player2.SetAttributes(_player1);
    }

    public void BeginBattle()
    {
        view.PrintFortyHyphen();
        ExecutePlayerTurn(_player1);
    }

    public void ExecutePlayerTurn(Player player)
    {
        ExecuteStartOfTurn(player);
        while (player.IsPlayerTurn())
        {
            ExecuteTurnLoopLogic(player);
            if (IsGameOver(player) || IsGameOver(player.opponent)) return;
        }
        ExecutePlayerTurn(player.opponent);
    }

    public void ExecuteStartOfTurn(Player player)
    {
        PrintRoundStart(player.teamName);
        player.SetupStartOfRound();
    }

    public void ExecuteTurnLoopLogic(Player player)
    {
        PrintStartOfTurn(player);
        player.ChooseActions();
    }

    public bool IsGameOver(Player player)
    {
        for (int unit = 0; unit < player.activeUnits.Count; unit++)
        {
            if (player.activeUnits[unit].stats.HP > 0)
            {
                return false;
            }
        }
        EndGame(player);
        return true;
    }

    public void EndGame(Player player)
    {
        if (player.teamName == _player1.teamName) view.PrintWinnerMsg(_player2.teamName);
        else view.PrintWinnerMsg(_player1.teamName);
    }

    public void PrintStartOfTurn(Player player)
    {
        ListTeams();
        player.ShowRemainingTurns();
        player.ShowTurnOrder();
    }

    public void PrintRoundStart(string teamName)
    {
        view.PrintRoundStartMsg(teamName);
        view.PrintFortyHyphen();
    }

    public void ListTeams()
    {
        PrintTeamStatusList(_player1.teamName, _player1.activeUnits);
        PrintTeamStatusList(_player2.teamName, _player2.activeUnits);
        view.PrintFortyHyphen();
    }

    public void PrintTeamStatusList(string teamName, List<Unit> teamUnitList)
    {
        view.PrintTeamNameMsg(teamName);
        ListTeamOnBoardInInitialOrder(teamUnitList);
    }

    public void ListTeamOnBoardInInitialOrder(List<Unit> teamUnitList)
    {
        int samuraiIndex = teamUnitList.FindIndex(unit => unit is Samurai);
        var orderedForPrinting = teamUnitList.Skip(samuraiIndex).Concat(teamUnitList.Take(samuraiIndex));
        ListTeamOnView(orderedForPrinting);
    }
    
    public void ListTeamOnView(IEnumerable<Unit> orderedForPrinting)
    {
        int count = 0;
        foreach (var unit in orderedForPrinting)
        {
            PrintUnitLine(unit, count);
            count++;
        }
        while (count < 4)
        {
            view.PrintEmptyLetterIndex(IntToAlphabet(count));
            count++;
        }
    }

    public void PrintUnitLine(Unit unit, int count)
    {
        if (unit.stats.HP > 0 || unit is Samurai) 
            view.PrintUnitLineMsg(IntToAlphabet(count), (unit.name, unit.stats.HP, unit.stats.MaxHP, unit.stats.MP, unit.stats.MaxMP));
        else view.PrintEmptyLetterIndex(IntToAlphabet(count));
    }

    public char IntToAlphabet(int number) //funcion general
    {
        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        if (number >= 0 && number < alphabet.Length) return alphabet[number];
        return '?';
    }
}