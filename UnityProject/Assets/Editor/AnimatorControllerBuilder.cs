using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace CompanyCEO.UnityRemake.Editor
{
    public static class AnimatorControllerBuilder
    {
        [MenuItem("Company CEO Simulator/Create Humanoid Animator Controller")]
        public static void CreateController()
        {
            const string folder = "Assets/Animations";
            const string path = folder + "/OfficeHumanoid.controller";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets", "Animations");
            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("State", AnimatorControllerParameterType.Int);
            var root = controller.layers[0].stateMachine;
            foreach (var label in new[] { "Idle", "Walk", "Typing", "Talking", "Listening", "Drinking Coffee", "Phone Call", "Sitting" })
            {
                var state = root.AddState(label);
                state.writeDefaultValues = true;
            }
            AssetDatabase.SaveAssets();
            Selection.activeObject = controller;
            Debug.Log("Created OfficeHumanoid.controller. Assign licensed Humanoid clips to its states after importing animation assets.");
        }
    }
}
