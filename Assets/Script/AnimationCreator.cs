using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


//Create an animation clip at 5fps

public class AnimationCreator : MonoBehaviour
{
    public string fileName;
    public Sprite[] sprites;
    [ContextMenu("Create Animation")]
    private void CreateAni()
    {
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 5f;

        EditorCurveBinding spriteBinding = new EditorCurveBinding();
        spriteBinding.type = typeof(SpriteRenderer);
        spriteBinding.path = "";
        spriteBinding.propertyName = "m_Sprite";

        ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            spriteKeyFrames[i] = new ObjectReferenceKeyframe();
            spriteKeyFrames[i].time = (float)i/5f;
            spriteKeyFrames[i].value = sprites[i];

        }

        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, spriteKeyFrames);

        AssetDatabase.CreateAsset(clip, "Assets/Animation/" + fileName + ".anim");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPathWithClip
                ("Assets/Animation/" + fileName + ".controller", clip);

        fileName = "";
        sprites = new Sprite[0];
    }
}
