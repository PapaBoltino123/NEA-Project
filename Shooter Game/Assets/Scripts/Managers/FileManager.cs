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

        if (data != null)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GameData));
            MemoryStream stream = new MemoryStream();
            serializer.Serialize(stream, data);

            string toStore = Encoding.ASCII.GetString(stream.ToArray());
            toStore = CheckToStore(toStore);
            toStore = Player.Instance.playerState.ToString() + "\n" + toStore;
            WriteToFile(toStore);
            Debug.Log("Saved");
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
            byte[] byteArray = Encoding.UTF8.GetBytes(toLoad);
            MemoryStream stream = new MemoryStream(byteArray); stream.Position = 0;

            try
            {
                GameData data = (GameData)serializer.Deserialize(stream);
                dataBroadcast.LoadGame(this, data);
                Debug.Log("Loaded");
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
        Debug.Log("New");
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

        string mostRecentFile = entries
            .OrderByDescending(entry =>
            {
                var parts = Path.GetFileName(entry).Split('.');
                return
                (
                    Year: int.Parse(parts[2]),
                    Month: int.Parse(parts[1]),
                    Day: int.Parse(parts[0]),
                    Hour: int.Parse(parts[3]),
                    Minute: int.Parse(parts[4]),
                    Second: int.Parse(parts[5])
                );
            })
            .First();

        string playerState = File.ReadLines(mostRecentFile).First();
        if (playerState != "ALIVE")
            return null;
        else
            return mostRecentFile;
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
    public float CalculateAverageScore()
    {
        int scoreSum = 0;
        string[] entries = Directory.GetFileSystemEntries(folderPath, "*", SearchOption.AllDirectories);

        string[] validEntriesForCalc = entries.Where(file => 
        (File.ReadAllLines(file).First() == "DEAD"
        )).ToArray();

        if (validEntriesForCalc.Length > 0)
        {
            foreach (string entry in validEntriesForCalc)
            {
                string toLoad = String.Join("", File.ReadAllLines(entry).Skip(1).ToArray());
                XmlSerializer serializer = new XmlSerializer(typeof(GameData));
                byte[] byteArray = Encoding.UTF8.GetBytes(toLoad);
                MemoryStream stream = new MemoryStream(byteArray); stream.Position = 0;

                try
                {
                    GameData data = (GameData)serializer.Deserialize(stream);
                    scoreSum += data.score;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            return scoreSum / validEntriesForCalc.Length;
        }
        else
            return -10;
    }
}
