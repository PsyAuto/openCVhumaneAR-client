using System.Collections.Generic;

namespace AgentAPI
{
    public class Objects
    {
        public static float myRadius = 2.0f;
        public static string myUserId = "";
        public static List<string> globalUserIds = new List<string>();
        public static int currentStage = 0;
        public static UserData userData = new UserData();
        public static List<UserData> allUsersData  = new List<UserData>();
        public static List<int> allMarkerIds = new List<int>();

        [System.Serializable]
        public class UserData
        {
            public string userID;
            public string socketID;
            public int SelectedMarkerIndex = -1;
            public string[] NeighborKeywords = new string[0];
            public string[] MyKeywords = new string[0];
            public string[] NewKeywords = new string[0];
            public int CurrentStage;
            public string MyArticle;
            public string[] NeighborArticles = new string[0];
            public int __v;
        }

        public class Neighbour
        {
            public int selectedMarkerIndex;
            public string userID;
        }
    }
}