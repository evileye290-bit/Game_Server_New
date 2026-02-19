using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumerateUtility;

namespace ManagerServerLib
{
    public class SchoolManager
    {
        private SortedDictionary<int, int> studentCount = new SortedDictionary<int, int>();

        public void SetSchoolStudentCount(SortedDictionary<int, int> dic)
        {
            this.studentCount = dic;

            foreach (SchoolType type in Enum.GetValues(typeof(SchoolType)))
            {
                int enumType = (int)type;
                if (!studentCount.ContainsKey(enumType))
                {
                    studentCount.Add(enumType, 0);
                }
            }
        }

        public int GetSchoolId()
        {
            int id = studentCount.First().Key;
            studentCount[id] += 1;
            return id;
        }
    }
}
