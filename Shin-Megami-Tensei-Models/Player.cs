using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei_Models;

public class Player
{
    public string teamName;
    private List<Unit> units;
    public List<Unit> activeUnits;
    public List<Unit> activeUnitsBySpd;
    private int fullTurns;
    private int blinkingTurns;
    public View view;
    
    public FileGetter fileGetter;

    public Player(View view,(string teamName, List<Unit> team) teamTuple)
    {
        teamName = teamTuple.Item1;
        units = teamTuple.Item2;
        activeUnits = teamTuple.Item2.Take(4).ToList();
        this.view = view;
        fileGetter = new FileGetter();
    }

    public void ShowRemainingTurns()
    {
        view.WriteLine($"Full Turns: {fullTurns}");
        view.WriteLine($"Blinking Turns: {blinkingTurns}");
        view.WriteLine("----------------------------------------");
    }
    
    public void CalculateInitialTurns()
    {
        fullTurns = CountLivingUnits();
        blinkingTurns = 0;
        OrderActiveUnitsBySpd();
    }
    
    public void OrderActiveUnitsBySpd()
    {
        activeUnitsBySpd = activeUnits.OrderByDescending(unit => unit.stats.Spd).ToList();
    }

    public void ShowTurnOrder()
    {
        view.WriteLine("Orden:");
        int j = 1;
        for (int i = 0; i < activeUnitsBySpd.Count; i++)
        {
            if (activeUnitsBySpd[i].stats.HP > 0)
            {
                view.WriteLine($"{j}-{activeUnitsBySpd[i].name}");
                j++;
            }
        }
        view.WriteLine("----------------------------------------");
    }
    
    public bool IsPlayerTurn()
    {
        if (fullTurns == 0 && blinkingTurns == 0)
        {
            return false;
        }
        return true;
    }

    public void Actions(Player opponent)
    {
        if (activeUnitsBySpd[0].stats.HP == 0)
        {
            MoveFirstUnitToBack();
            Actions(opponent);
        }
        else
        {
            List<int> turnsConsumed = activeUnitsBySpd[0].TakeAction(opponent);
            AnalyzeTurnData(turnsConsumed);
            MoveFirstUnitToBack();
        }
    }

    public void AnalyzeTurnData(List<int> turnsConsumed)
    {
        if (!CheckIfPlayerSurrendered(turnsConsumed))
        {
            UpdateTurnCount(turnsConsumed);
        }
    }
    
    public void UpdateTurnCount(List<int> turnsConsumed)
    {
        if (turnsConsumed[0] != 50 && turnsConsumed[1] != 50)
        {
            ReduceTurnsAppropriately(turnsConsumed);
        }
        else
        {
            view.WriteLine($"Se han consumido {fullTurns} Full Turn(s) y {blinkingTurns} Blinking Turn(s)");
            fullTurns = 0;
            blinkingTurns = 0;
        }
    }

    public void ReduceTurnsAppropriately(List<int> turnsConsumed)
    {
        if (turnsConsumed[1] == -1)
        {
            ReduceWkTurns();
        }
        else
        {
            ReduceRsNuNeutralTurns(turnsConsumed);
        }
    }

    public void ReduceWkTurns()
    {
        if (fullTurns > 0)
        {
            fullTurns--;
            blinkingTurns++;
            PrintConsumedTurns(1,0,1);
        }
        else
        {
            blinkingTurns--;
            PrintConsumedTurns(0,1,0);
        }
    }

    public void ReduceRsNuNeutralTurns(List<int> turnsConsumed)
    {
        int actualBlinkingConsumed = RestarBlinkingTurns(turnsConsumed[1]);
        int totalFullToConsume = CalcularDeudaDeBlinkingTurns(actualBlinkingConsumed, turnsConsumed);
        int actualFullConsumed = RestarFullTurns(totalFullToConsume);
        PrintConsumedTurns(actualFullConsumed, actualBlinkingConsumed, 0);
    }

    private int RestarBlinkingTurns(int requestedBlinking)
    {
        int actualBlinkingConsumed = Math.Min(blinkingTurns, requestedBlinking);
        blinkingTurns -= actualBlinkingConsumed;
        return actualBlinkingConsumed;
    }

    private int CalcularDeudaDeBlinkingTurns(int actualBlinkingConsumed, List<int> turnsConsumed)
    {
        int blinkingDebt = turnsConsumed[1] - actualBlinkingConsumed;
        int totalFullToConsume = turnsConsumed[0] + blinkingDebt;
        return totalFullToConsume;
    }

    private int RestarFullTurns(int totalFullToConsume)
    {
        int actualFullConsumed = Math.Min(fullTurns, totalFullToConsume);
        fullTurns -= actualFullConsumed;
        return actualFullConsumed;
    }

    public void PrintConsumedTurns(int fullConsumed, int blinkingConsumed, int blinkingObtained)
    {
        view.WriteLine($"Se han consumido {fullConsumed} Full Turn(s) y {blinkingConsumed} Blinking Turn(s)");
        view.WriteLine($"Se han obtenido {blinkingObtained} Blinking Turn(s)");
        view.WriteLine("----------------------------------------");
    }

    public bool CheckIfPlayerSurrendered(List<int> turnsConsumed)
    {
        if (turnsConsumed[0] == -100 && turnsConsumed[1] == -100)
        {
            RemoveHealthFromPlayerActiveUnits();
            UpdateSurrenderStats();
            return true;
        }
        return false;
    }

    public void RemoveHealthFromPlayerActiveUnits()
    {
        for (int i = 0; i < activeUnits.Count; i++)
        {
            activeUnits[i].stats.HP = 0;
        }
    }

    public void UpdateSurrenderStats()
    {
        fullTurns = 0;
        blinkingTurns = 0;
        view.WriteLine($"{teamName} se rinde");
        view.WriteLine("----------------------------------------");
    }

    public void MoveFirstUnitToBack()
    {
        Unit firstUnit = activeUnitsBySpd[0];
        activeUnitsBySpd.RemoveAt(0);
        activeUnitsBySpd.Add(firstUnit);
    }

    public int PrintTargetUnits()
    {
        int livingUnitCounter = 1;
        for (int i = 0; i < activeUnits.Count; i++)
        {
            livingUnitCounter = PrintUnitIfAlive(activeUnits[i], livingUnitCounter);
        }
        view.WriteLine($"{livingUnitCounter}-Cancelar");
        return ObtainTargetFromUserInput(livingUnitCounter);
    }

    public int PrintUnitIfAlive(Unit unit, int j)
    {
        if (unit.stats.HP != 0)
        {
            view.WriteLine($"{j}-{unit.name} HP:{unit.stats.HP}/{unit.stats.MaxHP} MP:{unit.stats.MP}/{unit.stats.MaxMP}");
            j++;
        }
        return j;
    }

    public int ObtainTargetFromUserInput(int i)
    {
        int input = fileGetter.ReturnIntFromUserInput(view);
        view.WriteLine("----------------------------------------");
        if (fileGetter.CheckIfInputIsValid(input, 1, i))
        {
            return input;
        }
        view.WriteLine("Error: Input inválido");
        return -1;
    }

    public int CountLivingUnits()
    {
        int i = 0;
        for (int j = 0; j < activeUnits.Count; j++)
        {
            if (activeUnits[j].stats.HP > 0)
            {
                i++;
            }
        }
        return i;
    }
}