// DamageNumberHandler.cs
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

// Keeps track of the num and where the num should terminate (usually 5 units away from the initial spawn point)
public class NumberHelper
{
    public VisualElement num;
    public Vector2 goalVector;
}

// Spawns damage numbers on the screen when an attack or heal happens. Currently only has white numbers, but should change depending on the effect (poison, damage, heal, etc)
public class DamageNumberHandler : MonoBehaviour
{
    public VisualElement battleUIRoot;
    public VisualTreeAsset damageNumber;
    public List<NumberHelper> numHelperList = new List<NumberHelper>(); // List of all numbers on the screen. They all fly up and get deleted at a respective position

    private void Update()
    {
        int counter = 0;

        // Moves each number up by an amount. If number reaches num.goalVector's value, destroy number.
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

    // Calculates the actual position we want to place the UI element on the screen, relative to where the entity targeted is.
    public Vector2 WorldToScreenPoint(Camera camera, Vector2 worldPosition)
    {
        Vector3 viewportPosition = camera.WorldToViewportPoint(worldPosition);

        // Map viewport coordinates to UI space
        float x = viewportPosition.x * battleUIRoot.resolvedStyle.width;
        float y = (1 - viewportPosition.y) * battleUIRoot.resolvedStyle.height; // Invert Y for UI Toolkit

        return new Vector2(x, y);
    }
}
