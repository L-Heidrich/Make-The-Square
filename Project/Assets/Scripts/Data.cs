using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class Data : MonoBehaviour
{
    public static void SaveProfile(ProfileData profile)
    {
        try
        {
            string path = Application.persistentDataPath + "/profile.dt";

            if (File.Exists(path))
            {
                File.Delete(path);
            }
            FileStream file = File.Create(path);

            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, profile);
            file.Close();
        }
        catch
        {
            Debug.Log("Error while saving");
        }
    }

    public static ProfileData LoadProfile()
    {
        ProfileData dat = new ProfileData();

        try
        {
            string path = Application.persistentDataPath + "/profile.dt";

            if (File.Exists(path))
            {
                FileStream file = File.Open(path, FileMode.Open);
                BinaryFormatter bf = new BinaryFormatter();
                dat = (ProfileData)bf.Deserialize(file);
            }
        }
        catch
        {
            Debug.Log("Error while loading");

        }
       


        return dat;
    }
}
