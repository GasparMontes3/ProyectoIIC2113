using Shin_Megami_Tensei_View;
using System.Reflection; //estrategia recomendada por IA
using System.Text.Json.Serialization; //estrategia recomendada por IA

namespace Shin_Megami_Tensei_Models;

public abstract class Unit
{
    public FileGetter fileGetter;
    public View view;
    protected readonly Dictionary<int, Func<Player, List<int>>> actionMap;
    public string name { get; set; }
    public Stats stats { get; set; }
    public Affinity affinity { get; set; }
    
    [JsonIgnore]
    public List<Skill> skills { get; set; }
    
    [JsonPropertyName("skills")]
    public List<string> skillNames { get; set; }
    
    protected Unit()
    {
        skills = new List<Skill>();
        fileGetter = new FileGetter();
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

    
    //Funciones de Attack
    public List<int> Attack(Player opponent)
    {
        view.WriteLine($"Seleccione un objetivo para {name}");
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
        int input = opponent.PrintTargetUnits();
        if (input == opponent.CountLivingUnits() + 1)
        {
            return ReturnMrCancel();
        }
        return ReturnTargetBasedOnLivingUnits(opponent, input);
    }

    public Unit ReturnTargetBasedOnLivingUnits(Player opponent, int input)
    {
        List<Unit> livingUnits = opponent.activeUnits.Where(unit => unit.stats.HP > 0).ToList();
        return livingUnits[input-1];
    }

    public List<int> DelegateDamageData((int damage, Unit target, string affinity) attackInfo)
    {
        List<int> turnsUsed = RecieveAttackDamage((attackInfo.damage, attackInfo.affinity), attackInfo.target);
        if (turnsUsed[1] == 50) { ReduceHP(attackInfo.damage); } //No se si aplica para E1, pero analizar lo que se imprime con Repel
        return turnsUsed;
    }
    
    public int CalculateAttackDamage(int strength, double modifier)
    {
        return (int)(strength * modifier * 0.0114);
    }

    public List<int> RecieveAttackDamage((int, string) damageInfo, Unit target)
    {
        string attackAction = FindAttackAction(damageInfo.Item2);
        view.WriteLine($"{name} {attackAction} a {target.name}");
        return CalculateDamageRecieved(target, damageInfo);
    }

    public string FindAttackAction(string attackType) //Puede que tenga que expandirse para E2
    {
        if (attackType == "Phys")
        {
            return "ataca";
        }
        return "dispara";
    }
    
    public List<int> CalculateDamageRecieved(Unit target, (int damage, string damageType) damageInfo)
    {
        Type affinityType = typeof(Affinity);
        PropertyInfo affinityProperty = affinityType.GetProperty(damageInfo.damageType);
        string affinity = (string)affinityProperty.GetValue(target.affinity);
        return target.EffectByAffinity(damageInfo.damage, affinity);
    }
    
    public List<int> EffectByAffinity(int damage, string affinity)
    {
        var effectData = AffinityEffects.GetValueOrDefault(affinity, AffinityEffects["-"]);
        return CalculateFinalDamage(
            (effectData.DmgMultiplier, damage), 
            (effectData.FullTurnCost, effectData.BlinkingTurnCost)
        );
    }

    public List<int> CalculateFinalDamage((double multiplier, double finalDamage) damageInfo, (int full, int blinking) turnsConsumed)
    {
        damageInfo.finalDamage = damageInfo.multiplier * damageInfo.finalDamage;
        ReduceHP((int)damageInfo.finalDamage);
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
        view.WriteLine($"Seleccione una habilidad para que {name} use");
        int j = 1;
        for (int i = 0; i < skills.Count; i++)
        {
            if (stats.MP >= skills[i].cost)
            {
                view.WriteLine($"{j}-{skills[i].name} MP:{skills[i].cost}");
                j++;
            }
        }
        view.WriteLine($"{j}-Cancelar");
        return ObtainAbilityFromUserInput(j);
    }
    
    public int ObtainAbilityFromUserInput(int i) //Lo mismo que ObtainTargetFromUserInput, podria pasarse todo a una funcion en FileGetter
    {
        int input = fileGetter.ReturnIntFromUserInput(view);
        view.WriteLine("----------------------------------------");
        if (fileGetter.CheckIfInputIsValid(input, 1, i)) { return input; }
        view.WriteLine("Error: Input inválido");
        return -1;
    }

    //Funciones de Summon
    public List<int> Summon(Player player)
    {
        view.WriteLine("Error: Código de Summon no implementado");
        return null;
    }
    
    //Funciones de PassTurn
    public List<int> PassTurn(Player player)
    {
        view.WriteLine("Error: Código de PassTurn no implementado");
        return null;
    }

    public void ReduceHP(int amount)
    {
        stats.HP -= amount;
        view.WriteLine($"{name} recibe {amount} de daño");
        if (stats.HP <= 0) { stats.HP = 0; }
        view.WriteLine($"{name} termina con HP:{stats.HP}/{stats.MaxHP}");
        view.WriteLine("----------------------------------------");
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