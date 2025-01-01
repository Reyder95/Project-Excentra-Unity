using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class NumberHelper
{
    public VisualElement num;
    public Vector2 goalVector;
}

public class DamageNumberHandler : MonoBehaviour
{
    public VisualElement battleUIRoot;
    public VisualTreeAsset damageNumber;
    public List<NumberHelper> numHelperList = new List<NumberHelper>();

    private void Update()
    {
        int counter = 0;

        while (counter < numHelperList.Count)
        {
            var currentTop = numHelperList[counter].num.style.top.value;

            numHelperList[counter].num.style.top = new StyleLength(currentTop.value - 0.5f);

            if (numHelperList[counter].num.style.top.value.value < numHelperList[counter].goalVector.y)
            {
                battleUIRoot.Remove(numHelperList[counter].num);
                numHelperList.RemoveAt(counter);
            }
            else
            {
                counter++;
            }
        }
    }

    public void SpawnDamageNumber(GameObject target, int amount)
    {
        VisualElement currNum = damageNumber.CloneTree();
        currNum.Q<Label>().text = amount.ToString();
        Vector2 worldPosition = WorldToScreenPoint(Camera.main, target.transform.position);
        currNum.style.left = worldPosition.x;
        currNum.style.top = worldPosition.y;

        battleUIRoot.Add(currNum);

        NumberHelper numHelper = new NumberHelper();
        numHelper.num = currNum;
        numHelper.goalVector = new Vector2(worldPosition.x, worldPosition.y - 100);

        numHelperList.Add(numHelper);
    }

    public Vector2 WorldToScreenPoint(Camera camera, Vector2 worldPosition)
    {
        Vector3 viewportPosition = camera.WorldToViewportPoint(worldPosition);

        // Map viewport coordinates to UI space
        float x = viewportPosition.x * battleUIRoot.resolvedStyle.width;
        float y = (1 - viewportPosition.y) * battleUIRoot.resolvedStyle.height; // Invert Y for UI Toolkit

        return new Vector2(x, y);
    }
}
