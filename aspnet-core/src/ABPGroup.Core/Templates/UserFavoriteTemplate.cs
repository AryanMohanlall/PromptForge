using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using ABPGroup.Authorization.Users;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABPGroup.Templates;

public class UserFavoriteTemplate : CreationAuditedEntity<long>, IMayHaveTenant
{
    public int? TenantId { get; set; }

    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    public int TemplateId { get; set; }

    [ForeignKey(nameof(TemplateId))]
    public Template Template { get; set; }
}
