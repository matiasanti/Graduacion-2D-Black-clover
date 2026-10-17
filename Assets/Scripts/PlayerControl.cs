using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public enum Control
    {
        Left,
        Right,
        Jump
    }

    private const string LeftKeyPreference = "Control_Left";
    private const string RightKeyPreference = "Control_Right";
    private const string JumpKeyPreference = "Control_Jump";

    [SerializeField] float speed = 5f;
    [SerializeField] float jumpForce = 7f;
    [SerializeField] LayerMask groundLayer;
    bool isGrounded = false;
    Rigidbody2D rb;
    SpriteRenderer sprite;
    Animator anim;

    public static KeyCode GetKey(Control control)
    {
        KeyCode defaultKey = GetDefaultKey(control);
        KeyCode savedKey = (KeyCode)PlayerPrefs.GetInt(GetPreferenceKey(control), (int)defaultKey);
        return System.Enum.IsDefined(typeof(KeyCode), savedKey) ? savedKey : defaultKey;
    }

    public static void SetKey(Control control, KeyCode key)
    {
        PlayerPrefs.SetInt(GetPreferenceKey(control), (int)key);
        PlayerPrefs.Save();
    }

    public static bool IsKeyAssigned(KeyCode key, Control exceptControl)
    {
        foreach (Control control in System.Enum.GetValues(typeof(Control)))
        {
            if (control != exceptControl && GetKey(control) == key)
            {
                return true;
            }
        }

        return false;
    }

    private static string GetPreferenceKey(Control control)
    {
        switch (control)
        {
            case Control.Left:
                return LeftKeyPreference;
            case Control.Right:
                return RightKeyPreference;
            default:
                return JumpKeyPreference;
        }
    }

    private static KeyCode GetDefaultKey(Control control)
    {
        switch (control)
        {
            case Control.Left:
                return KeyCode.A;
            case Control.Right:
                return KeyCode.D;
            default:
                return KeyCode.Space;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        float horizontalInput = (Input.GetKey(GetKey(Control.Right)) ? 1f : 0f)
            - (Input.GetKey(GetKey(Control.Left)) ? 1f : 0f);
        if (horizontalInput == 0f
            && !PlayerPrefs.HasKey(LeftKeyPreference)
            && !PlayerPrefs.HasKey(RightKeyPreference))
        {
            horizontalInput = Input.GetAxis("Horizontal");
        }

        if (horizontalInput > 0)
        {
            sprite.flipX = false;
        }
        if (horizontalInput < 0)
        {
            sprite.flipX = true;
        }

        rb.velocity = new Vector2(horizontalInput * speed, rb.velocity.y);
        if (Input.GetKeyDown(GetKey(Control.Jump)) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        Collider2D col = GetComponent<Collider2D>();
        isGrounded = Physics2D.OverlapCircle(
            transform.position - transform.up * ((col.bounds.extents.y / transform.localScale.y - col.offset.y) * transform.localScale.y),
            0.01f,
            groundLayer);
        anim.SetBool("Run", horizontalInput != 0);
        anim.SetFloat("Y", isGrounded ? 0 : rb.velocity.y);
    }
}
