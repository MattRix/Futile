using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Text;

//PR = The Futile Atlas Generator
//http://blogs.unity3d.com/2012/10/25/unity-serialization/
//http://www.jacobpennock.com/Blog/?p=670
public class PRWindow : EditorWindow
{
	public static string DATA_FILE_PATH = "ProjectSettings/Packrat.asset";


	[MenuItem ("Window/Packrat")]
	static void Init () 
	{
		// Get existing open window or if none, make a new one:
		PRWindow window = (PRWindow)EditorWindow.GetWindow (typeof (PRWindow));
		window.position = new Rect(100,100,300,500);
		window.title = "Packrat";
		window.Show(); 
	}

	private List<PRAtlasLink> _atlasLinks = new List<PRAtlasLink>();
	private List<PRAtlasLink> _atlasLinksToGenerate = new List<PRAtlasLink>();
	private bool _hasDataChanged = false;
	private Vector2 _scrollPos = new Vector2(0,0);
	private PRAtlasGenerator _activeGenerator = null;
	private string _progressMessage = "";

	private FileSystemWatcher _watcher;
	private bool _needsLoadData;

	public void OnEnable()
	{
		LoadData();

		_watcher = new FileSystemWatcher();
		_watcher.Path = Path.GetDirectoryName(DATA_FILE_PATH);
		_watcher.Filter = Path.GetFileName(DATA_FILE_PATH);
		_watcher.EnableRaisingEvents = true;
		_watcher.Changed += (object sender, FileSystemEventArgs e) => 
		{
			_needsLoadData = true;
		};
	}

	public void OnDisable()
	{
		SaveData();

	}

	private void LoadData()
	{
		_atlasLinks.Clear();

		if(File.Exists(DATA_FILE_PATH))
		{
			string dataString = File.ReadAllText(DATA_FILE_PATH);

			Dictionary<string,object> dict = dataString.dictionaryFromJson();

			if(dict == null) //the json was broken
			{
				Debug.LogWarning("The Futile Atlas Generator Data at '"+DATA_FILE_PATH+"' was not correct JSON!");
			}
			else 
			{
				List<object> linkDicts = dict["links"] as List<object>;

				for(int d = 0; d<linkDicts.Count; d++)
				{
					Dictionary<string,object> linkDict = linkDicts[d] as Dictionary<string,object>;

					PRAtlasLink link = new PRAtlasLink(linkDict);

					_atlasLinks.Add(link);
				}
			}
		}
		else 
		{
			//Debug.Log ("NO DATA FOUND AT " + DATA_FILE_PATH);
		}

		Repaint();
	}
	
	public void OnGUI () 
	{
		if(_needsLoadData)
		{
			_needsLoadData = false;
			LoadData();
		}

		int linkToMoveIndex = -1;
		int moveDelta = 0;

		_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

		if(_activeGenerator != null)
		{
			GUILayout.Label("Building " + Path.GetFileNameWithoutExtension(_activeGenerator.link.atlasFilePath), EditorStyles.boldLabel);
			//GUILayout.BeginHorizontal();
			GUILayout.Label(_progressMessage); 

			GUILayout.Space(10.0f);
		}
		else  
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label ("Packrat", EditorStyles.boldLabel);
			GUILayout.FlexibleSpace(); 
			GUI.backgroundColor = new Color(0.8f,1.0f,0.8f);

			if(_atlasLinks.Count > 0)
			{
				GUILayout.BeginVertical();

				GUILayout.Space(5.0f);

				if(GUILayout.Button("Generate Selected"))
				{
					for(int n = 0; n<_atlasLinks.Count;n++)
					{
						PRAtlasLink link = _atlasLinks[n];
						if(link.shouldGenerate)
						{
							if(!_atlasLinksToGenerate.Contains(link))
							{
								_atlasLinksToGenerate.Add(link);
							}
						}
					}
				}
				GUILayout.EndVertical();
			}

			GUI.backgroundColor = Color.white;
			GUILayout.EndHorizontal();
			GUILayout.Space(15.0f);
		}


		GUI.color = Color.white.CloneWithNewAlpha(0.6f);
		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
		GUI.color = Color.white;
	
		for(int n = 0; n<_atlasLinks.Count;n++)
		{
			PRAtlasLink link = _atlasLinks[n];
			string linkName = Path.GetFileNameWithoutExtension(link.atlasFilePath);

			EditorGUILayout.BeginHorizontal();
			link.shouldFoldout = EditorGUILayout.Foldout(link.shouldFoldout,linkName);
			GUILayout.FlexibleSpace();
			if(link.shouldFoldout)
			{
				GUI.backgroundColor = new Color(0.8f,0.8f,0.8f);
				EditorGUI.BeginDisabledGroup(n == 0);
				if(GUILayout.Button("\u25B3")) //up (u25B2 is bigger)
				{
					linkToMoveIndex = n;
					moveDelta = -1;
				}
				EditorGUI.EndDisabledGroup();
				
				EditorGUI.BeginDisabledGroup(n == _atlasLinks.Count-1);
				if(GUILayout.Button("\u25BD")) //down (u25BC is bigger)
				{
					linkToMoveIndex = n;
					moveDelta = 1;
				}
				EditorGUI.EndDisabledGroup();
				GUI.backgroundColor = Color.white;
			}

			GUI.backgroundColor = new Color(0.6f,0.8f,0.6f);
			bool shouldGenerate = GUILayout.Toggle(link.shouldGenerate,"");

			if(link.shouldGenerate != shouldGenerate)
			{
				link.shouldGenerate = shouldGenerate;
				_hasDataChanged = true;
			}

			GUILayout.Space(-5.0f);
			GUI.backgroundColor = Color.white;

			GUI.backgroundColor = new Color(0.8f,1.0f,0.8f);
			if(GUILayout.Button("Generate"))
			{
				if(!_atlasLinksToGenerate.Contains(link)) _atlasLinksToGenerate.Add(link);
			}
			GUI.backgroundColor = Color.white;

			EditorGUILayout.EndHorizontal();
			if(!link.shouldFoldout) 
			{
				GUI.color = Color.white.CloneWithNewAlpha(0.6f);
				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1)); //divider
				GUI.color = Color.white;
				continue;
			}

			EditorGUILayout.BeginVertical();

			GUILayout.Space(10.0f);

			GUILayout.BeginHorizontal();
			EditorGUILayout.SelectableLabel("" + Path.GetFileName(link.sourceFolderPath));
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Change Source"))
			{
				string sourceFolderPath = EditorUtility.OpenFolderPanel("Find source images folder","Assets","");
				
				if(sourceFolderPath.Length != 0)
				{
					link.sourceFolderPath = PRUtils.GetTrueProjectRelativePath(sourceFolderPath);

					_hasDataChanged = true;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.SelectableLabel("" + Path.GetFileName(link.atlasFilePath));
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Change Atlas"))
			{
				string atlasFilePath = EditorUtility.SaveFilePanel("Choose output atlas location (in /Resources)",link.atlasFilePath,Path.GetFileName(link.atlasFilePath)+".txt","txt");
				
				if(atlasFilePath.Length != 0)
				{ 
					atlasFilePath = PRUtils.GetTrueProjectRelativePath(atlasFilePath);
					
					link.atlasFilePath = Path.GetDirectoryName(atlasFilePath) + "/" + Path.GetFileNameWithoutExtension(atlasFilePath);
					
					_hasDataChanged = true;
				}
			}
			GUILayout.EndHorizontal();

//			bool shouldAutoGenerate = EditorGUILayout.Toggle("Auto Generate", link.shouldAutoGenerate);
//			if(link.shouldAutoGenerate != shouldAutoGenerate)
//			{
//				link.shouldAutoGenerate = shouldAutoGenerate;
//				_hasDataChanged = true;
//			}

			EditorGUI.BeginChangeCheck();

			link.shouldAddSubfolders = EditorGUILayout.Toggle(new GUIContent("Include subfolders?","Note:\nSubfolder names will be used in element names"), link.shouldAddSubfolders);

			link.shouldUseBytes = EditorGUILayout.Toggle(new GUIContent("Export as .bytes?","Stores the image as png bytedata for reduced file size"), link.shouldUseBytes);
			
			link.shouldTrim = EditorGUILayout.Toggle(new GUIContent("Trim sprites?", "Remove alpha from edges of the sprite"), link.shouldTrim);

			if(link.shouldTrim)
			{
				bool shouldPadTrim = EditorGUILayout.Toggle( new GUIContent("\tShould pad trim?", "Prevent trimming to the very edge"), link.trimPadding != 0);

				if(shouldPadTrim)
				{
					if(link.trimPadding == 0)
					{
						link.trimPadding = 1;
					}
				}
				else 
				{
					link.trimPadding = 0;
				}

				//link.trimPadding = EditorGUILayout.IntSlider("Trim padding",link.trimPadding,0,8);
			}

			//link.scale = Mathf.Max(0.0f, Mathf.Min(10.0f,EditorGUILayout.FloatField("Scale",link.scale)));
			float precision = 0.025f;
			link.scale = Mathf.Round(EditorGUILayout.Slider(new GUIContent("Scale","Shrink the contents of the atlas"), link.scale, precision,1.0f) / precision)*precision;


			//link.padding = Mathf.Max(0, Mathf.Min(16,EditorGUILayout.IntField("Padding",link.padding)));

			link.padding = EditorGUILayout.IntSlider(new GUIContent("Padding","Add spacing between elements"),link.padding,0,8);

			link.extrude = EditorGUILayout.IntSlider(new GUIContent("Extrude","Duplicate pixels on the edges"),link.extrude,0,8);

			if(EditorGUI.EndChangeCheck())
			{
				_hasDataChanged = true;
			}

			GUILayout.Space(15.0f);

			GUILayout.BeginHorizontal();

			GUI.backgroundColor = new Color(1.0f,1.0f,0.9f);
			if(GUILayout.Button("View Atlas"))
			{
				PRViewAtlasWindow.Show(link);
			}
			GUI.backgroundColor = Color.white;

			GUILayout.FlexibleSpace();
			GUI.backgroundColor = new Color(0.8f,0.8f,1.0f);
			if(GUILayout.Button("Duplicate Atlas"))
			{
				_atlasLinks.Insert(n+1,link.GetDuplicate());
			}
			GUI.backgroundColor = Color.white;

			GUI.backgroundColor = new Color(1.0f,0.8f,0.8f);
			if(GUILayout.Button("Delete Atlas"))
			{
				bool didConfirmDelete = EditorUtility.DisplayDialog("Delete atlas?", "Are you sure you want to delete " + linkName+"?", "Delete", "Cancel");
				if(didConfirmDelete)
				{
					_atlasLinks.RemoveAt(n);
					_hasDataChanged = true;	
					n--;
				}
			}
			GUI.backgroundColor = Color.white;
			GUILayout.EndHorizontal();


			GUILayout.Space(20.0f);

			EditorGUILayout.EndVertical();

			GUI.color = Color.white.CloneWithNewAlpha(0.6f);
			GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
			GUI.color = Color.white;
		}

		GUILayout.Space(20.0f);

		if(GUILayout.Button("Add Atlas"))
		{
			string sourceFolderPath = EditorUtility.OpenFolderPanel("Find source images folder","/Assets","");

			if(sourceFolderPath.Length != 0)
			{
				string sourceFolderName = Path.GetFileName(sourceFolderPath);

				sourceFolderPath = PRUtils.GetTrueProjectRelativePath(sourceFolderPath);

				string atlasFilePath = EditorUtility.SaveFilePanel("Choose result atlas location (in /Resources)",sourceFolderName+".txt",sourceFolderName,"txt");

				if(atlasFilePath.Length != 0)
				{ 
					atlasFilePath = PRUtils.GetTrueProjectRelativePath(atlasFilePath);

					//remove the extension
					atlasFilePath = Path.GetDirectoryName(atlasFilePath) + "/" + Path.GetFileNameWithoutExtension(atlasFilePath);
					
					_atlasLinks.Add(new PRAtlasLink(sourceFolderPath, atlasFilePath));
					_hasDataChanged = true;
				}
			}
		}

		GUILayout.Space(20.0f);

		GUI.contentColor = new Color(0.6f,0.6f,0.6f);
//		GUILayout.Label("Reminder: set the atlas to transparent,");
//		GUILayout.Space(-5.0f); 
//		GUILayout.Label("with a max size of 4096");
		GUI.skin.label.wordWrap = true;
		GUILayout.Label("Tip: set the atlas to transparent with a max size of 4096");
		GUI.contentColor = Color.white; 

		//TODO: Fix Atlas Image Settings (set transparent and 4096)

		EditorGUILayout.EndScrollView();

		if(linkToMoveIndex != -1)
		{
			if(moveDelta == 1) //if we're moving down, just move the link below up instead :)
			{
				linkToMoveIndex++;
			}

			PRAtlasLink link = _atlasLinks[linkToMoveIndex];

			_atlasLinks.RemoveAt(linkToMoveIndex);
			_atlasLinks.Insert(linkToMoveIndex-1,link);

			_hasDataChanged = true;
		}

		if(_hasDataChanged)
		{
			_hasDataChanged = false;
			SaveData();
		}

		GenerateAtlases(); 
	}

	private int _updateFrames = 0;

	public void Update()
	{
		_updateFrames++;
		
		if(_updateFrames % 1 == 0) //note: Update is called 100 times per second
		{
			GenerateAtlases();
			AdvanceActiveGenerator();
		} 
	}

	public void GenerateAtlases()
	{
		if(_activeGenerator == null)
		{
			if(_atlasLinksToGenerate.Count > 0)
			{
				PRAtlasLink link = _atlasLinksToGenerate[0];

				//create a generator
				_activeGenerator = new PRAtlasGenerator(link);
				_atlasLinksToGenerate.RemoveAt(0);

				AdvanceActiveGenerator();
			}
		}
	}

	private void AdvanceActiveGenerator ()
	{
		if(_activeGenerator != null)
		{
			if(_activeGenerator.Advance())
			{
				_progressMessage = _activeGenerator.progressMessage;
				Repaint(); //this will cause the progress message to be shown in the GUI
			}
			else 
			{
				_activeGenerator = null;
				GenerateAtlases();

				if(_activeGenerator == null) //if it's null, we just built the last atlas we had to generate!
				{
					Debug.Log("Packrat: Refreshing Asset Database");
					AssetDatabase.Refresh();
				}

				Repaint();
			}
		}
	}

	private void SaveData()
	{
		_watcher.EnableRaisingEvents = false;
		if(_atlasLinks.Count == 0) //delete data file if we have no atlas links
		{
			if(File.Exists(DATA_FILE_PATH)) File.Delete(DATA_FILE_PATH);
		}
		else 
		{
			string[] linkStrings = new string[_atlasLinks.Count];

			for(int n = 0; n<_atlasLinks.Count;n++)
			{
				PRAtlasLink link = _atlasLinks[n];

				linkStrings[n] = link.GetJSONString();
			}

			string jsonText = "{\"links\":[\n\n"+string.Join(",\n\n",linkStrings)+"\n\n]}";

			File.WriteAllText(DATA_FILE_PATH, jsonText);
		}
		_watcher.EnableRaisingEvents = true;
	}
}

public class PRViewAtlasWindow : EditorWindow
{
	public static PRViewAtlasWindow instance = null;

	private PRAtlasLink _link;
	private string _imagePath;
	private Texture2D _texture;

	private FileSystemWatcher _watcher;

	private bool _shouldShowActualSize = false;
	private bool _isSmooth = true;
	private bool _needsAtlasUpdate = false;

	public static void Show(PRAtlasLink link)
	{
		//string linkName = Path.GetFileNameWithoutExtension(link.atlasFilePath);
		// Get existing open window or if none, make a new one:
		PRViewAtlasWindow window = (PRViewAtlasWindow)EditorWindow.GetWindow(typeof (PRViewAtlasWindow));
		window.position = new Rect(Screen.width/2,Screen.height/2,512,512);
		window.title = "Viewing " + Path.GetFileNameWithoutExtension(link.atlasFilePath);
		window.ShowUtility(); 
		//window.maximized = true;
		window.Setup(link); 
	}

	public void Setup(PRAtlasLink link) 
	{
		if(_watcher != null)
		{
			_watcher.EnableRaisingEvents = false;
			_watcher.Dispose();
			_watcher = null;
		}

		_link = link;
		_imagePath = link.atlasFilePath;

		if(link.shouldUseBytes)
		{
			_imagePath += "_png.bytes";
		}
		else 
		{
			_imagePath += ".png";
		}

		LoadTexture();

		_watcher = new FileSystemWatcher(Path.GetDirectoryName(_imagePath));
		_watcher.EnableRaisingEvents = true;
		_watcher.Changed += (object sender, FileSystemEventArgs e) => {_needsAtlasUpdate = true;};
	}

	void LoadTexture()
	{
		if(_texture != null)
		{
			Object.DestroyImmediate(_texture,true);
		}

		_texture = new Texture2D(0,0,TextureFormat.ARGB32,false,false);
		_texture.wrapMode = TextureWrapMode.Clamp; //so we don't get pixels from the other edge when scaling
		if(_isSmooth)
		{
			_texture.filterMode = FilterMode.Bilinear;
		}
		else 
		{
			_texture.filterMode = FilterMode.Point;
		}
		_texture.LoadImage(File.ReadAllBytes(_imagePath)); 
	}

	public void UpdateAtlas()
	{
		LoadTexture();
		Repaint();
	}

	public void OnEnable()
	{
		instance = this;
	}

	public void OnDisable()
	{
		_needsAtlasUpdate = true;
		Object.DestroyImmediate(_texture,true);
		instance = null;
	}

	public void OnGUI()
	{
		if(_needsAtlasUpdate)
		{
			_needsAtlasUpdate = false;
			LoadTexture();
		}
		if(_texture == null) return;

		GUILayout.BeginHorizontal();
		{
			if(GUILayout.Button("RELOAD"))
			{
				LoadTexture();
			}

			if(GUILayout.Button(_isSmooth ? "SHARP" : "SMOOTH"))
			{
				_isSmooth = !_isSmooth;
				if(_isSmooth)
				{
					_texture.filterMode = FilterMode.Bilinear;
				}
				else 
				{
					_texture.filterMode = FilterMode.Point;
				}
			}

			if(GUILayout.Button(_shouldShowActualSize ? "SCALED TO FIT" : "ACTUAL SIZE"))
			{
				_shouldShowActualSize = !_shouldShowActualSize;
			}
		}
		GUILayout.EndHorizontal();


		float w = _shouldShowActualSize ? _texture.width : position.width;
		float h = _shouldShowActualSize ? _texture.height : position.height-32.0f;

		GUI.DrawTexture(new Rect(0,32,w,h),_texture,ScaleMode.ScaleToFit);
	}

	public PRAtlasLink link
	{
		get {return _link;}
	}
}

//
//public class PRAssetProcessor : AssetPostprocessor 
//{
//	public static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) 
//	{ 
//		//return; //comment this out if you want verbose logs when importing
//		
//		//Debug.Log("OnPostprocessAllAssets:");
//		if(importedAssets.Length != 0) importedAssets.Log("importedAssets");
//		if(deletedAssets.Length != 0) deletedAssets.Log("deletedAssets");
//		if(movedAssets.Length != 0) movedAssets.Log("movedAssets");
//		if(movedFromAssetPaths.Length != 0) movedFromAssetPaths.Log("movedFromAssetPaths");
//		
//		bool doesNeedRefresh = false; //set true if we do anything to the assets
//		
//		for(int s = 0;s<importedAssets.Length;s++)
//		{
////			if(importedAssets[s] == "Assets/Resources/Data/Episodes.xml")
////			{
////				TextAsset asset = AssetDatabase.LoadAssetAtPath(importedAssets[s],typeof(TextAsset)) as TextAsset;
////			}
//		}
//
//		if(doesNeedRefresh)
//		{
//			AssetDatabase.Refresh();
//		}
//		
//	}
//}