using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
[SelectionBase]
public class WallMaker : MonoBehaviour
{
  public GameObject PostPrefab;
  public GameObject WallPrefab;
  public bool Tilt = true;
  public bool BlenderFixPost = true;
  public bool BlenderFixWall = true;
  public bool BlenderFixEndCaps = true;
  public bool WallOriginCenter = false;
  public float WallPrefabLength = 10;
  public bool Rearrange = false;
  public float DefaultSegmentLength = 3;
  public Tool LastTool;
  public int SelectedPost = -1;
  public bool initialized = false;
  public GameObject endCaps;
  public GameObject[] endCapInstances;


  [HideInInspector]
  public List<GameObject> Posts;
  [HideInInspector]
  public List<GameObject> Walls;
  public void Initialize()
  {
    if (PostPrefab && WallPrefab)
    {
      Posts = new List<GameObject>() { };
      Walls = new List<GameObject>() { };
      AddSegment();
      AddSegment();
      initialized = true;
    }
  }
  public void AddSegment(int index = -1)
  {
    if (index == -1)
    {
      index = Posts.Count;
    }
    Undo.RecordObject(this, "Add wall segment");
    if (index == Posts.Count)
    {
      Posts.Add(PrefabUtility.InstantiatePrefab(PostPrefab, transform) as GameObject);
    }
    else
    {
      Posts.Insert(index, PrefabUtility.InstantiatePrefab(PostPrefab, transform) as GameObject);
    }
    SelectedPost = index;
    if (BlenderFixPost)
    {
      Posts[index].transform.Rotate(new Vector3(0, 0, Random.Range(-180, 180)));
    }
    else
    {
      Posts[index].transform.Rotate(new Vector3(0, Random.Range(-180, 180), 0));
    }

    if (Posts.Count == 1)
    {
      Posts[0].transform.position = transform.position;
      TiltPost(index);
      Walls.Add(null);
    }
    else
    {
      if (index == 0)
      {
        Walls.Insert(1, PrefabUtility.InstantiatePrefab(WallPrefab, transform) as GameObject);
      }
      else if (index == Posts.Count - 1)
      {
        Walls.Add(PrefabUtility.InstantiatePrefab(WallPrefab, transform) as GameObject);
      }
      else
      {
        Walls.Insert(index, PrefabUtility.InstantiatePrefab(WallPrefab, transform) as GameObject);
      }
      Vector3 NewPostPos;
      if (Posts.Count > 2)
      {
        if (index == Posts.Count - 1)
        {
          //new posts at the end of the wall are added along the direction of the old last wall segment
          NewPostPos = Posts[index - 1].transform.position + (Posts[index - 1].transform.position - Posts[index - 2].transform.position).normalized * DefaultSegmentLength;
        }
        else if (index == 0)
        {
          //new posts at the beginning of the wall are added along the direction of the old first wall segment
          NewPostPos = Posts[1].transform.position - (Posts[2].transform.position - Posts[1].transform.position).normalized * DefaultSegmentLength;
        }
        else
        {
          //new posts in any other position go halfway between the posts they're being added between
          NewPostPos = (Posts[index - 1].transform.position + Posts[index + 1].transform.position) / 2;
        }
      }
      else
      {
        //if there was only one post before we added one, just put it slightly north of the old one
        NewPostPos = Posts[index - 1].transform.position + new Vector3(0, 0, DefaultSegmentLength);
      }
      Posts[index].transform.position = NewPostPos;
      TiltPost(index);
      if (index == 0)
        WallFacing(1);
      else
        WallFacing(index);
      if (index == 0)
        Undo.RegisterCreatedObjectUndo(Walls[1], "Add wall segment");
      else
        Undo.RegisterCreatedObjectUndo(Walls[index], "Add wall segment");
    }
    Undo.RegisterCreatedObjectUndo(Posts[index], "Add wall segment");
  }
  public void TiltPost(int index)
  {
    float TiltAngle = 15;
    if (index < 0 || index >= Posts.Count)
    {
      Debug.LogError($"Invalid post index {index}!");
      return;
    }
    if (BlenderFixPost)
    {
      Vector3 TurnAxis = Vector3.Cross(Vector3.up, Posts[index].transform.forward).normalized;
      Posts[index].transform.Rotate(TurnAxis, -Vector3.Angle(Vector3.up, Posts[index].transform.forward), Space.World);
    }
    else
    {
      Vector3 TurnAxis = Vector3.Cross(Vector3.up, Posts[index].transform.up).normalized;
      Posts[index].transform.Rotate(TurnAxis, -Vector3.Angle(Vector3.up, Posts[index].transform.up), Space.World);
    }
    if (endCapInstances != null && endCapInstances.Length > 0 && (index == 0 || index == Posts.Count - 1))
    {
      int IndexCap;
      Vector3 VecToPost;
      if (index == 0)
      {
        VecToPost = (Posts[0].transform.position - Posts[1].transform.position).normalized;
        IndexCap = 0;

      }
      else
      {
        VecToPost = (Posts[index].transform.position - Posts[index - 1].transform.position).normalized;
        IndexCap = 1;
      }
      Vector2 FlatVector = new Vector2(VecToPost.x, VecToPost.z).normalized;
      endCapInstances[IndexCap].transform.position = Posts[index].transform.position;
      if (Tilt)
      {
        endCapInstances[IndexCap].transform.position += new Vector3(0, 0, (Mathf.Abs(FlatVector.y) * .35f));
      }
      endCapInstances[IndexCap].transform.rotation = Quaternion.LookRotation(VecToPost);
      if (BlenderFixEndCaps)
      {
        endCapInstances[IndexCap].transform.Rotate(new Vector3(-90, 0, 0), Space.Self);
        if (Tilt)
        {
          endCapInstances[IndexCap].transform.Rotate(new Vector3(0, Mathf.Asin(FlatVector.x) / Mathf.PI * 2 * -15, 0), Space.Self);
        }
      }
      else
      {
        if (Tilt)
        {
          endCapInstances[IndexCap].transform.Rotate(new Vector3(0, 0, Mathf.Asin(FlatVector.x) / Mathf.PI * 2 * -15), Space.Self);
        }
      }
    }
    if (Tilt)
    {
      Posts[index].transform.Rotate(new Vector3(TiltAngle, 0, 0), Space.World);
    }
  }
  public void RemoveSegment(int index = -1)
  {
    if (index == -1)
    {
      index = Posts.Count - 1;
    }
    if (Posts == null || Posts.Count < 2)
    {
      Debug.LogError("Too few segments to remove!");
      return;
    }
    if (index >= Posts.Count || index < -1)
    {
      Debug.LogError($"Invalid index {index}! Use -1 to remove last segment automatically.");
      return;
    }

    Undo.DestroyObjectImmediate(Posts[index]);
    if (index == 0) //if we're referencing the origin post, we need to remove the *next* wall instead of the previous one (leaving Walls[0] as null)
    {
      Undo.DestroyObjectImmediate(Walls[1]);
      Undo.RecordObject(this, "Remove wall segment");
      Walls.RemoveAt(1);
    }
    else
    {
      Undo.DestroyObjectImmediate(Walls[index]);
      Undo.RecordObject(this, "Remove wall segment");
      Walls.RemoveAt(index);
    }
    Posts.RemoveAt(index);

  }
  public void UpdateFence(bool ToggledBlendFix, bool ChangedPostPrefab, bool ChangedWallprefab, bool ChangedEndCaps)
  {
    string UpdateMessage = "Update wall";
    //TODO: Probably worth just taking out the undo operations for model replacements in this function (doesn't work well anyways)
    //TODO: figure out undo callback to updatefence, too (so it visually changes when value changes are undone/redone)
    if (PostPrefab && WallPrefab)
    {
      if (ChangedEndCaps)
      {
        if (endCapInstances == null || endCapInstances.Length == 0)
        {
          if (endCaps)
          {
            endCapInstances = new GameObject[] { PrefabUtility.InstantiatePrefab(endCaps, transform) as GameObject, PrefabUtility.InstantiatePrefab(endCaps, transform) as GameObject };
            Undo.RegisterCreatedObjectUndo(endCapInstances[0], UpdateMessage);
            Undo.RegisterCreatedObjectUndo(endCapInstances[1], UpdateMessage);
          }
        }
        else
        {
          if (endCaps)
          {
            Undo.DestroyObjectImmediate(endCapInstances[0]);
            Undo.DestroyObjectImmediate(endCapInstances[1]);
            Undo.RecordObject(this, UpdateMessage);
            endCapInstances = new GameObject[] { PrefabUtility.InstantiatePrefab(endCaps, transform) as GameObject, PrefabUtility.InstantiatePrefab(endCaps, transform) as GameObject };
            Undo.RegisterCreatedObjectUndo(endCapInstances[0], UpdateMessage);
            Undo.RegisterCreatedObjectUndo(endCapInstances[1], UpdateMessage);
          }
          else
          {
            //TODO: Re-determine if Undo.DestroyObjectImmediate has to be before or after Undo.RecordObject for it to properly collapse into one undo step with everything tracked
            Undo.DestroyObjectImmediate(endCapInstances[0]);
            Undo.DestroyObjectImmediate(endCapInstances[1]);
            Undo.RecordObject(this, UpdateMessage);
            endCapInstances = null;
          }
        }
      }
      if (!initialized)
      {
        Initialize();
      }
      else
      {
        Transform t;
        for (int i = 0; i < Posts.Count; i++)
        {
          t = Posts[i].transform;
          if (ChangedPostPrefab)
          {
            Posts[i] = PrefabUtility.InstantiatePrefab(PostPrefab, transform) as GameObject;
            Posts[i].transform.position = t.position;
            //TODO: transfer scale (for each axis a, get difference between prefab.transform.scale[a] and t.scale[a], apply difference to posts[i].transform.scale[a]
            //swizzle if blendfix toggled
            //Posts[i].transform.localScale = PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject).transform.localScale;
            Posts[i].transform.rotation = t.rotation;
            if (ToggledBlendFix)
            {
              if (BlenderFixPost)
              {
                Posts[i].transform.Rotate(-90, 0, 0, Space.Self);
              }
              else
              {
                Posts[i].transform.Rotate(90, 0, 0, Space.Self);
              }
            }
            TiltPost(i);
            Undo.DestroyObjectImmediate(t.gameObject);
            Undo.RegisterCreatedObjectUndo(Posts[i].gameObject, UpdateMessage);
          }
          else
          {
            Undo.RecordObject(Posts[i], UpdateMessage);
            if (ToggledBlendFix)
            {
              if (BlenderFixPost)
              {
                Posts[i].transform.Rotate(-90, 0, 0, Space.Self);
              }
              else
              {
                Posts[i].transform.Rotate(90, 0, 0, Space.Self);
              }
            }
            TiltPost(i);
          }
          if (i > 0)
          {
            t = Walls[i].transform;
            if (ChangedWallprefab)
            {
              Walls[i] = PrefabUtility.InstantiatePrefab(WallPrefab, transform) as GameObject;
              //TODO: once scale transfer is worked out for posts, do here too
              WallFacing(i);
              Undo.DestroyObjectImmediate(t.gameObject);
              Undo.RegisterCreatedObjectUndo(Posts[i].gameObject, UpdateMessage);
            }
            else
            {
              Walls[i].transform.localScale = PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject).transform.localScale;
              WallFacing(i);
            }
          }
        }
      }
    }
  }

  public void WallFacing(int index)
  {
    //TODO: Shear wall? (alternately: no vertical handle for post movement)
    if (index == 0)
    {
      Debug.LogError("Invalid index 1");
      return;
    }
    Vector3 LastPostPos = Posts[index - 1].transform.position;
    Vector3 NextPostPos = Posts[index].transform.position;
    Vector3 VecToPost = LastPostPos - NextPostPos;
    Vector3 Scale = Walls[index].transform.localScale;
    Vector2 FlatVector = new Vector2(VecToPost.x, VecToPost.z).normalized;
    //varPos+(asin(abs(cos(varRot)))/pi*2)*.35
    if (WallOriginCenter)
    {
      Walls[index].transform.position = (LastPostPos + NextPostPos) / 2;
    }
    else
    {
      Walls[index].transform.position = LastPostPos;
    }
    if (Tilt)
    {
      Walls[index].transform.position += new Vector3(0, 0, (Mathf.Abs(FlatVector.y) * .35f));
    }
    Walls[index].transform.rotation = Quaternion.LookRotation(VecToPost);
    if (BlenderFixWall)
    {
      Walls[index].transform.Rotate(new Vector3(-90, 0, 0), Space.Self);
      if (Tilt)
      {
        Walls[index].transform.Rotate(new Vector3(0, Mathf.Asin(FlatVector.x) / Mathf.PI * 2 * -15, 0), Space.Self);
      }
      Walls[index].transform.localScale = new Vector3(Scale.x, VecToPost.magnitude * 100 / WallPrefabLength, Scale.z);
    }
    else
    {
      if (Tilt)
      {
        Walls[index].transform.Rotate(new Vector3(0, 0, Mathf.Asin(FlatVector.x) / Mathf.PI * 2 * -15), Space.Self);
      }
      if (WallPrefabLength > 0)
        Walls[index].transform.localScale = new Vector3(Scale.x, Scale.y, VecToPost.magnitude / WallPrefabLength);
    }
  }

}

[CustomEditor(typeof(WallMaker))]
public class Inspector_WallMaker : Editor
{
  WallMaker wallMaker;
  private void Awake()
  {
    wallMaker = target as WallMaker;
    wallMaker.Rearrange = false;
    if (wallMaker.Posts != null)
    {
      for (int i = 0; i < wallMaker.Posts.Count; i++)
      {
        wallMaker.TiltPost(i);
        if (i > 0)
        {
          wallMaker.WallFacing(i);
        }
      }
    }
  }
  public override void OnInspectorGUI()
  {
    bool BlendFixOld = wallMaker.BlenderFixPost;
    GameObject PostPrefabOld = wallMaker.PostPrefab;
    GameObject WallPrefabOld = wallMaker.WallPrefab;
    GameObject EndCapsOld = wallMaker.endCaps;
    bool update = false;
    EditorGUI.BeginChangeCheck();
    EditorGUILayout.PropertyField(serializedObject.FindProperty("PostPrefab"));
    EditorGUI.indentLevel++;
    EditorGUILayout.PropertyField(serializedObject.FindProperty("BlenderFixPost"), new GUIContent("Apply Blender fixes for Post"));
    EditorGUI.indentLevel--;
    EditorGUILayout.PropertyField(serializedObject.FindProperty("WallPrefab"));
    EditorGUI.indentLevel++;
    EditorGUILayout.PropertyField(serializedObject.FindProperty("BlenderFixWall"), new GUIContent("Apply Blender fixes for Wall"));
    EditorGUILayout.PropertyField(serializedObject.FindProperty("WallOriginCenter"), new GUIContent("Wall origin is centered"));
    EditorGUI.indentLevel--;
    EditorGUILayout.PropertyField(serializedObject.FindProperty("endCaps"));
    EditorGUI.indentLevel++;
    EditorGUILayout.PropertyField(serializedObject.FindProperty("BlenderFixEndCaps"), new GUIContent("Apply Blender fixes for Endcaps"));
    EditorGUI.indentLevel--;
    EditorGUILayout.PropertyField(serializedObject.FindProperty("WallPrefabLength"));

    EditorGUILayout.PropertyField(serializedObject.FindProperty("Tilt"));

    if (EditorGUI.EndChangeCheck())
    {
      update = true;

    }

    EditorGUILayout.PropertyField(serializedObject.FindProperty("DefaultSegmentLength"));

    EditorGUI.BeginDisabledGroup(!wallMaker.initialized);
    if (wallMaker.Rearrange)
    {
      if (GUILayout.Button("Done Moving Fenceposts", GUILayout.Width(160)))
      {
        FinishMovingPosts();
      }
    }
    else
    {
      if (GUILayout.Button("Move Fenceposts", GUILayout.Width(160)))
      {
        BeginMovingPosts();
      }
    }
    EditorGUI.EndDisabledGroup();
    GUILayout.BeginHorizontal();
    {
      EditorGUI.BeginDisabledGroup(!wallMaker.initialized);
      if (GUILayout.Button("<- +", GUILayout.Width(50)))
      {
        BeginMovingPosts();
        if (wallMaker.Rearrange && wallMaker.SelectedPost > 0 && wallMaker.SelectedPost < wallMaker.Posts.Count)
        {
          wallMaker.AddSegment(wallMaker.SelectedPost);
        }
        else
        {
          wallMaker.AddSegment(0);
        }
      }
      EditorGUI.EndDisabledGroup();
      EditorGUI.BeginDisabledGroup(!wallMaker.initialized || wallMaker.Posts.Count < 3);
      if (GUILayout.Button("-", GUILayout.Width(30)))
      {
        if (wallMaker.Rearrange && wallMaker.SelectedPost > -1 && wallMaker.SelectedPost < wallMaker.Posts.Count)
        {
          wallMaker.RemoveSegment(wallMaker.SelectedPost);
          if (wallMaker.SelectedPost > 0)
            wallMaker.SelectedPost--;
        }
        else
        {
          wallMaker.RemoveSegment();
        }
      }
      EditorGUI.EndDisabledGroup();
      EditorGUI.BeginDisabledGroup(!wallMaker.initialized);
      if (GUILayout.Button("+ ->", GUILayout.Width(50)))
      {
        BeginMovingPosts();
        if (wallMaker.Rearrange && wallMaker.SelectedPost > -1 && wallMaker.SelectedPost < wallMaker.Posts.Count - 1)
        {
          wallMaker.AddSegment(wallMaker.SelectedPost + 1);
        }
        else
        {
          wallMaker.AddSegment();
        }
      }
      EditorGUI.EndDisabledGroup();
      if (wallMaker.initialized)
      {
        EditorGUILayout.LabelField($"Segment count: {wallMaker.Posts.Count - 1}");
      }
    }
    GUILayout.EndHorizontal();

    if (GUILayout.Button("Reset", GUILayout.Width(100)))
    {
      wallMaker.Initialize();
    }

    serializedObject.ApplyModifiedProperties();
    if (update)
    {
      wallMaker.UpdateFence(BlendFixOld != wallMaker.BlenderFixPost, PostPrefabOld != wallMaker.PostPrefab, WallPrefabOld != wallMaker.WallPrefab, EndCapsOld != wallMaker.endCaps);
    }
  }
  private void OnDisable()
  {
    if (wallMaker.Rearrange)
    {
      FinishMovingPosts();
    }
  }

  void BeginMovingPosts()
  {
    if (!wallMaker.Rearrange)
    {
      wallMaker.Rearrange = true;
      wallMaker.LastTool = Tools.current;
      Tools.current = Tool.None;
      wallMaker.SelectedPost = -1;
    }
  }

  void FinishMovingPosts()
  {
    Tools.current = wallMaker.LastTool;
    wallMaker.Rearrange = false;
  }

  void OnSceneGUI()
  {
    WallMaker wallMaker = target as WallMaker;
    if (wallMaker.Rearrange)
    {
      Vector3 NewPos;
      Transform t;
      Handles.color = new Color(.5f, 1f, .5f);
      if (wallMaker.endCapInstances != null)
      {
        //TODO: handles to angle end caps
      }
      for (int i = 0; i < wallMaker.Posts.Count; i++)
      {
        t = wallMaker.Posts[i].transform;
        if (i == wallMaker.SelectedPost)
        {
          EditorGUI.BeginChangeCheck();
          //TODO: custom handles: X arrow, Z arrow, X-and-Z square, Y twist
          //TODO: Other controls: Y axis (if I can work out shearing the walls), edge slide, post wonkiness (X and Z rotation, which'll have to be stored elsewhere and then applied in the tilt function)
          NewPos = Handles.PositionHandle(t.position, Quaternion.identity);
          if (i > 0)
          {
            wallMaker.WallFacing(i);
          }
          if (i < wallMaker.Posts.Count - 1)
          {
            wallMaker.WallFacing(i + 1);
          }
          if (EditorGUI.EndChangeCheck())
          {
            Undo.RecordObject(t, "Move fencepost");
            t.position = NewPos;
            if (i == 0 || i == wallMaker.Posts.Count - 1)
              wallMaker.TiltPost(i);
          }
        }
        else
        {
          float ButtonSize = .1f;
          float ButtonPickSize = .1f;
          if (Handles.Button(t.position, Quaternion.LookRotation(Camera.current.transform.forward, Camera.current.transform.up), HandleUtility.GetHandleSize(t.position) * ButtonSize, HandleUtility.GetHandleSize(t.position) * ButtonPickSize, Handles.DotHandleCap))
          {
            wallMaker.SelectedPost = i;
          }
        }
        float AddSize = .1f;
        if (i > 0 && (i == wallMaker.SelectedPost || i == wallMaker.SelectedPost + 1))
        {
          if (Handles.Button((t.position + wallMaker.Posts[i - 1].transform.position) / 2, Quaternion.LookRotation(Camera.current.transform.forward, Camera.current.transform.up), HandleUtility.GetHandleSize(t.position) * AddSize, HandleUtility.GetHandleSize(t.position) * AddSize, Handles.RectangleHandleCap))
          {
            wallMaker.AddSegment(i);
          }
        }
        else if (i == 0 && wallMaker.SelectedPost == 0)
        {
          if (Handles.Button(t.position - (wallMaker.Posts[i + 1].transform.position - t.position).normalized * wallMaker.DefaultSegmentLength, Quaternion.LookRotation(Camera.current.transform.forward, Camera.current.transform.up), HandleUtility.GetHandleSize(t.position) * AddSize, HandleUtility.GetHandleSize(t.position) * AddSize, Handles.RectangleHandleCap))
          {
            wallMaker.AddSegment(0);
          }
        }
        if (i == wallMaker.Posts.Count - 1 && wallMaker.SelectedPost == i)
        {
          if (Handles.Button(t.position - (wallMaker.Posts[i - 1].transform.position - t.position).normalized * wallMaker.DefaultSegmentLength, Quaternion.LookRotation(Camera.current.transform.forward, Camera.current.transform.up), HandleUtility.GetHandleSize(t.position) * AddSize, HandleUtility.GetHandleSize(t.position) * AddSize, Handles.RectangleHandleCap))
          {
            wallMaker.AddSegment();
          }
        }

      }
    }

  }
}
