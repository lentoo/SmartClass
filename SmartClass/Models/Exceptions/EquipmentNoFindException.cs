using System;

namespace SmartClass.Models.Exceptions
{
    [Serializable]
    public class EquipmentNoFindException : ApplicationException
    {
        public EquipmentNoFindException(string exception) : base(exception)
        {

        }
    }
}