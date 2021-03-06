/*
 * AppManager.cs
 * RpgFramework
 * Created by com.loccy on 10/28/2015 15:14:14.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using ICSharpCode.SharpZipLib.Zip;

public class GameManager : MonoBehaviour
{
	public static GameManager instance;

	private string message;

	void Awake()
	{
		instance = this;
	}

	void Start()
	{
		Init ();
	}

	/// <summary>
	/// 初始化
	/// </summary>
	void Init()
	{
		DontDestroyOnLoad (gameObject);

		Util.Add<UIManager> (gameObject);
		Util.Add<SoundManager> (gameObject);
		//Util.Add<TimeManager> (gameObject);
		//Util.Add<NetworkManager> (gameObject);
		Util.Add<ResourceManager> (gameObject);
		Util.Add<StateManager> (gameObject);
		//Util.Add<SocketClient>(gameObject);
		Util.Add<HttpClient>(gameObject);

		//释放资源
//		CheckExtractResource ();
//		ZipConstants.DefaultCodePage = 65001;
//		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		Application.targetFrameRate = Const.GameFrameRate;

		OnInitScene ();
	}

	/// <summary>
	/// 释放资源
	/// </summary>
	public void CheckExtractResource()
	{
		bool isExists = Directory.Exists (Util.DataPath) && File.Exists (Util.DataPath + "files.txt");
		if (isExists || Const.DebugMode)
		{
			StartCoroutine (OnUpdateResource ());
			return;   //文件已经解压过了，自己可添加检查文件列表逻辑
		}
		StartCoroutine (OnExtractResource ());    //启动释放协成 
	}

	IEnumerator OnExtractResource()
	{
		yield return new WaitForSeconds (1);
		string dataPath = Util.DataPath;  //数据目录
		string resPath = Util.AppContentPath (); //游戏包资源目录

		if (Directory.Exists (dataPath))
			Directory.Delete (dataPath, true);
		Directory.CreateDirectory (dataPath);

		string infile = resPath + "files.txt";
		string outfile = dataPath + "files.txt";
		if (File.Exists (outfile))
			File.Delete (outfile);

		message = "正在解包文件:>files.txt";
		if (Application.platform == RuntimePlatform.Android)
		{
			WWW www = new WWW (infile);
			yield return www;

			if (www.isDone)
			{
				File.WriteAllBytes (outfile, www.bytes);
			}
			yield return 0;
		}
		else
			File.Copy (infile, outfile, true);
		yield return new WaitForEndOfFrame ();

		//释放所有文件到数据目录
		string[] files = File.ReadAllLines (outfile);
		foreach (var file in files)
		{
			infile = resPath + file;  //
			outfile = dataPath + file;
			message = "正在解包文件:>" + file;
			Log.i ("正在解包文件:>" + infile);

			string dir = Path.GetDirectoryName (outfile);
			if (!Directory.Exists (dir))
				Directory.CreateDirectory (dir);

			if (Application.platform == RuntimePlatform.Android)
			{
				WWW www = new WWW (infile);
				yield return www;

				if (www.isDone)
				{
					File.WriteAllBytes (outfile, www.bytes);
				}
				yield return 0;
			}
			else
				File.Copy (infile, outfile, true);
			yield return new WaitForEndOfFrame ();
		}
		message = "解包完成!!!";
		Log.i (message);
		yield return new WaitForSeconds (0.1f);
		message = string.Empty;

		//释放完成，开始启动更新资源
		StartCoroutine (OnUpdateResource ());
	}

	/// <summary>
	/// 启动更新下载
	/// </summary>
	IEnumerator OnUpdateResource()
	{
		if (!Const.UpdateMode)
		{
			OnResourceInited ();
			yield break;
		}
		WWW www = null;
		string dataPath = Util.DataPath;  //数据目录
		string url = string.Empty;
#if UNITY_5
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			url = Const.WebUrl + "/ios/";
		}
		else
		{
			url = Const.WebUrl + "android/5x/";
		}
#else
		if (Application.platform == RuntimePlatform.IPhonePlayer){
		url = Const.WebUrl + "/iphone/";
		} else {
		url = Const.WebUrl + "android/4x/";
		}
#endif
		string random = DateTime.Now.ToString ("yyyymmddhhmmss");
		string listUrl = url + "files.txt?v=" + random;
		if (Debug.isDebugBuild)
			Log.w ("LoadUpdate---->>>" + listUrl);

		www = new WWW (listUrl);
		yield return www;
		if (www.error != null)
		{
			OnUpdateFailed (string.Empty);
			yield break;
		}
		if (!Directory.Exists (dataPath))
		{
			Directory.CreateDirectory (dataPath);
		}
		File.WriteAllBytes (dataPath + "files.txt", www.bytes);
		string filesText = www.text;
		string[] files = filesText.Split ('\n');

		for (int i = 0; i < files.Length; i++)
		{
			if (string.IsNullOrEmpty (files [i]))
				continue;
			string[] keyValue = files [i].Split ('|');
			string f = keyValue [0].Remove (0, 1);
			string localfile = (dataPath + f).Trim ();
			string path = Path.GetDirectoryName (localfile);
			if (!Directory.Exists (path))
			{
				Directory.CreateDirectory (path);
			}
			string fileUrl = url + f + "?v=" + random;
			bool canUpdate = !File.Exists (localfile);
			if (!canUpdate)
			{
				string remoteMd5 = keyValue [1].Trim ();
				string localMd5 = Util.md5file (localfile);
				canUpdate = !remoteMd5.Equals (localMd5);
				if (canUpdate)
					File.Delete (localfile);
			}
			if (canUpdate)
			{   //本地缺少文件
				Log.i (fileUrl);
				message = "downloading>>" + fileUrl;
				www = new WWW (fileUrl);
				yield return www;
				if (www.error != null)
				{
					OnUpdateFailed (path);
					yield break;
				}
				File.WriteAllBytes (localfile, www.bytes);
			}
		}
		yield return new WaitForEndOfFrame ();
		message = "更新完成!!";
		OnResourceInited ();
	}

	void OnUpdateFailed(string file)
	{
		message = "更新失败!>" + file;
	}

	/// <summary>
	/// 资源初始化结束
	/// </summary>
	public void OnResourceInited()
	{

		//初始化基础数据、时间同步等

		EventSystem.Instance.FireEvent(EventCode.GameStart, true);
	}

	/// <summary>
	/// 初始化场景
	/// </summary>
	public void OnInitScene()
	{
		Log.i("initScene-->>" + Application.loadedLevelName);
	}

	/// <summary>
	/// 析构函数
	/// </summary>
	void OnDestroy()
	{
		Log.i ("gameManager was destroyed");
	}
}

