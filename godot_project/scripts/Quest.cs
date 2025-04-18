using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost
{
    public enum QuestStatus
    {
        Available,
        Active,
        Completed,
        Failed
    }

    public class Quest
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<QuestObjective> Objectives { get; set; } = new List<QuestObjective>();
        public string Reward { get; set; }
        public QuestStatus Status { get; set; } = QuestStatus.Available;
    }

    public class QuestObjective
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; } = false;
        public int CurrentAmount { get; set; } = 0;
        public int RequiredAmount { get; set; } = 1;
    }
}
