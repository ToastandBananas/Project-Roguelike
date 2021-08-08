public class PlayerAttack : Attack
{
    public override void DetermineAttack(CharacterManager targetsCharacterManager, Stats targetsStats)
    {
        StartRandomMeleeAttack(targetsCharacterManager, targetsStats);
    }
}
