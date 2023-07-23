using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

using UMA;
using UMA.CharacterSystem;

using System.IO;
using UnityEngine.AI;

public class Spawner : MonoBehaviour
{
	public DynamicCharacterAvatar avatar;
    public UMARandomizer Randomizer;

	public int count = 10;
	public int range = 15;
	public int posx = 0;
	public int posz = 0;

    private const string ANIMATION_FOLDER = "Assets/Animations/";
    private const string CONTROLLER_FOLDER = "Assets/Controllers/";

    private AnimatorController[] controllers;

	// Default methods

	void Awake()
	{
        if (!Directory.Exists(CONTROLLER_FOLDER))
		{
            Directory.CreateDirectory(CONTROLLER_FOLDER);
        }
    } 
    
    void Start()
    {
        initControllers();
		initAvatars();
    }

	void OnApplicationQuit()
	{
        if (Directory.Exists(CONTROLLER_FOLDER))
        {
            AssetDatabase.DeleteAsset(CONTROLLER_FOLDER);
        }

        AssetDatabase.Refresh();
    }

	void Quit()
    {
        UnityEditor.EditorApplication.isPlaying = false;
		Application.Quit();
    }

    // Controllers and Avatars initialization

    #region Controllers initialization
    private void initControllers()
	{
		List<string> animationsNames = new List<string>();

        if (Directory.Exists(ANIMATION_FOLDER))
		{
            DirectoryInfo animationFolderInfo = new DirectoryInfo(ANIMATION_FOLDER);
            FileInfo[] animations = animationFolderInfo.GetFiles("*.fbx"); //Getting all fbx files

			foreach (FileInfo anim in animations)
			{
                animationsNames.Add(anim.Name);
            }
        }
		else
		{
			Debug.LogError("Animations folder is missing!");
            Quit();
        }

		if (animationsNames.Count > 0)
		{
            controllers = new AnimatorController[animationsNames.Count];

            for (int i = 0; i < animationsNames.Count; i++)
            {
                // Get the animation for the controller
                AnimationClip clip = AssetDatabase.LoadAssetAtPath(ANIMATION_FOLDER + animationsNames[i], typeof(AnimationClip)) as AnimationClip;

                // Controller initialization
                string controllerName = CONTROLLER_FOLDER + animationsNames[i] + ".controller";
                AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPathWithClip(controllerName, clip);
                controller.AddParameter("SpeedMultiplier", AnimatorControllerParameterType.Float);

                // Controller parameters
                var rootStateMachine = controller.layers[0].stateMachine;
                rootStateMachine.states[0].state.speedParameterActive = true;
                rootStateMachine.states[0].state.speedParameter = "SpeedMultiplier";
                controllers[i] = controller;
            }
        }
		else
		{
			controllers = null;
            Debug.LogError("At least one animation is required!");
            Quit();
        }
    }
    #endregion

    #region Avatars initialization
    private void initAvatars()
    {
		if (controllers != null)
		{
            DynamicCharacterAvatar[] avatars = new DynamicCharacterAvatar[count];
            for (int i = 0; i < count; i++)
            {
                avatars[i] = Instantiate(avatar, new Vector3(Random.Range(posx - range, posx + range), 0, Random.Range(posz - range, posz + range)), Quaternion.Euler(0, Random.Range(0, 360), 0));
				avatars[i].name = "Avatar_" + (i + 1);
				avatars[i].raceAnimationControllers.defaultAnimationController = controllers[Random.Range(0, controllers.Length)]; //minInclusive..maxExclusive
                avatars[i].gameObject.AddComponent<NavMeshAgent>();
                avatars[i].gameObject.AddComponent<MovementAI>();
                Randomize(avatars[i]);
            }
        }
		else
		{
            Debug.LogError("Cannot find any animation controller!");
            Quit();
        }
    }
    #endregion

    #region Random Avatar 
    private void Randomize(DynamicCharacterAvatar Avatar)
	{
		Avatar.WardrobeRecipes.Clear();
        if (Avatar != null)
			{
				RandomAvatar ra = Randomizer.GetRandomAvatar();
				Avatar.ChangeRaceData(ra.RaceName);
				//Avatar.BuildCharacterEnabled = true;
				var RandomDNA = ra.GetRandomDNA();
				Avatar.predefinedDNA = RandomDNA;
				var RandomSlots = ra.GetRandomSlots();

				if (ra.SharedColors != null && ra.SharedColors.Count > 0)
				{
					foreach(RandomColors rc in ra.SharedColors)
					{
						if (rc.ColorTable != null)
						{
							Avatar.SetColor(rc.ColorName, GetRandomColor(rc), false);
						}
					}
				}
				foreach (string s in RandomSlots.Keys)
				{
					List<RandomWardrobeSlot> RandomWardrobe = RandomSlots[s];
					RandomWardrobeSlot uwr = GetRandomWardrobe(RandomWardrobe);
					if (uwr.WardrobeSlot != null)
					{
						AddRandomSlot(Avatar, uwr);
					}
				}
			}
    }

    public RandomWardrobeSlot GetRandomWardrobe(List<RandomWardrobeSlot> wardrobeSlots)
	{
		int total = 0;

		foreach (RandomWardrobeSlot rws in wardrobeSlots)
			total += rws.Chance;

		foreach(RandomWardrobeSlot rws in wardrobeSlots)
		{
			if (UnityEngine.Random.Range(0,total) < rws.Chance)
			{
				return rws;
			}
		}
		return wardrobeSlots[wardrobeSlots.Count - 1];
	}

	private OverlayColorData GetRandomColor(RandomColors rc)
	{
		int inx = UnityEngine.Random.Range(0, rc.ColorTable.colors.Length);
		return rc.ColorTable.colors[inx];
	}

	private void AddRandomSlot(DynamicCharacterAvatar Avatar, RandomWardrobeSlot uwr)
	{
		Avatar.SetSlot(uwr.WardrobeSlot);
		if (uwr.Colors != null)
		{
			foreach(RandomColors rc in uwr.Colors)
			{
				if (rc.ColorTable != null)
				{
					OverlayColorData ocd = GetRandomColor(rc);
					Avatar.SetColor(rc.ColorName, ocd, false);
				}
			}
		}
	}
    #endregion
}
