using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iCuboid.HRMS.DataConnector.PublicTypes
{
    public struct DocType
    {
        #region Internal

        public string Name { get; }

        public DocType(string name)
        {
            this.Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Equality implementation

        public static bool operator ==(DocType a, DocType b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(DocType a, DocType b)
        {
            return !(a == b);
        }

        public bool Equals(DocType other)
        {
            return String.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is DocType && Equals((DocType)obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        #endregion

       
        public static readonly DocType Employee = new DocType("Employee");
        public static readonly DocType Attendance = new DocType("Attendance");
        public static readonly DocType Timesheet = new DocType("Timesheet");
        public static readonly DocType ShiftType = new DocType("Shift Type");
        public static readonly DocType ShiftAssignment = new DocType("Shift Assignment");
    }
}
