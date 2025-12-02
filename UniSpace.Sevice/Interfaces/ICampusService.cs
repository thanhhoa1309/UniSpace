using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniSpace.BusinessObject.DTOs.CampusDTOs;

namespace UniSpace.Service.Interfaces
{
    public interface ICampusService
    {
        // Create
        Task<CampusDto?> CreateCampusAsync(CreateCampusDto createDto);

        // Read
        Task<List<CampusDto>> GetAllCampusesAsync();
        Task<CampusDto?> GetCampusByIdAsync(Guid id);
        Task<List<CampusDto>> GetCampusesByNameAsync(string name);

        // Update
        Task<CampusDto?> UpdateCampusAsync(UpdateCampusDto updateDto);

        // Delete
        Task<bool> DeleteCampusAsync(Guid id);
        Task<bool> SoftDeleteCampusAsync(Guid id);

        // Additional
        Task<bool> CampusExistsAsync(Guid id);
        Task<bool> CampusNameExistsAsync(string name, Guid? excludeId = null);
    }
}
