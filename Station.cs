namespace MiniPlayer
{
    class Station
    {
        public string uri { get; set; }
        public Point location { get; set; }
        public Size size { get; set; }
        public float zoom { get; set; } = 1.0f;

        public Station(string uri, Point location, Size size, float zoom)
        {
            this.uri = uri;
            this.location = location;
            this.size = size;
            this.zoom = zoom;
        }
    };
}
