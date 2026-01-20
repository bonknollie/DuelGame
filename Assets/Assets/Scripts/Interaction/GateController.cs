using System.Linq;
using UnityEngine;

[AddComponentMenu("Interaction/GateController")]
public class GateController : MonoBehaviour
{
    [Header("Gate Transform")]
    public Transform gateTransform; // assign the mesh root to move/rotate
    public Vector3 openOffset = new Vector3(0f, 3f, 0f);
    public float openSpeed = 2f;

    [Header("Behaviour")]
    [Tooltip("If > 0, gate will open proportionally when at least this many levers are active. If 0, all linked levers must be on.")]
    public int requiredLevers = 0;
    [Tooltip("Disable the gate's collider while fully open to avoid trapping players.")]
    public bool disableColliderWhileOpen = true;
    public Collider gateCollider;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent onOpenStart;
    public UnityEngine.Events.UnityEvent onFullyOpen;
    public UnityEngine.Events.UnityEvent onCloseStart;
    public UnityEngine.Events.UnityEvent onFullyClosed;
    [Header("Audio")]
    public AudioClip openStartClip;
    public AudioClip fullyOpenClip;
    public AudioClip closeStartClip;
    public AudioClip fullyClosedClip;
    [Range(0f,1f)] public float audioVolume = 1f;

    [Header("Linked Levers (optional)")]
    public Lever[] linkedLevers;

    bool opening = false;
    Vector3 closedPos;
    Vector3 openPos;
    bool wasOpening = false;

    void Start()
    {
        if (gateTransform == null) gateTransform = transform;
        closedPos = gateTransform.localPosition;
        openPos = closedPos + openOffset;

        if (linkedLevers != null && linkedLevers.Length > 0)
        {
            foreach (var l in linkedLevers)
            {
                if (l != null) l.OnStateChanged += OnLeverChanged;
            }
            // initial evaluate
            EvaluateLevers();
        }
    }

    void OnDestroy()
    {
        if (linkedLevers != null)
        {
            foreach (var l in linkedLevers)
                if (l != null) l.OnStateChanged -= OnLeverChanged;
        }
    }

    void OnLeverChanged(bool _)
    {
        EvaluateLevers();
    }

    void EvaluateLevers()
    {
        if (linkedLevers == null || linkedLevers.Length == 0)
        {
            opening = false;
            return;
        }

        int onCount = linkedLevers.Count(l => l != null && l.IsOn);
        if (requiredLevers > 0)
        {
            opening = onCount >= requiredLevers;
        }
        else
        {
            // require all levers by default
            opening = onCount == linkedLevers.Length;
        }
    }

    void Update()
    {
        var target = opening ? openPos : closedPos;
        // detect start/stop events
        if (opening && !wasOpening)
        {
            onOpenStart?.Invoke();
        }
        if (!opening && wasOpening)
        {
            onCloseStart?.Invoke();
        }

        gateTransform.localPosition = Vector3.MoveTowards(gateTransform.localPosition, target, openSpeed * Time.deltaTime);

        float t = (gateTransform.localPosition - closedPos).magnitude / (openPos - closedPos).magnitude;
        bool fullyOpen = Mathf.Approximately(t, 1f) || t > 0.999f;
        bool fullyClosed = Mathf.Approximately(t, 0f) || t < 0.001f;

        if (fullyOpen && !wasOpening)
        {
            onFullyOpen?.Invoke();
            if (disableColliderWhileOpen && gateCollider != null) gateCollider.enabled = false;
            PlaySound(fullyOpenClip, audioVolume);
        }
        if (fullyClosed && wasOpening)
        {
            onFullyClosed?.Invoke();
            if (disableColliderWhileOpen && gateCollider != null) gateCollider.enabled = true;
            PlaySound(fullyClosedClip, audioVolume);
        }

        wasOpening = opening;
    }

    void PlaySound(AudioClip clip, float vol)
    {
        if (clip == null) return;
        var src = GetComponent<AudioSource>();
        if (src != null)
        {
            src.PlayOneShot(clip, vol);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, vol);
        }
    }

    // Public controls
    public void Open() { opening = true; }
    public void Close() { opening = false; }
}
