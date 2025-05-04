using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApp.Infrastructure.Agents;
using WebApp.Infrastructure.Helpers;
using WebApp.Infrastructure.Models;

namespace WebApp.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComplianceController : ControllerBase
    {
        private readonly GraphSharePointHelper _graphSharePointHelper;
        private readonly EmbeddingService _embeddingService;
        private readonly SearchService _searchService;
        private readonly ComplainceAgent _complianceAgent;

        public ComplianceController()
        {
            _graphSharePointHelper = new GraphSharePointHelper();
            _complianceAgent = new ComplainceAgent();
        }
        /// <summary>
        /// Fetch all the compliance & security policies from SharePoint
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Fetch all reported users from SharePoint
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Fetch all the reported violations from SharePoint
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Fetch all the reported keywords from SharePoint
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetKeywords")]
        public async Task<IActionResult> GetKeywords()
        {
            string fieldsToSelect = "UserEmail,UserDisplayName,MainKeyword,DetectedDateTime";

            var messagesList = await _graphSharePointHelper.FetchDataFromSharePointList("msgs", fieldsToSelect);
            var emailsList = await _graphSharePointHelper.FetchDataFromSharePointList("emails", fieldsToSelect);

            var allKeywords = new List<object>();

            void AddKewords(List<Dictionary<string, object>> sourceList)
            {
                foreach (var item in sourceList)
                {
                    if (item.TryGetValue("UserEmail", out var emailObj) &&
                        item.TryGetValue("MainKeyword", out var violationObj) && item.TryGetValue("DetectedDateTime", out var timeObj))
                    {
                        var email = emailObj?.ToString();
                        var violation = violationObj?.ToString();
                        var displayName = item.TryGetValue("UserDisplayName", out var displayNameObj)
                            ? displayNameObj?.ToString()
                            : "";
                        var time = timeObj?.ToString();

                        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(violation))
                        {
                            allKeywords.Add(new
                            {
                                Email = email.ToLower(),
                                DisplayName = displayName ?? "",
                                ViolationExplanation = violation,
                                Time = time
                            });
                        }
                    }
                }
            }

            AddKewords(messagesList);
            AddKewords(emailsList);
            return Ok(allKeywords);
        }

        /// <summary>
        /// Fetch all the reported keywords based on date range from SharePoint
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetKeywordsByDate")]
        public async Task<IActionResult> GetKeywordsByDate(string startDate, string endDate)
        {
            string fieldsToSelect = "UserEmail,UserDisplayName,MainKeyword,ModifiedDate";

            var messagesList = await _graphSharePointHelper.FetchDataFromSharePointList("msgs", fieldsToSelect);
            var emailsList = await _graphSharePointHelper.FetchDataFromSharePointList("emails", fieldsToSelect);

            var allKeywords = new List<object>();

            if (!DateTime.TryParse(startDate, out var startDateTime) || !DateTime.TryParse(endDate, out var endDateTime))
            {
                return BadRequest("Invalid date range.");
            }

            void AddKewords(List<Dictionary<string, object>> sourceList)
            {
                foreach (var item in sourceList)
                {
                    if (item.TryGetValue("ModifiedDate", out var modifiedObj) &&
                        DateTime.TryParse(modifiedObj?.ToString(), out var modifiedDate))
                    {
                        if (modifiedDate >= startDateTime && modifiedDate <= endDateTime)
                        {
                            if (item.TryGetValue("UserEmail", out var emailObj) &&
                                item.TryGetValue("MainKeyword", out var violationObj))
                            {
                                var email = emailObj?.ToString();
                                var violation = violationObj?.ToString();
                                var displayName = item.TryGetValue("UserDisplayName", out var displayNameObj)
                                    ? displayNameObj?.ToString()
                                    : "";

                                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(violation))
                                {
                                    allKeywords.Add(new
                                    {
                                        Email = email.ToLower(),
                                        DisplayName = displayName ?? "",
                                        ViolationExplanation = violation
                                    });
                                }
                            }
                        }
                    }
                }
            }

            AddKewords(messagesList);
            AddKewords(emailsList);

            return Ok(allKeywords);
        }


        /// <summary>
        /// Search violation
        /// </summary>
        /// <param name="userMessage"></param>
        /// <returns></returns>
        [HttpPost("FetchMsgFromSearch")]
        public async Task<IActionResult> FetchMsgFromSearch([FromBody] string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return BadRequest("User message is required.");
            }

            try
            {
                var agentInstance = await _complianceAgent.CreateOrReuseComplianceAgentAsync();

                var prompt = $"Check the following content against the compliance and security documents stored in Azure AI Search: {userMessage}";
                var similarDocs = await _complianceAgent.AskAgentAsync(agentInstance, prompt);

                if (similarDocs != null && similarDocs.Any())
                {
                    return Ok(similarDocs);
                }

                return Ok("Not applicable!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }


        /// <summary>
        /// Upload file in the document library SharePoint
        /// </summary>
        /// <param name="fileObj"></param>
        /// <returns></returns>
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