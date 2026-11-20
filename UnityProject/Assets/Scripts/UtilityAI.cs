using System;
using UnityEngine;

namespace CompanyCEO.UnityRemake
{
    [Serializable]
    public sealed class EmployeeNeeds
    {
        [Range(0, 100)] public float energy = 78f;
        [Range(0, 100)] public float hunger = 24f;
        [Range(0, 100)] public float social = 42f;
        [Range(0, 100)] public float motivation = 72f;
        [Range(0, 100)] public float stress = 28f;
        [Range(0, 100)] public float workload = 34f;
        [Range(0, 100)] public float restNeed = 18f;
        [Range(0, 100)] public float jobSatisfaction = 74f;

        public void Tick(float delta, bool working)
        {
            energy = Mathf.Clamp(energy + (working ? -.018f : .008f) * delta, 0, 100);
            hunger = Mathf.Clamp(hunger + .012f * delta, 0, 100);
            social = Mathf.Clamp(social - .006f * delta, 0, 100);
            workload = Mathf.Clamp(workload + (working ? .012f : -.008f) * delta, 0, 100);
            stress = Mathf.Clamp(stress + (working ? .008f : -.014f) * delta, 0, 100);
            restNeed = Mathf.Clamp(restNeed + (energy < 30 ? .015f : -.006f) * delta, 0, 100);
            jobSatisfaction = Mathf.Clamp(jobSatisfaction + (stress > 80 ? -.01f : .002f) * delta, 0, 100);
        }
    }

    public enum UtilityActivity { Work, Socialize, Drink, Eat, Meeting, Phone, Rest }

    public static class UtilityAIDecision
    {
        public static UtilityActivity Choose(EmployeeNeeds n, string role, int seed)
        {
            var work = 35f + n.motivation * .35f - n.workload * .2f;
            var social = n.social * .95f + n.stress * .18f;
            var drink = n.hunger * .55f + n.stress * .12f;
            var eat = n.hunger * 1.2f;
            var meeting = role is "CEO" or "CFO" or "COO" ? 44f + (seed % 7) : 22f;
            var phone = role is "Secretary" or "Sales" ? 58f : 18f;
            var rest = n.restNeed * 1.4f + (100f - n.energy) * .7f;
            var best = UtilityActivity.Work;
            var score = work;
            void Test(UtilityActivity activity, float candidate) { if (candidate > score) { score = candidate; best = activity; } }
            Test(UtilityActivity.Socialize, social); Test(UtilityActivity.Drink, drink); Test(UtilityActivity.Eat, eat);
            Test(UtilityActivity.Meeting, meeting); Test(UtilityActivity.Phone, phone); Test(UtilityActivity.Rest, rest);
            return best;
        }
    }
}
