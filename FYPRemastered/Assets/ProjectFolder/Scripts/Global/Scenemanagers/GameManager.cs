using System;
using UnityEngine;

public enum CharacterType
{
    Player,
    Enemy
}



public class GameManager : MonoBehaviour
{
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
                Playerdied();
                break;
            case CharacterType.Enemy:
                EnemyDied(obj);
                break;
        }
    }

    private void Playerdied()
    {
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
}
