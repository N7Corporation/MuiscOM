using UnityEngine;
using MusicOM.Infrastructure.Logging;

namespace MusicOM.Core
{
    public class AppInitializer : MonoBehaviour
    {
        [SerializeField] private AppConfig _appConfig;

        private ILogger _logger;

        private void Awake()
        {
            if (ServiceLocator.IsInitialized)
            {
                Debug.LogWarning("[AppInitializer] Already initialized. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void Initialize()
        {
            Debug.Log("[AppInitializer] Starting initialization...");

            InitializeConfig();
            InitializeLogging();
            InitializeFrameRate();
            RegisterServices();

            ServiceLocator.MarkInitialized();
            _logger.Log("[AppInitializer] Initialization complete");
        }

        private void InitializeConfig()
        {
            if (_appConfig == null)
            {
                _appConfig = Resources.Load<AppConfig>("Config/AppConfig");
                if (_appConfig == null)
                {
                    Debug.LogError("[AppInitializer] AppConfig not found! Using default values.");
                    _appConfig = ScriptableObject.CreateInstance<AppConfig>();
                }
            }

            ServiceLocator.Register(_appConfig);
            Debug.Log($"[AppInitializer] AppConfig loaded: targetFrameRate={_appConfig.targetFrameRate}");
        }

        private void InitializeLogging()
        {
            _logger = new UnityLogger();
            ServiceLocator.Register(_logger);
        }

        private void InitializeFrameRate()
        {
            Application.targetFrameRate = _appConfig.targetFrameRate;
            QualitySettings.vSyncCount = 0;

            Debug.Log($"[AppInitializer] Frame rate set to {_appConfig.targetFrameRate}Hz");
        }

        private void RegisterServices()
        {
            var sceneController = gameObject.AddComponent<SceneController>();
            ServiceLocator.Register(sceneController);
        }

        private void OnDestroy()
        {
            if (ServiceLocator.IsInitialized)
            {
                ServiceLocator.Clear();
            }
        }
    }
}
