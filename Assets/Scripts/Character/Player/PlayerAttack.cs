public class PlayerAttack : Attack
{
    public override void DetermineAttack(CharacterManager targetsCharacterManager, Stats targetsStats)
    {
        if (canAttack)
            StartRandomMeleeAttack(targetsCharacterManager, targetsStats);
    }
}
