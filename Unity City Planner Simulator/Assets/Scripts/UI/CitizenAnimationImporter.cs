using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using UnityEditor.Animations;

public class CitizenAnimationImporter : EditorWindow
{
    [MenuItem("Tools/Citizen Animation Batch Importer")]
    public static void ShowWindow()
    {
        GetWindow<CitizenAnimationImporter>("Citizen Animation Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Citizen Animation Importer with Transitions", EditorStyles.boldLabel);

        if (GUILayout.Button("Import Citizen Animations"))
        {
            ImportCitizenAnimations();
        }
    }

    private void ImportCitizenAnimations()
    {
        // Path to the folder containing all citizen tilesheets
        string basePath = EditorUtility.OpenFolderPanel("Select Tilesheets Folder", "", "");

        if (string.IsNullOrEmpty(basePath)) return;

        // Create folders for prefabs and animations
        string prefabFolderPath = Path.Combine(Application.dataPath, "Prefabs/Citizens");
        string animationFolderPath = Path.Combine(Application.dataPath, "Animations/Citizens");
        Directory.CreateDirectory(prefabFolderPath);
        Directory.CreateDirectory(animationFolderPath);

        // Get all tilesheet files
        string[] tilesheetFiles = Directory.GetFiles(basePath, "*.png")
            .Where(f => Path.GetFileNameWithoutExtension(f).All(char.IsDigit))
            .OrderBy(f => f)
            .ToArray();

        // Process each tilesheet
        foreach (string tilesheetPath in tilesheetFiles)
        {
            string tilesheetName = Path.GetFileNameWithoutExtension(tilesheetPath);

            // Import the specific tilesheet
            string relativePath = tilesheetPath.Substring(Application.dataPath.Length - 6);

            // Force Unity to reimport the sprite with the correct settings
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(relativePath);
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;

                // Specify sprite sheet settings
                importer.spritesheet = new SpriteMetaData[]
                {
                    CreateSpriteMetaData(0, "Down_0"),
                    CreateSpriteMetaData(1, "Down_1"),
                    CreateSpriteMetaData(2, "Down_2"),
                    CreateSpriteMetaData(3, "Left_0"),
                    CreateSpriteMetaData(4, "Left_1"),
                    CreateSpriteMetaData(5, "Left_2"),
                    CreateSpriteMetaData(6, "Right_0"),
                    CreateSpriteMetaData(7, "Right_1"),
                    CreateSpriteMetaData(8, "Right_2"),
                    CreateSpriteMetaData(9, "Up_0"),
                    CreateSpriteMetaData(10, "Up_1"),
                    CreateSpriteMetaData(11, "Up_2")
                };

                importer.SaveAndReimport();
            }

            // Reload sprites after import
            AssetDatabase.Refresh();
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(relativePath)
                .OfType<Sprite>()
                .ToArray();

            // Verify we have enough sprites
            if (sprites.Length < 12) // 3 sprites * 4 directions
            {
                Debug.LogWarning($"Insufficient sprites in tilesheet {tilesheetName}");
                continue;
            }

            // Create a new game object for the citizen
            GameObject citizenPrefab = new GameObject($"Citizen_{tilesheetName}");

            // Add Sprite Renderer and Animator components
            SpriteRenderer spriteRenderer = citizenPrefab.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprites[0]; // Set default sprite
            Animator animator = citizenPrefab.AddComponent<Animator>();

            // Create animation clips (order: down, left, right, up)
            AnimationClip walkDownClip = CreateAnimationClip(sprites.Skip(0).Take(3).ToArray(), $"{tilesheetName}_Down");
            AnimationClip walkLeftClip = CreateAnimationClip(sprites.Skip(3).Take(3).ToArray(), $"{tilesheetName}_Left");
            AnimationClip walkRightClip = CreateAnimationClip(sprites.Skip(6).Take(3).ToArray(), $"{tilesheetName}_Right");
            AnimationClip walkUpClip = CreateAnimationClip(sprites.Skip(9).Take(3).ToArray(), $"{tilesheetName}_Up");

            // Create an Animator Controller with transitions
            RuntimeAnimatorController animatorController = CreateAnimatorControllerWithTransitions(
                walkDownClip, walkLeftClip, walkRightClip, walkUpClip);

            animator.runtimeAnimatorController = animatorController;

            // Save the prefab
            string prefabPath = Path.Combine(prefabFolderPath, $"Citizen_{tilesheetName}.prefab");
            PrefabUtility.SaveAsPrefabAsset(citizenPrefab, prefabPath);

            // Clean up
            DestroyImmediate(citizenPrefab);
        }

        AssetDatabase.Refresh();
        Debug.Log($"Imported animations for {tilesheetFiles.Length} citizens!");
    }

    private SpriteMetaData CreateSpriteMetaData(int index, string name)
    {
        return new SpriteMetaData
        {
            name = name,
            rect = new Rect(index * 32, 0, 32, 32), // Adjust size as needed
            alignment = 0,
            pivot = new Vector2(0.5f, 0.5f)
        };
    }

    private AnimationClip CreateAnimationClip(Sprite[] directionSprites, string clipName)
    {
        // Create animation clip
        AnimationClip clip = new AnimationClip();
        clip.name = $"Walk_{clipName}";

        // Create object curve for sprite animation
        EditorCurveBinding spriteBinding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        // Prepare keyframes
        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[directionSprites.Length];
        for (int i = 0; i < directionSprites.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i * 0.2f,  // 5 frames per second
                value = directionSprites[i]
            };
        }

        // Set animation properties
        clip.wrapMode = WrapMode.Loop;
        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

        // Save the animation clip
        string clipPath = Path.Combine(Application.dataPath, $"Animations/Citizens/{clip.name}.anim");
        AssetDatabase.CreateAsset(clip, clipPath.Substring(Application.dataPath.Length - 6));

        return clip;
    }

    private RuntimeAnimatorController CreateAnimatorControllerWithTransitions(
        AnimationClip walkDown, AnimationClip walkLeft,
        AnimationClip walkRight, AnimationClip walkUp)
    {
        // Create unique animator controller for each citizen
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(
            $"Assets/Animations/Citizens/Citizens_{walkDown.name}_AnimatorController.controller");

        // Create parameters with explicit type
        controller.AddParameter("IsMovingLeft", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsMovingRight", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsMovingUp", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsMovingDown", AnimatorControllerParameterType.Bool);

        // Get root state machine
        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

        // Create states for each direction
        AnimatorState downState = stateMachine.AddState("Walk Down");
        AnimatorState leftState = stateMachine.AddState("Walk Left");
        AnimatorState rightState = stateMachine.AddState("Walk Right");
        AnimatorState upState = stateMachine.AddState("Walk Up");

        // Assign animation clips to states
        downState.motion = walkDown;
        leftState.motion = walkLeft;
        rightState.motion = walkRight;
        upState.motion = walkUp;

        // Create transitions based on boolean parameters
        AddTransition(stateMachine, downState, leftState, "IsMovingLeft", true);
        AddTransition(stateMachine, downState, rightState, "IsMovingRight", true);
        AddTransition(stateMachine, downState, upState, "IsMovingUp", true);

        AddTransition(stateMachine, leftState, downState, "IsMovingDown", true);
        AddTransition(stateMachine, leftState, upState, "IsMovingUp", true);
        AddTransition(stateMachine, leftState, rightState, "IsMovingRight", true);

        AddTransition(stateMachine, rightState, downState, "IsMovingDown", true);
        AddTransition(stateMachine, rightState, upState, "IsMovingUp", true);
        AddTransition(stateMachine, rightState, leftState, "IsMovingLeft", true);

        AddTransition(stateMachine, upState, downState, "IsMovingDown", true);
        AddTransition(stateMachine, upState, leftState, "IsMovingLeft", true);
        AddTransition(stateMachine, upState, rightState, "IsMovingRight", true);

        return controller;
    }

    private void AddTransition(AnimatorStateMachine stateMachine, AnimatorState fromState, AnimatorState toState, string paramName, bool paramValue)
    {
        AnimatorStateTransition transition = fromState.AddTransition(toState);
        transition.AddCondition(AnimatorConditionMode.If, paramValue ? 1 : 0, paramName);
        transition.duration = 0.1f; // Quick transition between states
        transition.exitTime = 0f; // Transition immediately
        transition.hasExitTime = false; // No exit time, transition based on parameter
    }
}