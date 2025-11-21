using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

// It's a callback so use it as such (for onSpawned)
public struct SpawnRequest
{
    public Vector2 position;
    public string entityKey;
    public Action<GameObject> onSpawned;
}

public class OverworldController : MonoBehaviour
{
    private bool _active = false;
    private GameObject _player;

    public List<GameObject> CharacterList { get; private set; }
    public int LeaderIndex { get; private set; } = 0;

    // Events
    public event Action<SpawnRequest> OnSpawnRequested;
    public event Action<GameObject> OnEntitySpawned;
    public event Action<GameObject> OnLeaderChanged;

    private void Start()
    {
        OnSpawnRequested += HandleSpawnRequest;

        CharacterList = new List<GameObject>();

        SpawnPlayersOntoField();

        HandleChangeLeader(0);
    }

    public GameObject SpawnEntity(Vector2 position, GameObject entity)
    {
        return GameObject.Instantiate(entity, position, Quaternion.identity);
    }

    public void SpawnPlayersOntoField()
    {
        GameObject spawnedRioka = SpawnEntity(new Vector2(0, 0), ExcentraDatabase.TryGetEntity("Rioka"));
        spawnedRioka.GetComponent<PlayerInput>().enabled = true;

        CharacterList.Add(spawnedRioka);
    }
    
    // Event Handlers
    public void HandleSpawnRequest(SpawnRequest request)
    {
        GameObject spawnedEntity = SpawnEntity(request.position, ExcentraDatabase.TryGetEntity(request.entityKey));

        request.onSpawned?.Invoke(spawnedEntity);
    }

    public void HandleChangeLeader(int leaderIndex)
    {
        LeaderIndex = leaderIndex;

        OnLeaderChanged?.Invoke(CharacterList[LeaderIndex]);
    }
}
