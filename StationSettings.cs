using System.Text.Json.Serialization;

namespace MiniPlayer
{
    class StationSettings
    {
        public List<Station> stations { get; set; } = new();
        public int current_station { get; set; }
        public void Add(Station s)
        {
            stations.Add(s);
        }

        [JsonIgnore]
        public Station Current
        {
            get
            {
                return stations[current_station];
            }
            set
            {
                stations[current_station] = value;
            }
        }

        public Station this[int index]
        {
            get { return stations[index]; }
        }
        public Dictionary<string, int> GetStationIndices()
        {
            Dictionary<string, int> s = new Dictionary<string, int>();
            for (int i = 0; i < stations.Count; i++)
            {
                s[stations[i].uri] = i;
            }
            return s;
        }
        [JsonIgnore]
        public Station NextStation
        {
            get
            {
                current_station = (current_station + 1) % stations.Count;
                return stations[current_station];
            }
        }
        [JsonIgnore]
        public Station PrevStation
        {
            get
            {
                current_station = (current_station + stations.Count - 1) % stations.Count;
                return stations[current_station];
            }
        }
    }
}
