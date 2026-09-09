using HikvisionBackend1.DTOs;
using HikvisionBackend1.Models;

namespace HikvisionBackend1.Interfaces
{
    public interface ICamera
    {
        Task<ResponceDTO> AddCamera(Camera model);

        Task<ResponceDTO> GetAllCameras();

        Task<ResponceDTO> GetCameraById(int id);

        Task<ResponceDTO> UpdateCamera(int id, Camera model);

        Task<ResponceDTO> DeleteCamera(int id);
    }
}