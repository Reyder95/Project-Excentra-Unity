using UnityEngine;
using UnityEngine.UIElements;

public class BattleUIHelper : MonoBehaviour
{
    UIDocument uiDoc;
    VisualElement root;

    ProgressBar mechanicCast;

    bool isCasting = false;
    float castSpeed = 35f;

    public event System.Action<BattleUIHelper> OnCastEnd;

    private void Start()
    {
        uiDoc = GetComponent<UIDocument>();
        root = uiDoc.rootVisualElement;

        mechanicCast = root.Q<ProgressBar>("mechanic-cast");
    }

    private void Update()
    {
        if (isCasting)
        {
            mechanicCast.value += castSpeed * Time.deltaTime;

            if (mechanicCast.value >= 100f)
            {
                Debug.Log("Testing??");
                isCasting = false;
                mechanicCast.style.visibility = Visibility.Hidden;
                mechanicCast.value = 0f;
                OnCastEnd?.Invoke(this);
            }
        }
    }

    public void StartCast(EnemyMechanic mechanic)
    {
        if (mechanic != null)
        {
            mechanicCast.value = 0f;
            mechanicCast.style.visibility = Visibility.Visible; 
            mechanicCast.Q<Label>("mechanic-name").text = mechanic.mechanicName;
            isCasting = true;
        }
    }
}
