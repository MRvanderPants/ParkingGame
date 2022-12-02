using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Type declarations
public enum GoalType {
    CaptureTarget,
    Stealth,
    Ticket,
    CarMoving,
    HyperMode,
};

/**
 * Mission dataset used by the level to create and setup a mission on runtime
 */
[System.Serializable]
public class GoalData {
    public GoalType goalType;
    public int value;
    public float timeLimit;
    public GameObject targetPrefab;
    public Color32 targetColour;
}

/**
 * The base mission settings for each mission. Should not be mutated!
 * Set in the inspector.
 */
[System.Serializable]
public class BaseMissionSettings {
    [Header("General")]
    public string name;
    public string description;
    public GoalType goalType;
    public Sprite icon;

    [Header("Settings")]

    [Tooltip("The minimal level the player should be before this becomes available")]
    public int minLevel;

    [Tooltip("The base amount of time the player gets for this mission")]
    public float baseDuration;

    [Tooltip("Whether or not this mission type will be used in the random circulation.")]
    public bool isRandomized = true;
}
#endregion

public class MissionController : MonoBehaviour {

    public float missionCountMultiplier = 0.4f;
    public BaseMissionSettings[] baseMissionSettings;

    private readonly List<GoalData> missions = new List<GoalData>();

    public static MissionController main;

    public GoalData CurrentGoalData {
        get {
            if (this.missions.Count <= 0) {
                return null;
            }
            return this.missions[0];
        }
    }

    void Awake() {
        MissionController.main = this;
    }

    public List<GoalData> GenerateMissions(LevelData levelData) {
        this.missions.AddRange(this.GenerateGoalData(levelData));
        return this.missions;
    }

    public bool EndMission() {
        this.missions.RemoveAt(0);
        return this.missions.Count <= 0;
    }

    public BaseMissionSettings GetMissionSettingsForType(GoalType goalType) {
        for (int i = 0; i < this.baseMissionSettings.Length; i++) {
            if (this.baseMissionSettings[i].goalType == goalType) {
                return this.baseMissionSettings[i];
            }
        }
        return null;
    }

    public LevelData UpdateLevelData(LevelData levelData, int levelIndex) {
        levelData.levelIndex = levelIndex;
        levelData.missionCount = this.GetMissionCountForLevelIndex(levelIndex);
        levelData.availableTypes = this.GetGoalTypesForLevelIndex(levelIndex);
        return levelData;
    }

    public GoalType[] GetGoalTypesForLevelIndex(int levelIndex) {
        List<GoalType> types = new List<GoalType>();
        for (int i = 0; i < this.baseMissionSettings.Length; i++) {
            if (this.baseMissionSettings[i].isRandomized && this.baseMissionSettings[i].minLevel <= levelIndex) {
                types.Add(this.baseMissionSettings[i].goalType);
            }
        }
        return types.ToArray();
    }

    public int GetMissionCountForLevelIndex(int levelIndex) {
        int missionCount = Mathf.RoundToInt(levelIndex * this.missionCountMultiplier);
        return Mathf.Max(1, missionCount);
    }

    private GoalType GenerateGoalType(LevelData levelData) {
        int r = UnityEngine.Random.Range(0, levelData.availableTypes.Length);
        return levelData.availableTypes[r];
    }

    private Color32 GenerateColor() {
        byte r = (byte)UnityEngine.Random.Range(0, 255);
        byte g = (byte)UnityEngine.Random.Range(0, 255);
        byte b = (byte)UnityEngine.Random.Range(0, 255);
        return new Color32(r, g, b, 255);
    }

    private List<GoalData> GenerateGoalData(LevelData levelData) {
        List<GoalData> goalList = new List<GoalData>();
        for (int i = 0; i < levelData.missionCount; i++) {
            GoalType goalType = this.GenerateGoalType(levelData);
            BaseMissionSettings settings = this.GetMissionSettingsForType(goalType);
            float timeLimit = this.CalculateTimeLimit(settings, levelData);
            goalList.Add(new GoalData() {
                goalType = goalType,
                value = 0,
                timeLimit = timeLimit,
                targetPrefab = Resources.Load<GameObject>("Prefabs/Car"),
                targetColour = this.GenerateColor(),
            });
        }
        return goalList;
    }

    private float CalculateTimeLimit(BaseMissionSettings settings, LevelData levelData) {
        return Mathf.Max(
            settings.baseDuration * (1f - (0.05f * levelData.levelIndex)),
            10f
        );
    }
}
