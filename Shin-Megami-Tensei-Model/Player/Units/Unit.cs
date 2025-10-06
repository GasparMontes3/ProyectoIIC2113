using System.Reflection; //estrategia recomendada por IA
using System.Text.Json.Serialization; //estrategia recomendada por IA

namespace Shin_Megami_Tensei_Model;

public abstract class Unit
{
    public ViewPrinter view;
    protected readonly Dictionary<int, Func<Player, List<int>>> actionMap;
    public string name { get; set; }
    public Stats stats { get; set; }
    public Affinity affinity { get; set; }
    
    [JsonIgnore]
    public List<Skill> skills { get; set; }
    
    [JsonPropertyName("skills")]
    public List<string> skillNames { get; set; } = new List<string>();
    
    protected Unit()
    {
        skills = new List<Skill>();
        actionMap = InitializeActionMap();
    }
    
    public Unit(string name, Stats stats, Affinity affinity, List<Skill> skills)
    {
        this.name = name;
        this.stats = stats;
        this.affinity = affinity; 
        this.skills = skills;
    }
    
    private static readonly Dictionary<string, (double DmgMultiplier, int FullTurnCost, int BlinkingTurnCost)> AffinityEffects = new()
    {
        { "Wk", (DmgMultiplier: 1.5, FullTurnCost: 1, BlinkingTurnCost: -1) },
        { "Rs", (DmgMultiplier: 0.5, FullTurnCost: 0, BlinkingTurnCost: 1)  },
        { "Nu", (DmgMultiplier: 0.0, FullTurnCost: 0, BlinkingTurnCost: 2)  },
        { "Dr", (DmgMultiplier: 0.0, FullTurnCost: 50, BlinkingTurnCost: 0) },
        { "Rp", (DmgMultiplier: 0.0, FullTurnCost: 0, BlinkingTurnCost: 50)  },
        { "-",  (DmgMultiplier: 1.0, FullTurnCost: 0, BlinkingTurnCost: 1)  }
    };
    
    public abstract List<int> TakeAction(Player opponent);
    public abstract int PrintActionSelection();
    protected abstract Dictionary<int, Func<Player, List<int>>> InitializeActionMap();

    public void SetView(ViewPrinter view) { this.view = view; }
    
    //Funciones de Attack
    public List<int> Attack(Player opponent)
    {
        view.PrintSelectTarget(name);
        (int, Unit) attackData = PrepareAttackData(opponent, (stats.Str, 54));
        if (attackData.Item2.name == "Mr. Cancel") { return [70,0]; }
        return DelegateDamageData((attackData.Item1, attackData.Item2, "Phys"));
    }

    public (int, Unit) PrepareAttackData(Player opponent, (int statValue, int modifier) attackData)
    {
        int damage = CalculateAttackDamage(attackData.statValue, attackData.modifier);
        Unit target = DefineTarget(opponent);
        return (damage, target);
    }

    public Unit DefineTarget(Player opponent)
    {
        int input = opponent.PrintTargetUnits(opponent.activeUnits);
        if (input == opponent.CountLivingUnits() + 1)
        {
            return ReturnMrCancel();
        }
        return ReturnTargetBasedOnLivingUnits(opponent, input);
    }

    private Unit ReturnTargetBasedOnLivingUnits(Player opponent, int input)
    {
        List<Unit> livingUnits = opponent.activeUnits.Where(unit => unit.stats.HP > 0).ToList();
        return livingUnits[input-1];
    }

    protected List<int> DelegateDamageData((int damage, Unit target, string affinity) attackInfo)
    {
        List<int> turnsUsed = RecieveAttackDamage((attackInfo.damage, attackInfo.affinity), attackInfo.target);
        if (turnsUsed[1] == 50) { ReduceHP(attackInfo.damage, (attackInfo.target.name, attackInfo.affinity)); }
        return turnsUsed;
    }
    
    private int CalculateAttackDamage(int strength, double modifier)
    {
        int damage = (int)(strength * modifier * 0.0114);
        return damage;
    }

    private List<int> RecieveAttackDamage((int, string) damageInfo, Unit target)
    {
        string attackAction = FindAttackAction(damageInfo.Item2);
        view.PrintAttackMsg(name, attackAction, target.name);
        return CalculateDamageRecieved(target, damageInfo);
    }

    private string FindAttackAction(string attackType) //Puede que tenga que expandirse para E2
    {
        if (attackType == "Phys")
        {
            return "ataca";
        }
        return "dispara";
    }
    
    private List<int> CalculateDamageRecieved(Unit target, (int damage, string damageType) damageInfo)
    {
        Type affinityType = typeof(Affinity);
        PropertyInfo affinityProperty = affinityType.GetProperty(damageInfo.damageType);
        string affinity = (string)affinityProperty.GetValue(target.affinity);
        return target.EffectByAffinity(name, damageInfo.damage, affinity);
    }
    
    private List<int> EffectByAffinity(string attackerName, int damage, string affinity)
    {
        var effectData = AffinityEffects.GetValueOrDefault(affinity, AffinityEffects["-"]);
        return CalculateFinalDamage(
            (effectData.DmgMultiplier, damage), 
            (effectData.FullTurnCost, effectData.BlinkingTurnCost),
            (attackerName, affinity)
        );
    }

    public List<int> CalculateFinalDamage((double multiplier, double finalDamage) damageInfo, (int full, int blinking) turnsConsumed, (string attackerName, string affinity) attackInfo)
    {
        damageInfo.finalDamage = damageInfo.multiplier * damageInfo.finalDamage;
        ReduceHP((int)damageInfo.finalDamage, attackInfo);
        return [turnsConsumed.full, turnsConsumed.blinking];
    }
    
    //Funciones de UseSkill
    public List<int> UseSkill(Player opponent)
    {
        int input = ChooseSkill();
        if (input == opponent.CountLivingUnits() + 1)
        {
            return [70,0];
        }
        return [70,0]; //Por ahora siempre da Cancel, para E2 se amplia uso
    }

    public int ChooseSkill()
    {
        view.PrintChooseSkillMsg(name);
        int j = 1;
        for (int i = 0; i < skills.Count; i++)
        {
            if (stats.MP >= skills[i].cost)
            {
                view.PrintListedSkillData(j, skills[i].name, skills[i].cost);
                j++;
            }
        }
        view.PrintCancel(j);
        return ObtainAbilityFromUserInput(j);
    }
    
    public int ObtainAbilityFromUserInput(int i) //Lo mismo que ObtainTargetFromUserInput, podria pasarse todo a una funcion en FileGetter
    {
        int input = view.ReturnIntFromUserInput();
        view.PrintFortyHyphen();
        if (view.IsInputValid(input, 1, i)) { return input; }
        view.PrintInvalidInput();
        return -1;
    }

    //Funciones de Summon
    public List<int> Summon(Player opponent)
    {
        Player player = opponent.opponent;
        int benchMonster = ChooseInvokedMonster(player);
        if (benchMonster == player.benchedUnits.Count + 1) { return [70,0]; }
        int activeMonster = ChooseBenchedMonster(player);
        if (activeMonster == player.activeUnits.Count) { return [70,0]; } //CountLivingUnitsWithoutSamurai
        player.SwitchSummonedUnit(benchMonster-1, activeMonster);
        return [80, 1];
    }

    private int ChooseInvokedMonster(Player player)
    {
        view.PrintSelectMonster();
        return player.PrintTargetUnits(player.benchedUnits);
    }
    
    private int ChooseBenchedMonster(Player player)
    {
        if (player.IsUnitSamurai(name))
        {
            view.PrintSelectSummonPosition();
            return player.PrintActiveMonsters();
        }
        return player.FindUnitIndexFromName(name);
    }
    
    //Funciones de PassTurn
    public List<int> PassTurn(Player opponent)
    {
        return [80, 1];
    }

    public void ReduceHP(int amount, (string attackerName, string affinity) attackInfo)
    {
        stats.HP -= amount;
        if (stats.HP <= 0) { stats.HP = 0; }
        view.PrintAffinityMsg((attackInfo.attackerName, name), (attackInfo.affinity, amount));
        view.PrintRecieveDamage(name, amount, (stats.HP, stats.MaxHP));
    }

    public Unit ReturnMrCancel()
    {
        return new Monster
        {
            name = "Mr. Cancel",
            stats = new Stats(),
            affinity = new Affinity(),
            skills = new List<Skill>()
        };
    }
}