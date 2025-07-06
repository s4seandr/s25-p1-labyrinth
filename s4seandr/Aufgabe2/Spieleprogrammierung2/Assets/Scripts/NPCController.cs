using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    public Transform player;
    NavMeshAgent agent;
    Animator animator;

    public NPCDecisionManager decisionManager;

    public float speed = 3.5f;
    public float thinkInterval = 5f;
    public float actionDuration = 4f;
    public Vector3 lastPlayerPosition;

    private float nextThinkTime = 0f;
    private float actionTimer = 0f;
    private string currentAction = "";
    public Transform StartPosition;

    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    IEnumerator Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        lastPlayerPosition = player.position;
        yield return new WaitForSeconds(1);
        if (decisionManager != null)
        {
            string initialPrompt = "Du bist ein NPC in einem Labyrinth. Außer dir gibt es eine weitere Person. Beobachte ihn und entscheide, wie du dich verhälst. Merke dir auch deine vorherigen Entscheidungen.";
            StartCoroutine(decisionManager.GetDecisionFromLLM(initialPrompt));
        }
        else
        {
            Debug.LogError("decisionManager ist in Start() null!");
        }
    }

    void Update()
    {
        if (decisionManager == null)
        {
            Debug.LogError("decisionManager ist null!");
            return; // oder anderes Handling
        }

        if (Time.time >= nextThinkTime)
        {
            // Baue den Prompt aus Spielsituation
            string prompt = GeneratePrompt();
            if (decisionManager != null)
            {
                StartCoroutine(decisionManager.GetDecisionFromLLM(prompt));
            }
            else
            {
                Debug.LogError("decisionManager ist null!");
            }
            nextThinkTime = Time.time + thinkInterval;
        }

        // Wende die Aktion an, die von der KI kam
        if (!string.IsNullOrEmpty(decisionManager.currentAction))
        {
            currentAction = decisionManager.currentAction;
            decisionManager.currentAction = ""; // Zurücksetzen
            actionTimer = actionDuration;
        }

        if (actionTimer > 0f)
        {
            PerformAction(currentAction);
            actionTimer -= Time.deltaTime;
        }
    }

    string GeneratePrompt()
    {
        // Blickrichtung des SPIELERS auf den NPC
        Vector3 dirToNPC = (transform.position - player.position).normalized;
        float playerAngle = Vector3.Angle(player.forward, dirToNPC);
        string schautNpcAn = playerAngle < 60f ? "ja" : "nein";
        // Blickrichtung des NPC auf den SPIELER
        float distance = Vector3.Distance(transform.position, player.position);
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        string blickrichtung = angle < 60f ? "ja" : "nein";
        // Bewegung des SPIELERS
        Vector3 playerDelta = player.position - lastPlayerPosition;
        float playerSpeed = playerDelta.magnitude / Time.deltaTime;
        string bewegung = "steht still";
        // Richtung relativ zum NPC
        if (playerSpeed > 0.05f)
        {
            float moveAngle = Vector3.Angle(playerDelta.normalized, dirToNPC);
            if (moveAngle < 60f)
                bewegung = "kommt auf dich zu";
            else if (moveAngle > 120f)
                bewegung = "läuft von dir weg";
            else
                bewegung = "läuft seitlich";
        }
        lastPlayerPosition = player.position;
        //Absenden des Prompts
        Debug.Log("Distanz: " + distance + ", Bewegung: " + bewegung + ", Schautan: " + blickrichtung + ", Wird angeschaut: " + schautNpcAn);
        return $"Die Person ist {distance:F1} Meter entfernt. Sie {bewegung}. Schaut die Person dich an? {schautNpcAn}. Schaust du ihn an? {blickrichtung}. " +
               $"Gib genau eine der folgenden Aktionen zurück: WinkeDemSpieler, GehZuSpieler, GehWeg, KriecheZuSpieler, KriecheWeg, LaufeZuSpieler, RenneWeg.";
    }

    void PerformAction(string action)
    {
        switch (action)
        {
            case "GehZuSpieler":
                MoveTowards(player.position, speed * 0.5f);
                break;
            case "RennWeg":
                MoveAwayFrom(player.position, speed * 1.0f);
                break;
            case "KriecheZuSpieler":
                CrouchTo(player.position, speed * 0.2f);
                break;
            case "KriecheWeg":
                CrouchAwayFrom(player.position, speed * 0.2f); 
                break;
            case "LaufeZuSpieler":
                RunTowards(player.position, speed * 1f);
                break;
            case "LaufeWeg":
                RunAwayFrom(player.position, speed * 1f);
                break;
            case "WinkeDemSpieler":
                WaveTowards(player.position, 0);
                break;
        }
    }

    void MoveTowards(Vector3 target, float moveSpeed)
    {
        agent.destination = player.position;
        animator.SetFloat("Speed", moveSpeed);
        animator.SetBool("IsCrouching", false);
        animator.SetBool("IsWaving", false);
        //Debug.Log("NPC-Speed: " + moveSpeed);
    }

    void MoveAwayFrom(Vector3 from, float moveSpeed)
    {
        Vector3 escapeTarget = FindFurthestReachablePointFromPlayer(from, 30f, 16);
        agent.destination = escapeTarget;
        animator.SetFloat("Speed", moveSpeed);
        animator.SetBool("IsCrouching", false);
        animator.SetBool("IsWaving", false) ;
        //Debug.Log("NPC-Speed: " + moveSpeed);
    }

    void CrouchTo(Vector3 target, float moveSpeed)
    {
        agent.destination = player.position;
        animator.SetFloat("Speed", moveSpeed);
        animator.SetBool("IsCrouching", true);
        animator.SetBool("IsWaving", false);
        //Debug.Log("NPC-Speed: " + moveSpeed);
    }

    void CrouchAwayFrom(Vector3 from, float moveSpeed)
    {
        Vector3 escapeTarget = FindFurthestReachablePointFromPlayer(from, 30f, 16);
        agent.destination = escapeTarget;
        animator.SetFloat("Speed", moveSpeed);
        animator.SetBool("IsCrouching", true);
        animator.SetBool("IsWaving", false);
        //Debug.Log("NPC-Speed: " + moveSpeed);
    }

    void RunTowards(Vector3 target, float moveSpeed)
    {
        agent.destination = player.position;
        animator.SetFloat("Speed", moveSpeed);
        animator.SetBool("IsCrouching", false);
        animator.SetBool("IsWaving", false);
        //Debug.Log("NPC-Speed: " + moveSpeed);
    }

    void RunAwayFrom(Vector3 from, float moveSpeed)
    {
        Vector3 escapeTarget = FindFurthestReachablePointFromPlayer(from, 30f, 16);
        agent.destination = escapeTarget;
        animator.SetFloat("Speed", moveSpeed);
        animator.SetBool("IsCrouching", false);
        animator.SetBool("IsWaving", false);
        //Debug.Log("NPC-Speed: " + moveSpeed);
    }

    void WaveTowards(Vector3 from, float moveSpeed)
    {
        agent.ResetPath();
        animator.SetFloat("Speed", moveSpeed);
        animator.SetBool("IsCrouching", false);
        animator.SetBool("IsWaving", true);
        //Debug.Log("NPC-Speed: " + moveSpeed);
    }

    Vector3 FindFurthestReachablePointFromPlayer(Vector3 from, float searchRadius, int samples)
    {
        float bestScore = 0f;
        Vector3 bestTarget = StartPosition.position;

        for (int i = 0; i < samples; i++)
        {
            float angle = i * Mathf.PI * 2f / samples;
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            Vector3 candidate = transform.position + dir * searchRadius;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    float pathLength = GetPathLength(path);
                    float playerDistance = Vector3.Distance(hit.position, from);

                    float score = playerDistance + pathLength * 0.5f;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestTarget = hit.position;
                    }
                }
            }
        }
        return bestTarget;
    }

    float GetPathLength(NavMeshPath path)
    {
        float length = 0f;
        for (int i = 1; i < path.corners.Length; i++)
        {
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }
        return length;
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(agent.transform.position), FootstepAudioVolume);
            }
        }
    }

}
