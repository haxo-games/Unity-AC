using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    CharacterController controller;
    Vector3 velocity;
    bool isGrounded;
    bool isCrouching;
    float crouchTimer;
    bool lerpCrouch;
    bool wasUngrounded;
    float initialUngroundedY;

    [SerializeField] AudioClip crouchAudioClip;
    [SerializeField] AudioClip uncrouchAudioClip;
    [SerializeField] AudioClip jumpAudioClip;
    [SerializeField] AudioClip landAudioClip;
    [SerializeField] AudioClip stepAudioClip;

    public float speed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 0.7f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        Debug.Log(wasUngrounded);

        if (!isGrounded)
        {
            if (!wasUngrounded)
            {
                initialUngroundedY = transform.position.y;
                wasUngrounded = true;
            }
        }
        else if (wasUngrounded)
        {
            float distanceFallen = Mathf.Abs(transform.position.y - initialUngroundedY);
            wasUngrounded = false;

            if (distanceFallen >= 1.5f)
                SfxManager.instance.PlaySound(landAudioClip, transform, 0.4f);
            else
                SfxManager.instance.PlaySound(stepAudioClip, transform, 0.4f);

        }

        if (lerpCrouch)
        {
            crouchTimer += Time.deltaTime;
            float p = crouchTimer * crouchTimer;

            if (isCrouching)
                controller.height = Mathf.Lerp(controller.height, 1, p);
            else
                controller.height = Mathf.Lerp(controller.height, 2, p);

            if (p > 1f)
            {
                lerpCrouch = false;
                crouchTimer = 0f;
            }
        }
    }

    public void ProcessMove(Vector2 input)
    {
        Vector3 worldDirection = transform.TransformDirection(new Vector3(input.x, 0, input.y));
        float processedSpeed = speed;

        if (isCrouching)
            processedSpeed /= 2f;

        velocity.x = worldDirection.x * processedSpeed;
        velocity.z = worldDirection.z * processedSpeed;
        velocity.y += gravity * Time.deltaTime;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        controller.Move(velocity * Time.deltaTime);
    }

    public void Jump()
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // v² = u² + 2as -> u = √(-2 × gravity × jumpHeight)
            SfxManager.instance.PlaySound(jumpAudioClip, transform, 0.4f);
        }
    }

    public void ToggleCrouch()
    {
        isCrouching = !isCrouching;
        crouchTimer = 0;
        lerpCrouch = true;

        if (isCrouching)
            SfxManager.instance.PlaySound(crouchAudioClip, transform, 0.4f);
        else
            SfxManager.instance.PlaySound(uncrouchAudioClip, transform, 0.4f);
    }
}
