namespace Shin_Megami_Tensei_Model;

public class TeamValidator
{
    public bool IsTeamsValidated((string team1Name, List<Unit> team1, string team2Name, List<Unit> team2) teamsTuple)
    {
        if (IsTeamAnalysisValid(teamsTuple.team1))
        {
            if (IsTeamAnalysisValid(teamsTuple.team2))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsTeamAnalysisValid(List<Unit> team)
    {
        if (CountUnitsAmount(team) <= 8)
        {
            return IsSamuraiConditionsValid(team);
        }
        return false;
    }
    
    public int CountUnitsAmount(List<Unit> team)
    {
        int count = 0;
        foreach (Unit character in team)
        {
            if (IsCharacterIsRepeated(character, team)) { return 100; }
            count++;
        }
        return count;
    }

    public bool IsCharacterIsRepeated(Unit character, List<Unit> team)
    {
        int count = 0;
        for (int i = 0; i < team.Count; i++)
        {
            if (character.name == team[i].name)
            {
                count++;
            }
        }
        return IsOverOne(count);
    }

    public bool IsOverOne(int n) //funcion general
    {
        if (n > 1)
        {
            return true;
        }
        return false;
    }

    public bool IsSamuraiConditionsValid(List<Unit> team)
    {
        if (CountSamuraiAmount(team) == 1) { return true; }
        return false;
    }
    
    public int CountSamuraiAmount(List<Unit> team)
    {
        int samuraiCount = 0;
        for (int unit = 0; unit < team.Count; unit++)
        {
            int result = AnalyzeIfIsSamurai(team, unit);
            if (result == 100) return 100;
            samuraiCount += result;
        }
        return samuraiCount;
    }

    public int AnalyzeIfIsSamurai(List<Unit> team, int unit)
    {
        if (team[unit] is Samurai)
        {
            if (CountDifferentSamuraiAbilities(team[unit]) > 8) return 100;
            return 1;
        }
        return 0;
    }

    public int CountDifferentSamuraiAbilities(Unit samurai)
    {
        int count = 0;
        for (int i = 0; i < samurai.skills.Count; i++)
        {
            if (IsAbilityIsRepeated(samurai.skills[i].name, samurai)) return 100;
            count++;
        }
        return count;
    }

    public bool IsAbilityIsRepeated(string skill, Unit samurai)
    {
        int count = 0;
        for (int i = 0; i < samurai.skills.Count; i++)
        {
            if (skill == samurai.skills[i].name)
            {
                count++;
            }
        }
        return IsOverOne(count);
    }
}