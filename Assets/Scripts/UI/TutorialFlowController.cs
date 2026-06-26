using System;
using System.Collections.Generic;

public sealed class TutorialFlowController
{
    private readonly TutorialManager tutorialManager;
    private readonly Action showBattle;
    private readonly Action showGrowth;
    private readonly Action refreshTutorial;

    public TutorialFlowController(
        TutorialManager tutorialManager,
        Action showBattle,
        Action showGrowth,
        Action refreshTutorial)
    {
        this.tutorialManager = tutorialManager;
        this.showBattle = showBattle;
        this.showGrowth = showGrowth;
        this.refreshTutorial = refreshTutorial;
    }

    public void HandleTutorialAction()
    {
        if (tutorialManager == null)
            return;

        switch (tutorialManager.CurrentStep)
        {
            case 0:
                tutorialManager.BeginTutorial();
                showBattle?.Invoke();
                break;
            case 1:
                showBattle?.Invoke();
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

        IReadOnlyList<StoryIntroCut> cuts = tutorialManager.StoryCuts;
        bool isLastCut =
            cuts.Count == 0 ||
            tutorialManager.CurrentStoryCutIndex >= cuts.Count - 1;

        tutorialManager.AdvanceStoryIntro();

        if (isLastCut)
        {
            tutorialManager.BeginTutorial();
            showBattle?.Invoke();
        }
    }

    public void HandleStoryIntroSkip()
    {
        if (tutorialManager == null)
            return;

        tutorialManager.SkipStoryIntro();
        refreshTutorial?.Invoke();
    }
}
