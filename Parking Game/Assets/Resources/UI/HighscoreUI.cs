using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HighscoreUI : MonoBehaviour {

    public Color32 currentScoreColour;

    private GameObject tableRowPrefab;
    private RectTransform table;
    private TextMeshProUGUI scoreLabel;
    private readonly int numberOfRows = 6;
    private readonly string prefsName = "score_";
    private readonly List<GameObject> rows = new List<GameObject>();

    void Start() {
        this.tableRowPrefab = Resources.Load<GameObject>("Prefabs/TableRow");
        this.scoreLabel = this.transform.Find("Panel").Find("CurrentScoreLabel").GetComponent<TextMeshProUGUI>();
        this.table = this.transform.Find("Panel").Find("Table").Find("Container").GetComponent<RectTransform>();
    }

    public void Activate() {
        this.RemoveRows();
        this.CreateRows();
    }

    public void Close() {
        UIController.main.ToggleHighscores(false);
        UIController.main.ToggleMainMenu(true);
    }

    private void RemoveRows() {
        for (int i = 0; i < this.rows.Count; i++) {
            Destroy(this.rows[i]);
        }
        this.rows.Clear();
    }

    private void CreateRows() {
        int currentScore = LevelController.main.Score;
        this.scoreLabel.text = (currentScore * 100).ToString();
        List<int> highscores = new List<int>();
        for (int i = 0; i < this.numberOfRows; i++) {
            int score = PlayerPrefs.GetInt(this.prefsName + i, 0);
            highscores.Add(score);
        }

        int scoreAdjusted = -1;
        for (int j = 0; j < highscores.Count; j++) {

            // Insert current score into the highscores
            if (scoreAdjusted == -1 && currentScore > highscores[j]) {
                highscores.Insert(j, currentScore);
                scoreAdjusted = j;
            }

            // Create rows if < 6
            if (j < this.numberOfRows) {
                this.CreateRow(highscores[j], scoreAdjusted == j);
            }
        }

        if (scoreAdjusted != -1) {
            this.SubmitScores(highscores);
        }
    }

    private void SubmitScores(List<int> highscores) {
        for (int i = 0; i < this.numberOfRows; i++) {
            int score = i > highscores.Count ? 0 : highscores[i];
            PlayerPrefs.SetInt(this.prefsName + i, score);
        }
    }

    private void CreateRow(int score, bool currentScore) {
        this.table = this.transform.Find("Panel").Find("Table").Find("Container").GetComponent<RectTransform>();
        GameObject row = Instantiate(this.tableRowPrefab, this.table);
        RectTransform rect = row.GetComponent<RectTransform>();
        Vector2 pos = rect.anchoredPosition;
        pos.y = -40f * this.rows.Count;
        rect.anchoredPosition = pos;
        row.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = (score * 100).ToString();
        if(currentScore) {
            UIAlignColourWithButton alignment = row.GetComponent<UIAlignColourWithButton>();
            alignment.defaultColor = this.currentScoreColour;
            alignment.ApplyDefault();
        }
        this.rows.Add(row);
    }
}
