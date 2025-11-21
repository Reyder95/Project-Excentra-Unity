using UnityEngine;
using System;

public class GameContext
{
    public GameStateController GameState { get; private set; }

    private static GameContext _current;
    public static GameContext Current => _current ??= new GameContext();

    // Unity Script References
    public OverworldController OverworldController { get; private set; }
    public PreloadDatabase PreloadDatabase { get; private set; }

    public GameContext()
    {
        GameState = new GameStateController();
    }

    public void Initialize(OverworldController overworldController, PreloadDatabase preloadDatabase)
    {
        OverworldController = overworldController;
        PreloadDatabase = preloadDatabase;

        // Load Databases
        ExcentraDatabase.LoadEntities(PreloadDatabase.entityPrefabs);
        //ExcentraDatabase.LoadDocuments(uiDocs);
        //ExcentraDatabase.LoadUIAssets(uiSubDocs);
        //ExcentraDatabase.LoadSkills(skills);
        //ExcentraDatabase.LoadStatuses(statusEffects);
        //ExcentraDatabase.LoadMiscPrefabs(miscPrefabs);
        //ExcentraDatabase.LoadBossPhases(bossPhases);
        //ExcentraDatabase.LoadEnemyMechanics(miscMechanics);

        Current.GameState.ChangeState(global::GameState.Exploration);
    }

    public void Update()
    {
        //Debug.Log("GameContext Updating!");
    }
}
