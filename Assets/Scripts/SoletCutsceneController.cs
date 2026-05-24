using UnityEngine;
using UnityEngine.Playables;

public class SoletCutsceneController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayableDirector director;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Settings")]
    [SerializeField] private bool unlimitedStamina = true;

    private void Start()
    {
        if (playerMovement == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerMovement = playerObj.GetComponent<PlayerMovement>();
        }
    }

    public void OnCutsceneFinished()
    {
        if (QuestManager.instance != null)
            QuestManager.instance.ActivateQuest("quest_run");

        if (unlimitedStamina && playerMovement != null)
        {
            playerMovement.UnlockSprint();
            playerMovement.SetUnlimitedStamina(true);
        }

        if (director != null)
            director.Stop();
    }

    public void PlayCutscene()
    {
        if (director != null)
        {
            Debug.Log("Катсцена Солета запущена");
            director.Play();
        }
    }
}