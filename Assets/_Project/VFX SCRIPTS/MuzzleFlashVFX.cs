using UnityEngine;

public class MuzzleFlashVFX : MonoBehaviour
{
    public ParticleSystem muzzleFlash;

    public void Play() => muzzleFlash?.Play();
}