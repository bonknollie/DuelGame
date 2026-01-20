using System;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Interaction/Lever")]
public class Lever : MonoBehaviour, IInteractable
{
    [Header("Handle")]
    public Transform handle; // assign the rotating part of your model
    public Vector3 offEuler = Vector3.zero;
    public Vector3 onEuler = new Vector3(-30f, 0f, 0f);
    public float animSpeed = 6f;

    [Header("State")]
    public bool startOn = false;
    public bool isLocked = false; // when locked, interaction does nothing

    [Header("Events")]
    public UnityEvent<bool> onToggled; // param = isOn

    // C# event for code subscriptions
    public event Action<bool> OnStateChanged;

    bool targetOn;
    bool currentOn;
    [Header("Audio")]
    public AudioClip toggleClip;
    [Range(0f,1f)] public float toggleVolume = 1f;

    void Start()
    {
        currentOn = startOn;
        targetOn = startOn;
        if (handle != null)
        {
            handle.localEulerAngles = currentOn ? onEuler : offEuler;
        }
    }

    void Update()
    {
        if (handle == null) return;
        var desired = targetOn ? onEuler : offEuler;
        handle.localEulerAngles = Vector3.Lerp(handle.localEulerAngles, desired, Time.deltaTime * animSpeed);
    }

    public void Interact(GameObject interactor = null)
    {
        if (isLocked) return;
        SetState(!targetOn);
    }

    public void SetState(bool on)
    {
        if (targetOn == on) return;
        targetOn = on;
        currentOn = on;
        onToggled?.Invoke(on);
        OnStateChanged?.Invoke(on);
        PlaySound(toggleClip, toggleVolume);
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

    public bool IsOn => targetOn;
}
