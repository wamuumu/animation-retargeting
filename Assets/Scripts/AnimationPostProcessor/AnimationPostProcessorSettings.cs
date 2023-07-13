using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationPostProcessorSettings", menuName = "AnimationPostProcessor/Settings", order = 1)]
public class AnimationPostProcessorSettings : ScriptableObject 
{
    public bool enabled = true; // specify is these settings are enabled
    public Avatar referenceAvatar; // reference to the UMA avatar
    public GameObject referenceFBX; // reference to the Mixamo animation
    
    public bool enableTranslationDoF = true; // degrees of freedom for bones structure
    public ModelImporterAnimationType animationType = ModelImporterAnimationType.Human; // default animation type
    public bool loopTime = true; // loop the animation
    public bool loopPose = true; // loop the pose
    public bool lockRootRotation = true; // bake into pose for root rotation
    public bool lockRootHeightY = true; // bake into pose for root rotation on Y
    public bool keepOriginalPositionY = false; // avoid original bake pose
    public bool heightFromFeet = true; // enable feet bake pose
    public bool renameClips = true; // rename clip with animation name
}
