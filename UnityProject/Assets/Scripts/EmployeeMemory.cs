using System;
using System.Collections.Generic;
using UnityEngine;

namespace CompanyCEO.UnityRemake
{
    [Serializable]
    public sealed class MemoryEvent
    {
        public string type;
        public string description;
        public float importance;
        public float time;
    }

    [Serializable]
    public sealed class SocialRelation
    {
        public string otherId;
        [Range(-100, 100)] public float friendship;
        [Range(-100, 100)] public float trust;
        [Range(-100, 100)] public float respect;
        [Range(-100, 100)] public float rivalry;
        [Range(0, 100)] public float professionalCommitment = 60;
    }

    public sealed class EmployeeMemory : MonoBehaviour
    {
        [SerializeField] private List<MemoryEvent> memories = new();
        [SerializeField] private List<SocialRelation> relations = new();
        public IReadOnlyList<MemoryEvent> Memories => memories;
        public IReadOnlyList<SocialRelation> Relations => relations;

        public void Remember(string type, string description, float importance = 50f)
        {
            memories.Add(new MemoryEvent { type = type, description = description, importance = importance, time = CompanyTimeSystem.Instance?.CompanyMinutes ?? Time.time });
            if (memories.Count > 40) memories.RemoveAt(0);
        }

        public SocialRelation RelationWith(string otherId)
        {
            var relation = relations.Find(x => x.otherId == otherId);
            if (relation != null) return relation;
            relation = new SocialRelation { otherId = otherId };
            relations.Add(relation);
            return relation;
        }

        public void ApplyInteraction(string otherId, float trustDelta, float respectDelta, float friendshipDelta)
        {
            var relation = RelationWith(otherId);
            relation.trust = Mathf.Clamp(relation.trust + trustDelta, -100, 100);
            relation.respect = Mathf.Clamp(relation.respect + respectDelta, -100, 100);
            relation.friendship = Mathf.Clamp(relation.friendship + friendshipDelta, -100, 100);
        }
    }
}
