using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FMODManager : MonoBehaviour
{
    public static FMODManager Instance { get; private set; }

    [Header("Tank SFX")]
    [SerializeField] private EventReference tankShot;
    [SerializeField] private EventReference tankDriftStart;
    [SerializeField] private EventReference engineNoise;
    [SerializeField] private EventReference tankTrackRattle;
    [SerializeField] private EventReference reloadTankShell;
    [SerializeField] private EventReference turretRotation;
    [SerializeField] private EventReference dopamineKillNoise;
    [SerializeField] private EventReference tankDamage;
    [SerializeField] private EventReference tankDriftHum;

    [Header("AT Turret SFX")]
    [SerializeField] private EventReference turretShot;
    [SerializeField] private EventReference turretDeath;
    [SerializeField] private EventReference playerSpotted;

    [Header("Music")]
    [SerializeField] private EventReference mainMenuMusic;
    [SerializeField] private EventReference musicTrackOne;   // game level music
    // Set in code, must match the scene names exactly
    private const string SplashSceneName = "Splash Screen";
    private const string MainMenuSceneName = "Main Menu";

    [Header("Ambience")]
    [SerializeField] private EventReference warAmbience;

    [Header("Objectives")]
    [SerializeField] private EventReference gateOpen;
    [SerializeField] private EventReference captureZone;

    private EventInstance _engineInstance;
    private EventInstance _trackRattleInstance;
    private EventInstance _turretInstance;
    private EventInstance _captureZoneInstance;
    private EventInstance _musicInstance;
    private EventInstance _playerSpottedInstance;
    private EventInstance _warAmbienceInstance;
    private EventInstance _driftHumInstance;

    private bool _isStopped = false;

    private Coroutine _engineFadeCoroutine;
    private Coroutine _trackFadeCoroutine;
    private Coroutine _turretFadeCoroutine;
    private Coroutine _driftHumFadeCoroutine;

    private enum MusicType { None, Menu, Game }
    private MusicType _currentMusic = MusicType.None;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _engineInstance = RuntimeManager.CreateInstance(engineNoise);
        _trackRattleInstance = RuntimeManager.CreateInstance(tankTrackRattle);
        _turretInstance = RuntimeManager.CreateInstance(turretRotation);
        _captureZoneInstance = RuntimeManager.CreateInstance(captureZone);
        _playerSpottedInstance = RuntimeManager.CreateInstance(playerSpotted);
        _warAmbienceInstance = RuntimeManager.CreateInstance(warAmbience);
        _driftHumInstance = RuntimeManager.CreateInstance(tankDriftHum);
    }

    void Start()
    {
        if (Instance != this) return; // ignore duplicates that are about to be destroyed

        // Covers the very first scene in case sceneLoaded fired before we subscribed
        ApplySceneAudio(SceneManager.GetActiveScene().name);
    }

    void OnEnable()
    {
        CaptureZoneTask.PlayerStartCapturing.AddListener(OnPlayerStartCapturing);
        EmplacementTargetingState.PlayerDetected.AddListener(PlayPlayerDetected);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        CaptureZoneTask.PlayerStartCapturing.RemoveListener(OnPlayerStartCapturing);
        EmplacementTargetingState.PlayerDetected.RemoveListener(PlayPlayerDetected);
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnDestroy()
    {
        _engineInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _engineInstance.release();
        _trackRattleInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _trackRattleInstance.release();
        _turretInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _turretInstance.release();
        _captureZoneInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _captureZoneInstance.release();
        _musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _musicInstance.release();
        _playerSpottedInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _playerSpottedInstance.release();
        _warAmbienceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _warAmbienceInstance.release();
        _driftHumInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _driftHumInstance.release();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplySceneAudio(scene.name);
    }

    private void ApplySceneAudio(string sceneName)
    {
        // Splash and main menu share the same music, so it carries over without restarting
        bool isMenu = sceneName == SplashSceneName || sceneName == MainMenuSceneName;

        if (isMenu)
        {
            // Main menu: only the menu music plays
            _isStopped = true;
            StopGameplayLoops();
            PlayMusic(MusicType.Menu);
        }
        else
        {
            // Game scene: menu music fades out, everything else switches on
            _isStopped = false;
            PlayMusic(MusicType.Game);
            PlayWarAmbienceSound();
        }
    }

    private void StopGameplayLoops()
    {
        if (_engineFadeCoroutine != null) { StopCoroutine(_engineFadeCoroutine); _engineFadeCoroutine = null; }
        if (_trackFadeCoroutine != null) { StopCoroutine(_trackFadeCoroutine); _trackFadeCoroutine = null; }
        if (_turretFadeCoroutine != null) { StopCoroutine(_turretFadeCoroutine); _turretFadeCoroutine = null; }
        if (_driftHumFadeCoroutine != null) { StopCoroutine(_driftHumFadeCoroutine); _driftHumFadeCoroutine = null; }

        _engineInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _trackRattleInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _turretInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _captureZoneInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _playerSpottedInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _warAmbienceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _driftHumInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    public void StopAllSounds()
    {
        _isStopped = true;

        if (_engineFadeCoroutine != null) { StopCoroutine(_engineFadeCoroutine); _engineFadeCoroutine = null; }
        if (_trackFadeCoroutine != null) { StopCoroutine(_trackFadeCoroutine); _trackFadeCoroutine = null; }
        if (_turretFadeCoroutine != null) { StopCoroutine(_turretFadeCoroutine); _turretFadeCoroutine = null; }
        if (_driftHumFadeCoroutine != null) { StopCoroutine(_driftHumFadeCoroutine); _driftHumFadeCoroutine = null; }

        _engineInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _trackRattleInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _turretInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _captureZoneInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _playerSpottedInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _warAmbienceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _driftHumInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _currentMusic = MusicType.None;
    }

    public void PlayTankDamageNoise()
    {
        RuntimeManager.PlayOneShot(tankDamage);
    }

    // Engine
    public void PlayEngineNoise(Vector3 pos)
    {
        if (_isStopped) return;

        if (_engineFadeCoroutine != null) { StopCoroutine(_engineFadeCoroutine); _engineFadeCoroutine = null; }
        _engineInstance.setVolume(1f);
        _engineInstance.set3DAttributes(RuntimeUtils.To3DAttributes(pos));
        _engineInstance.getPlaybackState(out PLAYBACK_STATE state);
        if (state == PLAYBACK_STATE.STOPPED || state == PLAYBACK_STATE.STOPPING)
            _engineInstance.start();
    }

    public void StopEngineNoise()
    {
        _engineFadeCoroutine = StartCoroutine(FadeOutAndStop(_engineInstance));
    }

    // Track rattle
    public void PlayTankTrackRattle(Vector3 pos)
    {
        if (_isStopped) return;

        if (_trackFadeCoroutine != null) { StopCoroutine(_trackFadeCoroutine); _trackFadeCoroutine = null; }
        _trackRattleInstance.setVolume(1f);
        _trackRattleInstance.set3DAttributes(RuntimeUtils.To3DAttributes(pos));
        _trackRattleInstance.getPlaybackState(out PLAYBACK_STATE state);
        if (state == PLAYBACK_STATE.STOPPED || state == PLAYBACK_STATE.STOPPING)
            _trackRattleInstance.start();
    }

    public void StopTankTrackRattle()
    {
        _trackFadeCoroutine = StartCoroutine(FadeOutAndStop(_trackRattleInstance));
    }

    // Drift hum
    public void PlayTankDriftHum(Vector3 pos)
    {
        if (_isStopped) return;

        if (_driftHumFadeCoroutine != null) { StopCoroutine(_driftHumFadeCoroutine); _driftHumFadeCoroutine = null; }
        _driftHumInstance.setVolume(1f);
        _driftHumInstance.set3DAttributes(RuntimeUtils.To3DAttributes(pos));
        _driftHumInstance.getPlaybackState(out PLAYBACK_STATE state);
        if (state == PLAYBACK_STATE.STOPPED || state == PLAYBACK_STATE.STOPPING)
            _driftHumInstance.start();
    }

    public void StopTankDriftHum()
    {
        _driftHumFadeCoroutine = StartCoroutine(FadeOutAndStop(_driftHumInstance));
    }

    // Turret
    public void PlayTurretRotation(Vector3 pos)
    {
        if (_turretFadeCoroutine != null) { StopCoroutine(_turretFadeCoroutine); _turretFadeCoroutine = null; }
        _turretInstance.setVolume(1f);
        _turretInstance.set3DAttributes(RuntimeUtils.To3DAttributes(pos));
        _turretInstance.getPlaybackState(out PLAYBACK_STATE state);
        if (state == PLAYBACK_STATE.STOPPED || state == PLAYBACK_STATE.STOPPING)
            _turretInstance.start();
    }

    public void StopTurretRotation()
    {
        _turretFadeCoroutine = StartCoroutine(FadeOutAndStop(_turretInstance));
    }

    // Capture Zone
    private void OnPlayerStartCapturing()
    {
        _captureZoneInstance.getPlaybackState(out PLAYBACK_STATE state);
        if (state == PLAYBACK_STATE.STOPPED || state == PLAYBACK_STATE.STOPPING)
            _captureZoneInstance.start();
    }

    private void OnPlayerStoppedCapturing()
    {
        _captureZoneInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    // Ambience
    public void PlayWarAmbienceSound()
    {
        if (_isStopped) return;
        _warAmbienceInstance.getPlaybackState(out PLAYBACK_STATE state);
        if (state == PLAYBACK_STATE.STOPPED || state == PLAYBACK_STATE.STOPPING)
            _warAmbienceInstance.start();
    }

    // Fade helper
    private IEnumerator FadeOutAndStop(EventInstance instance, float fadeDuration = 0.2f)
    {
        float timer = 0f;
        instance.getVolume(out float startVolume);

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            instance.setVolume(Mathf.Lerp(startVolume, 0f, timer / fadeDuration));
            yield return null;
        }

        instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        instance.setVolume(startVolume);
    }

    // Music
    public bool IsMusicPlaying() => _currentMusic != MusicType.None;

    public void PlayMusicTrackOne() => PlayMusic(MusicType.Game);
    public void PlayMainMenuMusic() => PlayMusic(MusicType.Menu);

    private void PlayMusic(MusicType type)
    {
        if (_currentMusic == type) return; // already playing the right track

        // Fade out whatever is playing, then release (it keeps fading after release)
        _musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _musicInstance.release();

        _currentMusic = type;
        var evt = type == MusicType.Menu ? mainMenuMusic : musicTrackOne;
        _musicInstance = RuntimeManager.CreateInstance(evt);
        _musicInstance.start();
    }

    public void PlayGateOpen(Vector3 pos)
    {
        EventInstance instance = RuntimeManager.CreateInstance(gateOpen);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(pos));
        instance.start();
        instance.release();
    }

    private void PlayPlayerDetected()
    {
        _playerSpottedInstance.getPlaybackState(out PLAYBACK_STATE state);
        if (state == PLAYBACK_STATE.STOPPED || state == PLAYBACK_STATE.STOPPING)
            _playerSpottedInstance.start();
    }

    // One shots
    public void PlayTankShot(Vector3 pos) => RuntimeManager.PlayOneShot(tankShot, pos);
    public void PlayTankDriftStart(Vector3 pos) => RuntimeManager.PlayOneShot(tankDriftStart, pos);
    public void PlayReloadTankShell(Vector3 pos) => RuntimeManager.PlayOneShot(reloadTankShell, pos);
    public void PlayATDeathSound(Vector3 pos) => RuntimeManager.PlayOneShot(turretDeath, pos);
    public void PlayATShotSound(Vector3 pos) => RuntimeManager.PlayOneShot(turretShot, pos);
    public void PlayDopamineKillNoise() => RuntimeManager.PlayOneShot(dopamineKillNoise);
}