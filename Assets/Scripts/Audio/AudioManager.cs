using System;
using System.Collections;
using UnityEngine;
using MusicOM.Core;
using MusicOM.Infrastructure.Logging;

namespace MusicOM.Audio
{
    public enum PlaybackState
    {
        Stopped,
        Playing,
        Paused,
        Loading
    }

    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;

        [Header("Spatial Audio Settings")]
        [SerializeField] private float _spatialBlend = 1f;
        [SerializeField] private float _minDistance = 1f;
        [SerializeField] private float _maxDistance = 50f;
        [SerializeField] private AudioRolloffMode _rolloffMode = AudioRolloffMode.Logarithmic;

        private IAppLogger _logger;
        private PlaybackState _state = PlaybackState.Stopped;
        private float _volume = 1f;
        private bool _isMuted;

        public event Action<PlaybackState> OnStateChanged;
        public event Action<float> OnProgressChanged;
        public event Action OnTrackEnded;

        public PlaybackState State => _state;
        public float Volume => _volume;
        public bool IsMuted => _isMuted;
        public bool IsPlaying => _state == PlaybackState.Playing;
        public float CurrentTime => _musicSource != null ? _musicSource.time : 0f;
        public float Duration => _musicSource != null && _musicSource.clip != null ? _musicSource.clip.length : 0f;
        public float Progress => Duration > 0 ? CurrentTime / Duration : 0f;

        private void Awake()
        {
            if (_musicSource == null)
            {
                _musicSource = gameObject.AddComponent<AudioSource>();
                ConfigureSpatialAudio(_musicSource);
            }
        }

        private void Start()
        {
            _logger = ServiceLocator.Get<IAppLogger>();
            ServiceLocator.Register(this);

            _logger?.Log("[AudioManager] Initialized");
        }

        private void Update()
        {
            if (_state == PlaybackState.Playing)
            {
                OnProgressChanged?.Invoke(Progress);

                if (_musicSource != null && !_musicSource.isPlaying && _musicSource.clip != null)
                {
                    SetState(PlaybackState.Stopped);
                    OnTrackEnded?.Invoke();
                }
            }
        }

        public void Play(AudioClip clip)
        {
            if (clip == null)
            {
                _logger?.LogWarning("[AudioManager] Cannot play null clip");
                return;
            }

            _musicSource.clip = clip;
            _musicSource.Play();
            SetState(PlaybackState.Playing);

            _logger?.Log($"[AudioManager] Playing: {clip.name}");
        }

        public void Play()
        {
            if (_musicSource.clip == null)
            {
                _logger?.LogWarning("[AudioManager] No clip loaded");
                return;
            }

            if (_state == PlaybackState.Paused)
            {
                _musicSource.UnPause();
            }
            else
            {
                _musicSource.Play();
            }

            SetState(PlaybackState.Playing);
        }

        public void Pause()
        {
            if (_state != PlaybackState.Playing) return;

            _musicSource.Pause();
            SetState(PlaybackState.Paused);

            _logger?.Log("[AudioManager] Paused");
        }

        public void Stop()
        {
            _musicSource.Stop();
            _musicSource.time = 0;
            SetState(PlaybackState.Stopped);

            _logger?.Log("[AudioManager] Stopped");
        }

        public void TogglePlayPause()
        {
            if (_state == PlaybackState.Playing)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        public void Seek(float normalizedTime)
        {
            if (_musicSource.clip == null) return;

            normalizedTime = Mathf.Clamp01(normalizedTime);
            _musicSource.time = normalizedTime * _musicSource.clip.length;

            _logger?.Log($"[AudioManager] Seeked to {normalizedTime:P0}");
        }

        public void SeekForward(float seconds = 10f)
        {
            if (_musicSource.clip == null) return;

            var newTime = Mathf.Min(_musicSource.time + seconds, _musicSource.clip.length);
            _musicSource.time = newTime;
        }

        public void SeekBackward(float seconds = 10f)
        {
            if (_musicSource.clip == null) return;

            var newTime = Mathf.Max(_musicSource.time - seconds, 0);
            _musicSource.time = newTime;
        }

        public void SetVolume(float volume)
        {
            _volume = Mathf.Clamp01(volume);
            ApplyVolume();

            _logger?.Log($"[AudioManager] Volume: {_volume:P0}");
        }

        public void Mute()
        {
            _isMuted = true;
            ApplyVolume();
        }

        public void Unmute()
        {
            _isMuted = false;
            ApplyVolume();
        }

        public void ToggleMute()
        {
            _isMuted = !_isMuted;
            ApplyVolume();
        }

        private void ApplyVolume()
        {
            if (_musicSource != null)
            {
                _musicSource.volume = _isMuted ? 0f : _volume;
            }
        }

        public AudioSource CreateSpatialSource(Vector3 position, Transform parent = null)
        {
            var go = new GameObject("SpatialAudioSource");
            go.transform.position = position;

            if (parent != null)
            {
                go.transform.SetParent(parent);
            }

            var source = go.AddComponent<AudioSource>();
            ConfigureSpatialAudio(source);

            return source;
        }

        private void ConfigureSpatialAudio(AudioSource source)
        {
            source.spatialBlend = _spatialBlend;
            source.minDistance = _minDistance;
            source.maxDistance = _maxDistance;
            source.rolloffMode = _rolloffMode;
            source.spatialize = true;
            source.playOnAwake = false;
        }

        public void SetMusicSourcePosition(Vector3 position)
        {
            if (_musicSource != null)
            {
                _musicSource.transform.position = position;
            }
        }

        private void SetState(PlaybackState newState)
        {
            if (_state != newState)
            {
                _state = newState;
                OnStateChanged?.Invoke(_state);
            }
        }

        public IEnumerator LoadAudioFromUrl(string url, Action<AudioClip> onComplete)
        {
            SetState(PlaybackState.Loading);
            _logger?.Log($"[AudioManager] Loading audio from: {url}");

            using var www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
            yield return www.SendWebRequest();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                var clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                _logger?.Log("[AudioManager] Audio loaded successfully");
                SetState(PlaybackState.Stopped);
                onComplete?.Invoke(clip);
            }
            else
            {
                _logger?.LogError($"[AudioManager] Failed to load audio: {www.error}");
                SetState(PlaybackState.Stopped);
                onComplete?.Invoke(null);
            }
        }

        private void OnDestroy()
        {
            if (ServiceLocator.IsRegistered<AudioManager>())
            {
                ServiceLocator.Unregister<AudioManager>();
            }
        }
    }
}
