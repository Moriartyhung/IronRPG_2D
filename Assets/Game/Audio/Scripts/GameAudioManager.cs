using UnityEngine;
using UnityEngine.SceneManagement;

public class GameAudioManager : MonoBehaviour
{
    private const string VillageBgmPath = "Audio/bgm_village";
    private const string RuinsBgmPath = "Audio/bgm_ruins";
    private const string BattleBgmPath = "Audio/bgm_battle";
    private const string CaveRoadBgmPath = "Audio/bgm_cave_road";
    private const string SwingMissPath = "Audio/sfx_player_swing_miss";
    private const string HitEnemyPath = "Audio/sfx_player_hit_enemy";
    private const string PlayerHurtPath = "Audio/sfx_player_hurt";
    private const string EnemyDefeatedPath = "Audio/sfx_enemy_defeated";
    private const string MusicVolumeKey = "IronRPG.Settings.MusicVolume";
    private const string SfxVolumeKey = "IronRPG.Settings.SfxVolume";
    private const float DefaultMusicVolume = 0.28f;
    private const float DefaultSfxVolume = 0.9f;

    private static GameAudioManager instance;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioClip villageBgm;
    private AudioClip ruinsBgm;
    private AudioClip battleBgm;
    private AudioClip caveRoadBgm;
    private AudioClip swingMissSfx;
    private AudioClip hitEnemySfx;
    private AudioClip playerHurtSfx;
    private AudioClip enemyDefeatedSfx;
    private AudioClip currentMusic;
    private float musicVolume = DefaultMusicVolume;
    private float sfxVolume = DefaultSfxVolume;
    private float nextMusicCheckTime;
    private bool musicPausedBySettings;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        EnsureInstance();
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    public static void PlayPlayerAttackMiss()
    {
        EnsureInstance().PlaySfx(instance.swingMissSfx, 0.72f);
    }

    public static void PlayPlayerHitEnemy()
    {
        EnsureInstance().PlaySfx(instance.hitEnemySfx, 0.82f);
    }

    public static void PlayPlayerHurt()
    {
        EnsureInstance().PlaySfx(instance.playerHurtSfx, 0.86f);
    }

    public static void PlayEnemyDefeated()
    {
        EnsureInstance().PlaySfx(instance.enemyDefeatedSfx, 0.76f);
    }

    public static float MusicVolume => EnsureInstance().musicVolume;
    public static float SfxVolume => EnsureInstance().sfxVolume;

    public static void SetMusicVolume(float volume)
    {
        EnsureInstance().SetMusicVolumeInternal(volume);
    }

    public static void SetSfxVolume(float volume)
    {
        EnsureInstance().SetSfxVolumeInternal(volume);
    }

    public static void SetMusicPausedBySettings(bool paused)
    {
        EnsureInstance().SetMusicPausedBySettingsInternal(paused);
    }

    private static GameAudioManager EnsureInstance()
    {
        if (instance != null)
        {
            return instance;
        }

        GameObject audioObject = new GameObject(nameof(GameAudioManager));
        instance = audioObject.AddComponent<GameAudioManager>();
        DontDestroyOnLoad(audioObject);
        instance.InitializeSourcesAndClips();
        return instance;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureInstance().RefreshMusic(true);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeSourcesAndClips();
    }

    private void Update()
    {
        if (Time.unscaledTime < nextMusicCheckTime)
        {
            return;
        }

        nextMusicCheckTime = Time.unscaledTime + 0.6f;
        RefreshMusic(false);
    }

    private void InitializeSourcesAndClips()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, musicVolume));
        sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, sfxVolume));
        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;

        villageBgm ??= Resources.Load<AudioClip>(VillageBgmPath);
        ruinsBgm ??= Resources.Load<AudioClip>(RuinsBgmPath);
        battleBgm ??= Resources.Load<AudioClip>(BattleBgmPath);
        caveRoadBgm ??= Resources.Load<AudioClip>(CaveRoadBgmPath);
        swingMissSfx ??= Resources.Load<AudioClip>(SwingMissPath);
        hitEnemySfx ??= Resources.Load<AudioClip>(HitEnemyPath);
        playerHurtSfx ??= Resources.Load<AudioClip>(PlayerHurtPath);
        enemyDefeatedSfx ??= Resources.Load<AudioClip>(EnemyDefeatedPath);
    }

    private void RefreshMusic(bool forceRestart)
    {
        InitializeSourcesAndClips();
        AudioClip desiredMusic = ChooseMusicForCurrentScene();
        if (desiredMusic == null)
        {
            return;
        }

        bool musicChanged = desiredMusic != currentMusic;
        if (musicChanged || forceRestart)
        {
            currentMusic = desiredMusic;
            musicSource.clip = currentMusic;
            musicSource.time = 0f;
        }

        if (musicPausedBySettings)
        {
            if (musicSource.isPlaying)
            {
                musicSource.Pause();
            }

            return;
        }

        if (musicChanged || forceRestart || !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    private AudioClip ChooseMusicForCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        return sceneName switch
        {
            "ForestScene" => battleBgm,
            "CaveScene" => IsCaveBossBattleActive() ? battleBgm : caveRoadBgm,
            "ToCaveScene" => caveRoadBgm,
            "TutorialScene" => HasActiveEnemies() ? battleBgm : ruinsBgm,
            "VillageScene" => villageBgm,
            "PlayerHouseInteriorScene" => villageBgm,
            "ChiefHouseInteriorScene" => villageBgm,
            "TavernInteriorScene" => villageBgm,
            "WeaponShopInteriorScene" => villageBgm,
            "MainMenuScene" => villageBgm,
            _ => villageBgm
        };
    }

    private static bool IsCaveBossBattleActive()
    {
        return BossIntroDialogueTrigger.HasCompleted && !BossCoreBattleController.HasDefeated;
    }

    private static bool HasActiveEnemies()
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null && enemies[i].gameObject.activeInHierarchy && enemies[i].CurrentHealth > 0)
            {
                return true;
            }
        }

        return false;
    }

    private void PlaySfx(AudioClip clip, float volume)
    {
        InitializeSourcesAndClips();
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    private void SetMusicVolumeInternal(float volume)
    {
        InitializeSourcesAndClips();
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        PlayerPrefs.Save();
    }

    private void SetSfxVolumeInternal(float volume)
    {
        InitializeSourcesAndClips();
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
        PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
        PlayerPrefs.Save();
    }

    private void SetMusicPausedBySettingsInternal(bool paused)
    {
        InitializeSourcesAndClips();
        if (musicPausedBySettings == paused)
        {
            return;
        }

        musicPausedBySettings = paused;
        if (musicPausedBySettings)
        {
            musicSource.Pause();
        }
        else if (currentMusic != null)
        {
            musicSource.UnPause();
            if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }
    }
}
