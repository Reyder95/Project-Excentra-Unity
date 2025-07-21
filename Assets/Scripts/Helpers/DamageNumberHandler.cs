// DamageNumberHandler.cs
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

// Keeps track of the num and where the num should terminate (usually 5 units away from the initial spawn point)
public class NumberHelper
{
    public VisualElement num;
    public GameObject target;
    public Vector2 goalVector;
    public float floatOffset = 0f; // How much it has floated up

    public float randomLeft = 0f;
    public float randomTop = 0f;
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

        while (counter < numHelperList.Count)
        {
            var helper = numHelperList[counter];

            if (helper.target == null)
            {
                battleUIRoot.Remove(helper.num);
                numHelperList.RemoveAt(counter);
                continue;
            }

            // Increase vertical float over time
            helper.floatOffset += 150f * Time.deltaTime; // move up 50px per second

            // Update world position
            Vector2 worldPosition = WorldToScreenPoint(Camera.main, helper.target.transform.position);

            // Apply float offset
            float y = worldPosition.y + helper.randomTop - helper.floatOffset;
            float x = worldPosition.x + helper.randomLeft;

            // Set UI element position
            helper.num.style.top = y;
            helper.num.style.left = x;

            // Delete once passed goal vector
            if (helper.floatOffset >= 200f)
            {
                battleUIRoot.Remove(helper.num);
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
        int randomSpread = 50;
        float randomLeft = Random.Range(-randomSpread, randomSpread);
        float randomTop = Random.Range(-randomSpread, randomSpread);  
        currNum.style.left = worldPosition.x + randomLeft;
        currNum.style.top = worldPosition.y + randomTop;

        battleUIRoot.Add(currNum);

        NumberHelper numHelper = new NumberHelper();
        numHelper.randomLeft = randomLeft;
        numHelper.randomTop = randomTop;
        numHelper.num = currNum;
        numHelper.target = target;

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
