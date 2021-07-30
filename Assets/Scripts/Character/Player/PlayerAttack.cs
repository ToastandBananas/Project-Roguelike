public class PlayerAttack : Attack
{
    void Update()
    {
        if (canAttack && GameControls.gamePlayActions.playerAttack.WasPressed)
        {
            //DoAttack();
        }
    }

    public override void DetermineAttack(Stats targetsStats)
    {
        StartMeleeAttack(targetsStats);
    }
}
