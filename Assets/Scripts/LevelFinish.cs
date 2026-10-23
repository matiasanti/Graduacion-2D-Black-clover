using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class LevelFinish : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerControl player = other.GetComponentInParent<PlayerControl>();
        if (player != null)
        {
            player.FinishLevel();
        }
    }
}
