using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UMA;
using UMA.CharacterSystem;
using UnityEngine.AI;

public class Spawner : MonoBehaviour
{
	public DynamicCharacterAvatar avatar;
    public UMARandomizer Randomizer;

	public int count = 10;
	public int range = 15;
	public int posx = 0;
	public int posz = 0;

    private string animDir = "Assets/Animations";
    private ArrayList aniNames;
    private UnityEditor.Animations.AnimatorController[] controllers;
    
	// Start is called before the first frame update
    void Start()
    {
        initControllers();
		initAvatars();
    }

    #region Controllers initialization
    private void initControllers(){
        if (Directory.Exists("Assets/Controllers"))
		{
            Directory.Delete("Assets/Controllers", true);
        }
        Directory.CreateDirectory("Assets/Controllers");
        DirectoryInfo dir = new DirectoryInfo(animDir);

        FileInfo[] files = dir.GetFiles("*.fbx"); //Getting all fbx files
        
		aniNames = new ArrayList();
		foreach(FileInfo file in files)
        {
            aniNames.Add(animDir + "/" + file.Name);
        }
		int aniCount = aniNames.Count;

        controllers = new UnityEditor.Animations.AnimatorController[aniCount];
        for(int i = 0; i < aniCount; i++){
            AnimationClip clip = AssetDatabase.LoadAssetAtPath(aniNames[i].ToString(), typeof(AnimationClip)) as AnimationClip;

            int pFrom = aniNames[i].ToString().IndexOf("@") + "@".Length;
            int pTo = aniNames[i].ToString().LastIndexOf(".fbx");

            string animationName = aniNames[i].ToString().Substring(pFrom, pTo - pFrom);

            string controllerName = "Assets/Controllers/" + animationName + ".controller";
            UnityEditor.Animations.AnimatorController controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPathWithClip(controllerName, clip);
			controller.AddParameter("SpeedMultiplier", AnimatorControllerParameterType.Float);
            var rootStateMachine = controller.layers[0].stateMachine;
			rootStateMachine.states[0].state.speedParameterActive = true;
			rootStateMachine.states[0].state.speedParameter = "SpeedMultiplier";
            controllers[i] = controller;
        }
    }
    #endregion

    #region Avatars initialization
    private void initAvatars()
    {
        DynamicCharacterAvatar[] avatars = new DynamicCharacterAvatar[count];
        for (int i = 0; i < count; i++)
        {
            avatars[i] = Instantiate(avatar, new Vector3(Random.Range(posx - range, posx + range), 0, Random.Range(posz - range, posz + range)), Quaternion.Euler(0, Random.Range(0, 360), 0));
            avatars[i].raceAnimationControllers.defaultAnimationController = controllers[Random.Range(0, controllers.Length)]; //minInclusive..maxExclusive
            avatars[i].gameObject.AddComponent<NavMeshAgent>(); 
            avatars[i].gameObject.AddComponent<MovementAI>();
            Randomize(avatars[i]);
        }
    }
    #endregion

    #region Random Avatar 
    private void Randomize(DynamicCharacterAvatar Avatar){
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

    public RandomWardrobeSlot GetRandomWardrobe(List<RandomWardrobeSlot> wardrobeSlots){
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

	private OverlayColorData GetRandomColor(RandomColors rc){
		int inx = UnityEngine.Random.Range(0, rc.ColorTable.colors.Length);
		return rc.ColorTable.colors[inx];
	}

	private void AddRandomSlot(DynamicCharacterAvatar Avatar, RandomWardrobeSlot uwr){
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
