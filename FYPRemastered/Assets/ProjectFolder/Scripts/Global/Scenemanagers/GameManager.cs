using System;
using UnityEngine;

public enum CharacterType
{
    Player,
    Enemy
}

public enum PlayerPart
{
    Position,
    DefenceCollider
}

public class GameManager : MonoBehaviour
{
    public GameObject Player { get; private set; }
    public GameObject PlayerDefenceCollider { get; private set; }

    public static GameManager Instance { get; private set; }
    public static event Action OnPlayerDied;
    public static event Action OnPlayerRespawn;


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CharacterDied(CharacterType type, GameObject obj = null)
    {
        switch (type)
        {
            case CharacterType.Player:
                PlayerDied();
                break;
            case CharacterType.Enemy:
                EnemyDied(obj);
                break;
        }
    }

    private void PlayerDied()
    {
        //Debug.LogError("Player Died");
        OnPlayerDied?.Invoke();
        
    }

    private void EnemyDied(GameObject obj)
    {
        // Not yet Implemented
    }

    public static void PlayerRespawned()
    {
        OnPlayerRespawn?.Invoke();
    }

    public void SetPlayer()
    {
        if (Player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
            {
                Player = playerObj;
                //Debug.LogError("Player assigned in GameManager: " + PlayerTransform.name);
            }
            else
            {
                Debug.LogWarning("Player not found! Ensure the Player has the correct tag.");
            }

            if(PlayerDefenceCollider == null)
            {
                GameObject playerDef = GameObject.FindGameObjectWithTag("MainCamera");

                if(playerDef != null)
                {
                    PlayerDefenceCollider = playerDef;
                }
                else
                {
                    Debug.LogError("Player Defence Collider Not found - Ensure Correct Tag is assigned");
                }
            }
        }
    }

    public Transform _test;
    public Transform GetPlayerPosition(PlayerPart part)
    {
        if (!Player || !PlayerDefenceCollider) { return null; }
        Transform playerPart = part switch
        {
            PlayerPart.Position => Player.transform,
            PlayerPart.DefenceCollider => PlayerDefenceCollider.transform,
            _ => null
        };
        
        return playerPart;
    }
}
