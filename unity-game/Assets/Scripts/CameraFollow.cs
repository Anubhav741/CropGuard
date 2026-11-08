
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;

    public Vector3 offset = new Vector3(0f, 5f, -7f);

    void LateUpdate()
    {
        if (player == null)
            return;

        transform.position = player.position + offset;

        transform.LookAt(player.position + Vector3.up * 1.2f);
    }
}
