using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.AddtionalEventStructures;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Text;
using System.Linq;

public class FileManager
{
    private string folderPath = Application.persistentDataPath + @"/SavedFiles";
    public EventBroadcaster dataBroadcast;

    public FileManager(EventBroadcaster dataBroacast)
    {
        this.dataBroadcast = dataBroacast;

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log("Folder created.");
        }
        else
            Debug.Log("Folder already exists");
    }
    public void SaveGame()
    {
        dataBroadcast.SaveGame(this);
        GameData data = GameManager.Instance.savedData;

        if (data != null )
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GameData));
            MemoryStream stream = new MemoryStream();
            serializer.Serialize(stream, data);

            string toStore = Encoding.ASCII.GetString(stream.ToArray());
            toStore = CheckToStore(toStore);
            toStore = Player.Instance.playerState.ToString() + "\n" + toStore;
            WriteToFile(toStore);
            GameManager.Instance.savedData = new GameData();
        }
    }
    public void LoadGame()
    {
        string filePath = RecentFile();
        if (filePath == null)
            NewGame();
        else
        {
            string playerState = File.ReadLines(filePath).First();

            if (playerState != "ALIVE")
                NewGame();

            string toLoad = String.Join("", File.ReadAllLines(filePath).Skip(1).ToArray());
            XmlSerializer serializer = new XmlSerializer(typeof(GameData));
            object obj = null;
            byte[] byteArray = Encoding.UTF8.GetBytes(toLoad);
            MemoryStream stream = new MemoryStream(byteArray); stream.Position = 0;

            try
            {
                GameData data = (GameData)serializer.Deserialize(stream);
                dataBroadcast.LoadGame(this, data);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
    public void NewGame()
    {
        dataBroadcast.NewGame(this);
    }
    private void WriteToFile(string toStore)
    {
        var dateTime = DateTime.Now;
        var date = dateTime.ToString("dd/MM/yyyy");
        var time = dateTime.ToString("HH:mm:ss");
        string path = date + "." + time + ".txt";
        path = path.Replace('/', '.');
        path = path.Replace(':', '.');
        path = folderPath + @"/" + path;

        try
        {
            File.WriteAllText(path, toStore);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
    public string RecentFile()
    {
        if (!Directory.EnumerateFileSystemEntries(folderPath).Any())
            return null;

        string[] entries = Directory.GetFileSystemEntries(folderPath, "*", SearchOption.AllDirectories);
        Array.Sort(entries); 
        Array.Reverse(entries);

        string playerState = File.ReadLines(entries[0]).First();
        if (playerState != "ALIVE")
            return null;

        return entries[0];
    }
    private static string CheckToStore(string s)
    {
        string output = "";
        if (s[0] != '<')
        {
            output = s.Remove(0, LinearSearch(s, '<'));
            return output;
        }
        else
            return s;

        int LinearSearch(string input, char ch)
        {
            int pos = 0;

            for (pos = 0; pos < input.Length; pos++)
            {
                if (input[pos] == ch)
                    break;
            }
            return pos;
        }
    }
}
