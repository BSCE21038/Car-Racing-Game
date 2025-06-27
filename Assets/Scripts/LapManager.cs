using System.Collections;
using System.Linq;                // ← needed for the LINQ filter
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LapManager : MonoBehaviour
{
    public GameObject finishFX;
    public Transform playerCar;         // assign PlayerCar transform
    public GameObject finishFXPrefab;   // optional second FX
    public AudioSource finishAudioSource;

    private MonoBehaviour[] aiControllers;  // ← changed from RCC_CarAI[]

[Header("Scene Effects")]
public GameObject sparkObject;   // assign the Spark GameObject in the scene here

    [Header("Race Settings")]
    public int totalLaps = 1;

    [Header("UI References")]
    public TextMeshProUGUI lapText;
    public TextMeshProUGUI timerText;
    public GameObject finishPanel;
    public TextMeshProUGUI finalTimeText;
    public TextMeshProUGUI bestTimeText;
    public TextMeshProUGUI resultText;

    [Header("Countdown")]
    public TextMeshProUGUI countdownText;   
    public float countdownDuration = 3f;

    private int currentLap = 0;
    private float startTime;
    private bool raceFinished = false;
    private bool countingDown = true;    // prevent laps/timer until countdown ends

    void Start()
    {
        // Hide finish UI and init displays
        finishPanel.SetActive(false);
        lapText.text = $"Lap: {currentLap}/{totalLaps}";
        timerText.text = "0.00";

  if (sparkObject != null && resultText != null)
        sparkObject.SetActive(false);
        resultText.text = "";
        // Find every MonoBehaviour whose class name contains both "AI" and "Car"
        // (this catches your RCC AI Car Controller script)
        aiControllers = FindObjectsOfType<MonoBehaviour>()
            .Where(mb => mb.GetType().Name.Contains("AI") && mb.GetType().Name.Contains("Car"))
            .ToArray();

        // Disable them until the race starts
        foreach (var ai in aiControllers)
            ai.enabled = false;

        // Begin the 3…2…1 countdown
        StartCoroutine(StartCountdown());
    }

    void Update()
    {
        // Only update timer after countdown and before finish
        if (countingDown || raceFinished) return;
        timerText.text = (Time.time - startTime).ToString("F2");
    }

    void UpdateLapUI()
    {
        lapText.text = $"Lap: {currentLap}/{totalLaps}";
    }

    // Called by your LapTrigger when the player crosses the line
    public void CompleteLap()
    {
        if (countingDown || raceFinished) return;

        currentLap++;
        if (currentLap >= totalLaps)
            FinishRace();
        else
            UpdateLapUI();
    }

    void FinishRace()
    {
        raceFinished = true;
        float finishTime = Time.time - startTime;
        finalTimeText.text = $"Time: {finishTime:F2}";
            finishPanel.SetActive(true);

if (sparkObject != null && resultText != null)
        resultText.text = "You Win!";

    sparkObject.SetActive(true);
        // Best‐time save/load
        float best = PlayerPrefs.GetFloat("BestTime", float.MaxValue);
        if (finishTime < best)
        {
            best = finishTime;
            PlayerPrefs.SetFloat("BestTime", best);
            PlayerPrefs.Save();
        }
        bestTimeText.text = best == float.MaxValue ? "Best: N/A" : $"Best: {best:F2}";

        // Show finish UI
        finishPanel.SetActive(true);

        // Spawn finishFX at the player's position
        if (finishFX != null && playerCar != null)
            Instantiate(finishFX, playerCar.position, Quaternion.identity);

        // Optional: spawn secondary FX at camera
        if (finishFXPrefab != null)
            Instantiate(finishFXPrefab, Camera.main.transform.position, Quaternion.identity);

        // Play finish-line sound
        if (finishAudioSource != null)
            finishAudioSource.Play();
    }

    public void RestartRace()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void LoseRace()
    {
        // If race already ended (win or lose), ignore
        if (raceFinished) return;
        raceFinished = true;

        // Show the finish UI
        finishPanel.SetActive(true);

        // Show lose message
        if (resultText != null)
            resultText.text = "You Lose…";

        // (Optional) play a lose sound or dim the screen here
    }


    private IEnumerator StartCountdown()
    {
        float t = countdownDuration;
        while (t > 0f)
        {
            if (countdownText != null)
                countdownText.text = Mathf.Ceil(t).ToString();
            yield return new WaitForSeconds(1f);
            t -= 1f;
        }

        if (countdownText != null)
            countdownText.text = "Go!";
        yield return new WaitForSeconds(0.5f);
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        // Countdown done: start the race
        countingDown = false;
        startTime = Time.time;

        // Re-enable all AI controllers
        foreach (var ai in aiControllers)
            ai.enabled = true;
    }
}
