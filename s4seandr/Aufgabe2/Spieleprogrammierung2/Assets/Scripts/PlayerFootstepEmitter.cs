using UnityEngine;

public class PlayerFootstepEmitter : MonoBehaviour
{
    public float loudness = 5f;
    public float stepInterval = 0.5f;
    private float timer = 0f;

    void Update()
    {
        if (IsMoving())
        {
            timer += Time.deltaTime;
            if (timer > stepInterval)
            {
                timer = 0f;
                NPCSoundSystem.Instance.ReportFootstep(transform.position, loudness);
            }
        }
    }

    bool IsMoving()
    {
        return Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;
    }
}
