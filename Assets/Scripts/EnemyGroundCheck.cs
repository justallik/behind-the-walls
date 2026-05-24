using UnityEngine;

public class EnemyGroundCheck : MonoBehaviour
{
    [Header("Settings")]
    public float checkDistance = 1.1f;
    public LayerMask groundLayer;
    public bool isGrounded;

    private void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, checkDistance, groundLayer);

        Debug.DrawRay(transform.position, Vector3.down * checkDistance, isGrounded ? Color.green : Color.red);
    }
}