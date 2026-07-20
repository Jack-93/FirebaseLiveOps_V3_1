using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class BattleEnemyAnimatorAssetGenerator
{
    private const string AnimationRoot =
        "Assets/Resources/Battle/Enemies/Animations/";
    private const string OutputRoot =
        "Assets/Resources/Battle/Enemies/";
    private const float ClipSamples = 60f;

    static BattleEnemyAnimatorAssetGenerator()
    {
        EditorApplication.delayCall += EnsureAssets;
    }

    [MenuItem("Tools/Battle/Rebuild Enemy Animator Assets")]
    public static void RebuildAll()
    {
        EnsureAssets();
    }

    private static void EnsureAssets()
    {
        EnsureAssets("CatMelee_1");
        EnsureAssets("CatMage_1");
        EnsureAssets("CatDash_1");
        EnsureBossAssets("BossCatCerberus");
    }

    private static void EnsureBossAssets(string enemyKey)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        string animationRoot = AnimationRoot + enemyKey;
        string outputRoot = OutputRoot + enemyKey;
        Sprite[] idleFrames = LoadFrames(animationRoot, "Idle");
        Sprite[] leftFrames = LoadFrames(animationRoot, "SkillLeft");
        Sprite[] centerFrames = LoadFrames(animationRoot, "SkillCenter");
        Sprite[] rightFrames = LoadFrames(animationRoot, "SkillRight");
        Sprite[] deathFrames = LoadFrames(animationRoot, "Death");
        if (idleFrames.Length != 8 ||
            leftFrames.Length != 8 ||
            centerFrames.Length != 8 ||
            rightFrames.Length != 8 ||
            deathFrames.Length != 8)
        {
            return;
        }

        EnsureFolder(outputRoot);
        AnimationClip idle = GetOrCreateClip(
            outputRoot,
            enemyKey + "_Idle",
            idleFrames,
            8f,
            true,
            Vector2.one,
            new Vector2(1.018f, 0.985f),
            0f,
            0f);
        AnimationClip skillLeft = GetOrCreateClip(
            outputRoot,
            enemyKey + "_SkillLeft",
            leftFrames,
            12f,
            false,
            Vector2.one,
            new Vector2(1.06f, 0.96f),
            0f,
            -1.5f);
        AnimationClip skillCenter = GetOrCreateClip(
            outputRoot,
            enemyKey + "_SkillCenter",
            centerFrames,
            12f,
            false,
            Vector2.one,
            new Vector2(1.08f, 0.94f),
            0f,
            0f);
        AnimationClip skillRight = GetOrCreateClip(
            outputRoot,
            enemyKey + "_SkillRight",
            rightFrames,
            12f,
            false,
            Vector2.one,
            new Vector2(1.06f, 0.96f),
            0f,
            1.5f);
        AnimationClip death = GetOrCreateClip(
            outputRoot,
            enemyKey + "_Death",
            deathFrames,
            10f,
            false,
            Vector2.one,
            new Vector2(0.97f, 0.97f),
            0f,
            -2f);

        string controllerPath = outputRoot + "/" + enemyKey + ".controller";
        AnimatorController controller =
            AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(
                controllerPath);
            ConfigureBossController(
                controller,
                idle,
                skillLeft,
                skillCenter,
                skillRight,
                death);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void EnsureAssets(string enemyKey)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        string animationRoot = AnimationRoot + enemyKey;
        string outputRoot = OutputRoot + enemyKey;
        string controllerPath = outputRoot + "/" + enemyKey + ".controller";
        Sprite[] idleFrames = LoadFrames(animationRoot, "Idle");
        Sprite[] moveFrames = LoadFrames(animationRoot, "Move");
        Sprite[] attackFrames = LoadFrames(animationRoot, "Attack");
        Sprite[] deathFrames = LoadFrames(animationRoot, "Death");
        if (idleFrames.Length != 8 ||
            moveFrames.Length != 8 ||
            attackFrames.Length != 8 ||
            deathFrames.Length != 8)
        {
            return;
        }

        EnsureFolder(outputRoot);
        AnimationClip idle = GetOrCreateClip(
            outputRoot,
            enemyKey + "_Idle",
            idleFrames,
            8f,
            true,
            new Vector2(1f, 1f),
            new Vector2(1.025f, 0.98f),
            0f,
            0f);
        AnimationClip move = GetOrCreateClip(
            outputRoot,
            enemyKey + "_Move",
            moveFrames,
            12f,
            true,
            new Vector2(1f, 1f),
            new Vector2(1.03f, 0.98f),
            0f,
            -1f);
        AnimationClip attack = GetOrCreateClip(
            outputRoot,
            enemyKey + "_Attack",
            attackFrames,
            12f,
            false,
            new Vector2(1f, 1f),
            new Vector2(1.12f, 0.9f),
            0f,
            7f);
        AnimationClip death = GetOrCreateClip(
            outputRoot,
            enemyKey + "_Death",
            deathFrames,
            10f,
            false,
            new Vector2(1f, 1f),
            new Vector2(0.96f, 0.96f),
            0f,
            -8f);

        AnimatorController controller =
            AssetDatabase.LoadAssetAtPath<AnimatorController>(
                controllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(
                controllerPath);
            ConfigureController(controller, idle, move, attack, death);
        }
        else
        {
            EnsureMoveControllerState(controller, move);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static AnimationClip GetOrCreateClip(
        string outputRoot,
        string name,
        Sprite[] frames,
        float spriteFps,
        bool loop,
        Vector2 startScale,
        Vector2 peakScale,
        float startRotation,
        float peakRotation)
    {
        string path = outputRoot + "/" + name + ".anim";
        AnimationClip existing =
            AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (existing != null)
            return existing;

        AnimationClip clip = CreateClip(
            name,
            frames,
            spriteFps,
            loop,
            startScale,
            peakScale,
            startRotation,
            peakRotation);
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static Sprite[] LoadFrames(string animationRoot, string cue)
    {
        string folder = animationRoot + "/" + cue;
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folder });
        List<Sprite> frames = new List<Sprite>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetDirectoryName(path)?.Replace("\\", "/") != folder)
                continue;

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
                frames.Add(sprite);
        }

        frames.Sort((left, right) =>
            string.CompareOrdinal(left.name, right.name));
        return frames.ToArray();
    }

    private static AnimationClip CreateClip(
        string name,
        Sprite[] frames,
        float spriteFps,
        bool loop,
        Vector2 startScale,
        Vector2 peakScale,
        float startRotation,
        float peakRotation)
    {
        float duration = frames.Length / spriteFps;
        AnimationClip clip = new AnimationClip
        {
            name = name,
            frameRate = ClipSamples
        };

        ObjectReferenceKeyframe[] spriteKeys =
            new ObjectReferenceKeyframe[frames.Length + 1];
        for (int index = 0; index < frames.Length; index++)
        {
            spriteKeys[index] = new ObjectReferenceKeyframe
            {
                time = index / spriteFps,
                value = frames[index]
            };
        }
        spriteKeys[frames.Length] = new ObjectReferenceKeyframe
        {
            time = duration,
            value = loop ? frames[0] : frames[frames.Length - 1]
        };
        AnimationUtility.SetObjectReferenceCurve(
            clip,
            EditorCurveBinding.PPtrCurve(
                string.Empty,
                typeof(Image),
                "m_Sprite"),
            spriteKeys);

        SetTransformCurve(
            clip,
            "m_LocalScale.x",
            startScale.x,
            peakScale.x,
            duration);
        SetTransformCurve(
            clip,
            "m_LocalScale.y",
            startScale.y,
            peakScale.y,
            duration);
        SetTransformCurve(
            clip,
            "localEulerAnglesRaw.z",
            startRotation,
            peakRotation,
            duration);

        AnimationClipSettings settings =
            AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        return clip;
    }

    private static void SetTransformCurve(
        AnimationClip clip,
        string property,
        float start,
        float peak,
        float duration)
    {
        AnimationCurve curve = new AnimationCurve(
            new Keyframe(0f, start),
            new Keyframe(duration * 0.55f, peak),
            new Keyframe(duration, start));
        AnimationUtility.SetEditorCurve(
            clip,
            EditorCurveBinding.FloatCurve(
                string.Empty,
                typeof(RectTransform),
                property),
            curve);
    }

    private static void ConfigureController(
        AnimatorController controller,
        AnimationClip idle,
        AnimationClip move,
        AnimationClip attack,
        AnimationClip death)
    {
        controller.AddParameter("Idle", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Move", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine stateMachine =
            controller.layers[0].stateMachine;
        AnimatorState idleState = stateMachine.AddState("Idle");
        idleState.motion = idle;
        AnimatorState moveState = stateMachine.AddState("Move");
        moveState.motion = move;
        AnimatorState attackState = stateMachine.AddState("Attack");
        attackState.motion = attack;
        AnimatorState deathState = stateMachine.AddState("Death");
        deathState.motion = death;
        stateMachine.defaultState = idleState;

        AddTriggerTransition(stateMachine, idleState, "Idle");
        AddTriggerTransition(stateMachine, moveState, "Move");
        AddTriggerTransition(stateMachine, attackState, "Attack");
        AddTriggerTransition(stateMachine, deathState, "Death");

        AnimatorStateTransition returnToIdle =
            attackState.AddTransition(idleState);
        returnToIdle.hasExitTime = true;
        returnToIdle.exitTime = 1f;
        returnToIdle.duration = 0f;
    }

    private static void ConfigureBossController(
        AnimatorController controller,
        AnimationClip idle,
        AnimationClip skillLeft,
        AnimationClip skillCenter,
        AnimationClip skillRight,
        AnimationClip death)
    {
        string[] triggers =
        {
            "Idle",
            "SkillLeft",
            "SkillCenter",
            "SkillRight",
            "Death"
        };
        foreach (string trigger in triggers)
        {
            controller.AddParameter(
                trigger,
                AnimatorControllerParameterType.Trigger);
        }

        AnimatorStateMachine stateMachine =
            controller.layers[0].stateMachine;
        AnimatorState idleState = AddBossState(
            stateMachine,
            "Idle",
            idle);
        AnimatorState leftState = AddBossState(
            stateMachine,
            "SkillLeft",
            skillLeft);
        AnimatorState centerState = AddBossState(
            stateMachine,
            "SkillCenter",
            skillCenter);
        AnimatorState rightState = AddBossState(
            stateMachine,
            "SkillRight",
            skillRight);
        AddBossState(stateMachine, "Death", death);
        stateMachine.defaultState = idleState;

        AddReturnToIdle(leftState, idleState);
        AddReturnToIdle(centerState, idleState);
        AddReturnToIdle(rightState, idleState);
    }

    private static AnimatorState AddBossState(
        AnimatorStateMachine stateMachine,
        string name,
        AnimationClip clip)
    {
        AnimatorState state = stateMachine.AddState(name);
        state.motion = clip;
        AddTriggerTransition(stateMachine, state, name);
        return state;
    }

    private static void AddReturnToIdle(
        AnimatorState from,
        AnimatorState idle)
    {
        AnimatorStateTransition transition = from.AddTransition(idle);
        transition.hasExitTime = true;
        transition.exitTime = 1f;
        transition.duration = 0f;
    }

    private static void EnsureMoveControllerState(
        AnimatorController controller,
        AnimationClip move)
    {
        if (!HasTrigger(controller, "Move"))
        {
            controller.AddParameter(
                "Move",
                AnimatorControllerParameterType.Trigger);
        }

        AnimatorStateMachine stateMachine =
            controller.layers[0].stateMachine;
        foreach (ChildAnimatorState child in stateMachine.states)
        {
            if (child.state.name != "Move")
                continue;

            if (child.state.motion == null)
                child.state.motion = move;
            return;
        }

        AnimatorState moveState = stateMachine.AddState("Move");
        moveState.motion = move;
        AddTriggerTransition(stateMachine, moveState, "Move");
    }

    private static bool HasTrigger(
        AnimatorController controller,
        string trigger)
    {
        foreach (AnimatorControllerParameter parameter in controller.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Trigger &&
                parameter.name == trigger)
            {
                return true;
            }
        }

        return false;
    }

    private static void AddTriggerTransition(
        AnimatorStateMachine stateMachine,
        AnimatorState target,
        string trigger)
    {
        AnimatorStateTransition transition =
            stateMachine.AddAnyStateTransition(target);
        transition.hasExitTime = false;
        transition.duration = 0f;
        transition.canTransitionToSelf = false;
        transition.AddCondition(
            AnimatorConditionMode.If,
            0f,
            trigger);
    }

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder))
            return;

        string parent = Path.GetDirectoryName(folder)?.Replace("\\", "/");
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
    }
}
