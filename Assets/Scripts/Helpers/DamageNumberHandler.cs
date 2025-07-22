// DamageNumberHandler.cs
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
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

public class TextHelper
{
    public VisualElement text;
    public GameObject target;
    public float floatOffset = 0f; // How much it has floated up
}

// Spawns damage numbers on the screen when an attack or heal happens. Currently only has white numbers, but should change depending on the effect (poison, damage, heal, etc)
public class DamageNumberHandler : MonoBehaviour
{
    public VisualElement battleUIRoot;
    public VisualTreeAsset damageNumber;
    public VisualTreeAsset popupText;
    public List<NumberHelper> numHelperList = new List<NumberHelper>(); // List of all numbers on the screen. They all fly up and get deleted at a respective position
    public List<TextHelper> popupTextList = new List<TextHelper>();


    private void Update()
    {
        int popupCounter = 0;

        while (popupCounter < popupTextList.Count)
        {
            var textPopup = popupTextList[popupCounter];


            textPopup.floatOffset += 100f * Time.deltaTime;

            Vector2 uiPosition = WorldToUIPosition(Camera.main, popupTextList[popupCounter].target.transform.position);
            textPopup.text.style.top = uiPosition.y - (textPopup.text.resolvedStyle.height / 2f) - textPopup.floatOffset;
            textPopup.text.style.left = uiPosition.x - (textPopup.text.resolvedStyle.width / 2f);

            if (textPopup.floatOffset >= 150f)
            {
                battleUIRoot.Remove(textPopup.text);
                popupTextList.RemoveAt(popupCounter);
                continue;
            }
            else
            {
                popupCounter++;
            }
        }

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
            Vector2 worldPosition = WorldToUIPosition(Camera.main, helper.target.transform.position);

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

    public void SpawnPopupText(GameObject target, Image icon, string text)
    {
        Vector2 uiPosition = WorldToUIPosition(Camera.main, target.transform.position);

        VisualElement newPopup = popupText.CloneTree();
        newPopup.Q<Label>("text").text = text;
        newPopup.Q<VisualElement>("icon").style.backgroundImage = icon != null ? icon.sprite.texture : null;
        newPopup.Q<VisualElement>("icon").style.display = icon != null ? DisplayStyle.Flex : DisplayStyle.None;  

        newPopup.style.position = Position.Absolute;
        newPopup.style.left = uiPosition.x;
        newPopup.style.top = uiPosition.y;

        TextHelper newText = new TextHelper();
        newText.text = newPopup;
        newText.target = target;

        battleUIRoot.Add(newPopup);

        newPopup.schedule.Execute(() =>
        {
            float centeredX = uiPosition.x - (newPopup.resolvedStyle.width / 2f);
            float centeredY = uiPosition.y - (newPopup.resolvedStyle.height / 2f);

            newPopup.style.left = centeredX;
            newPopup.style.top = centeredY;
        }).ExecuteLater(0);
        popupTextList.Add(newText);

    }

    public void SpawnDamageNumber(GameObject target, int amount)
    {
        VisualElement currNum = damageNumber.CloneTree();
        currNum.Q<Label>().text = amount.ToString();
        Vector2 worldPosition = WorldToUIPosition(Camera.main, target.transform.position);
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
    public Vector2 WorldToUIPosition(Camera camera, Vector2 worldPosition)
    {
        Vector3 screenPos = camera.WorldToScreenPoint(worldPosition);

        screenPos.y = Screen.height - screenPos.y;

        return new Vector2(screenPos.x, screenPos.y);
    }
}
