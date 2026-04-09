using Training.SkillDevelopment.Models.Common;
using CompetencyModel = Training.SkillDevelopment.Models.Competency.Competency;

namespace Training.SkillDevelopment.Repositories.Competency;

public interface ICompetencyRepository
{
    Task<CompetencyModel?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<PagedResult<CompetencyModel>> GetByDomainAsync(string domain, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompetencyModel>> GetByCriticalAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<CompetencyModel>> SearchAsync(string searchTerm, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<CompetencyModel>> GetAllActiveAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<CompetencyModel> CreateAsync(CompetencyModel competency, CancellationToken cancellationToken = default);
    Task<CompetencyModel> UpdateAsync(CompetencyModel competency, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
