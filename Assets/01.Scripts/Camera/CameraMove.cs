using UnityEngine;

public class CameraMove : MonoBehaviour
{
    private Transform target;

    private Vector3 velocity;
    private Vector3 targetPos;

    private void Start()
    {
        velocity = Vector3.zero;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            target = player.transform;
    }

    private void Update()
    {
        targetPos = new Vector3(target.position.x, target.position.y, transform.position.z);

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, 0.2f);
    }
}
