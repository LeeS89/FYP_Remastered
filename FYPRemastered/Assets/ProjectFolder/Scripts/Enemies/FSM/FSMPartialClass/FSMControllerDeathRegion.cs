public partial class EnemyFSMController : ComponentEvents
{
    #region Death Region
    private void OnDeathStatusUpdated(bool isDead)
    {
        if (AgentIsAlive) { AgentIsAlive = false; }

        ResetFSM(_deathState);
    }

    private void DisableAgent()
    {
        if (_agent.enabled)
        {
            _agent.enabled = false;
        }
        if (_obstacle.enabled)
        {
            _obstacle.enabled = false;
        }
    }

    private void ToggleGameObject(bool status)
    {
        gameObject.SetActive(status);
    }

    #endregion

}
