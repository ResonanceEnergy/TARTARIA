using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Notification system for in-game alerts and UI notifications.
    /// Manages notification queue, auto-dismiss, and event-driven UI integration.
    /// </summary>
    public class NotificationSystem : MonoBehaviour
    {
        static NotificationSystem _instance;
        public static NotificationSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("NotificationSystem");
                    _instance = go.AddComponent<NotificationSystem>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [SerializeField] int maxQueueSize = 20;
        [SerializeField] float defaultDuration = 5f;

        readonly List<Notification> _activeNotifications = new();
        public IReadOnlyList<Notification> ActiveNotifications => _activeNotifications;

        public event Action<Notification> OnNotificationAdded;
        public event Action<Notification> OnNotificationDismissed;

        void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            // Auto-dismiss expired notifications
            for (int i = _activeNotifications.Count - 1; i >= 0; i--)
            {
                var notif = _activeNotifications[i];
                if (Time.time >= notif.expireTime)
                {
                    DismissNotification(notif);
                }
            }
        }

        public void ShowNotification(string message, NotificationType type, float duration = -1f)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            float finalDuration = duration > 0f ? duration : defaultDuration;
            var notif = new Notification
            {
                id = Guid.NewGuid().ToString(),
                message = message,
                type = type,
                timestamp = Time.time,
                expireTime = Time.time + finalDuration
            };

            _activeNotifications.Add(notif);

            // Trim queue if over max size (remove oldest)
            while (_activeNotifications.Count > maxQueueSize)
            {
                var oldest = _activeNotifications[0];
                _activeNotifications.RemoveAt(0);
                Debug.LogWarning($"[NotificationSystem] Queue overflow, dismissed oldest: {oldest.message}");
            }

            Debug.Log($"[Notification|{type}] {message}");
            OnNotificationAdded?.Invoke(notif);
        }

        public void DismissNotification(Notification notif)
        {
            if (_activeNotifications.Remove(notif))
            {
                OnNotificationDismissed?.Invoke(notif);
            }
        }

        public void DismissAll()
        {
            while (_activeNotifications.Count > 0)
            {
                var notif = _activeNotifications[0];
                DismissNotification(notif);
            }
        }

        void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
    }

    [Serializable]
    public class Notification
    {
        public string id;
        public string message;
        public NotificationType type;
        public float timestamp;
        public float expireTime;
    }

    public enum NotificationType
    {
        Info,
        Warning,
        Achievement,
        Quest,
        Combat,
        Discovery
    }
}
