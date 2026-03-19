using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReactiveFlowEngine.Abstractions;
using ReactiveFlowEngine.Model;
using UnityEngine;

namespace ReactiveFlowEngine.Behaviors
{
    public class TextToSpeechBehavior : IBehavior
    {
        private readonly string _text;
        private readonly string _language;
        private readonly bool _isBlocking;
        private readonly ExecutionStages _stages;

        public ExecutionStages Stages => _stages;
        public bool IsBlocking => _isBlocking;

        public TextToSpeechBehavior(
            string text,
            string language = "en",
            bool isBlocking = true,
            ExecutionStages stages = ExecutionStages.Activation)
        {
            _text = text;
            _language = language;
            _isBlocking = isBlocking;
            _stages = stages;
        }

        public async UniTask ExecuteAsync(CancellationToken ct)
        {
            Debug.Log($"[RFE-TTS] Speaking: '{_text}' (lang={_language})");
            await UniTask.CompletedTask;
        }
    }
}
