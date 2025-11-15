using UnityEngine;

public class CalibrationScript : MonoBehaviour, IParticleHitReceiver
{
    bool particleHit = false;
    SpriteRenderer spriteRenderer;

    private float colorLerpTime = 0f;
    private float colorLerpDuration = 100f; // Duration for the color change  

    public MedicaAetherialTrigger triggerScript;

    public GameObject tankRune;
    public bool tankRuneActive = false;

    public Color colorData;
    public bool completed = false;

    public void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Update()
    {
        if (colorData == null || particleHit == false || completed)
            return;

        if (tankRuneActive && tankRune != null)
        {
            Debug.Log("Hi!");
            SpriteRenderer trSR = tankRune.GetComponent<SpriteRenderer>();

            Color trColor = trSR.color;
            trColor.a += 5f * Time.deltaTime;
            trSR.color = trColor;

        }


        if (Vector3.Distance(new Vector3(colorData.r, colorData.g, colorData.b), new Vector3(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b)) < 0.1f)
        {
            Debug.Log("Color already set, skipping lerp.");
            triggerScript.FinishColor();
            completed = true;
            return;
        }

        if (colorLerpTime < colorLerpDuration)
        {
            colorLerpTime += Time.deltaTime;
            float t = colorLerpTime / colorLerpDuration;
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, colorData, t);
        }
    }
    public void OnParticleHit(ParticleSystem.Particle particle)
    {
        if (!particleHit)
            particleHit = true;
    }
}
