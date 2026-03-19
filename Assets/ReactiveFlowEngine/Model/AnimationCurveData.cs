using System.Collections.Generic;
using UnityEngine;

namespace ReactiveFlowEngine.Model
{
    public sealed class AnimationCurveData
    {
        public List<KeyframeData> Keys { get; set; } = new List<KeyframeData>();
        public WrapMode PreWrapMode { get; set; } = WrapMode.ClampForever;
        public WrapMode PostWrapMode { get; set; } = WrapMode.ClampForever;

        public AnimationCurve ToAnimationCurve()
        {
            var keyframes = new Keyframe[Keys.Count];
            for (int i = 0; i < Keys.Count; i++)
            {
                var k = Keys[i];
                var keyframe = new Keyframe(k.Time, k.Value, k.InTangent, k.OutTangent, k.InWeight, k.OutWeight);
                keyframe.weightedMode = (WeightedMode)k.WeightedMode;
                keyframes[i] = keyframe;
            }
            var curve = new AnimationCurve(keyframes);
            curve.preWrapMode = PreWrapMode;
            curve.postWrapMode = PostWrapMode;
            return curve;
        }
    }

    public sealed class KeyframeData
    {
        public float Time { get; set; }
        public float Value { get; set; }
        public float InTangent { get; set; }
        public float OutTangent { get; set; }
        public float InWeight { get; set; }
        public float OutWeight { get; set; }
        public int WeightedMode { get; set; }
    }
}
