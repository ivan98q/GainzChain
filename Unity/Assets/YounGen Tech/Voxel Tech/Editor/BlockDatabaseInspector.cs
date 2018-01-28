using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using System;

namespace YounGenTech.VoxelTech {
    [CustomEditor(typeof(BlockDatabase))]
    public class BlockDatabaseInspector : Editor {

        SerializedProperty blockDataList;
        BlockDatabase current;

        int selectedBlockDataIndex;
        Vector2 blockDataScroll;

        SerializedProperty selectedMaterial = null;
        ReorderableList dataList = null;

        void OnEnable() {
            blockDataList = serializedObject.FindProperty("_blockDataList");

            current = target as BlockDatabase;
            dataList = new ReorderableList(serializedObject, blockDataList, true, true, true, true);
            dataList.drawElementCallback = PropertyGUI;
            dataList.drawHeaderCallback = PropertyHeader;
            dataList.elementHeightCallback = PropertyHeight;
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            Event e = Event.current;

            if(e.type == EventType.ExecuteCommand) {
                switch(e.commandName) {
                    case "ObjectSelectorUpdated":
                        if(selectedMaterial != null)
                            selectedMaterial.objectReferenceValue = EditorGUIUtility.GetObjectPickerObject();

                        serializedObject.ApplyModifiedProperties();
                        break;
                    case "ObjectSelectorClosed": selectedMaterial = null; break;
                }
            }

            dataList.DoLayoutList();

            /*EditorGUILayout.BeginHorizontal();
             {
                 GUILayout.FlexibleSpace();
                 if(blockDataList.arraySize > 0)
                     if(GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) blockDataList.DeleteArrayElementAtIndex(0);

                 if(GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) {
                     blockDataList.InsertArrayElementAtIndex(0);
                     ResetBlockData(blockDataList.GetArrayElementAtIndex(0));
                 }

                 GUILayout.FlexibleSpace();
             }
             EditorGUILayout.EndHorizontal();

             BlockDataListGUI();

             EditorGUILayout.BeginHorizontal();
             {
                 GUILayout.FlexibleSpace();
                 if(blockDataList.arraySize > 0)
                     if(GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) blockDataList.DeleteArrayElementAtIndex(blockDataList.arraySize - 1);

                 if(GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) {
                     blockDataList.InsertArrayElementAtIndex(blockDataList.arraySize);
                     ResetBlockData(blockDataList.GetArrayElementAtIndex(blockDataList.arraySize - 1));
                 }

                 GUILayout.FlexibleSpace();
             }
             EditorGUILayout.EndHorizontal();*/

            serializedObject.ApplyModifiedProperties();
        }

        void BlockDataListGUI() {
            blockDataScroll = GUILayout.BeginScrollView(blockDataScroll, GUI.skin.box);
            {
                for(int i = 0; i < blockDataList.arraySize; i++)
                    BlockDataPropertyGUI(new Rect(), blockDataList.GetArrayElementAtIndex(i));
            }
            GUILayout.EndScrollView();
        }

        void PropertyGUI(Rect rect, int index, bool isActive, bool isFocused) {
            SerializedProperty element = blockDataList.GetArrayElementAtIndex(index);
            EditorGUI.indentLevel++;
            BlockDataPropertyGUI(rect, element);
            EditorGUI.indentLevel--;
        }

        float PropertyHeight(int index) {
            SerializedProperty element = blockDataList.GetArrayElementAtIndex(index);
            //return EditorGUI.GetPropertyHeight(element);
            return element.isExpanded ? 350 : dataList.elementHeight;
        }

        void PropertyHeader(Rect rect) {
            GUI.Label(rect, "Blocks");
        }

        void BlockDataPropertyGUI(Rect rect, SerializedProperty element) {
            var name = element.FindPropertyRelative("_name");

            element.isExpanded = GUI.Toggle(rect, element.isExpanded, new GUIContent(!string.IsNullOrEmpty(name.stringValue) ? name.stringValue : "~No Name~"), "Foldout");

            //if(element.isExpanded) {
            //    var id = element.FindPropertyRelative("_id");
            //    var isSolid = element.FindPropertyRelative("_isSolid");
            //    var isOpaque = element.FindPropertyRelative("_isOpaque");

            //    var faceMaterialLeft = element.FindPropertyRelative("_faceMaterialLeft");
            //    var faceMaterialRight = element.FindPropertyRelative("_faceMaterialRight");
            //    var faceMaterialDown = element.FindPropertyRelative("_faceMaterialDown");
            //    var faceMaterialUp = element.FindPropertyRelative("_faceMaterialUp");
            //    var faceMaterialBack = element.FindPropertyRelative("_faceMaterialBack");
            //    var faceMaterialForward = element.FindPropertyRelative("_faceMaterialForward");

            //    rect.y += rect.height;
            //    EditorGUI.PropertyField(rect, name);

            //    rect.y += rect.height;
            //    EditorGUI.PropertyField(rect, id);

            //    rect.y += rect.height;
            //    EditorGUI.PropertyField(rect, isSolid);

            //    rect.y += rect.height;
            //    EditorGUI.PropertyField(rect, isOpaque);

            //    rect.y += rect.height;
            //    //GUI.Label(rect, "-X");
            //    GUILayout.BeginArea(new Rect(rect.x, rect.y, 200, 100));
            //    GUILayout.BeginHorizontal();
            //    GUILayout.Label("-X");
            //    BlockDataMaterialPropertyGUI(faceMaterialLeft);
            //    BlockDataMaterialPropertyGUI(faceMaterialRight);
            //    GUILayout.Label("-X");
            //    GUILayout.EndHorizontal();
            //    GUILayout.EndArea();
            //    //GUI.Label(rect, "+X");

            //    //GUI.Label(rect, "-Y");
            //    //BlockDataMaterialPropertyGUI(faceMaterialDown);
            //    //BlockDataMaterialPropertyGUI(faceMaterialUp);
            //    //GUI.Label(rect, "+Y");

            //    //GUI.Label(rect, "-Z");
            //    //BlockDataMaterialPropertyGUI(faceMaterialBack);
            //    //BlockDataMaterialPropertyGUI(faceMaterialForward);
            //    //GUI.Label(rect, "+Z");
            //}

            //EditorGUILayout.BeginHorizontal();
            //{
            //    element.isExpanded = GUILayout.Toggle(element.isExpanded, new GUIContent(!string.IsNullOrEmpty(name.stringValue) ? name.stringValue : "~No Name~"), "Foldout", GUILayout.ExpandWidth(false));

            //    //if(GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) {

            //    //}

            //    //if(GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) {

            //    //}
            //}
            //EditorGUILayout.EndHorizontal();

            GUILayout.BeginArea(new Rect(rect.x, rect.y + dataList.elementHeight * 4, 400, 350));
            {
                if(element.isExpanded) {
                    var id = element.FindPropertyRelative("_id");
                    var isSolid = element.FindPropertyRelative("_isSolid");
                    var isOpaque = element.FindPropertyRelative("_isOpaque");
                    var buildType = element.FindPropertyRelative("_buildType");

                    var faceMaterialLeft = element.FindPropertyRelative("_faceMaterialLeft");
                    var faceMaterialRight = element.FindPropertyRelative("_faceMaterialRight");
                    var faceMaterialDown = element.FindPropertyRelative("_faceMaterialDown");
                    var faceMaterialUp = element.FindPropertyRelative("_faceMaterialUp");
                    var faceMaterialBack = element.FindPropertyRelative("_faceMaterialBack");
                    var faceMaterialForward = element.FindPropertyRelative("_faceMaterialForward");

                    var originalBlockObject = element.FindPropertyRelative("_originalBlockObject");

                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(name);
                    EditorGUILayout.PropertyField(id);
                    EditorGUILayout.PropertyField(isSolid);
                    EditorGUILayout.PropertyField(isOpaque);

                    EditorGUILayout.PropertyField(buildType, true);

                    switch((BlockData.BlockBuildType)Enum.Parse(typeof(BlockData.BlockBuildType), buildType.enumValueIndex.ToString())) {
                        case BlockData.BlockBuildType.Material:
                            EditorGUILayout.BeginHorizontal(); {
                                GUILayout.FlexibleSpace();

                                EditorGUILayout.BeginVertical();
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    {
                                        GUILayout.Label("-X");
                                        BlockDataMaterialPropertyGUI(faceMaterialLeft);
                                        BlockDataMaterialPropertyGUI(faceMaterialRight);
                                        GUILayout.Label("+X");
                                    }

                                    EditorGUILayout.EndHorizontal();

                                    EditorGUILayout.BeginHorizontal();
                                    {
                                        GUILayout.Label("-Y");
                                        BlockDataMaterialPropertyGUI(faceMaterialDown);
                                        BlockDataMaterialPropertyGUI(faceMaterialUp);
                                        GUILayout.Label("+Y");
                                    }
                                    EditorGUILayout.EndHorizontal();

                                    EditorGUILayout.BeginHorizontal();
                                    {
                                        GUILayout.Label("-Z");
                                        BlockDataMaterialPropertyGUI(faceMaterialBack);
                                        BlockDataMaterialPropertyGUI(faceMaterialForward);
                                        GUILayout.Label("+Z");
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                                EditorGUILayout.EndVertical();

                                GUILayout.FlexibleSpace();
                            }
                            EditorGUILayout.EndHorizontal();

                            break;

                        case BlockData.BlockBuildType.BlockObject:
                            EditorGUILayout.PropertyField(originalBlockObject, true);
                            break;
                    }

                    EditorGUILayout.Separator();
                    EditorGUI.indentLevel--;
                }
            }
            GUILayout.EndArea();
        }

        void BlockDataPropertyGUI2(Rect rect, SerializedProperty element) {
            var name = element.FindPropertyRelative("_name");

            EditorGUILayout.BeginHorizontal();
            {
                element.isExpanded = GUILayout.Toggle(element.isExpanded, new GUIContent(!string.IsNullOrEmpty(name.stringValue) ? name.stringValue : "~No Name~"), "Foldout", GUILayout.ExpandWidth(false));

                //if(GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) {

                //}

                //if(GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) {

                //}
            }
            EditorGUILayout.EndHorizontal();

            if(element.isExpanded) {
                var id = element.FindPropertyRelative("_id");
                var isSolid = element.FindPropertyRelative("_isSolid");
                var isOpaque = element.FindPropertyRelative("_isOpaque");

                var faceMaterialLeft = element.FindPropertyRelative("_faceMaterialLeft");
                var faceMaterialRight = element.FindPropertyRelative("_faceMaterialRight");
                var faceMaterialDown = element.FindPropertyRelative("_faceMaterialDown");
                var faceMaterialUp = element.FindPropertyRelative("_faceMaterialUp");
                var faceMaterialBack = element.FindPropertyRelative("_faceMaterialBack");
                var faceMaterialForward = element.FindPropertyRelative("_faceMaterialForward");

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(name);
                EditorGUILayout.PropertyField(id);
                EditorGUILayout.PropertyField(isSolid);
                EditorGUILayout.PropertyField(isOpaque);

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();

                    EditorGUILayout.BeginVertical();
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("-X");
                            BlockDataMaterialPropertyGUI(faceMaterialLeft);
                            BlockDataMaterialPropertyGUI(faceMaterialRight);
                            GUILayout.Label("+X");
                        }

                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("-Y");
                            BlockDataMaterialPropertyGUI(faceMaterialDown);
                            BlockDataMaterialPropertyGUI(faceMaterialUp);
                            GUILayout.Label("+Y");
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("-Z");
                            BlockDataMaterialPropertyGUI(faceMaterialBack);
                            BlockDataMaterialPropertyGUI(faceMaterialForward);
                            GUILayout.Label("+Z");
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();

                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Separator();
                EditorGUI.indentLevel--;
            }
        }

        void BlockDataMaterialPropertyGUI(SerializedProperty element) {
            Texture2D material = AssetPreview.GetAssetPreview(element.objectReferenceValue);

            if(GUILayout.Button(new GUIContent(!element.objectReferenceValue ? "None" : "", material, element.objectReferenceValue ? element.objectReferenceValue.name : "None"), GUILayout.Width(64), GUILayout.Height(64))) {
                selectedMaterial = element;
                EditorGUIUtility.ShowObjectPicker<Material>(element.objectReferenceValue, false, "", 0);
            }
        }

        void ResetBlockData(SerializedProperty element) {
            element.FindPropertyRelative("_name").stringValue = "";
            element.FindPropertyRelative("_id").intValue = current.GetUniqueID();
            element.FindPropertyRelative("_isSolid").boolValue = false;
            element.FindPropertyRelative("_isOpaque").boolValue = false;

            element.FindPropertyRelative("_faceMaterialLeft").objectReferenceValue = null;
            element.FindPropertyRelative("_faceMaterialRight").objectReferenceValue = null;
            element.FindPropertyRelative("_faceMaterialDown").objectReferenceValue = null;
            element.FindPropertyRelative("_faceMaterialUp").objectReferenceValue = null;
            element.FindPropertyRelative("_faceMaterialBack").objectReferenceValue = null;
            element.FindPropertyRelative("_faceMaterialForward").objectReferenceValue = null;
        }
    }
}