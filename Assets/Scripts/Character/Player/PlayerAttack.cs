public class PlayerAttack : Attack
{
    //UIManager uiManager;

    public override void Start()
    {
        base.Start();

        //uiManager = UIManager.instance;
    }
    
    void Update()
    {
        if (canAttack /*&& /iManager.UIMenuActive() == false*/ && GameControls.gamePlayActions.playerAttack.WasPressed)
        {
            DoAttack();
        }
    }
}
