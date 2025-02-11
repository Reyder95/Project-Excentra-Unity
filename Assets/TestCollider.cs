using UnityEngine;

public class TestCollider : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("TRIGGER ENTERED");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //Debug.Log("TRIGGER EXITED");
    }
}
