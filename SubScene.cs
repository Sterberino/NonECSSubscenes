using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;


[ExecuteInEditMode]
public class SubScene : MonoBehaviour
{
    /// <summary>
    /// The Runtime representation of the Scene Asset. 
    /// </summary>
    [HideInInspector]
    public Scene EditingScene;

    /// <summary>
    /// The Scene that the SubScene represents.
    /// </summary>
    [Tooltip("The Scene that the SubScene represents.")]
    public SceneAsset SceneAsset;

    /// <summary>
    /// The SubScene loads on Start if true.
    /// </summary>
    [Tooltip("The SubScene loads on Start if true.")]
    public bool AutoLoadScene { get; set;}

    /// <summary>
    /// Returns AssetDatabase.GetAssetPath(SceneAsset);
    /// </summary>
    public string EditableScenePath{get{return AssetDatabase.GetAssetPath(SceneAsset);}}
    
    public Color HierarchyColor = Color.white;

    public bool IsLoaded{
        get{
            return EditingScene.isLoaded;
        }
    }

    /// <summary>
    /// Get a Hash128 GUID from the existing scene asset.
    /// </summary>
    public Hash128 SceneGUID { get {
            int data = SceneAsset.GetHashCode();
            return Hash128.Compute(ref data);
    }}

    public void Start()
    {
        if (AutoLoadScene)
        {
            this.OpenSubscene();
        }
    }



    /// <summary>
    /// Loads in the Subscene. 
    /// </summary>
    /// <returns>True if the Scene is open, false otherwise. Remember that scenes are loaded Asynchronously.</returns>
    public bool OpenSubscene()
    {
        Scene activeScene;
        if (Application.isPlaying)
        {
            activeScene = SceneManager.GetActiveScene();
            AsyncOperation op = SceneManager.LoadSceneAsync(this.SceneAsset.name, LoadSceneMode.Additive);
            op.completed += (x) => {
                Debug.Log("Loaded Scene");
                this.EditingScene = EditorSceneManager.GetSceneByName(this.SceneAsset.name);
                LoadSubsceneGameobjects();
            };
            SceneManager.SetActiveScene(activeScene);
        
        }
        else
        {
            activeScene = EditorSceneManager.GetActiveScene();
            this.EditingScene = EditorSceneManager.OpenScene(AssetDatabase.GetAssetOrScenePath(this.SceneAsset), OpenSceneMode.Additive);
            LoadSubsceneGameobjects();
            EditorUtility.SetDirty(this.gameObject);

            EditorSceneManager.SetActiveScene(activeScene);
        }

        this.EditingScene.isSubScene = true;
        return this.IsLoaded;
    }

    /// <summary>
    /// In the case that the subscene loses it's reference to the Scene represented by it's scene asset, you can attempt to reconnect it using this method. Will only work if the scene is open.
    /// </summary>
    /// <returns>True if the EditingScene reference is not null, false if it is null.</returns>
    public bool ReconnectEditingScene()
    {
        this.EditingScene = EditorSceneManager.GetSceneByName(this.SceneAsset.name);
        return this.EditingScene.name == this.SceneAsset.name && this.EditingScene.IsValid();
    }

    /// <summary>
    /// Closes the Subscene.
    /// </summary>
    /// <param name="SaveSubsceneOnClose"></param>
    /// <returns>True if the subscene is closed. False otherwise.</returns>
    public bool CloseSubscene(bool SaveSubsceneOnClose)
    {
        UnloadSubsceneGameobjects();

        if (Application.isPlaying)
        {
            AsyncOperation op = SceneManager.UnloadSceneAsync(this.EditingScene);
            op.completed += (x)=>{
                Debug.Log("Scene Unloaded");
            };

        }
        else
        {
            if (SaveSubsceneOnClose)
            {
                SaveSubScene();
            }
            EditorUtility.SetDirty(this.gameObject);
            EditorSceneManager.CloseScene(this.EditingScene, true);
        }
        return !this.IsLoaded;
    }

    /// <summary>
    /// Saves the scene represented by the subscene.
    /// </summary>
    /// <returns></returns>
    public bool SaveSubScene()
    {
        return EditorSceneManager.SaveScene(this.EditingScene);
    }

    /// <summary>
    /// Move all of the Gameobjects in the subscene to the active scene, and parent them to the Subscene Object.
    /// </summary>
    private void LoadSubsceneGameobjects()
    {
        if (EditingScene == null || !EditingScene.isLoaded)
        {
            Debug.LogWarning("Tried to load subscene objects when the editingScene was not loaded.");
            return;
        }

        GameObject[] gameobjects = EditingScene.GetRootGameObjects();
        foreach (GameObject g in gameobjects)
        {
            EditorSceneManager.MoveGameObjectToScene(g, gameObject.scene);
            g.transform.SetParent(this.transform);
        }


#if UNITY_EDITOR
        EditorApplication.RepaintHierarchyWindow();
#endif
    }

    private void UnloadSubsceneGameobjects()
    {
        if (EditingScene == null || !EditingScene.isLoaded)
        {
            return;
        }

        foreach(Transform child in this.GetComponentsInChildren<Transform>())
        {
            if(child == this.transform || child.parent != this.transform)
            {
                continue;
            }
            child.SetParent(null);
            EditorSceneManager.MoveGameObjectToScene(child.gameObject, EditingScene);
        }
    }

    /// <summary>
    /// Creates a new SubScene and moves the subSceneRootObject to it. Creates new Scene Asset for the SubScene and adds it to the build.  
    /// </summary>
    /// <param name="subSceneRootObject">The GameObject to move to the new SubScene</param>
    /// <param name="sceneAssetName">The name of the Scene Asset created for the SubScene.</param>
    /// <param name="closeSubsceneOnCreation">Closes the SubScene after Creation if true.</param>
    /// <returns>The new SubScene with the subSceneRootObject as a scene root object.</returns>
    public static SubScene CreateSubsceneFromGameobject(GameObject subSceneRootObject, string sceneAssetName, bool closeSubsceneOnCreation = false)
    {
        GameObject [] rootObjects = new GameObject[]{ subSceneRootObject};
        return CreateSubsceneFromGameobjects(rootObjects, sceneAssetName, closeSubsceneOnCreation);
    }


    /// <summary>
    /// Creates a new empty SubScene. Creates new Scene Asset for the SubScene and adds it to the build.  
    /// </summary>
    /// <param name="sceneAssetName">The name of the Scene Asset created for the SubScene.</param>
    /// <param name="closeSubsceneOnCreation">Closes the SubScene after Creation if true.</param>
    /// <returns>The new empty SubScene.</returns>
    public static SubScene CreateEmptySubscene(string sceneAssetName, bool closeSubsceneOnCreation = false)
    {
        //Just pass in an empty list of gameobjects to move to the new Scene
        return CreateSubsceneFromGameobjects(new List<GameObject>(), sceneAssetName, closeSubsceneOnCreation);
    }

    /// <summary>
    /// Creates a new SubScene and moves the subSceneRootObjects to it. Creates new Scene Asset for the SubScene and adds it to the build.  
    /// </summary>
    /// <param name="subSceneRootObject">The GameObject to move to the new SubScene</param>
    /// <param name="sceneAssetName">The name of the Scene Asset created for the SubScene.</param>
    /// <param name="closeSubsceneOnCreation">Closes the SubScene after Creation if true.</param>
    /// <returns>The new SubScene with the subSceneRootObjects as scene root object.</returns>
    public static SubScene CreateSubsceneFromGameobjects(List<GameObject> subSceneRootObjects, string sceneAssetName, bool closeSubsceneOnCreation = false)
    {
        return CreateSubsceneFromGameobjects(subSceneRootObjects.ToArray(), sceneAssetName, closeSubsceneOnCreation);
    }

    /// <summary>
    /// Creaates a new SubScene and moves the subSceneRootObjects to it. Creates new Scene Asset for the SubScene and adds it to the build.  
    /// </summary>
    /// <param name="subSceneRootObject">The GameObject to move to the new SubScene</param>
    /// <param name="sceneAssetName">The name of the Scene Asset created for the SubScene.</param>
    /// <param name="closeSubsceneOnCreation">Closes the SubScene after Creation if true.</param>
    /// <returns>The new SubScene with the subSceneRootObjects as scene root object.</returns>
    public static SubScene CreateSubsceneFromGameobjects(GameObject [] subSceneRootObjects, string sceneAssetName, bool closeSubsceneOnCreation = false)
    {
        //Create the proper path for the scenes if it does not already exist.
        if(!AssetDatabase.IsValidFolder("Assets/Scenes/"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Scenes/Subscenes"))
        {
            AssetDatabase.CreateFolder("Assets/Scenes", "Subscenes");
        }

        /*Create a new scene and scene asset. If you want to change the
        default subscene location yourself, you can just change this path.*/
        string path = "Assets/Scenes/Subscenes/" + sceneAssetName + ".unity";
        Scene activeScene = EditorSceneManager.GetActiveScene();
        path = AssetDatabase.GenerateUniqueAssetPath(path);
        int indexOfName = path.LastIndexOf('/') + 1;
        string sceneName = path.Substring(indexOfName, path.Length - indexOfName - ".unity".Length);
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        scene.name = sceneName;

        /*You have to set the original active scene as the active scene so that 
        instantiated objects are spawned in the current scene*/
        EditorSceneManager.SetActiveScene(activeScene);
        //Move the gameobject to the new scene
        foreach(GameObject rootObject in subSceneRootObjects)
        {
            SceneManager.MoveGameObjectToScene(rootObject, scene);
        }
        EditorSceneManager.SaveScene(scene, path);

        /*Get the scene asset, create a new gameobject with subscene component, 
        set the subscene scene as the newly created scene*/
        SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
        GameObject subSceneGO = new GameObject(scene.name);
        SubScene subScene = subSceneGO.AddComponent<SubScene>();
        subScene.AutoLoadScene = false;
        subScene.SceneAsset = sceneAsset;
        subScene.EditingScene = scene;

        /*Get the list of scenes in the Editor Build Settings, Add the new scene to 
         the list. Assign the scenes property as the updated list.*/
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        if (closeSubsceneOnCreation)
        {
            subScene.CloseSubscene(true);
        }
        else
        {
            subScene.OpenSubscene();
        }
        return subScene;
    }

    //This method is for changing the Hierarchy color. Repainting the hierarchy window allows you to see the color change immediately.
    public void OnValidate()
    {
#if UNITY_EDITOR
        EditorApplication.RepaintHierarchyWindow();
#endif
    }

    //Close the scene if the gameobject is destroyed so there isn't an unaccounted for Scene open in the hierarchy. By default, save the subscene if working in the editor.
    public void OnDestroy()
    {
#if UNITY_EDITOR
        this.CloseSubscene(true);
#endif
        if (Application.isPlaying)
        {
            this.CloseSubscene(false);
        }
    }
}