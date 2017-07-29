namespace Model.Actuators
{
    public class SensorBase
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Online { get; set; }
        public string State { get; set; }       
        public int Type { get; set; }
        public bool Controllable { get; set; }
    }
}