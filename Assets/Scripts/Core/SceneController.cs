using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MusicOM.Infrastructure.Logging;

namespace MusicOM.Core
{
    public class SceneController : MonoBehaviour
    {
        private readonly Stack<string> _navigationStack = new();
        private string _currentPanel;
        private readonly Dictionary<string, AsyncOperation> _loadedScenes = new();
        private ILogger _logger;

        public event Action<string> OnPanelChanged;
        public event Action<string, float> OnLoadProgress;

        public string CurrentPanel => _currentPanel;
        public bool CanGoBack => _navigationStack.Count > 0;

        private void Start()
        {
            _logger = ServiceLocator.Get<ILogger>();
            _logger?.Log("[SceneController] Initialized");
        }

        public void NavigateTo(string panelName, bool addToHistory = true)
        {
            if (string.IsNullOrEmpty(panelName))
            {
                _logger?.LogWarning("[SceneController] Cannot navigate to empty panel name");
                return;
            }

            if (_currentPanel == panelName)
            {
                _logger?.Log($"[SceneController] Already on panel: {panelName}");
                return;
            }

            if (addToHistory && !string.IsNullOrEmpty(_currentPanel))
            {
                _navigationStack.Push(_currentPanel);
            }

            StartCoroutine(LoadPanelAsync(panelName));
        }

        public void GoBack()
        {
            if (!CanGoBack)
            {
                _logger?.LogWarning("[SceneController] Navigation stack is empty");
                return;
            }

            var previousPanel = _navigationStack.Pop();
            StartCoroutine(LoadPanelAsync(previousPanel, unloadCurrent: true));
        }

        public void ClearHistory()
        {
            _navigationStack.Clear();
            _logger?.Log("[SceneController] Navigation history cleared");
        }

        private IEnumerator LoadPanelAsync(string panelName, bool unloadCurrent = true)
        {
            _logger?.Log($"[SceneController] Loading panel: {panelName}");

            if (unloadCurrent && !string.IsNullOrEmpty(_currentPanel))
            {
                yield return UnloadPanelAsync(_currentPanel);
            }

            var scenePath = $"Scenes/Panels/{panelName}";

            if (!Application.CanStreamedLevelBeLoaded(panelName))
            {
                _logger?.LogWarning($"[SceneController] Scene not found: {scenePath}. Panel may need to be created.");
                yield break;
            }

            var loadOp = SceneManager.LoadSceneAsync(panelName, LoadSceneMode.Additive);
            if (loadOp == null)
            {
                _logger?.LogError($"[SceneController] Failed to load panel: {panelName}");
                yield break;
            }

            loadOp.allowSceneActivation = false;

            while (loadOp.progress < 0.9f)
            {
                OnLoadProgress?.Invoke(panelName, loadOp.progress);
                yield return null;
            }

            loadOp.allowSceneActivation = true;
            yield return loadOp;

            _loadedScenes[panelName] = loadOp;
            _currentPanel = panelName;

            OnPanelChanged?.Invoke(panelName);
            _logger?.Log($"[SceneController] Panel loaded: {panelName}");
        }

        private IEnumerator UnloadPanelAsync(string panelName)
        {
            if (!_loadedScenes.ContainsKey(panelName))
            {
                yield break;
            }

            _logger?.Log($"[SceneController] Unloading panel: {panelName}");

            var scene = SceneManager.GetSceneByName(panelName);
            if (scene.isLoaded)
            {
                var unloadOp = SceneManager.UnloadSceneAsync(scene);
                if (unloadOp != null)
                {
                    yield return unloadOp;
                }
            }

            _loadedScenes.Remove(panelName);
        }

        public void PreloadPanel(string panelName)
        {
            if (_loadedScenes.ContainsKey(panelName))
            {
                return;
            }

            StartCoroutine(PreloadPanelAsync(panelName));
        }

        private IEnumerator PreloadPanelAsync(string panelName)
        {
            if (!Application.CanStreamedLevelBeLoaded(panelName))
            {
                yield break;
            }

            var loadOp = SceneManager.LoadSceneAsync(panelName, LoadSceneMode.Additive);
            if (loadOp == null)
            {
                yield break;
            }

            loadOp.allowSceneActivation = false;

            while (loadOp.progress < 0.9f)
            {
                yield return null;
            }

            _loadedScenes[panelName] = loadOp;
            _logger?.Log($"[SceneController] Panel preloaded: {panelName}");
        }
    }
}
