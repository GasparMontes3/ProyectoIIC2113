namespace Shin_Megami_Tensei_Model;

public class TurnCalculator
{
    public int fullTurns;
    public int blinkingTurns;
    private ViewPrinter view;

    public TurnCalculator(ViewPrinter view)
    {
        this.view = view;
    }

    public void UpdateAttributes(int fullTurns, int blinkingTurns)
    {
        this.fullTurns = fullTurns;
        this.blinkingTurns = blinkingTurns;
    }
    
    public void CalculateInitialTurns(int  fullTurns)
    {
        this.fullTurns = fullTurns;
        blinkingTurns = 0;
    }
    
    public void UpdateTurnCount(List<int> turnsConsumed)
    {
        if (turnsConsumed[0] == 80) ReduceTurnsOnSummonPass(turnsConsumed);
        else if (turnsConsumed[0] != 50 && turnsConsumed[1] != 50) ReduceTurnsAppropriately(turnsConsumed);
        else
        {
            view.PrintTurnConsumption(fullTurns, blinkingTurns);
            fullTurns = 0;
            blinkingTurns = 0;
        }
    }

    public void ReduceTurnsAppropriately(List<int> turnsConsumed)
    {
        if (turnsConsumed[1] == -1) ReduceTurnsWhenWkAffinity(); 
        else ReduceTurnsWhenRsNuNeutralAffinity(turnsConsumed); 
    }

    public void ReduceTurnsWhenWkAffinity()
    {
        if (fullTurns > 0)
        {
            fullTurns--;
            blinkingTurns++;
            view.PrintTurnConsumptionSummary(1,0,1);
        }
        else
        {
            blinkingTurns--;
            view.PrintTurnConsumptionSummary(0,1,0);
        }
    }

    public void ReduceTurnsWhenRsNuNeutralAffinity(List<int> turnsConsumed)
    {
        List<int> actualTurnsConsumed = CalculateActualTurnsConsumed(turnsConsumed);
        view.PrintTurnConsumptionSummary(actualTurnsConsumed[0], actualTurnsConsumed[1], 0);
    }
    
    private void ReduceTurnsOnSummonPass(List<int> turnsConsumed)
    {
        turnsConsumed[0] = 0;
        List<int> actualTurnsConsumed = CalculateActualTurnsConsumed(turnsConsumed);
        if (actualTurnsConsumed[0] > 0) HandleBlinkingTurnAddition(actualTurnsConsumed);
        else view.PrintTurnConsumptionSummary(actualTurnsConsumed[0], actualTurnsConsumed[1], 0);
    }

    private List<int> CalculateActualTurnsConsumed(List<int> turnsConsumed)
    {
        int actualBlinkingConsumed = SubtractBlinkingTurns(turnsConsumed[1]);
        int totalFullToConsume = CalculateBlinkingTurnsDebt(actualBlinkingConsumed, turnsConsumed);
        int actualFullConsumed = SubtractFullTurns(totalFullToConsume);
        return [actualFullConsumed, actualBlinkingConsumed];
    }

    private int SubtractBlinkingTurns(int requestedBlinking)
    {
        int actualBlinkingConsumed = Math.Min(blinkingTurns, requestedBlinking);
        blinkingTurns -= actualBlinkingConsumed;
        return actualBlinkingConsumed;
    }

    private int CalculateBlinkingTurnsDebt(int actualBlinkingConsumed, List<int> turnsConsumed)
    {
        int blinkingDebt = turnsConsumed[1] - actualBlinkingConsumed;
        int totalFullToConsume = turnsConsumed[0] + blinkingDebt;
        return totalFullToConsume;
    }

    private int SubtractFullTurns(int totalFullToConsume)
    {
        int actualFullConsumed = Math.Min(fullTurns, totalFullToConsume);
        fullTurns -= actualFullConsumed;
        return actualFullConsumed;
    }

    private void HandleBlinkingTurnAddition(List<int> actualTurnsConsumed)
    {
        view.PrintTurnConsumptionSummary(actualTurnsConsumed[0], actualTurnsConsumed[1], 1);
        blinkingTurns++;
    }
}