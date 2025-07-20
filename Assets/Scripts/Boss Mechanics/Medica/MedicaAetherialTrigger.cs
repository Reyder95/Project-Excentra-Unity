using System.Collections.Generic;
using UnityEngine;

public class MedicaAetherialTrigger : AbilityTrigger
{
    public List<GameObject> squares = new List<GameObject>();

    public override void ActivateTrigger(BattleManager battleManager, EnemyMechanic mechanic, object data = null)
    {
        if (data is AetherialCalibrationData calibrationData)
        {
            int counter = 0;

            while (counter < calibrationData.numBlue)
            {
                if (counter < squares.Count)
                {
                    squares[counter].GetComponent<SpriteRenderer>().color = new Color(0f, 0f, 1f);
                }

                counter++;
            }

            while (counter < squares.Count)
            {
                squares[counter].GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f);
                counter++;
            }

        }
    }
}
