
using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;


namespace DNExtensions.VFXManager
{
    [DisallowMultipleComponent]
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }
    
        [Header("Settings")]
        [Tooltip("If true, the post-processing effects will be automatically set up on Awake.")]
        [SerializeField] private bool autoSetupPostProcessing = true;
        
        [Header("References")]
        [SerializeField] private Volume postProcessingVolume;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image fullScreenImage;
        [SerializeField] private SOVFEffectsSequence[] effectsSequences;
    

        private Sequence _vfxSequence;
        public LensDistortion LensDistortion { get; private set; }
        public ChromaticAberration ChromaticAberration { get; private set; }
        public MotionBlur MotionBlur { get; private set; }
        public Vignette Vignette { get; private set; }
        public Sprite DefaultIconSprite { get; private set; }
        public Sprite DefaultFullScreenSprite { get; private set; }
        public Vector3 DefaultFullScreenPosition { get; private set; }
        public Vector3 DefaultFullScreenScale { get; private set; }
        public Vector3 DefaultFullScreenRotation { get; private set; }
        public Vector3 DefaultIconPosition { get; private set; }
        public Vector3 DefaultIconScale { get; private set; }
        public Vector3 DefaultIconRotation { get; private set; }
        public Color DefaultIconColor { get; private set; }
        public Color DefaultFullScreenColor { get; private set; }
        public float DefaultLensDistortionIntensity { get; private set; }
        public float DefaultLensDistortionXMultiplier { get; private set; }
        public float DefaultLensDistortionYMultiplier { get; private set; }
        public Vector2 DefaultLensDistortionCenter { get; private set; }
        public float DefaultLensDistortionScale { get; private set; }
        public float DefaultChromaticAberrationIntensity { get; private set; }
        public float DefaultMotionBlurIntensity { get; private set; }
        public float DefaultMotionBlurClamp { get; private set; }
        public float DefaultVignetteIntensity { get; private set; }
        public float DefaultVignetteSmoothness { get; private set; }
        public Vector2 DefaultVignetteCenter { get; private set; }
        public bool DefaultVignetteRounded { get; private set; }
        
        public Image FullScreenImage => fullScreenImage;
        public Image IconImage => iconImage;


        private void OnValidate()
        {
            if (effectsSequences == null || effectsSequences.Length == 0) return;
            for (int i = 0; i < effectsSequences.Length; i++)
            {
                for (int j = i + 1; j < effectsSequences.Length; j++)
                {
                    if (effectsSequences[i] == effectsSequences[j])
                    {
                        Debug.LogWarning($"Duplicate SOVFEffectsSequence found: {effectsSequences[i].name}.", this);
                    }
                }
            }
        }


        private void Awake()
        {
            if (Instance && Instance != this) 
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (iconImage)
            {
                DefaultIconSprite = iconImage.sprite;
                DefaultIconColor = iconImage.color;
                DefaultIconPosition = iconImage.rectTransform.localPosition;
                DefaultIconScale = iconImage.rectTransform.localScale;
                DefaultIconRotation = iconImage.rectTransform.localEulerAngles;
                
                iconImage.color = Color.clear;
            }

            if (fullScreenImage)
            {
                DefaultFullScreenSprite = fullScreenImage.sprite;
                DefaultFullScreenColor = fullScreenImage.color;
                DefaultFullScreenPosition = fullScreenImage.rectTransform.localPosition;
                DefaultFullScreenScale = fullScreenImage.rectTransform.localScale;
                DefaultFullScreenRotation = fullScreenImage.rectTransform.localEulerAngles;
                
                fullScreenImage.color = Color.clear;
            }
            
            SetupPostProcessingVolume();
        }

        private void OnEnable()
        {
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        private void OnActiveSceneChanged(Scene currentScene, Scene nextScene)
        {
            SetupPostProcessingVolume();
        }
        

        private void SetupPostProcessingVolume()
        {
            if (!postProcessingVolume) postProcessingVolume = FindAnyObjectByType<Volume>();
            if (!postProcessingVolume)
            {
                Debug.Log("No Post Processing Volume found in the scene!");
            }

            if (postProcessingVolume.profile.TryGet(out Vignette vignette))
            {
                Vignette = vignette;
                DefaultVignetteIntensity = Vignette.intensity.value;
                DefaultVignetteSmoothness = Vignette.smoothness.value;
                DefaultVignetteCenter = Vignette.center.value;
                DefaultVignetteRounded = Vignette.rounded.value;
                if (!autoSetupPostProcessing) return;
                Vignette.active = true;
                Vignette.color.overrideState = true;
                Vignette.center.overrideState = true;
                Vignette.intensity.overrideState = true;
                Vignette.smoothness.overrideState = true;
                Vignette.rounded.overrideState = true;
            }

            if (postProcessingVolume.profile.TryGet(out LensDistortion lensDistortion))
            {
                LensDistortion = lensDistortion;
                DefaultLensDistortionIntensity = LensDistortion.intensity.value;
                DefaultLensDistortionXMultiplier = LensDistortion.xMultiplier.value;
                DefaultLensDistortionYMultiplier = LensDistortion.yMultiplier.value;
                DefaultLensDistortionCenter = LensDistortion.center.value;
                DefaultLensDistortionScale = LensDistortion.scale.value;
                if (!autoSetupPostProcessing) return;
                LensDistortion.active = true;
                LensDistortion.intensity.overrideState = true;
                LensDistortion.center.overrideState = true;
                LensDistortion.scale.overrideState = true;
                LensDistortion.xMultiplier.overrideState = true;
                LensDistortion.yMultiplier.overrideState = true;
            }

            if (postProcessingVolume.profile.TryGet(out ChromaticAberration chromaticAberration))
            {
                ChromaticAberration = chromaticAberration;
                DefaultChromaticAberrationIntensity = ChromaticAberration.intensity.value;
                if (!autoSetupPostProcessing) return;
                ChromaticAberration.active = true;
            }

            if (postProcessingVolume.profile.TryGet(out MotionBlur motionBlur))
            {
                MotionBlur = motionBlur;
                DefaultMotionBlurIntensity = MotionBlur.intensity.value;
                DefaultMotionBlurClamp = MotionBlur.clamp.value;
                if (!autoSetupPostProcessing) return;
                MotionBlur.active = true;
                MotionBlur.intensity.overrideState = true;
                MotionBlur.quality.overrideState = true;
                MotionBlur.clamp.overrideState = true;
            }

        }

        
        /// <summary>
        /// Plays a specific visual effects sequence.
        /// </summary>
        public float PlayVFX(SOVFEffectsSequence vfxSequence)
        {
            if (!vfxSequence) return 0;
            
            if (Application.isPlaying)
            {
                return vfxSequence.PlayEffects();
            }
            
            if (_vfxSequence.isAlive) _vfxSequence.Stop();
            var vfxDuration = vfxSequence.PlayEffects();
            
            _vfxSequence = Sequence.Create()
                .ChainDelay(vfxDuration)
                .ChainCallback(vfxSequence.ResetEffects);

            return vfxDuration;

        }
        
        /// <summary>
        /// Plays a random visual effects sequence from the available sequences.
        /// </summary>
        [Button]
        public float PlayRandomVFX()
        {
            ResetActiveEffects();
            
            var randomVFXIndex = Random.Range(0, effectsSequences.Length);
            var vfxSequence = effectsSequences[randomVFXIndex];
        
        
            if (!vfxSequence) return 0;
            
            if (Application.isPlaying)
            {
                return vfxSequence.PlayEffects();
            }
            
            if (_vfxSequence.isAlive) _vfxSequence.Stop();
            var vfxDuration = vfxSequence.PlayEffects();
            
            _vfxSequence = Sequence.Create()
                .ChainDelay(vfxDuration)
                .ChainCallback(vfxSequence.ResetEffects);

            return vfxDuration;
        }

        /// <summary>
        /// Gets a random sequence from the sequences list.
        /// </summary>
        public SOVFEffectsSequence GetRandomEffect()
        {
            var randomVFXIndex = Random.Range(0, effectsSequences.Length);
            var vfxSequence = effectsSequences[randomVFXIndex];

            return vfxSequence;
        }
        
        /// <summary>
        /// Resets all visual effects to their default state.
        /// </summary>
        [Button]
        public void ResetActiveEffects() 
        {
            if (iconImage)
            {
                iconImage.sprite = DefaultIconSprite;
                iconImage.color = DefaultIconColor;
                iconImage.rectTransform.localPosition = DefaultIconPosition;
                iconImage.rectTransform.localScale = DefaultIconScale;
                iconImage.rectTransform.localEulerAngles = DefaultIconRotation;
            }
            
            if (fullScreenImage)
            {
                fullScreenImage.sprite = DefaultFullScreenSprite;
                fullScreenImage.color = DefaultFullScreenColor;
                fullScreenImage.rectTransform.localPosition = DefaultFullScreenPosition;
                fullScreenImage.rectTransform.localScale = DefaultFullScreenScale;
                fullScreenImage.rectTransform.localEulerAngles = DefaultFullScreenRotation;
            }
            
            if (Vignette)
            {
                Vignette.intensity.value = DefaultVignetteIntensity;
            }
            
            if (LensDistortion)
            {
                LensDistortion.intensity.value = DefaultLensDistortionIntensity;
            }
            
            if (ChromaticAberration)
            {
                ChromaticAberration.intensity.value = DefaultChromaticAberrationIntensity;
            }
            
            if (MotionBlur)
            {
                MotionBlur.intensity.value = DefaultMotionBlurIntensity;
            }
            
            if (_vfxSequence.isAlive) _vfxSequence.Stop();
        }
    }
    

}

