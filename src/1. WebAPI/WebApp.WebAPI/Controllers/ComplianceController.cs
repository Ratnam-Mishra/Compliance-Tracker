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

        [HttpGet("GetDocuments")]
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

        [HttpGet("GetEmployeeDetails")]
        public async Task<IActionResult> GetEmployeeDetails()
        {
            string fieldsToSelect = "UserEmail,UserDisplayName,UserDepartment";

            var messagesList = await _graphSharePointHelper.FetchDataFromSharePointList("msgs", fieldsToSelect);
            var emailsList = await _graphSharePointHelper.FetchDataFromSharePointList("emails", fieldsToSelect);

            var allEmployees = new List<(string Email, string DisplayName, string Department)>();

            void AddEmployees(List<Dictionary<string, object>> sourceList)
            {
                foreach (var item in sourceList)
                {
                    if (item.TryGetValue("UserEmail", out var emailObj) &&
                        item.TryGetValue("UserDisplayName", out var nameObj))
                    {
                        var email = emailObj?.ToString();
                        var displayName = nameObj?.ToString();
                        var department = item.TryGetValue("UserDepartment", out var deptObj)
                            ? deptObj?.ToString()
                            : "";

                        if (!string.IsNullOrEmpty(email))
                        {
                            allEmployees.Add((email.ToLower(), displayName ?? "", department));
                        }
                    }
                }
            }

            AddEmployees(messagesList);
            AddEmployees(emailsList);

            var groupedEmployees = allEmployees
                .GroupBy(emp => emp.Email)
                .Select(g => new
                {
                    Email = g.Key,
                    DisplayName = g.First().DisplayName,
                    Department = g.First().Department,
                    Count = g.Count()
                })
                .ToList();

            return Ok(groupedEmployees);
        }

        [HttpGet("GetViolationDetails")]
        public async Task<IActionResult> GetViolationDetails()
        {
            string fieldsToSelect = "UserEmail,UserDisplayName,ViolationExplanation";

            var messagesList = await _graphSharePointHelper.FetchDataFromSharePointList("msgs", fieldsToSelect);
            var emailsList = await _graphSharePointHelper.FetchDataFromSharePointList("emails", fieldsToSelect);

            var allViolations = new List<object>();

            void AddViolations(List<Dictionary<string, object>> sourceList)
            {
                foreach (var item in sourceList)
                {
                    if (item.TryGetValue("UserEmail", out var emailObj) &&
                        item.TryGetValue("ViolationExplanation", out var violationObj))
                    {
                        var email = emailObj?.ToString();
                        var violation = violationObj?.ToString();
                        var displayName = item.TryGetValue("UserDisplayName", out var displayNameObj)
                            ? displayNameObj?.ToString()
                            : "";

                        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(violation))
                        {
                            allViolations.Add(new
                            {
                                Email = email.ToLower(),
                                DisplayName = displayName ?? "",
                                ViolationExplanation = violation
                            });
                        }
                    }
                }
            }

            AddViolations(messagesList);
            AddViolations(emailsList);
            return Ok(allViolations);
        }


        [HttpPost("UploadFileToSP")]
        public async Task<IActionResult> UploadFileToSP([FromBody] UploadFileDto fileObj)
        {
            if (fileObj == null || string.IsNullOrWhiteSpace(fileObj.name) || string.IsNullOrWhiteSpace(fileObj.fileBase64))
                return BadRequest("Missing file name or content.");

            var result = await _graphSharePointHelper.UploadFileToSharePoint(fileObj.name, fileObj.fileBase64);

            return Ok(new { status = !string.IsNullOrEmpty(result) });
        }
    }
}