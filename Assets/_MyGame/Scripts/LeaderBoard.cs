using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace MyGame
{
public class LeaderBoard : MonoBehaviour
    {

        [System.Serializable]
        class LeaderBoardData
        {
            public int bestScore = 0;
            public string playerName = "";
        };

        private LeaderBoardData data;
        private string savePath = Application.persistentDataPath + "/leader_board.save";

        public LeaderBoard()
        {
            if (File.Exists(savePath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(savePath, FileMode.Open);
                data = (LeaderBoardData)bf.Deserialize(file);
                file.Close();
            }
            else
            {
                data = new LeaderBoardData();
            }
        }

        public int BestScore()
        {
            return data.bestScore;
        }

        public void RecordScore(int currentScore, string playerName)
        {
            if (currentScore > data.bestScore)
            {
                data.bestScore = currentScore;
                data.playerName = playerName;

                // update leader board
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(savePath, FileMode.OpenOrCreate);
                bf.Serialize(file, data);
                file.Close();
            }
        }
    }
}
