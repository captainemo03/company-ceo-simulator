using System;
using UnityEngine;

namespace CompanyCEO.UnityRemake
{
    public enum SmartObjectAction { Work, Phone, Drink, Eat, Sit, Present }

    public sealed class SmartObject : MonoBehaviour
    {
        [SerializeField] private SmartObjectAction action = SmartObjectAction.Work;
        [SerializeField] private Transform[] interactionPoints;
        [SerializeField] private int capacity = 1;
        private bool[] slots;
        public SmartObjectAction Action => action;
        public int Capacity => capacity;

        public void Configure(SmartObjectAction nextAction, int nextCapacity)
        {
            action = nextAction;
            capacity = Mathf.Max(1, nextCapacity);
            slots = new bool[capacity];
        }

        private void Awake() => slots = new bool[Mathf.Max(1, capacity)];

        public bool TryReserve(out Transform point, out int slot)
        {
            slots ??= new bool[Mathf.Max(1, capacity)];
            for (var i = 0; i < slots.Length; i++)
            {
                if (slots[i]) continue;
                slots[i] = true; slot = i;
                point = interactionPoints != null && interactionPoints.Length > i ? interactionPoints[i] : transform;
                return true;
            }
            point = null; slot = -1; return false;
        }

        public void Release(int slot)
        {
            if (slots == null || slot < 0 || slot >= slots.Length) return;
            slots[slot] = false;
        }
    }
}
