using Microsoft.AspNetCore.Mvc;
using Workflows.Models;
using Workflows.Extensions;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Workflows.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Workflows.Services;

namespace Workflows.Controllers
{

    public class RequisitionWizardController : Controller
    {
        private readonly WorkflowsContext _context;
        private const string RequisitionWizardViewPath = "~/Views/Wizards/RequisitionWizard.cshtml";
        private readonly IApprovalService _approvalService;

        public RequisitionWizardController(WorkflowsContext context, IApprovalService approvalService)
        {
            _context = context;
            _approvalService = approvalService;
        }

        private List<string> GetSteps()
        {
            return new List<string> { "Intern", "Requisition", "Approvals", "Documents" };
        }
        // Step 1: Create Intern
        [HttpGet]
        public IActionResult Intern(Intern? intern = null)
        {
           
            // Check if the request method is GET to clear validation errors on initial load
            if (HttpContext.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.Clear();
            }
            intern = intern ?? new Intern();

            using (var db = new KtdaleaveContext())
            {
                var departments = db.Departments.ToList();

                // Create a SelectList from the departments
                var departmentItems = new SelectList(departments, "DepartmentCode", "DepartmentName");


                // Pass the SelectList to ViewBag
                ViewBag.DepartmentItems = departmentItems;

                ViewBag.Steps = GetSteps();
                ViewBag.CurrentStep = "Intern";
                ViewBag.CurrentStepIndex = 0;

                return View(RequisitionWizardViewPath,intern);
            }
        }
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateIntern([Bind("Id,DepartmentCode,'Idnumber',Firstname,Lastname,Othernames,Email,PhoneNumber,Status,CreatedAt,UpdatedAt")] Intern intern)
        {
            if (ModelState.IsValid)
            {
                intern.Status = "Inactive";
                intern.CreatedAt = DateTime.Now;
                intern.UpdatedAt = DateTime.Now;

                HttpContext.Session.SetObject("WizardIntern", intern); // Save to session
                return RedirectToAction("Requisition");
            }
            return Intern(intern);
        }
        // Step 1: Create Requisition
        [HttpGet]
        public IActionResult Requisition(Requisition? requisition = null)
        {
            var intern = HttpContext.Session.GetObject<Intern>("WizardIntern");
            if (intern == null)
            {
                // Handle the case where the intern is not in the session
                return RedirectToAction("CreateIntern");
            }
            // Check if the request method is GET to clear validation errors on initial load
            if (HttpContext.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.Clear();
            }
            /////QUERY THE DEPARTMENT FOR THE REQUISITION
            int internDepartmentCode = intern.DepartmentCode;
            using (var db = new KtdaleaveContext())
            {
                var department = db.Departments.FirstOrDefault(d => d.DepartmentCode == internDepartmentCode);

                // Create a SelectList from the departments
                //  var departmentItems = new SelectList(department, "DepartmentCode", "DepartmentName");

                ViewBag.DepartmentCode = department.DepartmentCode;
                ViewBag.DepartmentName = department.DepartmentName;
                ViewBag.Intern_id = intern.Id;
                ViewBag.InternFirstname = intern.Firstname;
                ViewBag.InternLastname = intern.Lastname;
                ViewBag.Steps = GetSteps();
                ViewBag.CurrentStep = "Requisition";
                return View(RequisitionWizardViewPath, requisition);
            }
        }

        [HttpPost]
        public IActionResult CreateRequisition([Bind("Id,DepartmentCode,Intern_id,AddedBy,Station,Status,Start_Date,End_Date,CreatedAt,UpdatedAt")] Requisition requisition)
        {
            if (ModelState.IsValid)
            {
                string employeePayrollNo = HttpContext.Session.GetString("EmployeePayrollNo");
                requisition.PayrollNo = employeePayrollNo;
                requisition.Status = "Inactive";
                requisition.CreatedAt = DateTime.Now;
                requisition.UpdatedAt = DateTime.Now;

                HttpContext.Session.SetObject("WizardRequisition", requisition);

                ////Create Approval Steps//////
                // Create the approval steps
                
                var approvalSteps = _approvalService.CreateApprovalSteps(requisition);

                // Store the approval steps in the session
                HttpContext.Session.SetObject("WizardApprovalSteps", approvalSteps);

                return RedirectToAction("Approval");
            }
            return Requisition(requisition);
        }

        public IActionResult Approval(Requisition? requisition = null)
        {
            // Retrieve the approval steps from the session
            var approvalSteps = HttpContext.Session.GetObject<List<Approval>>("WizardApprovalSteps");

            if (requisition == null)
            {
                // Handle the case where the approval steps are not found in the session
                return RedirectToAction("CreateRequisition");
            }

            ViewBag.Steps = GetSteps();
            ViewBag.CurrentStep = "Approvals";
            return View(RequisitionWizardViewPath, approvalSteps);
        }

        [HttpPost]
        public IActionResult CreateApproval(Approval approval)
        {
            if (ModelState.IsValid)
            {
              //  HttpContext.Session.Set("Approval", approval);
                return RedirectToAction("Documents");
            }
            return View(approval);
        }

        public IActionResult Document()
        {

            ViewBag.Steps = GetSteps();
            ViewBag.CurrentStep = "Document";
            return View("~/Views/Wizards/RequisitionWizard.cshtml");
        }

        [HttpPost]
        public IActionResult CreateDocument()
        {
            if (ModelState.IsValid)
            {
                //  HttpContext.Session.Set("Approval", approval);
                return RedirectToAction("AttachDocuments");
            }
            return View();
        }


    }

}
