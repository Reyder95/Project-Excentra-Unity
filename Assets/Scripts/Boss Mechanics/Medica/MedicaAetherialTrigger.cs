using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MedicaAetherialTrigger : AbilityTrigger
{
    public List<GameObject> squares = new List<GameObject>();
    public GameObject blueTankRune;
    public GameObject redTankRune;
    public List<GameObject> particleSystems = new List<GameObject>();

    BattleManager mainBattleManager;
    CustomLogicPassthrough passthrough;

    public int completed = 0;

    bool initialDone = false;
    bool newSet = false;

    public void FinishColor()
    {
        completed++;

        if (completed >= 4)
        {
            Debug.Log("End Mechanic!!");
            foreach (GameObject particleSystem in particleSystems)
            {
                if (particleSystem != null)
                {
                    UnityEngine.GameObject.Destroy(particleSystem);
                }
            }

            particleSystems.Clear();

            initialDone = true;
            Medica.ReprisalEffectPhaseTwo(mainBattleManager, passthrough);
        }
    }

    public void ColorSquares(AetherialCalibrationData data)
    {
        int counter = 0;

        if (data.numBlue == 1)
        {
            SpriteRenderer trSR = blueTankRune.GetComponent<SpriteRenderer>();
            Color trColor = trSR.color;
            trColor.a = 1.0f;
            trSR.color = trColor;
        }
        if (data.numRed == 1)
        {
            SpriteRenderer trSR = redTankRune.GetComponent<SpriteRenderer>();
            Color trColor = trSR.color;
            trColor.a = 1.0f;
            trSR.color = trColor;
        }
            

        while (counter < data.numBlue)
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

    public override void ActivateTrigger(BattleManager battleManager, EnemyMechanic mechanic, object data = null)
    {
        if (data is AetherialCalibrationData calibrationData)
        {
            if (!initialDone)
            {
                if (calibrationData.tank)
                {
                    if (calibrationData.numBlue == 1)
                        squares[0].GetComponent<CalibrationScript>().tankRuneActive = true;
                    if (calibrationData.numRed == 1)
                        squares[3].GetComponent<CalibrationScript>().tankRuneActive = true;
                }


                passthrough = calibrationData.passthrough;
                mainBattleManager = calibrationData.battleManager;
                int counter = 0;

                while (counter < calibrationData.numBlue)
                {

                    GameObject particleLineSpawned = UnityEngine.GameObject.Instantiate(ExcentraDatabase.TryGetMiscPrefab("particle-line"), new Vector2(1000, 1000), Quaternion.identity);
                    particleLineSpawned.GetComponent<ParticleLine>().SetContents(calibrationData.boss, squares[counter]);
                    particleSystems.Add(particleLineSpawned);

                    squares[counter].GetComponent<CalibrationScript>().colorData = new Color(0f, 0f, 1f);
                    squares[counter].GetComponent<CalibrationScript>().triggerScript = this;

                    //if (counter < squares.Count)
                    //{
                    //    squares[counter].GetComponent<SpriteRenderer>().color = new Color(0f, 0f, 1f);
                    //}

                    counter++;
                }

                while (counter < squares.Count)
                {
                    //squares[counter].GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f);
                    squares[counter].GetComponent<CalibrationScript>().colorData = new Color(1f, 0f, 0f);
                    squares[counter].GetComponent<CalibrationScript>().triggerScript = this;
                    GameObject particleLineSpawned = UnityEngine.GameObject.Instantiate(ExcentraDatabase.TryGetMiscPrefab("particle-line"), new Vector2(1000, 1000), Quaternion.identity);
                    particleLineSpawned.GetComponent<ParticleLine>().SetContents(calibrationData.boss, squares[counter]);
                    particleSystems.Add(particleLineSpawned);
                    counter++;
                }
            }
            else
            {
                ColorSquares(calibrationData);
            }

        }
        else if (initialDone)
        {
            foreach (GameObject square in squares)
            {
                if (square != null)
                {
                    GameObject particleLineSpawned = UnityEngine.GameObject.Instantiate(ExcentraDatabase.TryGetMiscPrefab("circle-burst"), square.transform.position, Quaternion.identity);
                    ParticleSystem particleSystem = particleLineSpawned.GetComponent<ParticleSystem>();
                    SpriteRenderer spriteRenderer = square.GetComponent<SpriteRenderer>();
                    var main = particleSystem.main;
                    main.startColor = spriteRenderer.color;
                    Color color = spriteRenderer.color;
                    color = new Color(0.3f, 0.3f, 0.3f);
                    spriteRenderer.color = color;


                    SpriteRenderer blueTank = blueTankRune.GetComponent<SpriteRenderer>();

                    Color newColor = blueTank.color;
                    newColor.a = 0.0f;
                    blueTank.color = newColor;

                    SpriteRenderer redTank = redTankRune.GetComponent<SpriteRenderer>();
                    newColor = redTank.color;
                    newColor.a = 0.0f;
                    redTank.color = newColor;

                }
            }
        }
    }
}
