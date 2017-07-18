using System;

namespace SmartClass.Models.Exceptions
{
    public class EquipmentNoFindException : ApplicationException
    {
        public EquipmentNoFindException(string exception) : base(exception)
        {
        }

    }
}