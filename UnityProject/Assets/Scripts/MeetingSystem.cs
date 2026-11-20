using System;
using System.Collections.Generic;
using UnityEngine;

namespace CompanyCEO.UnityRemake
{
    [Serializable]
    public sealed class OfficeMeeting
    {
        public string subject;
        public float startMinute;
        public float duration = 30f;
        public List<string> participantIds = new();
        public bool active;
        public bool completed;
    }

    public sealed class MeetingSystem : MonoBehaviour
    {
        [SerializeField] private List<OfficeMeeting> meetings = new();
        public IReadOnlyList<OfficeMeeting> Meetings => meetings;
        public event Action<OfficeMeeting> MeetingStarted;
        public event Action<OfficeMeeting> MeetingFinished;

        private void Start()
        {
            meetings.Add(new OfficeMeeting { subject = "Haftalık finans ve operasyon toplantısı", startMinute = 9f * 60f + 30f, duration = 30f, participantIds = new List<string> { "ceo", "cfo", "coo" } });
        }

        private void Update()
        {
            var now = CompanyTimeSystem.Instance?.CompanyMinutes ?? 0f;
            foreach (var meeting in meetings)
            {
                if (!meeting.active && !meeting.completed && now >= meeting.startMinute)
                {
                    meeting.active = true;
                    MeetingStarted?.Invoke(meeting);
                }
                if (meeting.active && now >= meeting.startMinute + meeting.duration)
                {
                    meeting.active = false;
                    meeting.completed = true;
                    MeetingFinished?.Invoke(meeting);
                }
            }
        }

        public void Schedule(string subject, float startMinute, float duration, params string[] participantIds)
        {
            meetings.Add(new OfficeMeeting { subject = subject, startMinute = startMinute, duration = duration, participantIds = new List<string>(participantIds) });
        }
    }
}
