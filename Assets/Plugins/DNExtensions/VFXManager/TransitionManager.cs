using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DNExtensions.VFXManager
{
    public static class TransitionManager
    {
        private static Sequence _activeTransition;
        private static SOVFEffectsSequence _pendingOutSequence;
        private static bool _isInitialized;
        
        static TransitionManager()
        {
            Initialize();
        }
        
        private static void Initialize()
        {
            if (_isInitialized) return;
            
            SceneManager.sceneLoaded += OnSceneLoaded;
            _isInitialized = true;
        }
        
        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!VFXManager.Instance) return;
            
            if (_pendingOutSequence)
            {
                VFXManager.Instance.PlayVFX(_pendingOutSequence);
                _pendingOutSequence = null;
            }
            else
            {
                VFXManager.Instance.ResetActiveEffects();
            }
        }
        
        /// <summary>
        /// Transitions to a new scene with optional visual effects sequences for in and out transitions.
        /// </summary>
        public static void TransitionToScene(string sceneName, SOVFEffectsSequence vfxSequenceIn = null, SOVFEffectsSequence vfxSequenceOut = null)
        {
            if (!VFXManager.Instance)
            {
                SceneManager.LoadScene(sceneName);
                return;
            }
            
            if (_activeTransition.isAlive)
            {
                _activeTransition.Stop();
                VFXManager.Instance.ResetActiveEffects();
            }
            

            var transitionDuration = VFXManager.Instance.PlayVFX(vfxSequenceIn);
            if (vfxSequenceOut) _pendingOutSequence = vfxSequenceOut;

            _activeTransition = Sequence.Create()
                .ChainDelay(transitionDuration)
                .ChainCallback(() =>
                {
                    SceneManager.LoadScene(sceneName);
                });
        }
        
        
        /// <summary>
        /// Transitions to a new scene by index with optional visual effects sequences for in and out transitions.
        /// </summary>
        public static void TransitionToScene(int sceneIndex, SOVFEffectsSequence vfxSequenceIn = null, SOVFEffectsSequence vfxSequenceOut = null)
        {
            if (!VFXManager.Instance)
            {
                SceneManager.LoadScene(sceneIndex);
                return;
            }
            
            if (_activeTransition.isAlive)
            {
                _activeTransition.Stop();
                VFXManager.Instance.ResetActiveEffects();
            }
            
            var transitionDuration = VFXManager.Instance.PlayVFX(vfxSequenceIn);
            if (vfxSequenceOut) _pendingOutSequence = vfxSequenceOut;

            _activeTransition = Sequence.Create()
                .ChainDelay(transitionDuration)
                .ChainCallback(() =>
                {
                    SceneManager.LoadScene(sceneIndex);
                });
        }
        
        /// <summary>
        /// Transitions to a new scene using a SceneField with optional visual effects sequences for in and out transitions.
        /// </summary>
        public static void TransitionToScene(SceneField scene, SOVFEffectsSequence vfxSequenceIn = null, SOVFEffectsSequence vfxSequenceOut = null)
        {
            if (!VFXManager.Instance)
            {
                scene?.LoadScene();
                return;
            }
            
            if (_activeTransition.isAlive)
            {
                _activeTransition.Stop();
                VFXManager.Instance.ResetActiveEffects();
            }
            
            var transitionDuration = VFXManager.Instance.PlayVFX(vfxSequenceIn);
            if (vfxSequenceOut) _pendingOutSequence = vfxSequenceOut;
            

            _activeTransition = Sequence.Create()
                .ChainDelay(transitionDuration)
                .ChainCallback(() =>
                {
                    scene?.LoadScene();
                });
        }
        
        /// <summary>
        /// Plays a transition then quits the application.
        /// </summary>
        public static void TransitionQuit(SOVFEffectsSequence vfxSequenceIn = null) {
    
            if (!VFXManager.Instance)
            {
                Application.Quit();
                return;
            }

            if (_activeTransition.isAlive)
            {
                _activeTransition.Stop();
                VFXManager.Instance.ResetActiveEffects();
            }

            var transitionDuration = VFXManager.Instance.PlayVFX(vfxSequenceIn);


            #if UNITY_EDITOR
            if (Application.isEditor && Application.isPlaying)
            {
                _activeTransition = Sequence.Create()
                    .ChainDelay(transitionDuration)
                    .ChainCallback(() => { UnityEditor.EditorApplication.isPlaying = false; });
                return;
            }
            #endif

            _activeTransition = Sequence.Create()
                .ChainDelay(transitionDuration)
                .ChainCallback(Application.Quit);
        }
    }

}