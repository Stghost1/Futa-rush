// ============================================================
// FUTA Rush — EventBus.cs
// Decoupled publish/subscribe event system.
// No tight coupling between systems.
// ============================================================

using System;
using System.Collections.Generic;

namespace FUTARush.Core
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> _events = new();

        public static void Subscribe<T>(Action<T> listener)
        {
            var type = typeof(T);
            if (_events.ContainsKey(type))
                _events[type] = Delegate.Combine(_events[type], listener);
            else
                _events[type] = listener;
        }

        public static void Unsubscribe<T>(Action<T> listener)
        {
            var type = typeof(T);
            if (!_events.ContainsKey(type)) return;
            _events[type] = Delegate.Remove(_events[type], listener);
            if (_events[type] == null) _events.Remove(type);
        }

        public static void Publish<T>(T eventData)
        {
            var type = typeof(T);
            if (_events.TryGetValue(type, out var del))
                (del as Action<T>)?.Invoke(eventData);
        }
    }

    // ── Event Payload Structs ──────────────────────────────────────────────
    public struct GameStartEvent { }

    public struct GameOverEvent
    {
        public int Score;
        public int Tokens;
    }

    public struct TokenCollectedEvent
    {
        public int Amount;
    }

    public struct ScoreChangedEvent
    {
        public int Score;
    }

    public struct SpeedChangedEvent
    {
        public float Speed;
    }
}
