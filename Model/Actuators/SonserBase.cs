namespace Model.Actuators
{
    public class SonserBase
    {
        public string Id { get; set; }

        public string Name { get; set; }
        public string Online { get; set; }
        public string State { get; set; }
        public bool IsOpen { get; set; }
        public int Type { get; set; }
    }
}