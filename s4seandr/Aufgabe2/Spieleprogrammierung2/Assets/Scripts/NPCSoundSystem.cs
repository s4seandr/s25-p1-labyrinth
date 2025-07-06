using UnityEngine;
using System;

public class NPCSoundSystem : MonoBehaviour
{
    public static NPCSoundSystem Instance;

    public event Action<Vector3, float> OnFootstepHeard;

    void Awake()
    {
        Instance = this;
    }

    public void ReportFootstep(Vector3 pos, float loudness)
    {
        OnFootstepHeard?.Invoke(pos, loudness);
    }
}
