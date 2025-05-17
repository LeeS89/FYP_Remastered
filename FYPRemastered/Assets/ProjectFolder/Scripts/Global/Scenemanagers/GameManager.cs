using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    private bool _playerHasMoved = false;
    
    public bool PlayerHasMoved
    {
        set
        {
            if (_playerHasMoved != value)
            {
                _playerHasMoved = value;
            }
            _onPlayerMovedinternal?.Invoke(_playerHasMoved);
        }
        get => _playerHasMoved;
        
    }

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
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
/*
    private void Start()
    {
        Application.targetFrameRate = 90;
    }*/

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetPlayer();
        //Debug.LogWarning("Scene Has Loaded In background Wohoo!!, for scene name: "+SceneManager.GetActiveScene().name);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
                Debug.LogWarning("Player has been Set");
            }
            else
            {
                Debug.LogError("Player not found! Ensure the Player has the correct tag.");
                return;
            }

            if (PlayerDefenceCollider == null)
            {
                GameObject playerDef = GameObject.FindGameObjectWithTag("MainCamera");

                if (playerDef != null)
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

    public void SetPlayer(GameObject player)
    {
        if (player != null)
        {
           // GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                Player = player;
                Debug.LogWarning("Player has been Set");
            }
            else
            {
                Debug.LogWarning("Player not found! Ensure the Player has the correct tag.");
                return;
            }

            if (PlayerDefenceCollider == null)
            {
                foreach (Transform child in player.transform)
                {
                    if (child.CompareTag("MainCamera"))
                    {
                        PlayerDefenceCollider = child.gameObject;
                        break;
                    }
                }
                    //GameObject playerDef = GameObject.FindGameObjectWithTag("MainCamera");

               /* if (playerDef != null)
                {
                    PlayerDefenceCollider = playerDef;
                }
                else
                {
                    Debug.LogError("Player Defence Collider Not found - Ensure Correct Tag is assigned");
                }*/
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

    public static event Action<bool> _onPlayerMovedinternal;
    private static Coroutine _playerMovedCoroutine;

    public static event Action<bool> OnPlayerMoved
    {
        add
        {
            bool wasEmpty = _onPlayerMovedinternal == null;
            _onPlayerMovedinternal += value;

            if(wasEmpty && Instance != null)
            {               
               // _playerMovedCoroutine = Instance.StartCoroutine(Instance.PlayerMovedroutine());
            }
        }
        remove
        {
            _onPlayerMovedinternal -= value;

            if(_onPlayerMovedinternal == null && _playerMovedCoroutine != null && Instance != null)
            {
                //Instance.StopCoroutine(_playerMovedCoroutine);
                _playerMovedCoroutine = null;
            }
        }
    }

   


    private IEnumerator PlayerMovedroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.25f);

            _onPlayerMovedinternal?.Invoke(_playerHasMoved);
        }
    }

    public void PlayerMoved()
    {
        _onPlayerMovedinternal?.Invoke(_playerHasMoved);
    }
}
