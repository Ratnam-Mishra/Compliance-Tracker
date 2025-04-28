using System;
using System.Collections.Generic;

namespace WebApp.Infrastructure.Models
{
    public class TenantUsersDto
    {
        public string? Id { get; set; }
        public string? DisplayName { get; set; }
        public string? GivenName { get; set; }
        public string? Surname { get; set; }
        public string? UserPrincipalName { get; set; }
        public string? Mail { get; set; }
        public string? JobTitle { get; set; }
        public string? Department { get; set; }
        public string? OfficeLocation { get; set; }
        public string? MobilePhone { get; set; }
        public List<string>? BusinessPhones { get; set; }
        public string? CompanyName { get; set; }
        public List<AssignedLicense>? AssignedLicenses { get; set; }
        public bool? AccountEnabled { get; set; }
        public DateTimeOffset? CreatedDateTime { get; set; }
        public DateTime? LastModifiedDateTime { get; set; }
    }

    public class AssignedLicense
    {
        public Guid SkuId { get; set; }
        public List<Guid>? DisabledPlans { get; set; }
    }
}
