using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SplashSequence : MonoBehaviour
{
    [Header("Title Cards (in order)")]
    [SerializeField] private CanvasGroup[] cards;

    [Header("Timing (seconds)")]
    [SerializeField] private float fadeInTime = 0.8f;
    [SerializeField] private float holdTime = 2f;
    [SerializeField] private float fadeOutTime = 0.8f;

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName = "Main Menu";

    private bool _skip;

    void Start()
    {
        // Hide every card before starting
        foreach (var card in cards) card.alpha = 0f;
        StartCoroutine(RunSequence());
    }

    void Update()
    {
        // Any key, mouse click or gamepad button skips the current card
        bool keyPressed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        bool clicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool padPressed = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;

        if (keyPressed || clicked || padPressed) _skip = true;
    }

    private IEnumerator RunSequence()
    {
        foreach (var card in cards)
        {
            _skip = false;
            yield return Fade(card, 0f, 1f, fadeInTime);
            yield return Wait(holdTime);
            yield return Fade(card, 1f, 0f, fadeOutTime);
        }

        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration && !_skip)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        group.alpha = to;
    }

    private IEnumerator Wait(float duration)
    {
        float t = 0f;
        while (t < duration && !_skip)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}