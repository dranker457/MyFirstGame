using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("ジャンプ設定")]
    [SerializeField] private float jumpHeight = 1.5f;

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

        // 重力・ジャンプ
        if (_controller.isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;

        // Spaceキーでジャンプ（接地中のみ）
        if (Input.GetButtonDown("Jump") && _controller.isGrounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }
}
