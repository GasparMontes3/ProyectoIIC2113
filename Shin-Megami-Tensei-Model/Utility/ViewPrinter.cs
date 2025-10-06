using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei_Model;

public class ViewPrinter
{
    private View view;

    public ViewPrinter(View _view)
    {
        view = _view;
    }
    
    private Dictionary<string, Func<string, string, int, string>> InitializeActionMap()
    {
        return new Dictionary<string, Func<string, string, int, string>>
        {
            { "Wk", (unitName, targetName, _) => $"{targetName} es débil contra el ataque de {unitName}" },
            { "Rs", (unitName, targetName, _) => $"{targetName} es resistente el ataque de {unitName}" },
            { "Nu", (unitName, targetName, _) => $"{targetName} bloquea el ataque de {unitName}" },
            { "Rp", (unitName, targetName, attackDamage) => $"{targetName} devuelve {attackDamage} daño a {unitName}" },
            { "Dr", (_, targetName, attackDamage) => $"{targetName} absorbe {attackDamage} daño" }
        };
    }

    //General
    public string ReadLine() { return view.ReadLine(); }
    public void PrintFortyHyphen() => view.WriteLine("----------------------------------------");
    public void PrintCancel(int index) => view.WriteLine($"{index}-Cancelar");
    public void PrintInvalidInput() => view.WriteLine("Error: Input inválido");
    public void PrintEmptyLetterIndex(char index) => view.WriteLine($"{index}-");
    public int ReturnIntFromUserInput()
    {
        string input = view.ReadLine();
        try
        {
            int.TryParse(input, out int numberInt);
            return numberInt;
        } 
        catch (Exception)
        {
            PrintInvalidInput();
            return -1;
        }
    }
    public bool IsInputValid(int input, int min, int max)
    {
        if (input >= min && input <= max)
        {
            return true;
        }
        return false;
    }
    public char IntToAlphabet(int number) //funcion general
    {
        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        if (number >= 0 && number < alphabet.Length) return alphabet[number];
        return '?';
    }

    //Game
    public void PrintInvalidTeams() => view.WriteLine("Archivo de equipos inválido");

    //FileGetter
    public void PrintChooseFile() => view.WriteLine("Elige un archivo para cargar los equipos");
    public void PrintFileName(int fileIndex, string fileName) => view.WriteLine($"{fileIndex}: {fileName}");

    //Unit
    public void PrintSelectTarget(string unitName) => view.WriteLine($"Seleccione un objetivo para {unitName}");
    public void PrintAttackMsg(string unitName, string attackAction, string targetName) =>
        view.WriteLine($"{unitName} {attackAction} a {targetName}");

    public void PrintAffinityMsg((string unitName, string targetName) names, (string affinity, int attackDamage) data)
    {
        if (data.affinity != "-")
        {
            var actionMap = InitializeActionMap();
            Func<string, string, int, string> messageBuilder = actionMap[data.affinity];
            string message = messageBuilder.Invoke(names.unitName, names.targetName, data.attackDamage);
            view.WriteLine(message);
        }
    }
    public void PrintChooseSkillMsg(string unitName) =>
        view.WriteLine($"Seleccione una habilidad para que {unitName} use");
    public void PrintListedSkillData(int listIndex, string skillName, int skillCost) =>
        view.WriteLine($"{listIndex}-{skillName} MP:{skillCost}");
    public void PrintSelectMonster() => view.WriteLine("Seleccione un monstruo para invocar");
    public void PrintSelectSummonPosition() => view.WriteLine("Seleccione una posición para invocar");
    public void PrintRecieveDamage(string unitName, int damageRecieved, (int currentHP, int maxHP) HPData)
    {
        view.WriteLine($"{unitName} recibe {damageRecieved} de daño");
        view.WriteLine($"{unitName} termina con HP:{HPData.currentHP}/{HPData.maxHP}");
        PrintFortyHyphen();
    }

    //Samurai
    public void PrintSamuraiActionSelection(string unitName)
        => view.WriteLine($"Seleccione una acción para {unitName}\n1: Atacar\n2: Disparar\n3: Usar Habilidad\n4: Invocar\n5: Pasar Turno\n6: Rendirse");

    //Monster
    public void PrintMonsterActionSelection(string unitName)
        => view.WriteLine($"Seleccione una acción para {unitName}\n1: Atacar\n2: Usar Habilidad\n3: Invocar\n4: Pasar Turno");

    //BattleClass
    public void PrintWinnerMsg(string teamName) => view.WriteLine($"Ganador: {teamName}");
    public void PrintRoundStartMsg(string teamName) => view.WriteLine($"Ronda de {teamName}");
    public void PrintTeamNameMsg(string teamName) => view.WriteLine($"Equipo de {teamName}");
    public void PrintUnitLineMsg(char letterIndex, (string name, int currentHP, int maxHP, int currentMP, int maxMP) unitData) 
        => view.WriteLine($"{letterIndex}-{unitData.name} HP:{unitData.currentHP}/{unitData.maxHP} MP:{unitData.currentMP}/{unitData.maxMP}");
    
    //Player
    public void PrintRemainingTurns(int fullTurns, int blinkingTurns)
    {
        view.WriteLine($"Full Turns: {fullTurns}");
        view.WriteLine($"Blinking Turns: {blinkingTurns}");
        PrintFortyHyphen();
    }
    public void PrintLineOrden() => view.WriteLine("Orden:");
    public void PrintListedUnit(int index, string unitName)  => view.WriteLine($"{index}-{unitName}");
    public void PrintSurrenderMsg(string teamName)
    {
        view.WriteLine($"{teamName} se rinde");
        PrintFortyHyphen();
    }
    public void PrintBenchableMonsterLine(int monsterPosition, int index, (string name, int currentHP, int maxHP, int currentMP, int maxMP) unitData) 
        => view.WriteLine($"{monsterPosition}-{unitData.name} HP:{unitData.currentHP}/{unitData.maxHP} MP:{unitData.currentMP}/{unitData.maxMP} (Puesto {index+1})");
    public void PrintEmptyMonsterSpot(int counter, int index) => view.WriteLine($"{counter}-Vacío (Puesto {index})");
    public void PrintSummonMsg(string unitName)
    {
        view.WriteLine($"{unitName} ha sido invocado");
        PrintFortyHyphen();
    }
    public void PrintLivingUnitLineMsg(int index, (string name, int currentHP, int maxHP, int currentMP, int maxMP) unitData) 
        => view.WriteLine($"{index}-{unitData.name} HP:{unitData.currentHP}/{unitData.maxHP} MP:{unitData.currentMP}/{unitData.maxMP}");
    
    //TurnCalculator
    public void PrintTurnConsumption(int fullConsumed, int blinkingConsumed) 
        => view.WriteLine($"Se han consumido {fullConsumed} Full Turn(s) y {blinkingConsumed} Blinking Turn(s)");
    public void PrintTurnConsumptionSummary(int fullConsumed, int blinkingConsumed, int blinkingObtained)
    {
        PrintTurnConsumption(fullConsumed, blinkingConsumed);
        view.WriteLine($"Se han obtenido {blinkingObtained} Blinking Turn(s)");
        PrintFortyHyphen();
    }
}