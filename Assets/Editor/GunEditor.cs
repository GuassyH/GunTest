using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Gun))]
public class GunEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        Gun gun = (Gun)target;

        GUILayout.Space(20f);
        //if(GUILayout.Button("Set Hipfire Position to Holder Pos")){ gun.HipfirePos = gun.Holder.localPosition;  }
        if(GUILayout.Button("Create Empty Fields")){  }

    }
}