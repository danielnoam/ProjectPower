using System;
using UnityEngine;
using PrimeTween;
using UnityEditor;


namespace DNExtensions.MenuSystem
{
    [Serializable]

    public class AnimatedPageObject
    {
        public RectTransform animatedObject;
        public Vector3 startPosition;
        public Vector3 endPosition;
        [Min(0)] public float duration = 1f;
        public Ease ease = Ease.Default;


        private Sequence _animationSequence;


        public void Animate(float delay = 0)
        {
            if (!animatedObject || !animatedObject.gameObject.activeInHierarchy) return;

            if (_animationSequence.isAlive) _animationSequence.Stop();

            _animationSequence = Sequence.Create(useUnscaledTime: true)
                .Group(Tween.UIAnchoredPosition3D(animatedObject, startPosition, endPosition, duration, ease,
                    startDelay: delay));
        }

        public void Reverse(float delay = 0)
        {
            if (!animatedObject || !animatedObject.gameObject.activeInHierarchy) return;

            if (_animationSequence.isAlive) _animationSequence.Stop();

            _animationSequence = Sequence.Create(useUnscaledTime: true)
                    .Group(Tween.UIAnchoredPosition3D(animatedObject, endPosition, startPosition, duration, ease,
                        startDelay: delay))
                ;
        }
    }
}