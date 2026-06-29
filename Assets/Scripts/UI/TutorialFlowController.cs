using System;

public sealed class TutorialFlowController
{
    private readonly TutorialManager tutorialManager;
    private readonly Action showBattle;
    private readonly Action showGrowth;
    private readonly Action showGacha;
    private readonly Action refreshTutorial;

    public TutorialFlowController(
        TutorialManager tutorialManager,
        Action showBattle,
        Action showGrowth,
        Action showGacha,
        Action refreshTutorial)
    {
        this.tutorialManager = tutorialManager;
        this.showBattle = showBattle;
        this.showGrowth = showGrowth;
        this.showGacha = showGacha;
        this.refreshTutorial = refreshTutorial;
    }

    public void HandleTutorialAction()
    {
        if (tutorialManager == null)
            return;

        switch (tutorialManager.CurrentStep)
        {
            case 0:
                if (tutorialManager.ShouldShowTutorialTicketGift)
                    tutorialManager.ClaimTutorialGachaTickets();

                showGacha?.Invoke();
                refreshTutorial?.Invoke();
                break;
            case 1:
                break;
            case 2:
                showGrowth?.Invoke();
                break;
            default:
                showBattle?.Invoke();
                break;
        }
    }

    public void HandleStoryIntroNext()
    {
        if (tutorialManager == null ||
            !tutorialManager.ShouldShowStoryIntro)
        {
            return;
        }

        var cuts = tutorialManager.StoryCuts;
        bool isLastCut =
            cuts.Count == 0 ||
            tutorialManager.CurrentStoryCutIndex >= cuts.Count - 1;

        tutorialManager.AdvanceStoryIntro();

        if (isLastCut)
        {
            tutorialManager.BeginTutorial();
            refreshTutorial?.Invoke();
        }
    }

    public void HandleStoryIntroPrevious()
    {
        if (tutorialManager == null ||
            !tutorialManager.ShouldShowStoryIntro)
        {
            return;
        }

        tutorialManager.PreviousStoryIntro();
        refreshTutorial?.Invoke();
    }

}
