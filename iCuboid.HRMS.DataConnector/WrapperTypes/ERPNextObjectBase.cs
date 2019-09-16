using iCuboid.HRMS.DataConnector.PublicTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iCuboid.HRMS.DataConnector.WrapperTypes
{
    public abstract class ERPNextObjectBase
    {
        public ERPObject Object { get; internal set; }

        internal ERPNextObjectBase()
        {
        }

        protected ERPNextObjectBase(ERPObject obj)
        {
            this.Object = obj;
        }

        protected dynamic data => Object.Data;

        public string name
        {
            get { return Object.Name; }
            set { Object.Name = value; }
        }

        public DocType ObjectType => Object.ObjectType;

        protected static T parseEnum<T>(string enumString)
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enum");
            }

            return (T)Enum.Parse(typeof(T), enumString, true);
        }
    }
}
