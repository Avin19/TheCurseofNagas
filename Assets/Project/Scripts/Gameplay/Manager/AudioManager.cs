using UnityEngine;
using UnityEngine.Audio;

using CurseOfNaga.Global;

namespace CurseOfNaga.Gameplay.Managers
{
    public class AudioManager : MonoBehaviour
    {
        public enum SFXClip { WALKING_ON_DIRT = 0, WALKING_IN_WATER, METAL_CLANG_0, SWORD_SWOOSH_0 = 1, STRETCH_BOW_0, SHOT_ARROW_0 }

        #region Singleton
        private static AudioManager _instance;
        public static AudioManager Instance { get => _instance; }

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
            else
                Destroy(gameObject);
        }
        #endregion Singleton

        public AudioSettings_SO _mainAudioSettings;
        [SerializeField] private AudioClip[] _sfxClips;
        [SerializeField] private AudioSource _playerSFXSource, _enemySFXSource;
        [SerializeField] private AudioSource _bgmSource;

        [SerializeField] private AudioMixer _masterMixer;

        private const string _MASTER_VOL = "MasterVolume", _BGM_VOL = "BGMVolume",
            _AMBIENT_VOL = "AmbientVolume", _SFX_VOL = "SFXVolume";

        private void Start()
        {
            AdjustAudioLevels();
        }

        public void AdjustAudioLevels()
        {
            _masterMixer.SetFloat(_MASTER_VOL, _mainAudioSettings.MasterVolume);
            _masterMixer.SetFloat(_SFX_VOL, _mainAudioSettings.SFXVolume);
            _masterMixer.SetFloat(_BGM_VOL, _mainAudioSettings.BGMVolume);
            _masterMixer.SetFloat(_AMBIENT_VOL, _mainAudioSettings.AmbientVolume);
        }

        public void PlayPlayerSFXClipOnce(SFXClip clip)
        {
            _playerSFXSource.PlayOneShot(_sfxClips[(int)clip]);
        }

        public void PlayEnemySFXClipOnce(SFXClip clip)
        {
            _enemySFXSource.PlayOneShot(_sfxClips[(int)clip]);
        }

        public void StopPlayerSFX() { _playerSFXSource.Stop(); }
        public void StopEnemySFX() { _enemySFXSource.Stop(); }
    }
}