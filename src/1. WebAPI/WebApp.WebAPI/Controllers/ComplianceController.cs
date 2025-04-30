using Microsoft.AspNetCore.Mvc;
using WebApp.Infrastructure.Helpers;
using WebApp.Infrastructure.Models;

namespace WebApp.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComplianceController : ControllerBase
    {
        private readonly GraphSharePointHelper _graphSharePointHelper;

        public ComplianceController()
        {
            _graphSharePointHelper = new GraphSharePointHelper();
        }

        [HttpGet("documents")]
        public async Task<IActionResult> GetDocuments()
        {
            var result = await _graphSharePointHelper.GetAllFilesFromLibrary();

            var filePairs = result
                .Where(file => file.WebUrl != null)
                .Select(file => new
                {
                    FileName = file.Name,
                    FileUrl = file.WebUrl
                })
                .ToList();

            return Ok(filePairs);
        }

        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployeeDetails()
        {
            string fieldsToSelect = "UserDisplayName,";

            var messagesList = await _graphSharePointHelper.FetchDataFromSharePointList("msgs", fieldsToSelect);
            var emailsList = await _graphSharePointHelper.FetchDataFromSharePointList("emails", fieldsToSelect);

            var allEmployees = new List<(string Email, string DisplayName)>();

            void AddEmployees(List<Dictionary<string, object>> sourceList)
            {
                foreach (var item in sourceList)
                {
                    if (item.TryGetValue("UserDisplayName", out var nameObj) &&
                        item.TryGetValue("UserDepartment", out var deptObj))
                    {
                        var department = deptObj?.ToString();
                        var displayName = nameObj?.ToString();

                        if (!string.IsNullOrEmpty(displayName))
                        {
                            allEmployees.Add((displayName, department ?? ""));
                        }
                    }
                }
            }

            AddEmployees(messagesList);
            AddEmployees(emailsList);

            var groupedEmployees = allEmployees
                .GroupBy(emp => emp.Email.ToLower())
                .Select(g => new
                {
                    DisplayName = g.First().DisplayName,
                    Department = g.Key,
                    Count = g.Count()
                })
                .ToList();

            return Ok(groupedEmployees);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFileToSP([FromBody] UploadFileDto fileObj)
        {
            if (fileObj == null || string.IsNullOrWhiteSpace(fileObj.name) || string.IsNullOrWhiteSpace(fileObj.fileBase64))
                return BadRequest("Missing file name or content.");

            var result = await _graphSharePointHelper.UploadFileToSharePoint(fileObj.name, fileObj.fileBase64);

            return Ok(new { status = !string.IsNullOrEmpty(result) });
        }
    }
}


//using Microsoft.AspNetCore.Mvc;
//using WebApp.Infrastructure.Helpers;
//using WebApp.Infrastructure.Models;

//namespace WebApp.WebAPI.Controllers
//{
//    public class ComplianceController : ControllerBase
//    {
//        GraphSharePointHelper graphSharePointHelper;
//        public ComplianceController()
//        {
//            graphSharePointHelper = new GraphSharePointHelper();
//        }
//        //public IActionResult Index()
//        //{
//        //    return View();
//        //}


//        [Route("GetDocuments")]
//        [HttpGet]
//        public async Task<IActionResult> GetDocuments()
//        {
//            var result = await graphSharePointHelper.GetAllFilesFromLibrary();
//            var filePairs = result
//                            .Where(file => file.WebUrl != null)
//                            .Select(file => new
//                            {
//                                FileName = file.Name,
//                                FileUrl = file.WebUrl
//                            })
//                            .ToList();

//            return Ok(filePairs);
//        }


//        [Route("GetEmployeeDetails")]
//        [HttpGet]
//        public async Task<IActionResult> GetEmployeeDetails()
//        {
//            string fieldsToSelect = "UserDisplayName,";

//            var messagesList = await graphSharePointHelper.FetchDataFromSharePointList("msgs", fieldsToSelect);
//            var emailsList = await graphSharePointHelper.FetchDataFromSharePointList("emails", fieldsToSelect);
//            var allEmployees = new List<(string Email, string DisplayName)>();

//            void AddEmployees(List<Dictionary<string, object>> sourceList)
//            {
//                foreach (var item in sourceList)
//                {
//                    if (item.TryGetValue("UserDisplayName", out var nameObj) &&
//                        item.TryGetValue("UserDepartment", out var deptObj))
//                    {
//                        var department = deptObj?.ToString();
//                        var displayName = nameObj?.ToString();

//                        if (!string.IsNullOrEmpty(displayName))
//                        {
//                            allEmployees.Add((displayName, department ?? ""));
//                        }
//                    }
//                }
//            }

//            AddEmployees(messagesList);
//            AddEmployees(emailsList);

//            var groupedEmployees = allEmployees
//                .GroupBy(emp => emp.Email.ToLower())
//                .Select(g => new
//                {
//                    DisplayName = g.First().DisplayName,
//                    Department = g.Key,
//                    Count = g.Count()
//                })
//                .ToList();

//            return Ok(groupedEmployees);
//        }


//        [Route("UploadFileToSP")]
//        [HttpGet]
//        public async Task<IActionResult> UploadFileToSP(UploadFileDto fileObj)
//        {
//            var result = await graphSharePointHelper.UploadFileToSharePoint(fileObj.name, fileObj.fileBase64);
//            if(!string.IsNullOrEmpty(result))
//                return Ok(new { status = true });
//            return Ok(new { status = false });
//        }
//    }
//}
