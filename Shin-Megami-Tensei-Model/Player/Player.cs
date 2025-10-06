namespace Shin_Megami_Tensei_Model;

public class Player
{
    public string teamName;
    public Player opponent;
    
    public List<Unit> units;
    public List<Unit> activeUnits;
    public List<Unit> benchedUnits;
    private List<Unit> activeUnitsBySpd;
    
    private TurnCalculator turnCalculator;
    private ViewPrinter view;
    private FileGetter fileGetter;

    public Player(ViewPrinter view,(string teamName, List<Unit> team) teamTuple)
    {
        teamName = teamTuple.teamName;

        units = teamTuple.team;
        activeUnits = new List<Unit>();
        benchedUnits = new List<Unit>();

        turnCalculator = new TurnCalculator(view);
        this.view = view;
        fileGetter = new FileGetter(view);
    }
    
    public void SetAttributes(Player opponent)
    {
        SetOpponent(opponent);
        SeparateUnits();
    }

    private void SetOpponent(Player opponent) => this.opponent = opponent;

    private void SeparateUnits()
    {
        for (int unit = 0; unit < units.Count; unit++)
        {
            if (unit < 4) activeUnits.Add(units[unit]);
            else benchedUnits.Add(units[unit]);
        }
    }

    public void ShowRemainingTurns() => view.PrintRemainingTurns(turnCalculator.fullTurns, turnCalculator.blinkingTurns);

    public void SetupStartOfRound()
    {
        int fullTurns = CountLivingUnits();
        turnCalculator.CalculateInitialTurns(fullTurns);
        OrderActiveUnitsBySpd();
    }
    
    public void OrderActiveUnitsBySpd()
    {
        activeUnitsBySpd = activeUnits.OrderByDescending(unit => unit.stats.Spd).ToList();
    }

    public void ShowTurnOrder()
    {
        view.PrintLineOrden();
        int counter = 1;
        for (int i = 0; i < activeUnitsBySpd.Count; i++)
        {
            if (activeUnitsBySpd[i].stats.HP > 0)
            {
                view.PrintListedUnit(counter, activeUnitsBySpd[i].name);
                counter++;
            }
        }
        view.PrintFortyHyphen();
    }
    
    public bool IsPlayerTurn()
    {
        if (turnCalculator.fullTurns == 0 && turnCalculator.blinkingTurns == 0)
        {
            return false;
        }
        return true;
    }

    public void ChooseActions()
    {
        if (activeUnitsBySpd[0].stats.HP == 0)
        {
            MoveFirstUnitToBack();
            ChooseActions();
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
        if (!IsPlayerSurrendered(turnsConsumed))
        {
            turnCalculator.UpdateTurnCount(turnsConsumed);
        }
    }

    public bool IsPlayerSurrendered(List<int> turnsConsumed)
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
        turnCalculator.fullTurns = 0;
        turnCalculator.blinkingTurns = 0;
        view.PrintSurrenderMsg(teamName);
    }

    public void MoveFirstUnitToBack()
    {
        Unit firstUnit = activeUnitsBySpd[0];
        activeUnitsBySpd.RemoveAt(0);
        activeUnitsBySpd.Add(firstUnit);
    }

    public int PrintTargetUnits(List<Unit> targetUnits)
    {
        int livingUnitCounter = 1;
        for (int i = 0; i < targetUnits.Count; i++)
        {
            livingUnitCounter = PrintUnitIfAlive(targetUnits[i], livingUnitCounter);
        }
        view.PrintCancel(livingUnitCounter);
        return ObtainTargetFromUserInput(livingUnitCounter);
    }

    public int PrintUnitIfAlive(Unit unit, int livingUnitCounter)
    {
        if (unit.stats.HP != 0)
        {
            view.PrintLivingUnitLineMsg(livingUnitCounter, (unit.name, unit.stats.HP, unit.stats.MaxHP, unit.stats.MP, unit.stats.MaxMP));
            livingUnitCounter++;
        }
        return livingUnitCounter;
    }

    public int ObtainTargetFromUserInput(int unitCounter)
    {
        int input = view.ReturnIntFromUserInput();
        view.PrintFortyHyphen();
        if (view.IsInputValid(input, 1, unitCounter))
        {
            return input;
        }
        view.PrintInvalidInput();
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

    public bool IsUnitSamurai(string unitName)
    { return units.Any(unit => unit.name == unitName && unit is Samurai); }

    public int PrintActiveMonsters()
    {
        int monsterCounter = 1;
        for (int i = 0; i < activeUnits.Count; i++)
        {
            monsterCounter = PrintActiveMonsterOnSummon(activeUnits[i], monsterCounter, i);
        }
        view.PrintCancel(monsterCounter);
        return ObtainTargetFromUserInput(monsterCounter);
    }
    
    public int PrintActiveMonsterOnSummon(Unit activeUnit, int monsterCounter, int monsterIndex)
    {
        if (activeUnit.stats.HP != 0 && activeUnit is Monster)
        {
            view.PrintBenchableMonsterLine(
                monsterCounter, 
                monsterIndex, 
                (
                    activeUnit.name, 
                    activeUnit.stats.HP, 
                    activeUnit.stats.MaxHP, 
                    activeUnit.stats.MP, 
                    activeUnit.stats.MaxMP
                ));
            monsterCounter++;
        }
        else if (activeUnit.stats.HP == 0 && activeUnit is Monster)
        {
            view.PrintEmptyMonsterSpot(monsterCounter, monsterIndex+1);
            monsterCounter++;
        }
        return monsterCounter;
    }

    public int FindUnitIndexFromName(string name)
    {
        for (int unit = 0; unit < activeUnits.Count; unit++)
        {
            if  (activeUnits[unit].name == name) return unit;
        }
        Console.WriteLine("Error: No se ha encontrado la unidad");
        return -1;
    }

    public void SwitchSummonedUnit(int benchIndex, int activeIndex)
    {
        Unit tempUnit = activeUnits[activeIndex];
        ReplaceActiveUnitWithBench(benchIndex, activeIndex);
        benchedUnits.RemoveAt(benchIndex);
        AddTempToBench(tempUnit);
    }

    private void ReplaceActiveUnitWithBench(int benchIndex, int activeIndex)
    {
        ReplaceUnitInSpdList(benchIndex, activeIndex);
        activeUnits[activeIndex] = benchedUnits[benchIndex];
        view.PrintSummonMsg(activeUnits[activeIndex].name);
    }

    private void ReplaceUnitInSpdList(int benchIndex, int activeIndex)
    {
        int indexToReplace = activeUnitsBySpd.FindIndex(unit => unit.name == activeUnits[activeIndex].name);
        if (activeUnitsBySpd[indexToReplace].stats.HP == 0)
        {
            activeUnitsBySpd.RemoveAt(indexToReplace);
            activeUnitsBySpd.Add(benchedUnits[benchIndex]);
        }
        else activeUnitsBySpd[indexToReplace] = benchedUnits[benchIndex];
    }

    private void AddTempToBench(Unit tempUnit)
    {
        if (tempUnit.stats.HP > 0)
        {
            benchedUnits.Insert(0, tempUnit);
            benchedUnits = benchedUnits.OrderBy(benchedUnit => {
                int index = units.FindIndex(u => u.name == benchedUnit.name);
                return index == -1 ? int.MaxValue : index;
            }).ToList(); //Lógica con ayuda de IA
        }
    }
}