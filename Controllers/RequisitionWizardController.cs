using Microsoft.AspNetCore.Mvc;
using Workflows.Models;
using Workflows.Extensions;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Workflows.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Workflows.Services;
using Workflows.ViewModels;
using Workflows.Attributes;

namespace Workflows.Controllers
{
  //  [CustomAuthorize] /// Used to ensure authenticated users view this class/pages
    public class RequisitionWizardController : Controller
    {
        private readonly WorkflowsContext _context;
        private const string RequisitionWizardViewPath = "~/Views/Wizards/RequisitionWizard.cshtml";
        private readonly IApprovalService _approvalService;
     //   private readonly IDocumentService _documentService;

        public RequisitionWizardController(WorkflowsContext context, IApprovalService approvalService 
         //   IDocumentService documentService
            )
        {
            _context = context;
            _approvalService = approvalService;
         //   _documentService = documentService;
        }

        private List<string> GetSteps()
        {
            return new List<string> { "Intern", "Requisition", "Approvals", "Documents", "Summary" };
        }
        //////////////////////////////////////////////// Step 1: Create Intern ///////////////////////////////////
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

        ////////////////////////////////////// Step 2: Create Requisition //////////////////////////////////////
        [HttpGet]
        public IActionResult Requisition(Requisition? requisition = null)
        {
            var intern = HttpContext.Session.GetObject<Intern>("WizardIntern");
            if (intern == null)
            {
                // Handle the case where the intern is not in the session
                return RedirectToAction("Intern");
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
                requisition.Start_Date = DateTime.Now;
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
                // Create the approval steps////
                /// Get the PayrollNo for the HOD,HR Officer,HOH
                int DepartmentCode = requisition.DepartmentCode;
                using (var db = new KtdaleaveContext())
                {
                    var department = db.Departments.FirstOrDefault(d => d.DepartmentCode == DepartmentCode);
                    string HOD;
                    if (department.DepartmentHd != null)
                    {
                         HOD = department.DepartmentHd; /// HOD PAYROLL
                    }
                    else
                    {
                        HOD = employeePayrollNo;
                    }

                    string HROfficer = "HUMAN RESOURCE ASSISTANT";
                    var HRemployee = db.EmployeeBkps.FirstOrDefault(d => d.Designation == HROfficer && d.EmpisCurrActive == 0);
                    var HROfficerPayrollNO = HRemployee.PayrollNo;

                    string HOHOfficer = "Head of HR Operations";
                    var HOHemployee = db.EmployeeBkps.FirstOrDefault(d => d.Designation == HOHOfficer && d.EmpisCurrActive == 0);
                    var HOH = HOHemployee.PayrollNo; //HOH Payroll

                    //Create the Steps using IApproval Service////
                    var approvalSteps = _approvalService.CreateApprovalSteps(requisition, HOD, HROfficerPayrollNO,HOH);

                    // Store the approval steps in the session
                    HttpContext.Session.SetObject("WizardApprovalSteps", approvalSteps);

                    return RedirectToAction("Approval");
                }
            }
            return Requisition(requisition);
        }

        ////////////////////////////////////// Step 3: Get Approvals //////////////////////////////////////
        [HttpGet]
        public IActionResult Approval(Requisition? requisition = null)
        {
            // Retrieve the approval steps from the session
            var approvalSteps = HttpContext.Session.GetObject<List<Approval>>("WizardApprovalSteps");

            if (requisition == null || approvalSteps == null)
            {
                // Handle the case where the approval steps are not found in the session
                return RedirectToAction("Requisition");
            }

            // Create a list to hold the view models
            // Create a dictionary to hold the employee names
            var employeeName = new Dictionary<string, string>();

            using (var db = new KtdaleaveContext())
                {
                foreach (var approvalStep in approvalSteps)
                {
                    var employee = db.EmployeeBkps.FirstOrDefault(d => d.PayrollNo == approvalStep.PayrollNo);
                    // Add to the dictionary
                    if (employee != null && !employeeName.ContainsKey(approvalStep.PayrollNo))
                    {
                        employeeName[approvalStep.PayrollNo] = employee.Fullname;
                    }
                }
                }
                ViewBag.EmployeeNames = employeeName;
                ViewBag.Steps = GetSteps();
                ViewBag.CurrentStep = "Approvals";
                return View(RequisitionWizardViewPath, approvalSteps);
            
        }

        [HttpPost]
        public IActionResult CreateApproval(Approval approval)
        {
            
              //  HttpContext.Session.Set("Approval", approval);
                return RedirectToAction("Document");
            
        }
        ////////////////////////////////////// Step 4: Create Documents //////////////////////////////////////
        [HttpGet]
        public IActionResult Document(Document? document = null)
        {
            var requisition = HttpContext.Session.GetObject<Requisition>("WizardRequisition");
           if (requisition == null)
            {
                // Handle the case where the approval steps are not found in the session
                return RedirectToAction("Requisition");
            }
            // Check if the request method is GET to clear validation errors on initial load
            if (HttpContext.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.Clear();

            }

            document = document ?? new Document();
            var documentTypes = _context.DocumentType.ToList();
            ViewBag.DocumentTypes = documentTypes;
            ViewBag.Steps = GetSteps();
            ViewBag.CurrentStep = "Documents";
            return View(RequisitionWizardViewPath, document);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDocument(Document document, List<IFormFile> files)
        {
            // Retrieve the requisition object from the session
            var requisition = HttpContext.Session.GetObject<Requisition>("WizardRequisition");
           
                var documentTypes = _context.DocumentType.ToList();
                var documentList = new List<Document>();
            // Ensure each file in the files list is not null and has content
            if (files.Count != documentTypes.Count)
            {
                ModelState.AddModelError("", "Please upload all required files.");
                return Document(document);
            }
            // Loop through returned and generate data for the Document entity
            for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    var documentType = documentTypes[i];

                    if (file != null && file.Length > 0)
                    {
                        // Save the file temporarily
                        var tempFilePath = Path.Combine("wwwroot/uploads/", file.FileName);
                    // Ensure the temporary directory exists
               //     if (!Directory.Exists(tempFilePath))
                 //   {
                  //      Directory.CreateDirectory(tempFilePath);
                 //   }
                    using (var stream = new FileStream(tempFilePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
    
                            document = new Document
                            {
                                Requisition_id = requisition.Id,
                                Intern_id = requisition.Intern_id,
                                DocumentType = documentType.Id,
                                FileName = file.FileName,
                                FileType = file.ContentType,
                                FileSize = file.Length,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            };

                            documentList.Add(document);
            
                    }
                }
            
                HttpContext.Session.SetObject("WizardDocumentList", documentList);
                return RedirectToAction("Summary");
                
               
        }
        ////////////////////////////////////// Step 5: Finish the wizard //////////////////////////////////////
        [HttpGet]
        public IActionResult Summary()
        {
            var intern = HttpContext.Session.GetObject<Intern>("WizardIntern");
            var requisition = HttpContext.Session.GetObject<Requisition>("WizardRequisition");
            var approvalSteps = HttpContext.Session.GetObject<List<Approval>>("WizardApprovalSteps");
            var documentList = HttpContext.Session.GetObject<List<Document>>("WizardDocumentList");

            if (intern == null || requisition == null || approvalSteps == null || documentList == null)
            {
                // Handle missing session data
                return RedirectToAction("Requisition"); // or another appropriate action
            }
            var viewModel = new SummaryViewModel
            {
                Intern = intern,
                Requisition = requisition,
                ApprovalSteps = approvalSteps,
                DocumentList = documentList
            };

            ViewBag.Steps = GetSteps();
            ViewBag.CurrentStep = "Summary";
            return View(RequisitionWizardViewPath, viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> CompleteWizard()
        {

            var intern = HttpContext.Session.GetObject<Intern>("WizardIntern");
            var requisition = HttpContext.Session.GetObject<Requisition>("WizardRequisition");
            var approvalSteps = HttpContext.Session.GetObject<List<Approval>>("WizardApprovalSteps");
            var documentList = HttpContext.Session.GetObject<List<Document>>("WizardDocumentList");
            if (requisition != null && intern != null && approvalSteps != null && documentList != null)
            {
                // Save the intern
                _context.Intern.Add(intern);
                await _context.SaveChangesAsync();

                // Save the requisition
                requisition.Intern_id = intern.Id;
                _context.Requisition.Add(requisition);
                await _context.SaveChangesAsync();

                // Update the Requisition_id for each approval
                approvalSteps.ForEach(a => a.Requisition_id = requisition.Id);

                // Save the approval steps
                _context.Approval.AddRange(approvalSteps);
                await _context.SaveChangesAsync();

                // Update the Requisition_id for each document
                documentList.ForEach(d => d.Requisition_id = requisition.Id);

                // Move the temporary files to a permanent location
                int DepartmentCode = requisition.DepartmentCode;
                using (var db = new KtdaleaveContext())
                {
                    var department = db.Departments.FirstOrDefault(d => d.DepartmentCode == DepartmentCode);
                    if (department != null)
                    {
                        string departmentName = department.DepartmentName;

                        foreach (var document in documentList)
                        {
                            var tempFilePath = Path.Combine("wwwroot/uploads/", document.FileName);
                       //     var departmentDirectory = Path.Combine("wwwroot/uploads/permanent", departmentName);
                        //    var permanentFilePath = Path.Combine(departmentDirectory, document.FileName);

                            // Ensure the department directory exists
                      //      if (!Directory.Exists(departmentDirectory))
                      //      {
                       //         Directory.CreateDirectory(departmentDirectory);
                       //     }

                            // Move the file to the permanent location
                            if (System.IO.File.Exists(tempFilePath))
                            {
                                System.IO.File.Move(tempFilePath, tempFilePath);
                            }

                            // Update the file path in the document object
                            document.FilePath = tempFilePath;
                        }
                    }
                }


                // Save the documents
                _context.Document.AddRange(documentList);
                await _context.SaveChangesAsync();



                // Clear the session
                HttpContext.Session.Remove("WizardRequisition");
                HttpContext.Session.Remove("WizardIntern");
                HttpContext.Session.Remove("WizardApprovalSteps");
                HttpContext.Session.Remove("WizardDocumentList");

                // Redirect to the Requisition Index action
                return RedirectToAction("Index", "Requisitions");
            }

            return RedirectToAction("Summary");


        }


        }

}
