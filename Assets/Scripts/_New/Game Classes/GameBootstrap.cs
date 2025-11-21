using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    public OverworldController overworldController;
    public PreloadDatabase preloadDatabase;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        GameContext.Current.Initialize(overworldController, preloadDatabase);
    }

    private void Update()
    {
        GameContext.Current.Update();
    }
}
