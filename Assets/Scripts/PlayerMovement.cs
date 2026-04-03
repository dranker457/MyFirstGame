using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;

    private CharacterController _controller;
    private Vector3 _velocity;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // 矢印キー / WASDで入力取得
        float horizontal = Input.GetAxisRaw("Horizontal"); // 左右
        float vertical   = Input.GetAxisRaw("Vertical");   // 前後

        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        if (move.magnitude > 1f)
            move.Normalize();

        _controller.Move(move * moveSpeed * Time.deltaTime);

        // 重力
        if (_controller.isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;

        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }
}
