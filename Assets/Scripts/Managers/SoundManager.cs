using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MusicType
{
    TITLE_MUSIC, BATTLE_MUSIC, MAP_MUSIC, SHOP_MUSIC
}

public enum SoundEffect
{
    CARD_OBTAIN, COIN_OBTAIN, GENERIC_CARD_PLAYED, GENERIC_DAMAGE_TAKEN,
    SHIELD_DAMAGE, CARD_HOVER, RELIC_OBTAIN, GAIN_BUFF, GAIN_DEBUFF,
    GAIN_CHARGE, NEW_CARD_SELECT, GAME_OVER, SHOP_PURCHASE,
    SHIELD_APPLY, EXPLOSION, HEAL_HEALTH, CHARGE_ENERGY, GENERIC_BUTTON_HOVER
}

public enum CharacterBlipName
{
    SHOPKEEPER, JACK, RENO, RYAN
}

[System.Serializable]
public struct CharacterBlip
{
    public CharacterBlipName characterName;
    public AudioClip sound;
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{

    public static SoundManager Instance { get; private set; }
    private AudioSource _audioSource;
    public void ResumeAudio() => _audioSource.Play();
    public void PauseAudio() => _audioSource.Pause();
    private float _desiredVolume = 0.5f;
    public void SetDesiredVolume(float volume)
    {
        _desiredVolume = volume;
        SetVolume(volume);
        PlayerPrefs.SetFloat("DesiredVolume", volume);
    }
    public void SetVolume(float volume) => _audioSource.volume = volume;
    public float GetVolume() => _audioSource.volume; // Returns actual volume of audio source.
    public float GetDesiredVolume() => _desiredVolume; // Returns desired volume of audio source.
    [Header("Music")]
    public AudioClip titleMusic;
    public AudioClip battleMusic;
    public AudioClip mapMusic;
    public AudioClip shopMusic;
    [Header("Global Sound Effects")]
    public AudioClip cardObtainSFX;
    public AudioClip coinObtainSFX;
    public AudioClip relicObtainSFX;
    public AudioClip newCardSelectSFX;
    public AudioClip healSFX;
    public AudioClip genericCardPlayedSFX;
    public AudioClip genericDamageTakenSFX;
    public AudioClip shieldDamageSFX;
    public AudioClip shieldApplySFX;
    public AudioClip cardHoverSFX;
    public AudioClip gainBuffSFX;
    public AudioClip gainDebuffSFX;
    public AudioClip gainChargeSFX;
    public AudioClip gameOverSFX;
    public AudioClip shopPurchaseSFX;
    public AudioClip explosionSFX;
    public AudioClip energyChargeSFX;
    public AudioClip genericButtonHoverSFX;
    [Header("Character Blips")]
    public List<CharacterBlip> characterBlips = new List<CharacterBlip>();
    private float _timeSinceLastBlip;

    private void Awake()
    {
        // Set this to the Instance if it is the first one.
        // Or else, destroy this.
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        // Set the audio source.
        _audioSource = GetComponent<AudioSource>();
        // Set the stored desired volume, if there is any. Or else set it to 0.5f.
        SetDesiredVolume(PlayerPrefs.HasKey("DesiredVolume") ? PlayerPrefs.GetFloat("DesiredVolume") : 0.5f);
        // Set this object to never destroy.
        DontDestroyOnLoad(gameObject);
    }

    // This sets the global volume (the AudioListener) that affects
    // ALL audio sources.
    public void SetGlobalVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void PlayOnLoop(MusicType musicType)
    {
        switch (musicType)
        {
            case MusicType.TITLE_MUSIC:
                _audioSource.clip = titleMusic;
                break;
            case MusicType.BATTLE_MUSIC:
                _audioSource.clip = battleMusic;
                break;
            case MusicType.SHOP_MUSIC:
                _audioSource.clip = shopMusic;
                break;
            case MusicType.MAP_MUSIC:
                _audioSource.clip = mapMusic;
                break;
        }
        _audioSource.loop = true;
        _audioSource.Play();
    }

    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        _audioSource.PlayOneShot(clip, volume);
    }

    // Searches the characterBlips list for a certain character, and plays that blip noise.
    // Doesn't play the blip noise if it's too close in proximity to the last blip.
    public void PlayBlip(CharacterBlipName name)
    {
        // Only be able to play the blip once in a certain time span.
        if (Time.time - _timeSinceLastBlip < 0.05f)
        {
            return;
        }
        _timeSinceLastBlip = Time.time;
        // Find the blip. If it doesn't exist, do nothing.
        int blip = characterBlips.FindIndex((cb) => cb.characterName == name);
        if (blip == -1)
        {
            Debug.Log("ERROR FINDING CHARACTER BLIP IN SOUNDMANAGER.CS! (" + name + ")");
            return;
        }
        PlayOneShot(characterBlips[blip].sound);
    }

    // These are for sound effects that are commonly used, i.e. card chosen.
    public void PlaySFX(SoundEffect sfx, float volumeOverride = -1)
    {
        switch (sfx)
        {
            case SoundEffect.CARD_OBTAIN:
                PlayOneShot(cardObtainSFX, (volumeOverride != -1) ? volumeOverride : 0.9f);
                break;
            case SoundEffect.COIN_OBTAIN:
                PlayOneShot(coinObtainSFX, (volumeOverride != -1) ? volumeOverride : 0.9f);
                break;
            case SoundEffect.GENERIC_CARD_PLAYED:
                PlayOneShot(genericCardPlayedSFX, (volumeOverride != -1) ? volumeOverride : 1);
                break;
            case SoundEffect.GENERIC_DAMAGE_TAKEN:
                PlayOneShot(genericDamageTakenSFX, (volumeOverride != -1) ? volumeOverride : 0.8f);
                break;
            case SoundEffect.SHIELD_DAMAGE:
                PlayOneShot(shieldDamageSFX, (volumeOverride != -1) ? volumeOverride : 0.7f);
                break;
            case SoundEffect.SHIELD_APPLY:
                PlayOneShot(shieldApplySFX, (volumeOverride != -1) ? volumeOverride : 0.5f);
                break;
            case SoundEffect.CARD_HOVER:
                PlayOneShot(cardHoverSFX, (volumeOverride != -1) ? volumeOverride : 0.25f);
                break;
            case SoundEffect.RELIC_OBTAIN:
                PlayOneShot(relicObtainSFX, (volumeOverride != -1) ? volumeOverride : 1);
                break;
            case SoundEffect.GAIN_BUFF:
                PlayOneShot(gainBuffSFX, (volumeOverride != -1) ? volumeOverride : 0.55f);
                break;
            case SoundEffect.GAIN_DEBUFF:
                PlayOneShot(gainDebuffSFX, (volumeOverride != -1) ? volumeOverride : 0.9f);
                break;
            case SoundEffect.GAIN_CHARGE:
                PlayOneShot(gainChargeSFX, (volumeOverride != -1) ? volumeOverride : 0.6f);
                break;
            case SoundEffect.NEW_CARD_SELECT:
                PlayOneShot(newCardSelectSFX, (volumeOverride != -1) ? volumeOverride : 0.9f);
                break;
            case SoundEffect.GAME_OVER:
                PlayOneShot(gameOverSFX, (volumeOverride != -1) ? volumeOverride : 1.8f);
                break;
            case SoundEffect.SHOP_PURCHASE:
                PlayOneShot(shopPurchaseSFX, (volumeOverride != -1) ? volumeOverride : 1);
                break;
            case SoundEffect.EXPLOSION:
                PlayOneShot(explosionSFX, (volumeOverride != -1) ? volumeOverride : 1);
                break;
            case SoundEffect.HEAL_HEALTH:
                PlayOneShot(healSFX, (volumeOverride != -1) ? volumeOverride : 1.2f);
                break;
            case SoundEffect.CHARGE_ENERGY:
                PlayOneShot(energyChargeSFX, (volumeOverride != -1) ? volumeOverride : 0.7f);
                break;
            case SoundEffect.GENERIC_BUTTON_HOVER:
                PlayOneShot(genericButtonHoverSFX, (volumeOverride != -1) ? volumeOverride : 1);
                break;
        }
    }

}
