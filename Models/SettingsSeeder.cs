using Workflows.Data;

namespace Workflows.Models
{
    public class SettingsSeeder
    {
        public static void SeedSettings(KtdaleaveContext ktdaContext, WorkflowsContext workflowsContext)
        {
            if (!workflowsContext.Setting.Any(s => s.Category == "Max No Of Interns"))
            {
                var departments = ktdaContext.Departments.ToList();

                foreach (var department in departments)
                {
                    var setting = new Setting
                    {
                        Category = "Max No Of Interns",
                        DepartmentCode = department.DepartmentCode, // Assuming DepartmentCode is an int
                        Key = "noOfInterns",
                        Value = "3",
                        Description = "No of Allowed Interns"
                    };

                    workflowsContext.Setting.Add(setting);
                }

                workflowsContext.SaveChanges();
            }
        }
    }
}
