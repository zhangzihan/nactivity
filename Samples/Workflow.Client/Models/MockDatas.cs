using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Workflow.Client.Models
{
    public class MockDatas
    {
        public static readonly string TenantId = "3b450000-00f0-5254-168b-08d6f4673e73";

        public static readonly IList<User> Users = new List<User>
        {
            new User
            {
                Id = "ba98ecce-26fa-4c8d-ac94-887dce70169b",
                Name = "管理员",
                Duty = "管理员",
                TenantId = TenantId
            },
            new User
            {
                Id = "1fcc0d62-a31b-4281-a84c-cfd59b4d0004",
                Name = "提交人",
                Duty = "职员",
                TenantId = TenantId
            },
            new User
            {
                Id = "9502b19e-e474-4e56-ada4-9c723c40e51f",
                Name = "上级主管",
                Duty = "上级主管",
                TenantId = TenantId
            },
            new User
            {
                Id = "8919f655-b126-4601-b083-614dc797c0dd",
                Name = "部门经理",
                Duty = "部门经理",
                TenantId = TenantId
            },
            new User
            {
                Id = "1b222f10-84b1-4047-9744-63b3d24bd4e9",
                Name = "HR",
                Duty = "HR",
                TenantId = TenantId
            }
        };

        public static readonly IList<LeaveRequest> Requests = new List<LeaveRequest>
        {
        };

        public static User AdminUser
        {
            get
            {
                return Users.FirstOrDefault(x => x.Id == "ba98ecce-26fa-4c8d-ac94-887dce70169b");
            }
        }
    }
}
