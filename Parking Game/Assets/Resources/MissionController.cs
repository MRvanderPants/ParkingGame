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
    [Tooltip("If set, overwrites the default music for the duration of the mission")]
    public AudioClip overwriteMusic;

    [Header("Generation Settings")]

    [Tooltip("The minimal level the player should be before this becomes available")]
    public int minLevel;

    [Tooltip("Whether or not this mission type will be used in the random circulation.")]
    public bool isRandomized = true;

    [Tooltip("Whether or not this minigame can appear multiple times in a row")]
    public bool notInARow = false;

    [Header("Gameplay Settings")]

    [Tooltip("The base amount of time the player gets for this mission")]
    public float baseDuration;

    [Tooltip("Check this to ignore the automated mission shortening")]
    public bool forceBaseDuration;

    [Tooltip("Whether the player should be unable to move once a car is captured")]
    public bool lockPlayerDuringCapture = true;

    [Tooltip("Whether or not the score needs to be multiplied in this mode")]
    public bool scoreMultiplierActive = false;
}
#endregion

public class MissionController : MonoBehaviour {

    public float missionCountMultiplier = 0.4f;
    public BaseMissionSettings[] baseMissionSettings;

    private GoalType lastGoalType = GoalType.CaptureTarget;

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

    public LevelData ResetLevelData(LevelData levelData, int levelIndex) {
        LevelData lvlData = MissionController.main.UpdateLevelData(levelData, levelIndex);
        this.ResetMissions();
        this.GenerateMissions(lvlData);
        return lvlData;
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

    public BaseMissionSettings GetCurrentMissionSettings() {
        return this.GetMissionSettingsForType(this.CurrentGoalData.goalType);
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
            if (this.baseMissionSettings[i].isRandomized
                && this.baseMissionSettings[i].minLevel <= levelIndex
                && (!this.baseMissionSettings[i].notInARow || lastGoalType != this.baseMissionSettings[i].goalType)) {
                types.Add(this.baseMissionSettings[i].goalType);
            }
        }
        return types.ToArray();
    }

    public int GetMissionCountForLevelIndex(int levelIndex) {
        int missionCount = Mathf.RoundToInt(levelIndex * this.missionCountMultiplier);
        return Mathf.Max(1, missionCount);
    }

    private void ResetMissions() {
        this.missions.Clear();
        this.lastGoalType = GoalType.CaptureTarget;
    }

    private List<GoalData> GenerateMissions(LevelData levelData) {
        this.missions.AddRange(this.GenerateGoalData(levelData));
        return this.missions;
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
            this.lastGoalType = goalType;
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

        // Check to see if we need to add hyper mode
        BaseMissionSettings hyperModeSettings = this.GetMissionSettingsForType(GoalType.HyperMode);
        if (levelData.levelIndex >= hyperModeSettings.minLevel) {
            float hyperTimeLimit = this.CalculateTimeLimit(hyperModeSettings, levelData);
            goalList.Add(new GoalData() {
                goalType = GoalType.HyperMode,
                value = 0,
                timeLimit = hyperTimeLimit,
                targetPrefab = Resources.Load<GameObject>("Prefabs/Car"),
                targetColour = this.GenerateColor(),
            });
        }
        return goalList;
    }

    private float CalculateTimeLimit(BaseMissionSettings settings, LevelData levelData) {
        if (settings.forceBaseDuration) {
            return settings.baseDuration;
        }

        return Mathf.Max(
            settings.baseDuration * (1f - (0.05f * levelData.levelIndex)),
            10f
        );
    }
}
