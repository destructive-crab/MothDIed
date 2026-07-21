using UnityEditor;

namespace banging_code.Editor
{
    [CustomEditor(typeof(AudioSourceDeco))]
    public class AudioSourceDecoInspector : UnityEditor.Editor
    {
        SerializedProperty sourceProp;
        SerializedProperty isPitchedProp;
        SerializedProperty minPitchProp;
        SerializedProperty maxPitchProp;
        SerializedProperty smoothStopProp;
        SerializedProperty smoothPlayProp;

        private void OnEnable()
        {
            sourceProp = serializedObject.FindProperty("Source");
            isPitchedProp = serializedObject.FindProperty("IsPitched");
            minPitchProp = serializedObject.FindProperty("MinPitch");
            maxPitchProp = serializedObject.FindProperty("MaxPitch");
            smoothStopProp = serializedObject.FindProperty("SmoothStop");
            smoothPlayProp = serializedObject.FindProperty("SmoothPlay");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(sourceProp);
            
            if((target as AudioSourceDeco)?.Source == null) return;
            
            EditorGUILayout.PropertyField(isPitchedProp);

            if (isPitchedProp.boolValue)
            {
                EditorGUILayout.PropertyField(minPitchProp);
                EditorGUILayout.PropertyField(maxPitchProp);
            }

            EditorGUILayout.PropertyField(smoothStopProp);
            EditorGUILayout.PropertyField(smoothPlayProp);

            serializedObject.ApplyModifiedProperties();
        } 
    }
}